using System.Threading.Channels;
using Cronos;
using EZ.Job.Core;
using Microsoft.Extensions.Hosting;

namespace EZJob.Recurring;

internal sealed class RecurringJobScheduler : BackgroundService
{
    private readonly RecurringJobManager _manager;
    private readonly IJobStore _store;
    private readonly Channel<Job> _channel;
    private readonly int _pollIntervalSeconds;

    public RecurringJobScheduler(
        RecurringJobManager manager,
        IJobStore store,
        RecurringChannel channel,
        EZJobOptions options)
    {
        _manager = manager;
        _store = store;
        _channel = channel.Instance;
        _pollIntervalSeconds = options.RecurringPollIntervalSeconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_pollIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            var definitions = _manager.GetAll();
            var now = DateTime.UtcNow;

            foreach (var def in definitions)
            {
                var cron = Cronos.CronExpression.Parse(def.CronExpression);
                var next = cron.GetNextOccurrence(def.LastCheckUtc);

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
                        RecurringJobId: def.Fingerprint);

                    await _store.AddAsync(job, stoppingToken).ConfigureAwait(false);
                    _channel.Writer.TryWrite(job);

                    def.LastCheckUtc = next.Value;
                    next = cron.GetNextOccurrence(next.Value);
                }
            }
        }
    }
}
