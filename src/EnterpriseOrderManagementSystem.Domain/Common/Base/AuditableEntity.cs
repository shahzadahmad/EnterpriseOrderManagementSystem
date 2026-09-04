namespace EnterpriseOrderManagementSystem.Domain.Common.Base;

/// <summary>
/// Represents an aggregate root that tracks
/// creation, modification, and deletion metadata.
/// </summary>
/// <typeparam name="TKey">
/// Identifier type.
/// </typeparam>
public abstract class AuditableEntity<TKey> : AggregateRoot<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Gets the UTC date/time when the entity was created.
    /// </summary>
    public DateTime CreatedOnUtc { get; protected set; }

    /// <summary>
    /// Gets the identifier of the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// Gets the UTC date/time of the most recent modification.
    /// </summary>
    public DateTime? LastModifiedOnUtc { get; protected set; }

    /// <summary>
    /// Gets the identifier of the user who last modified the entity.
    /// </summary>
    public string? LastModifiedBy { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the entity
    /// has been soft deleted.
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Gets the UTC date/time when the entity
    /// was deleted.
    /// </summary>
    public DateTime? DeletedOnUtc { get; protected set; }

    /// <summary>
    /// Gets the identifier of the user who deleted the entity.
    /// </summary>
    public string? DeletedBy { get; protected set; }

    /// <summary>
    /// Marks the entity as created.
    /// This method is intended to be called by the Infrastructure layer.
    /// </summary>
    public void MarkCreated(
        DateTime utcNow,
        string? createdBy)
    {
        CreatedOnUtc = utcNow;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Marks the entity as modified.
    /// </summary>
    public void MarkModified(
        DateTime utcNow,
        string? modifiedBy)
    {
        LastModifiedOnUtc = utcNow;
        LastModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Performs a soft delete.
    /// </summary>
    public void MarkDeleted(
        DateTime utcNow,
        string? deletedBy)
    {
        IsDeleted = true;
        DeletedOnUtc = utcNow;
        DeletedBy = deletedBy;
    }

    /// <summary>
    /// Restores a previously soft-deleted entity.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedOnUtc = null;
        DeletedBy = null;
    }
}