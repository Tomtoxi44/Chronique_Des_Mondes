using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddConnectionString("DefaultConnection");

// Add Scalar API Reference for all services
var scalar = builder.AddScalarApiReference(options =>
{
    // Configure global options. They will apply to all services
    options.WithTheme(ScalarTheme.Purple);
});


var migrations = builder.AddProject<Projects.Cdm_MigrationsManager>("migrations").WithReference(db).WaitFor(db);

var apiService = builder
    .AddProject<Projects.Cdm_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithReference(migrations)
    .WaitForCompletion(migrations);

builder.AddProject<Projects.Cdm_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

scalar
    .WithApiReference(apiService);

builder.Build().Run();
