using MediatR;

namespace CleanApi.Commands.Interfaces;

/// <summary>
/// Marker interface for commands without response
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Interface for commands that return a response
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
