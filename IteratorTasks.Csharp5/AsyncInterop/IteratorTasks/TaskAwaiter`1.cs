namespace IteratorTasks.Runtime.CompilerServices
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>Provides an awaiter for awaiting a <see cref="Task{TResult}"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion, INotifyCompletion
    {
        /// <summary>The task being awaited.</summary>
        private readonly Task<TResult> _task;

        /// <summary>Gets whether the task being awaited is completed.</summary>
        /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        public bool IsCompleted
        {
            get
            {
                return _task.IsCompleted;
            }
        }

        /// <summary>Initializes the <see cref="TaskAwaiter{TResult}"/>.</summary>
        /// <param name="task">The <see cref="Task{TResult}"/> to be awaited.</param>
        internal TaskAwaiter(Task<TResult> task)
        {
            Debug.Assert(task != null, null);
            _task = task;
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="Task{TResult}"/> associated with this
        /// <see cref="TaskAwaiter{TResult}"/>.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public void OnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(_task, continuation, true);
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="Task{TResult}"/> associated with this
        /// <see cref="TaskAwaiter{TResult}"/>.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public void UnsafeOnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(_task, continuation, true);
        }

        /// <summary>Ends the await on the completed <see cref="Task{TResult}"/>.</summary>
        /// <returns>The result of the completed <see cref="Task{TResult}"/>.</returns>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <exception cref="InvalidOperationException">The task was not yet completed.</exception>
        /// <exception cref="TaskCanceledException">The task was canceled.</exception>
        /// <exception cref="Exception">The task completed in a <see cref="TaskStatus.Faulted"/> state.</exception>
        public TResult GetResult()
        {
            TaskAwaiter.ValidateEnd(_task);
            return _task.Result;
        }
    }
}
