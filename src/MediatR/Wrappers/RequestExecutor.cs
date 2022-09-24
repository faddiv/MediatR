namespace MediatR.Wrappers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class RequestExecutor<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IRequestHandler<TRequest, TResponse> _handler;
    private readonly TRequest _request;
    private readonly CancellationToken _cancellationToken;
    private readonly IEnumerator<IPipelineBehavior<TRequest, TResponse>> _pipelines;
    private readonly RequestHandlerDelegate<TResponse> _nextHandler;
    public RequestExecutor(
        IRequestHandler<TRequest, TResponse> handler, 
        IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelines,
        TRequest request, 
        CancellationToken cancellationToken)
    {
        _nextHandler = new RequestHandlerDelegate<TResponse>(Next);
        _handler = handler;
        _request = request;
        _cancellationToken = cancellationToken;
        _pipelines = pipelines.GetEnumerator();
    }

    public Task<TResponse> Handle()
    {
        return Next();
    }

    private Task<TResponse> Next()
    {
        if(_pipelines.MoveNext())
        {
            return _pipelines.Current.Handle(_request, _cancellationToken, _nextHandler);
        } else
        {
            return _handler.Handle(_request, _cancellationToken);
        }
    }
}