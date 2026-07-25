using System;
using System.IO;
using CommandDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommandDock.Infrastructure.Persistence;

public class CommandDockDbContext : DbContext
{
    public DbSet<CommandDefinition> Commands => Set<CommandDefinition>();

    public static string DatabasePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CommandDock",
        "commanddock.db");

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        options.UseSqlite($"Data Source={DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommandDefinition>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).IsRequired().HasMaxLength(200);
            b.Property(c => c.Icon).HasMaxLength(16);
            b.Property(c => c.CommandText).IsRequired();
            b.HasIndex(c => c.Name);
        });
    }
}
