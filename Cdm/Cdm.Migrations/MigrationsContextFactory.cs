namespace Cdm.Migrations;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
/// Design-time factory for MigrationsContext
/// </summary>
public class MigrationsContextFactory : IDesignTimeDbContextFactory<MigrationsContext>
{
    public MigrationsContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MigrationsContext>();

        // Nos migrations sont du SQL brut idempotent écrit à la main : le ModelSnapshot n'est
        // volontairement pas maintenu, donc EF voit toujours des "pending model changes".
        // Sans ça, `dotnet ef database update` échoue en CI (et le déploiement de l'API est
        // annulé), alors que MigrationsManager — qui pose déjà le même Ignore — passe en local.
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));


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
