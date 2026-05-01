namespace Entravel.Domain.Common;

public abstract class BaseDomainModel
{
    public Guid Id { get; protected set; }
    public DateTime CreatedDate { get; protected set; }
    public DateTime? UpdatedDate { get; protected set; }
}

