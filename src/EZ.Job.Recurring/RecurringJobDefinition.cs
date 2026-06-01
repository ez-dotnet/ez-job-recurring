using System.Reflection;

namespace EZJob.Recurring;

public sealed class RecurringJobDefinition
{
    public string Fingerprint { get; }
    public string TypeName { get; }
    public string MethodName { get; }
    public string[] ArgumentTypes { get; }
    public object?[] Arguments { get; }
    public string CronExpression { get; }
    internal DateTime LastCheckUtc { get; set; }

    internal RecurringJobDefinition(Type type, MethodInfo method, object?[] args, string cronExpression)
    {
        Fingerprint = $"{type.FullName}:{method.Name}:{method.MetadataToken}";
        TypeName = type.FullName!;
        MethodName = method.Name;
        ArgumentTypes = method.GetParameters().Select(p => p.ParameterType.FullName!).ToArray();
        Arguments = args;
        CronExpression = cronExpression;
        LastCheckUtc = DateTime.UtcNow;
    }
}
