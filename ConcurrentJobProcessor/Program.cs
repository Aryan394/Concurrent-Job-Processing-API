using ConcurrentJobProcessor.Data;
using ConcurrentJobProcessor.Services;
using Microsoft.EntityFrameworkCore;

LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string is missing. Set ConnectionStrings__DefaultConnection in the .env file.");
}

builder.Services.AddDbContext<JobDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IJobQueue, JobQueue>();
builder.Services.AddHostedService<JobWorker>();
builder.Services.AddScoped<IJobStore, JobStore>();
builder.Services.AddScoped<JobRecoveryService>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var recoveryService =
        scope.ServiceProvider
            .GetRequiredService<JobRecoveryService>();

    await recoveryService.RecoverJobsAsync(
        CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

static void LoadDotEnv()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        var path = Path.Combine(directory.FullName, ".env");
        if (File.Exists(path))
        {
            ApplyEnvFile(path);
            return;
        }

        directory = directory.Parent;
    }
}

static void ApplyEnvFile(string path)
{
    foreach (var rawLine in File.ReadAllLines(path))
    {
        var line = rawLine.Trim();
        if (line.Length == 0 || line.StartsWith('#'))
            continue;

        var separator = line.IndexOf('=');
        if (separator <= 0)
            continue;

        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim();
        if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
            value = value[1..^1];

        Environment.SetEnvironmentVariable(key, value);
    }
}
