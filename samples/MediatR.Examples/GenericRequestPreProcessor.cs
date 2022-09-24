using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;

namespace MediatR.Examples;

public class GenericRequestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
{
    private readonly TextWriter _writer;

    public GenericRequestPreProcessor(TextWriter writer)
    {
        _writer = writer;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;//_writer.WriteLineAsync("- Starting Up");
    }
}