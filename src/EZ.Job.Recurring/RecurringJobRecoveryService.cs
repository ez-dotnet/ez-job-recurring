using Cronos;
using EZ.Job.Core;
using Microsoft.Extensions.Hosting;

namespace EZJob.Recurring;

internal sealed class RecurringJobRecoveryService : BackgroundService
{
    private readonly IJobStore _store;
    private readonly RecoveryChannel _channel;
    private readonly IRecurringStore _recurringStore;

    public RecurringJobRecoveryService(IJobStore store, RecoveryChannel channel, IRecurringStore recurringStore)
    {
        _store = store;
        _channel = channel;
        _recurringStore = recurringStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pending = await _store.GetPendingAsync(stoppingToken).ConfigureAwait(false);
        var definitions = (await _recurringStore.GetAllAsync(stoppingToken).ConfigureAwait(false))
            .Where(d => d.IsActive)
            .ToDictionary(d => d.Id.ToString());
        var now = DateTime.UtcNow;

        foreach (var job in pending)
        {
            if (job.RecurringJobId is null) continue;
            if (!definitions.TryGetValue(job.RecurringJobId, out var def)) continue;

            var cron = Cronos.CronExpression.Parse(def.CronExpression);

            var nextAfterCreated = cron.GetNextOccurrence(job.CreatedAt);
            var nextAfterNow = cron.GetNextOccurrence(now);

            if (nextAfterCreated == nextAfterNow)
            {
                _channel.Instance.Writer.TryWrite(job);
            }
        }
    }
}
