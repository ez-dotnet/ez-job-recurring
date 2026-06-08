using System.Linq.Expressions;

namespace EZJob.Recurring;

public interface IRecurringJobManager
{
    ValueTask<Guid> ScheduleAsync<T>(Expression<Action<T>> methodCall, string cronExpression, CancellationToken cancellationToken = default);
    ValueTask<Guid> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, string cronExpression, CancellationToken cancellationToken = default);
    ValueTask RemoveAsync<T>(Expression<Action<T>> methodCall);
    ValueTask RemoveAsync<T>(Expression<Func<T, Task>> methodCall);
    ValueTask PauseAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}
