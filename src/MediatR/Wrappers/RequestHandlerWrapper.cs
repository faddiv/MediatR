namespace MediatR.Wrappers;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public abstract class RequestHandlerBase : HandlerBase
{
    public abstract Task<object?> Handle(object request, CancellationToken cancellationToken,
        ServiceFactory serviceFactory);

}

public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
{
    public abstract Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
        ServiceFactory serviceFactory);
}

public class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override async Task<object?> Handle(object request, CancellationToken cancellationToken,
        ServiceFactory serviceFactory) =>
        await Handle((IRequest<TResponse>)request, cancellationToken, serviceFactory).ConfigureAwait(false);

    public override Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken,
        ServiceFactory serviceFactory)
    {
#if true
        Task<TResponse> Handler() => GetHandler<IRequestHandler<TRequest, TResponse>>(serviceFactory).Handle((TRequest) request, cancellationToken);

        return serviceFactory
            .GetInstances<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .Aggregate((RequestHandlerDelegate<TResponse>) Handler, (next, pipeline) => () => pipeline.Handle((TRequest)request, cancellationToken, next))();
        
#else
        var executor = new RequestExecutor<TRequest, TResponse>(
            GetHandler<IRequestHandler<TRequest, TResponse>>(serviceFactory),
            serviceFactory.GetInstances<IPipelineBehavior<TRequest, TResponse>>(),
            (TRequest) request,
            cancellationToken);
        return executor.Handle();
#endif
    }
}
