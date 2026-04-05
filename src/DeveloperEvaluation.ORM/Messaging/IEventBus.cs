using DeveloperEvaluation.Domain.Events;

namespace DeveloperEvaluation.ORM.Messaging;

public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : class;
    Task SendAsync<T>(T command) where T : class;
}