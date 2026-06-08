using System.Linq.Expressions;
using System.Reflection;
using EZ.Job.Core;

namespace EZJob.Recurring;

internal sealed class RecurringJobManager : IRecurringJobManager
{
    private readonly IRecurringStore _store;

    public RecurringJobManager(IRecurringStore store)
    {
        _store = store;
    }

    public async ValueTask AddInitialDefinitionsAsync(IEnumerable<RecurringJobDefinition> definitions, CancellationToken ct = default)
    {
        foreach (var def in definitions)
        {
            await _store.AddOrUpdateAsync(def.ToCoreDefinition(), ct).ConfigureAwait(false);
        }
    }

    public async ValueTask<Guid> ScheduleAsync<T>(Expression<Action<T>> methodCall, string cronExpression, CancellationToken cancellationToken = default)
    {
        var (method, args) = ExpressionHelpers.Extract(methodCall);
        var def = new RecurringJobDefinition(typeof(T), method, args, cronExpression);
        await _store.AddOrUpdateAsync(def.ToCoreDefinition(), cancellationToken).ConfigureAwait(false);
        return def.Id;
    }

    public async ValueTask<Guid> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, string cronExpression, CancellationToken cancellationToken = default)
    {
        var (method, args) = ExpressionHelpers.Extract(methodCall);
        var def = new RecurringJobDefinition(typeof(T), method, args, cronExpression);
        await _store.AddOrUpdateAsync(def.ToCoreDefinition(), cancellationToken).ConfigureAwait(false);
        return def.Id;
    }

    public async ValueTask RemoveAsync<T>(Expression<Action<T>> methodCall)
    {
        var (method, _) = ExpressionHelpers.Extract(methodCall);
        await RemoveByExpressionAsync(typeof(T), method).ConfigureAwait(false);
    }

    public async ValueTask RemoveAsync<T>(Expression<Func<T, Task>> methodCall)
    {
        var (method, _) = ExpressionHelpers.Extract(methodCall);
        await RemoveByExpressionAsync(typeof(T), method).ConfigureAwait(false);
    }

    public async ValueTask RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _store.RemoveAsync(id, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask PauseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _store.SetActiveAsync(id, false, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _store.SetActiveAsync(id, true, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask RemoveByExpressionAsync(Type type, MethodInfo method, CancellationToken ct = default)
    {
        var all = await _store.GetAllAsync(ct).ConfigureAwait(false);
        var matching = all.FirstOrDefault(d =>
            d.TypeName == type.FullName &&
            d.MethodName == method.Name &&
            d.ArgumentTypes.SequenceEqual(method.GetParameters().Select(p => p.ParameterType.FullName!)));

        if (matching is not null)
        {
            await _store.RemoveAsync(matching.Id, ct).ConfigureAwait(false);
        }
    }
}
