using System.Threading;

namespace MediatR.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using StructureMap;
using Xunit;

public class SendAsyncTests
{
    public const int DelayTime = 1;
    public class Ping : IRequest<Pong>
    {
        public List<string> Calls { get; } = new List<string>();
    }

    public class Pong
    {
    }

    public class PingHandler : IRequestHandler<Ping, Pong>
    {
        public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
        {
            request.Calls.Add("Handler Delaying");
            await Task.Delay(DelayTime).ConfigureAwait(false);
            request.Calls.Add("Handler Delayed");
            return new Pong();
        }
    }

    public class OuterAsyncBehavior : IPipelineBehavior<Ping, Pong>
    {
        public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken, RequestHandlerDelegate<Pong> next)
        {
            request.Calls.Add("Outer Behavior Before Delaying");
            await Task.Delay(DelayTime).ConfigureAwait(false);
            request.Calls.Add("Outer Behavior Before Delayed");
            var result = await next();
            request.Calls.Add("Outer Behavior After Delaying");
            await Task.Delay(DelayTime).ConfigureAwait(false);
            request.Calls.Add("Outer Behavior After Delayed");
            return result;
        }
    }

    public class InnerAsyncBehavior : IPipelineBehavior<Ping, Pong>
    {
        public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken, RequestHandlerDelegate<Pong> next)
        {
            request.Calls.Add("Inner Behavior Before Delaying");
            await Task.Delay(DelayTime).ConfigureAwait(false);
            request.Calls.Add("Inner Behavior Before Delayed");
            var result = await next();
            request.Calls.Add("Inner Behavior After Delaying");
            await Task.Delay(DelayTime).ConfigureAwait(false);
            request.Calls.Add("Inner Behavior After Delayed");
            return result;
        }
    }

    public class DuplicatingAsyncBehavior : IPipelineBehavior<Ping, Pong>
    {
        public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken, RequestHandlerDelegate<Pong> next)
        {
            request.Calls.Add("Duplicationg Behavior 0");
            var result = await next();
            request.Calls.Add("Duplicationg Behavior 1");
            result = await next();
            request.Calls.Add("Duplicationg Behavior 2");
            return result;
        }
    }

    [Fact]
    public async Task Should_execute_operations_in_order()
    {
        var container = new Container(cfg =>
        {
            cfg.For<IRequestHandler<Ping, Pong>>().Use<PingHandler>();
            cfg.For<IPipelineBehavior<Ping, Pong>>()
                .Use<OuterAsyncBehavior>();
            cfg.For<IPipelineBehavior<Ping, Pong>>()
                .Use<InnerAsyncBehavior>();
            cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var ping = new Ping();
        var response = await mediator.Send(ping);

        ping.Calls.ShouldBe(new[]
        {
            "Outer Behavior Before Delaying",
            "Outer Behavior Before Delayed",
            "Inner Behavior Before Delaying",
            "Inner Behavior Before Delayed",
            "Handler Delaying",
            "Handler Delayed",
            "Inner Behavior After Delaying",
            "Inner Behavior After Delayed",
            "Outer Behavior After Delaying",
            "Outer Behavior After Delayed"
        });
    }

    [Fact]
    public async Task Should_execute_operations_in_order_if_there_is_a_duplicating_behavior()
    {
        var container = new Container(cfg =>
        {
            cfg.For<IRequestHandler<Ping, Pong>>().Use<PingHandler>();
            cfg.For<IPipelineBehavior<Ping, Pong>>()
                .Use<DuplicatingAsyncBehavior>();
            cfg.For<IPipelineBehavior<Ping, Pong>>()
                .Use<InnerAsyncBehavior>();
            cfg.For<ServiceFactory>().Use<ServiceFactory>(ctx => t => ctx.GetInstance(t));
            cfg.For<IMediator>().Use<Mediator>();
        });

        var mediator = container.GetInstance<IMediator>();

        var ping = new Ping();
        var response = await mediator.Send(ping);

        ping.Calls.ShouldBe(new[]
        {
            "Duplicationg Behavior 0",
            "Inner Behavior Before Delaying",
            "Inner Behavior Before Delayed",
            "Handler Delaying",
            "Handler Delayed",
            "Inner Behavior After Delaying",
            "Inner Behavior After Delayed",
            "Duplicationg Behavior 1",
            "Inner Behavior Before Delaying",
            "Inner Behavior Before Delayed",
            "Handler Delaying",
            "Handler Delayed",
            "Inner Behavior After Delaying",
            "Inner Behavior After Delayed",
            "Duplicationg Behavior 2",
        });
    }
}