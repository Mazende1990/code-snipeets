using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Microsoft.AspNetCore.Server.HttpSys;

namespace Microsoft.AspNetCore.Server.HttpSys
{
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
        private RequestContext? _requestContext;

        private ManualResetValueTaskSourceCore<RequestContext> _mrvts = new()
        {
            RunContinuationsAsynchronously = false
        };

        public AsyncAcceptContext(HttpSysListener server, IRequestContextFactory requestContextFactory)
        {
            _server = server;
            _requestContextFactory = requestContextFactory;
            _preallocatedOverlapped = new(IOCallback, state: this, pinData: null);
        }

        public ValueTask<RequestContext> AcceptAsync()
        {
            _mrvts.Reset();
            AllocateNativeRequest();

            uint statusCode = QueueBeginGetContext();
            return IsSuccessOrPendingStatus(statusCode)
                ? new ValueTask<RequestContext>(this, _mrvts.Version)
                : ValueTask.FromException<RequestContext>(new HttpSysException((int)statusCode));
        }

        private static bool IsSuccessOrPendingStatus(uint statusCode) =>
            statusCode == ErrorSuccess || statusCode == ErrorIoPending;

        private static IOCompletionCallback CreateIOWaitCallback() =>
            (uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped) =>
            {
                var acceptContext = (AsyncAcceptContext)ThreadPoolBoundHandle.GetNativeOverlappedState(nativeOverlapped)!;
                acceptContext.HandleIOCompletion(errorCode, numBytes);
            };

        private void HandleIOCompletion(uint errorCode, uint numBytes)
        {
            try 
            {
                if (!IsSuccessfulCompletion(errorCode))
                {
                    _mrvts.SetException(new HttpSysException((int)errorCode));
                    return;
                }

                ProcessCompletedRequest(errorCode, numBytes);
            }
            catch (Exception exception)
            {
                _mrvts.SetException(exception);
            }
        }

        private bool IsSuccessfulCompletion(uint errorCode) =>
            errorCode == ErrorSuccess || errorCode == ErrorMoreData;

        private void ProcessCompletedRequest(uint errorCode, uint numBytes)
        {
            Debug.Assert(_requestContext != null);

            if (errorCode == ErrorSuccess)
            {
                CompleteSuccessfulRequest();
            }
            else
            {
                HandleInsufficientBuffer(numBytes);
            }
        }

        private void CompleteSuccessfulRequest()
        {
            var requestContext = _requestContext;
            _requestContext = null;
            _mrvts.SetResult(requestContext);
        }

        private void HandleInsufficientBuffer(uint numBytes)
        {
            AllocateNativeRequest(numBytes, _requestContext!.RequestId);
            uint statusCode = QueueBeginGetContext();

            if (!IsSuccessOrPendingStatus(statusCode))
            {
                _mrvts.SetException(new HttpSysException((int)statusCode));
            }
        }

        private uint QueueBeginGetContext()
        {
            uint statusCode;
            do
            {
                Debug.Assert(_requestContext != null);
                statusCode = TryReceiveHttpRequest(_requestContext);
            }
            while (ShouldRetryRequest(statusCode, _requestContext!));

            return statusCode;
        }

        private uint TryReceiveHttpRequest(RequestContext requestContext)
        {
            uint bytesTransferred = 0;
            return HttpApi.HttpReceiveHttpRequest(
                _server.RequestQueue.Handle,
                requestContext.RequestId,
                (uint)HttpApiTypes.HTTP_FLAGS.NONE,
                requestContext.NativeRequest,
                requestContext.Size,
                &bytesTransferred,
                _overlapped);
        }

        private bool ShouldRetryRequest(uint statusCode, RequestContext requestContext)
        {
            if (statusCode == ErrorInvalidParameter && requestContext.RequestId != 0)
            {
                requestContext.RequestId = 0;
                return true;
            }

            if (statusCode == ErrorMoreData)
            {
                AllocateNativeRequest(_mrvts.GetResult(default).Size);
                return true;
            }

            return false;
        }

        private void AllocateNativeRequest(uint? size = null, ulong requestId = 0)
        {
            _requestContext?.ReleasePins();
            _requestContext?.Dispose();

            var boundHandle = _server.RequestQueue.BoundHandle;
            if (_overlapped != null)
            {
                boundHandle.FreeNativeOverlapped(_overlapped);
            }

            _requestContext = _requestContextFactory.CreateRequestContext(size, requestId);
            _overlapped = boundHandle.AllocateNativeOverlapped(_preallocatedOverlapped);
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _requestContext?.ReleasePins();
                _requestContext?.Dispose();
                _requestContext = null;

                var boundHandle = _server.RequestQueue.BoundHandle;
                if (_overlapped != null)
                {
                    boundHandle.FreeNativeOverlapped(_overlapped);
                    _overlapped = null;
                }
            }
        }

        public RequestContext GetResult(short token) => _mrvts.GetResult(token);
        public ValueTaskSourceStatus GetStatus(short token) => _mrvts.GetStatus(token);
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _mrvts.OnCompleted(continuation, state, token, flags);
    }
}