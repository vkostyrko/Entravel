using Microsoft.EntityFrameworkCore;

namespace Entravel.EF.DbContext;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
}

