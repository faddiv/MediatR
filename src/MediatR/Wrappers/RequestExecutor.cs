namespace MediatR.Wrappers;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class RequestExecutor<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    private readonly IRequestHandler<TRequest, TResponse> _handler;
    private readonly TRequest _request;
    private readonly CancellationToken _cancellationToken;
    private readonly IList<IPipelineBehavior<TRequest, TResponse>> _pipelines;
    private readonly RequestHandlerDelegate<TResponse> _nextHandler;
    private int _index = 0;
    public RequestExecutor(
        IRequestHandler<TRequest, TResponse> handler,
        IList<IPipelineBehavior<TRequest, TResponse>> pipelines,
        TRequest request,
        CancellationToken cancellationToken)
    {
        _nextHandler = new RequestHandlerDelegate<TResponse>(Next);
        _handler = handler;
        _request = request;
        _cancellationToken = cancellationToken;
        _pipelines = pipelines;
    }

    public Task<TResponse> Handle()
    {
        return Next();
    }

    private Task<TResponse> Next()
    {
        if (_index < _pipelines.Count)
        {
            var pp = _pipelines[_index];
            _index++;
            return pp.Handle(_request, _cancellationToken, _nextHandler);
        } else
        {
            return _handler.Handle(_request, _cancellationToken);
        }
    }
}