using System.IO;
using System.Threading;

namespace MediatR.Examples;

using System.Threading.Tasks;

public class PingHandler : IRequestHandler<Ping, Pong>
{
    private readonly TextWriter _writer;
    private readonly Pong _result = new Pong { Message = "Pong" };

    public PingHandler(TextWriter writer)
    {
        _writer = writer;
    }

    public Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        //await _writer.WriteLineAsync($"--- Handled Ping: {request.Message}");
        return Task.FromResult(_result);
    }
}