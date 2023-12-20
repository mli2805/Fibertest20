using Grpc.Core;
using Grpc.Core.Interceptors;
using Iit.Fibertest.UtilsNet6;

namespace Iit.Fibertest.RtuDaemon;

public class RtuLoggerInterceptor : Interceptor
{
    private readonly ILogger<RtuLoggerInterceptor> _logger;

    public RtuLoggerInterceptor(ILogger<RtuLoggerInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        //LogCall<TRequest, TResponse>(MethodType.Unary, context);

        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            // Note: The gRPC framework also logs exceptions thrown by handlers to .NET Core logging.
            _logger.LogError(Logs.RtuService.ToInt(), ex, $"Error thrown by {context.Method}.");

            throw;
        }
    }
}