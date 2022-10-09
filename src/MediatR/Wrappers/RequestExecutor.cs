namespace MediatR.Wrappers;

using System;
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
    public RequestExecutor(
        IRequestHandler<TRequest, TResponse> handler,
        IList<IPipelineBehavior<TRequest, TResponse>> pipelines,
        TRequest request,
        CancellationToken cancellationToken)
    {
        _handler = handler;
        _request = request;
        _cancellationToken = cancellationToken;
        _pipelines = pipelines;
    }

    public Task<TResponse> Handle()
    {
        return Next(0);
    }

    private Task<TResponse> Next(int index)
    {
        if (index < _pipelines.Count)
        {
            var pp = _pipelines[index];
            Task<TResponse> NextHandler() => Next(index + 1);
            return pp.Handle(_request, _cancellationToken, NextHandler);
        } else
        {
            return _handler.Handle(_request, _cancellationToken);
        }
    }
}