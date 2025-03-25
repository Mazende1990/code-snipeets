using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Microsoft.AspNetCore.Server.HttpSys;

namespace Microsoft.AspNetCore.Server.HttpSys
{
    /// <summary>
    /// Manages asynchronous HTTP request acceptance for HttpSysListener
    /// </summary>
    internal unsafe class AsyncAcceptContext : IValueTaskSource<RequestContext>, IDisposable
    {
        private const uint ErrorSuccess = UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS;
        private const uint ErrorMoreData = UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA;
        private const uint ErrorIoPending = UnsafeNclNativeMethods.ErrorCodes.ERROR_IO_PENDING;
        private const uint ErrorInvalidParameter = UnsafeNclNativeMethods.ErrorCodes.ERROR_INVALID_PARAMETER;

        private static readonly IOCompletionCallback IOCallback = CreateIOWaitCallback();

        private readonly PreAllocatedOverlapped _preallocatedOverlapped;
        private readonly IRequestContextFactory _requestContextFactory;
        private readonly HttpSysListener _server;

        private NativeOverlapped* _overlapped;
        private RequestContext? _currentRequest;
        private ManualResetValueTaskSourceCore<RequestContext> _taskSource = new() 
        { 
            RunContinuationsAsynchronously = false 
        };

        public AsyncAcceptContext(HttpSysListener server, IRequestContextFactory requestContextFactory)
        {
            _server = server;
            _requestContextFactory = requestContextFactory;
            _preallocatedOverlapped = new PreAllocatedOverlapped(IOCallback, state: this, pinData: null);
        }

        /// <summary>
        /// Asynchronously accepts a new HTTP request context
        /// </summary>
        public ValueTask<RequestContext> AcceptAsync()
        {
            _taskSource.Reset();
            AllocateRequestContext();

            uint statusCode = BeginGetContext();
            return IsSuccessOrPendingStatus(statusCode)
                ? new ValueTask<RequestContext>(this, _taskSource.Version)
                : ValueTask.FromException<RequestContext>(new HttpSysException((int)statusCode));
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanupCurrentRequest();
                FreeNativeOverlapped();
            }
        }

        // IValueTaskSource implementation
        public RequestContext GetResult(short token) => _taskSource.GetResult(token);
        public ValueTaskSourceStatus GetStatus(short token) => _taskSource.GetStatus(token);
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _taskSource.OnCompleted(continuation, state, token, flags);

        private static IOCompletionCallback CreateIOWaitCallback() => (errorCode, numBytes, nativeOverlapped) =>
        {
            var context = (AsyncAcceptContext)ThreadPoolBoundHandle.GetNativeOverlappedState(nativeOverlapped)!;
            context.HandleIOCompletion(errorCode, numBytes);
        };

        private void HandleIOCompletion(uint errorCode, uint numBytes)
        {
            try
            {
                if (!IsSuccessfulCompletion(errorCode))
                {
                    _taskSource.SetException(new HttpSysException((int)errorCode));
                    return;
                }

                ProcessRequestCompletion(errorCode, numBytes);
            }
            catch (Exception ex)
            {
                _taskSource.SetException(ex);
            }
        }

        private void ProcessRequestCompletion(uint errorCode, uint numBytes)
        {
            if (errorCode == ErrorSuccess)
            {
                CompleteRequest();
            }
            else // ErrorMoreData
            {
                HandleBufferInsufficient(numBytes);
            }
        }

        private void CompleteRequest()
        {
            var completedRequest = _currentRequest;
            _currentRequest = null;
            _taskSource.SetResult(completedRequest);
        }

        private void HandleBufferInsufficient(uint numBytes)
        {
            AllocateRequestContext(numBytes, _currentRequest!.RequestId);
            uint statusCode = BeginGetContext();

            if (!IsSuccessOrPendingStatus(statusCode))
            {
                _taskSource.SetException(new HttpSysException((int)statusCode));
            }
        }

        private uint BeginGetContext()
        {
            uint statusCode;
            do
            {
                statusCode = TryReceiveRequest();
            }
            while (ShouldRetryRequest(statusCode));

            return statusCode;
        }

        private uint TryReceiveRequest()
        {
            uint bytesTransferred = 0;
            return HttpApi.HttpReceiveHttpRequest(
                _server.RequestQueue.Handle,
                _currentRequest!.RequestId,
                (uint)HttpApiTypes.HTTP_FLAGS.NONE,
                _currentRequest.NativeRequest,
                _currentRequest.Size,
                &bytesTransferred,
                _overlapped);
        }

        private bool ShouldRetryRequest(uint statusCode)
        {
            if (statusCode == ErrorInvalidParameter && _currentRequest!.RequestId != 0)
            {
                _currentRequest.RequestId = 0;
                return true;
            }

            if (statusCode == ErrorMoreData)
            {
                AllocateRequestContext(_currentRequest!.Size);
                return true;
            }

            return false;
        }

        private void AllocateRequestContext(uint? size = null, ulong requestId = 0)
        {
            CleanupCurrentRequest();
            FreeNativeOverlapped();

            _currentRequest = _requestContextFactory.CreateRequestContext(size, requestId);
            _overlapped = _server.RequestQueue.BoundHandle.AllocateNativeOverlapped(_preallocatedOverlapped);
        }

        private void CleanupCurrentRequest()
        {
            _currentRequest?.ReleasePins();
            _currentRequest?.Dispose();
            _currentRequest = null;
        }

        private void FreeNativeOverlapped()
        {
            if (_overlapped != null)
            {
                _server.RequestQueue.BoundHandle.FreeNativeOverlapped(_overlapped);
                _overlapped = null;
            }
        }

        private static bool IsSuccessOrPendingStatus(uint statusCode) => 
            statusCode == ErrorSuccess || statusCode == ErrorIoPending;

        private static bool IsSuccessfulCompletion(uint errorCode) => 
            errorCode == ErrorSuccess || errorCode == ErrorMoreData;
    }
}