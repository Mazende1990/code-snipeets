using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Microsoft.AspNetCore.Server.HttpSys;

namespace Microsoft.AspNetCore.Server.HttpSys
{
    /// <summary>
    /// Manages asynchronous HTTP request acceptance for HttpSysListener.
    /// </summary>
    internal unsafe class AsyncAcceptContext : IValueTaskSource<RequestContext>, IDisposable
    {
        private const uint Success = UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS;
        private const uint MoreData = UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA;
        private const uint IoPending = UnsafeNclNativeMethods.ErrorCodes.ERROR_IO_PENDING;
        private const uint InvalidParameter = UnsafeNclNativeMethods.ErrorCodes.ERROR_INVALID_PARAMETER;

        private static readonly IOCompletionCallback IoCompletionCallback = OnIoCompleted;

        private readonly PreAllocatedOverlapped _preallocatedOverlapped;
        private readonly IRequestContextFactory _requestContextFactory;
        private readonly HttpSysListener _server;

        private NativeOverlapped* _overlapped;
        private RequestContext? _requestContext;

        private ManualResetValueTaskSourceCore<RequestContext> _valueTaskSource = new()
        {
            RunContinuationsAsynchronously = false
        };

        public AsyncAcceptContext(HttpSysListener server, IRequestContextFactory requestContextFactory)
        {
            _server = server;
            _requestContextFactory = requestContextFactory;
            _preallocatedOverlapped = new(IoCompletionCallback, state: this, pinData: null);
        }

        /// <summary>
        /// Asynchronously accepts a new HTTP request context.
        /// </summary>
        internal ValueTask<RequestContext> AcceptAsync()
        {
            _valueTaskSource.Reset();
            AllocateNativeRequest();

            uint statusCode = BeginGetContext();
            return IsSuccessOrPending(statusCode)
                ? new ValueTask<RequestContext>(this, _valueTaskSource.Version)
                : ValueTask.FromException<RequestContext>(new HttpSysException((int)statusCode));
        }

        private static bool IsSuccessOrPending(uint statusCode) =>
            statusCode == Success || statusCode == IoPending;

        private static void OnIoCompleted(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
        {
            var acceptContext = (AsyncAcceptContext)ThreadPoolBoundHandle.GetNativeOverlappedState(nativeOverlapped)!;
            acceptContext.CompleteIo(errorCode, numBytes);
        }

        private void CompleteIo(uint errorCode, uint numBytes)
        {
            try
            {
                if (!IsCompletionSuccessful(errorCode))
                {
                    _valueTaskSource.SetException(new HttpSysException((int)errorCode));
                    return;
                }

                ProcessRequestCompletion(errorCode, numBytes);
            }
            catch (Exception exception)
            {
                _valueTaskSource.SetException(exception);
            }
        }

        private bool IsCompletionSuccessful(uint errorCode) =>
            errorCode == Success || errorCode == MoreData;

        private void ProcessRequestCompletion(uint errorCode, uint numBytes)
        {
            Debug.Assert(_requestContext != null);

            if (errorCode == Success)
            {
                CompleteRequestSuccessfully();
            }
            else
            {
                HandleInsufficientBuffer(numBytes);
            }
        }

        private void CompleteRequestSuccessfully()
        {
            var requestContext = _requestContext;
            _requestContext = null;
            _valueTaskSource.SetResult(requestContext);
        }

        private void HandleInsufficientBuffer(uint numBytes)
        {
            AllocateNativeRequest(numBytes, _requestContext!.RequestId);
            uint statusCode = BeginGetContext();

            if (!IsSuccessOrPending(statusCode))
            {
                _valueTaskSource.SetException(new HttpSysException((int)statusCode));
            }
        }

        private uint BeginGetContext()
        {
            uint statusCode;
            do
            {
                Debug.Assert(_requestContext != null);
                statusCode = TryReceiveRequest(_requestContext);
            }
            while (ShouldRetry(_requestContext!, statusCode));

            return statusCode;
        }

        private uint TryReceiveRequest(RequestContext requestContext)
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

        private bool ShouldRetry(RequestContext requestContext, uint statusCode)
        {
            if (statusCode == InvalidParameter && requestContext.RequestId != 0)
            {
                requestContext.RequestId = 0;
                return true;
            }

            if (statusCode == MoreData)
            {
                AllocateNativeRequest(_valueTaskSource.GetResult(default).Size);
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

        // IValueTaskSource implementations
        public RequestContext GetResult(short token) => _valueTaskSource.GetResult(token);
        public ValueTaskSourceStatus GetStatus(short token) => _valueTaskSource.GetStatus(token);
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _valueTaskSource.OnCompleted(continuation, state, token, flags);
    }
}