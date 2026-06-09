using System.Reflection;
using EZ.Job.Core;

namespace EZJob.Recurring;

public sealed class RecurringJobDefinition
{
    public Guid Id { get; }
    public string TypeName { get; }
    public string MethodName { get; }
    public string[] ArgumentTypes { get; }
    public object?[] Arguments { get; }
    public string CronExpression { get; }
    internal DateTime LastCheckUtc { get; set; }

    internal RecurringJobDefinition(Type type, MethodInfo method, object?[] args, string cronExpression)
    {
        Id = Guid.NewGuid();
        TypeName = type.FullName!;
        MethodName = method.Name;
        ArgumentTypes = method.GetParameters().Select(p => p.ParameterType.FullName!).ToArray();
        Arguments = args;
        CronExpression = cronExpression;
        LastCheckUtc = DateTime.UtcNow;
    }

    internal RecurringDefinition ToCoreDefinition() => new(
        Id, TypeName, MethodName, ArgumentTypes, Arguments,
        CronExpression, IsActive: true, DateTime.UtcNow, LastExecutionUtc: null);
}
