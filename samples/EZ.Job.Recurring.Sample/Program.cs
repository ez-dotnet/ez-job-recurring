using EZ.Job.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddEZJob(options =>
{
    options.WorkerCount = 4;
})
.AddRecurringJobs(options =>
{
    options.Register<EmailCleanupService>(s => s.LimparAsync(), "*/1 * * * *"); // a cada 1 minuto
});

builder.Services.AddHostedService<SampleRunner>();

using var host = builder.Build();
await host.RunAsync();

public class SampleRunner : BackgroundService
{
    private readonly IRecurringJobManager _recurring;

    public SampleRunner(IRecurringJobManager recurring)
    {
        _recurring = recurring;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("EZ.Job.Recurring rodando. Jobs recorrentes serão executados conforme cron.");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}

public class EmailCleanupService
{
    public async Task LimparAsync()
    {
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Limpando e-mails expirados...");
        await Task.Delay(100);
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Limpeza concluída.");
    }
}
