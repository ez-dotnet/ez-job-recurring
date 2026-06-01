using System.Collections.Concurrent;
using System.Linq.Expressions;
using EZ.Job.Core;

namespace EZJob.Recurring;

internal sealed class RecurringJobManager : IRecurringJobManager
{
    private readonly ConcurrentDictionary<string, RecurringJobDefinition> _definitions = new();

    public void AddInitialDefinitions(IEnumerable<RecurringJobDefinition> definitions)
    {
        foreach (var def in definitions)
        {
            _definitions.TryAdd(def.Fingerprint, def);
        }
    }

    public ValueTask ScheduleAsync<T>(Expression<Action<T>> methodCall, string cronExpression, CancellationToken cancellationToken = default)
    {
        var (method, args) = ExpressionHelpers.Extract(methodCall);
        var def = new RecurringJobDefinition(typeof(T), method, args, cronExpression);
        _definitions[def.Fingerprint] = def;
        return ValueTask.CompletedTask;
    }

    public ValueTask ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, string cronExpression, CancellationToken cancellationToken = default)
    {
        var (method, args) = ExpressionHelpers.Extract(methodCall);
        var def = new RecurringJobDefinition(typeof(T), method, args, cronExpression);
        _definitions[def.Fingerprint] = def;
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync<T>(Expression<Action<T>> methodCall)
    {
        var (method, _) = ExpressionHelpers.Extract(methodCall);
        var fingerprint = $"{typeof(T).FullName}:{method.Name}:{method.MetadataToken}";
        _definitions.TryRemove(fingerprint, out _);
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync<T>(Expression<Func<T, Task>> methodCall)
    {
        var (method, _) = ExpressionHelpers.Extract(methodCall);
        var fingerprint = $"{typeof(T).FullName}:{method.Name}:{method.MetadataToken}";
        _definitions.TryRemove(fingerprint, out _);
        return ValueTask.CompletedTask;
    }

    public IEnumerable<RecurringJobDefinition> GetAll() => _definitions.Values;
}
