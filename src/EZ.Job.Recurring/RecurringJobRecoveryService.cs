using Cronos;
using EZ.Job.Core;
using Microsoft.Extensions.Hosting;

namespace EZJob.Recurring;

internal sealed class RecurringJobRecoveryService : BackgroundService
{
    private readonly IJobStore _store;
    private readonly RecoveryChannel _channel;
    private readonly RecurringJobManager _manager;

    public RecurringJobRecoveryService(IJobStore store, RecoveryChannel channel, RecurringJobManager manager)
    {
        _store = store;
        _channel = channel;
        _manager = manager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pending = await _store.GetPendingAsync(stoppingToken).ConfigureAwait(false);
        var definitions = _manager.GetAll().ToDictionary(d => d.Fingerprint);
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
