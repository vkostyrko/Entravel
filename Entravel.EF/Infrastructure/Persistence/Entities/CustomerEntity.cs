namespace Entravel.EF.Infrastructure.Persistence.Entities;

internal sealed class CustomerEntity : PersistenceEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

