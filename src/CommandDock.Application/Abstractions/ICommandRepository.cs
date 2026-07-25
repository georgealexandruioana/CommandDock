using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandDock.Domain.Entities;

namespace CommandDock.Application.Abstractions;

public interface ICommandRepository
{
    Task<IReadOnlyList<CommandDefinition>> GetAllAsync();
    Task<CommandDefinition?> GetAsync(Guid id);
    Task AddAsync(CommandDefinition command);
    Task UpdateAsync(CommandDefinition command);
    Task DeleteAsync(Guid id);
}
