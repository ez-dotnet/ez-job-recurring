using EZ.Job.Core;
using EZJob.Recurring;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class EZJobRecurringExtensions
{
    public static EZJobBuilder AddRecurringJobs(this EZJobBuilder builder, Action<RecurringJobOptions>? configure = null)
    {
        var options = new RecurringJobOptions();
        configure?.Invoke(options);

        builder.Options.RecurringWorkerCount = options.WorkerCount;
        builder.Options.RecurringPollIntervalSeconds = options.PollIntervalSeconds;

        var manager = new RecurringJobManager();
        manager.AddInitialDefinitions(options.Definitions);

        builder.Services.AddSingleton<RecurringChannel>();
        builder.Services.AddSingleton<RecurringJobManager>(manager);
        builder.Services.AddSingleton<IRecurringJobManager>(manager);
        builder.Services.AddSingleton<RecurringJobScheduler>();
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<RecurringJobScheduler>());

        builder.Services.AddSingleton<RecurringJobRecoveryService>();
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<RecurringJobRecoveryService>());

        for (var i = 0; i < options.WorkerCount; i++)
        {
            builder.Services.AddSingleton<IHostedService>(sp =>
            {
                var channel = sp.GetRequiredService<RecurringChannel>().Instance;
                var store = sp.GetRequiredService<IJobStore>();
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                return new JobWorker(channel, store, scopeFactory);
            });
        }

        return builder;
    }
}
