using MediatR;

namespace DeveloperEvaluation.ORM.CQRS;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}