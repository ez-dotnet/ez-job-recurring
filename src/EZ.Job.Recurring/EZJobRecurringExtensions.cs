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

        builder.Services.AddSingleton<RecurringChannel>();
        builder.Services.AddSingleton(sp =>
        {
            var store = sp.GetRequiredService<IRecurringStore>();
            var manager = new RecurringJobManager(store);
            manager.AddInitialDefinitionsAsync(options.Definitions).GetAwaiter().GetResult();
            return manager;
        });
        builder.Services.AddSingleton<IRecurringJobManager>(sp => sp.GetRequiredService<RecurringJobManager>());
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
