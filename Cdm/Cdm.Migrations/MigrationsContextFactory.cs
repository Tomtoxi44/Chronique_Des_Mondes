namespace Cdm.Migrations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Design-time factory for MigrationsContext
/// </summary>
public class MigrationsContextFactory : IDesignTimeDbContextFactory<MigrationsContext>
{
    public MigrationsContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MigrationsContext>();
        
        // Use a temporary connection string for design time
        // The real connection string will come from Aspire at runtime
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ChroniqueDesMondes;Trusted_Connection=True;");
        
        return new MigrationsContext(optionsBuilder.Options);
    }
}
