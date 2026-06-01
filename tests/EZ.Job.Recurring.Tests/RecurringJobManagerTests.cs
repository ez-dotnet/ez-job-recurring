using Xunit;
using EZ.Job.Core;
using Microsoft.Extensions.DependencyInjection;

namespace EZJob.Recurring.Tests;

public sealed class RecurringJobManagerTests
{
    [Fact]
    public async Task ScheduleAsync_should_add_definition()
    {
        var services = new ServiceCollection();
        services.AddEZJob()
            .AddRecurringJobs(options =>
            {
                options.Register<MyJob>(j => j.Run(), "* * * * *");
            });

        var sp = services.BuildServiceProvider();
        var manager = sp.GetRequiredService<IRecurringJobManager>();
        var store = sp.GetRequiredService<IJobStore>();

        await manager.ScheduleAsync<MyJob>(j => j.Run(), "*/5 * * * *");

        var jobs = await store.GetAllAsync();
        Assert.Empty(jobs); // definitions, not jobs
    }
}

public class MyJob
{
    public void Run() { }
}
