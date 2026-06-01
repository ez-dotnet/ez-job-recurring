using System.Linq.Expressions;
using System.Reflection;
using EZ.Job.Core;

namespace EZJob.Recurring;

public class RecurringJobOptions
{
    internal List<RecurringJobDefinition> Definitions { get; } = [];
    public int PollIntervalSeconds { get; set; } = 30;
    public int WorkerCount { get; set; } = 1;

    public void Register<T>(Expression<Action<T>> methodCall, string cronExpression)
    {
        var (method, args) = ExpressionHelpers.Extract(methodCall);
        Definitions.Add(new RecurringJobDefinition(typeof(T), method, args, cronExpression));
    }

    public void Register<T>(Expression<Func<T, Task>> methodCall, string cronExpression)
    {
        var (method, args) = ExpressionHelpers.Extract(methodCall);
        Definitions.Add(new RecurringJobDefinition(typeof(T), method, args, cronExpression));
    }
}
