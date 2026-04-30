namespace Entravel.EF.Infrastructure.Persistence.Entities;

internal abstract class PersistenceEntityBase
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
