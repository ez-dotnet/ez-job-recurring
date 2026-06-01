using System.Threading.Channels;
using EZ.Job.Core;

namespace EZJob.Recurring;

internal sealed class RecurringChannel
{
    public Channel<Job> Instance { get; } = Channel.CreateUnbounded<Job>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false,
    });
}
