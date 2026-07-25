using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandDock.Application.Abstractions;
using CommandDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommandDock.Infrastructure.Persistence;

public class CommandRepository : ICommandRepository
{
    private readonly IDbContextFactory<CommandDockDbContext> _factory;

    public CommandRepository(IDbContextFactory<CommandDockDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<CommandDefinition>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Commands.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<CommandDefinition?> GetAsync(Guid id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Commands.FindAsync(id);
    }

    public async Task AddAsync(CommandDefinition command)
    {
        await using var db = await _factory.CreateDbContextAsync();
        command.CreatedUtc = DateTime.UtcNow;
        command.UpdatedUtc = command.CreatedUtc;
        db.Commands.Add(command);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(CommandDefinition command)
    {
        await using var db = await _factory.CreateDbContextAsync();
        command.UpdatedUtc = DateTime.UtcNow;
        db.Commands.Update(command);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var entity = await db.Commands.FindAsync(id);
        if (entity is null) return;
        db.Commands.Remove(entity);
        await db.SaveChangesAsync();
    }
}
