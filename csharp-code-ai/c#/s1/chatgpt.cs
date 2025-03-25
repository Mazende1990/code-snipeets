using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.HttpSys.Internal;

namespace Microsoft.AspNetCore.Server.HttpSys
{
    internal unsafe class AsyncAcceptContext : IValueTaskSource<RequestContext>, IDisposable
    {
        private static readonly IOCompletionCallback IOCallback = IOWaitCallback;

        private readonly PreAllocatedOverlapped _preallocatedOverlapped;
        private readonly IRequestContextFactory _requestContextFactory;

        private NativeOverlapped* _overlapped;
        private RequestContext? _requestContext;

        // Mutable struct - do not make this readonly
        private ManualResetValueTaskSourceCore<RequestContext> _mrvts = new()
        {
            RunContinuationsAsynchronously = false
        };

        internal AsyncAcceptContext(HttpSysListener server, IRequestContextFactory requestContextFactory)
        {
            Server = server;
            _requestContextFactory = requestContextFactory;
            _preallocatedOverlapped = new(IOCallback, state: this, pinData: null);
        }

        internal HttpSysListener Server { get; }

        #region Public API

        internal ValueTask<RequestContext> AcceptAsync()
        {
            _mrvts.Reset();
            AllocateRequestContext();

            uint statusCode = BeginHttpReceive();
            if (statusCode is not (UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS or UnsafeNclNativeMethods.ErrorCodes.ERROR_IO_PENDING))
            {
                return ValueTask.FromException<RequestContext>(new HttpSysException((int)statusCode));
            }

            return new ValueTask<RequestContext>(this, _mrvts.Version);
        }

        public RequestContext GetResult(short token) => _mrvts.GetResult(token);
        public ValueTaskSourceStatus GetStatus(short token) => _mrvts.GetStatus(token);

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _mrvts.OnCompleted(continuation, state, token, flags);
        }

        public void Dispose() => Dispose(disposing: true);

        #endregion

        #region I/O Handling

        private static unsafe void IOWaitCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
        {
            var context = (AsyncAcceptContext)ThreadPoolBoundHandle.GetNativeOverlappedState(nativeOverlapped)!;
            context.IOCompleted(errorCode, numBytes);
        }

        private void IOCompleted(uint errorCode, uint numBytes)
        {
            try
            {
                if (errorCode is not (UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS or UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA))
                {
                    _mrvts.SetException(new HttpSysException((int)errorCode));
                    return;
                }

                Debug.Assert(_requestContext != null);

                if (errorCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS)
                {
                    var result = _requestContext;
                    _requestContext = null; // Allow reuse of accept context
                    _mrvts.SetResult(result!);
                }
                else // ERROR_MORE_DATA
                {
                    AllocateRequestContext(numBytes, _requestContext.RequestId);

                    uint statusCode = BeginHttpReceive();
                    if (statusCode is not (UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS or UnsafeNclNativeMethods.ErrorCodes.ERROR_IO_PENDING))
                    {
                        _mrvts.SetException(new HttpSysException((int)statusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                _mrvts.SetException(ex);
            }
        }

        #endregion

        #region Request Management

        private uint BeginHttpReceive()
        {
            Debug.Assert(_requestContext != null);
            bool retry;

            uint statusCode;
            do
            {
                retry = false;
                uint bytesTransferred = 0;

                statusCode = HttpApi.HttpReceiveHttpRequest(
                    Server.RequestQueue.Handle,
                    _requestContext.RequestId,
                    (uint)HttpApiTypes.HTTP_FLAGS.NONE,
                    _requestContext.NativeRequest,
                    _requestContext.Size,
                    &bytesTransferred,
                    _overlapped);

                switch (statusCode)
                {
                    case UnsafeNclNativeMethods.ErrorCodes.ERROR_INVALID_PARAMETER when _requestContext.RequestId != 0:
                        _requestContext.RequestId = 0;
                        retry = true;
                        break;

                    case UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA:
                        AllocateRequestContext(bytesTransferred);
                        retry = true;
                        break;

                    case UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS when HttpSysListener.SkipIOCPCallbackOnSuccess:
                        IOCompleted(statusCode, bytesTransferred);
                        break;
                }
            }
            while (retry);

            return statusCode;
        }

        private void AllocateRequestContext(uint? size = null, ulong requestId = 0)
        {
            _requestContext?.ReleasePins();
            _requestContext?.Dispose();

            var boundHandle = Server.RequestQueue.BoundHandle;

            if (_overlapped != null)
            {
                boundHandle.FreeNativeOverlapped(_overlapped);
            }

            _requestContext = _requestContextFactory.CreateRequestContext(size, requestId);
            _overlapped = boundHandle.AllocateNativeOverlapped(_preallocatedOverlapped);
        }

        #endregion

        #region Cleanup

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _requestContext?.ReleasePins();
            _requestContext?.Dispose();
            _requestContext = null;

            if (_overlapped != null)
            {
                Server.RequestQueue.BoundHandle.FreeNativeOverlapped(_overlapped);
                _overlapped = null;
            }
        }

        #endregion
    }
}
