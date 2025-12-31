namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Marker interface for commands that don't return a result.
/// Commands represent state-changing operations.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Marker interface for commands that return a result.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command.</typeparam>
public interface ICommand<out TResult>
{
}
