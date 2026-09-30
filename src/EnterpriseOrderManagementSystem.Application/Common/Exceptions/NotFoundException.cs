namespace EnterpriseOrderManagementSystem.Application.Common.Exceptions;

/// <summary>
/// Represents an attempt to access a resource that does not exist.
/// </summary>
public sealed class NotFoundException : ApplicationException
{
    public NotFoundException(
        string resourceName,
        object resourceId)
        : base($"{resourceName} with identifier '{resourceId}' was not found.")
    {
    }
}