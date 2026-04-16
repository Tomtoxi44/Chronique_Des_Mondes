namespace Cdm.Migrations;

using Microsoft.Data.SqlClient;
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
        
        // Check for Azure SQL access token from environment variable
        var azureToken = Environment.GetEnvironmentVariable("AZURE_SQL_ACCESS_TOKEN");
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=ChroniqueDesMondesDb;Trusted_Connection=True;";
        
        // Use Azure SQL if token is available (production/CI environment)
        if (!string.IsNullOrEmpty(azureToken))
        {
            connectionString = "Server=cdm-server-sql.database.windows.net;Database=cdm-bdd-sql;";
            
            var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.AccessToken = azureToken;
            
            optionsBuilder.UseSqlServer(sqlConnection);
        }
        else
        {
            // Use local database for development
            optionsBuilder.UseSqlServer(connectionString);
        }
        
        return new MigrationsContext(optionsBuilder.Options);
    }
}
