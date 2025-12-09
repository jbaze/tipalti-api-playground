using MediatR;

namespace CleanApi.Commands.Interfaces;

/// <summary>
/// Interface for command handlers without response
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>, ICommandHandlerBase
    where TCommand : ICommand
{
}

/// <summary>
/// Interface for command handlers that return a response
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>, ICommandHandlerBase
    where TCommand : ICommand<TResponse>
{
}

/// <summary>
/// Marker interface for command handlers
/// </summary>
public interface ICommandHandlerBase
{
}
