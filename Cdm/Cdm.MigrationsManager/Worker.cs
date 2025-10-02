namespace Cdm.MigrationsManager;

using Cdm.Migrations;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using System.Diagnostics;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MigrationsContext>();

            await RunMigrationAsync(dbContext, cancellationToken);
            //await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(MigrationsContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    //private static async Task SeedDataAsync(MigrationContext dbContext, CancellationToken cancellationToken)
    //{
    //    SupportTicket firstTicket = new()
    //    {
    //        Title = "Test Ticket",
    //        Description = "Default ticket, please ignore!",
    //        Completed = true
    //    };

    //    var strategy = dbContext.Database.CreateExecutionStrategy();
    //    await strategy.ExecuteAsync(async () =>
    //    {
    //        // Seed the database
    //        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    //        await dbContext.Tickets.AddAsync(firstTicket, cancellationToken);
    //        await dbContext.SaveChangesAsync(cancellationToken);
    //        await transaction.CommitAsync(cancellationToken);
    //    });
    //}
}
