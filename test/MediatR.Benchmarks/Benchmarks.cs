using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using MediatR.Examples;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Benchmarks
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private IMediator _mediator;
        private readonly Ping _request = new Ping { Message = "Hello World" };
        private readonly Pinged _notification = new Pinged();
        private readonly IEnumerable<INotificationHandler<Pinged>> _notificationHandler = new List<INotificationHandler<Pinged>>
        {
            new PingedHandler(TextWriter.Null),
            new PingedAlsoHandler(TextWriter.Null),
            new ConstrainedPingedHandler<Pinged>(TextWriter.Null),
        };
        private readonly PingHandler _handler = new PingHandler(TextWriter.Null);
        private readonly List<IPipelineBehavior<Ping, Pong>> _behaviors = new List<IPipelineBehavior<Ping, Pong>>
                {
                    new RequestPreProcessorBehavior<Ping, Pong>(new List<IRequestPreProcessor<Ping>>{
                        new GenericRequestPreProcessor<Ping>(TextWriter.Null)
                    }),
                    new RequestPostProcessorBehavior<Ping, Pong>(new List<IRequestPostProcessor<Ping, Pong>>{
                        new GenericRequestPostProcessor<Ping, Pong>(TextWriter.Null)
                    }),
                    new GenericPipelineBehavior<Ping, Pong>(TextWriter.Null),
                };

        [GlobalSetup]
        public void GlobalSetup()
        {
            var services = new ServiceCollection();

            services.AddSingleton(TextWriter.Null);

            services.AddMediatR(typeof(Ping));
            services.AddSingleton(_handler);

            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            services.AddSingleton(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            services.AddSingleton(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));

            var provider = services.BuildServiceProvider();

            _mediator = new Mediator(new ServiceFactory(StupidFactory));
            //_mediator = provider.GetRequiredService<IMediator>();
        }

        public object StupidFactory(Type type)
        {
            if (type == typeof(IRequestHandler<Ping, Pong>))
            {
                return _handler;
            }
            else if (type == typeof(IEnumerable<IPipelineBehavior<Ping, Pong>>))
            {
                return _behaviors;
            }
            else if (type == typeof(IEnumerable<INotificationHandler<Pinged>>))
            {
                return _notificationHandler;
            }
            throw new NotSupportedException(type.FullName);
        }

        [Benchmark]
        public Task SendingRequests()
        {
            return _mediator.Send(_request);
        }

        [Benchmark]
        public Task PublishingNotifications()
        {
            return _mediator.Publish(_notification);
        }
    }
}
