#if NET40PLUS && !NET45PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(AwaitExtensions))]

#else

using System;
using IteratorTasks;
using IteratorTasks.Runtime.CompilerServices;

/// <summary>
/// Provides extension methods for threading-related types.
/// </summary>
public static class AwaitExtensions
{
    /// <summary>Gets an awaiter used to await this <see cref="Task"/>.</summary>
    /// <param name="task">The task to await.</param>
    /// <returns>An awaiter instance.</returns>
    public static TaskAwaiter GetAwaiter(this Task task)
    {
        if (task == null)
            throw new ArgumentNullException("task");

        return new TaskAwaiter(task);
    }

    /// <summary>Gets an awaiter used to await this <see cref="Task"/>.</summary>
    /// <typeparam name="TResult">Specifies the type of data returned by the task.</typeparam>
    /// <param name="task">The task to await.</param>
    /// <returns>An awaiter instance.</returns>
    public static TaskAwaiter<TResult> GetAwaiter<TResult>(this Task<TResult> task)
    {
        if (task == null)
            throw new ArgumentNullException("task");

        return new TaskAwaiter<TResult>(task);
    }
}

#endif
