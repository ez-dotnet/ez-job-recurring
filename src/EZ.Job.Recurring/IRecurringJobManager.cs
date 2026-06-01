using System.Linq.Expressions;

namespace EZJob.Recurring;

public interface IRecurringJobManager
{
    ValueTask ScheduleAsync<T>(Expression<Action<T>> methodCall, string cronExpression, CancellationToken cancellationToken = default);
    ValueTask ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, string cronExpression, CancellationToken cancellationToken = default);
    ValueTask RemoveAsync<T>(Expression<Action<T>> methodCall);
    ValueTask RemoveAsync<T>(Expression<Func<T, Task>> methodCall);
}
