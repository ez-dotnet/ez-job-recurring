using System.Threading.Channels;
using Cronos;
using EZ.Job.Core;
using Microsoft.Extensions.Hosting;

namespace EZJob.Recurring;

internal sealed class RecurringJobScheduler : BackgroundService
{
    private readonly IRecurringStore _recurringStore;
    private readonly IJobStore _store;
    private readonly Channel<Job> _channel;
    private readonly int _pollIntervalSeconds;

    public RecurringJobScheduler(
        IRecurringStore recurringStore,
        IJobStore store,
        RecurringChannel channel,
        EZJobOptions options)
    {
        _recurringStore = recurringStore;
        _store = store;
        _channel = channel.Instance;
        _pollIntervalSeconds = options.RecurringPollIntervalSeconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_pollIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            var now = DateTime.UtcNow;
            var definitions = await _recurringStore.GetAllAsync(stoppingToken).ConfigureAwait(false);

            foreach (var def in definitions.Where(d => d.IsActive))
            {
                var cron = Cronos.CronExpression.Parse(def.CronExpression);
                var lastCheck = def.LastExecutionUtc ?? def.CreatedAtUtc;
                var next = cron.GetNextOccurrence(lastCheck);

                while (next.HasValue && next.Value <= now)
                {
                    var job = new Job(
                        Guid.NewGuid().ToString("N"),
                        def.TypeName,
                        def.MethodName,
                        def.ArgumentTypes,
                        def.Arguments,
                        JobStatus.Enqueued,
                        now,
                        Error: null,
                        StartedAt: null,
                        CompletedAt: null,
                        RecurringJobId: def.Id.ToString());

                    await _store.AddAsync(job, stoppingToken).ConfigureAwait(false);
                    _channel.Writer.TryWrite(job);

                    await _recurringStore.AddOrUpdateAsync(def with { LastExecutionUtc = next.Value }, stoppingToken).ConfigureAwait(false);
                    next = cron.GetNextOccurrence(next.Value);
                }
            }
        }
    }
}
