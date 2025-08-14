using MediatR;

namespace Ambev.DeveloperEvaluation.ORM.CQRS;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}