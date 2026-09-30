using MediatR;

namespace EnterpriseOrderManagementSystem.Application.Common.Interfaces;

/// <summary>
/// Marker interface for commands that modify application state.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}