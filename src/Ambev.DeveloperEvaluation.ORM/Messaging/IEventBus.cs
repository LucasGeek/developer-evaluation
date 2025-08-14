using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.ORM.Messaging;

public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : class;
    Task SendAsync<T>(T command) where T : class;
}