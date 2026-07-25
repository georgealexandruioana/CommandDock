using CommandDock.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CommandDock.Infrastructure.Persistence;

public sealed class SqliteDatabaseInitializer : IDatabaseInitializer
{
    private readonly IDbContextFactory<CommandDockDbContext> _factory;

    public SqliteDatabaseInitializer(IDbContextFactory<CommandDockDbContext> factory)
    {
        _factory = factory;
    }

    public void EnsureCreated()
    {
        using var db = _factory.CreateDbContext();
        db.Database.EnsureCreated();
        try { db.Database.ExecuteSqlRaw("ALTER TABLE Commands ADD COLUMN Icon TEXT NULL"); }
        catch { /* column already exists — fresh DB created with column, or older DB already bootstrapped */ }
    }
}
