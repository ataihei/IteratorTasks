namespace IteratorTasks.Runtime.CompilerServices
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using SynchronizationContext = System.Threading.SynchronizationContext;

#if !PORTABLE
    using System.Security.Permissions;
#endif

    /// <summary>Provides an awaiter for awaiting a <see cref="Task"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
#if !PORTABLE
    [HostProtection(Synchronization = true, ExternalThreading = true)]
#endif
    public struct TaskAwaiter : ICriticalNotifyCompletion
    {
        /// <summary>Error message for <see cref="GetResult"/>.</summary>
        private const string InvalidOperationException_TaskNotCompleted = "The task has not yet completed.";

        /// <summary>The task being awaited.</summary>
        private readonly Task _task;

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

        /// <summary>Whether the current thread is appropriate for inlining the await continuation.</summary>
        private static bool IsValidLocationForInlining
        {
            get
            {
                SynchronizationContext current = SynchronizationContext.Current;
                return (current == null || current.GetType() == typeof(SynchronizationContext));
            }
        }

        /// <summary>Initializes the <see cref="TaskAwaiter"/>.</summary>
        /// <param name="task">The <see cref="Task"/> to be awaited.</param>
        internal TaskAwaiter(Task task)
        {
            Debug.Assert(task != null, null);
            _task = task;
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public void OnCompleted(Action continuation)
        {
            OnCompletedInternal(_task, continuation, continueOnCapturedContext: true);
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompletedInternal(_task, continuation, continueOnCapturedContext: true);
        }

        /// <summary>Ends the await on the completed <see cref="Task"/>.</summary>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <exception cref="InvalidOperationException">The task was not yet completed.</exception>
        /// <exception cref="TaskCanceledException">The task was canceled.</exception>
        /// <exception cref="Exception">The task completed in a Faulted state.</exception>
        public void GetResult()
        {
            ValidateEnd(_task);
        }

        /// <summary>
        /// Fast checks for the end of an await operation to determine whether more needs to be done prior to completing
        /// the await.
        /// </summary>
        /// <param name="task">The awaited task.</param>
        internal static void ValidateEnd(Task task)
        {
            if (task.Status != TaskStatus.RanToCompletion)
            {
                HandleNonSuccess(task);
            }
        }

        /// <summary>Handles validations on tasks that aren't successfully completed.</summary>
        /// <param name="task">The awaited task.</param>
        private static void HandleNonSuccess(Task task)
        {
            if (!task.IsCompleted)
            {
                try
                {
                    //task.Wait();
                }
                catch
                {
                }
            }

            if (task.Status != TaskStatus.RanToCompletion)
            {
                ThrowForNonSuccess(task);
            }
        }

        /// <summary>
        /// Throws an exception to handle a task that completed in a state other than
        /// <see cref="TaskStatus.RanToCompletion"/>.
        /// </summary>
        private static void ThrowForNonSuccess(Task task)
        {
            Debug.Assert(task.Status != TaskStatus.RanToCompletion, null);
            switch (task.Status)
            {
                case TaskStatus.Canceled:
                    throw new TaskCanceledException(task);
                case TaskStatus.Faulted:
                    throw PrepareExceptionForRethrow(task.Exception.InnerException);
                default:
                    throw new InvalidOperationException(InvalidOperationException_TaskNotCompleted);
            }
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
        /// </summary>
        /// <param name="task">The awaited task.</param>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <param name="continueOnCapturedContext">Whether to capture and marshal back to the current context.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        internal static void OnCompletedInternal(Task task, Action continuation, bool continueOnCapturedContext)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }

            SynchronizationContext sc = continueOnCapturedContext ? SynchronizationContext.Current : null;
            if (sc != null && sc.GetType() != typeof(SynchronizationContext))
            {
                task.ContinueWith(param0 =>
                {
                    try
                    {
                        sc.Post(state => ((Action)state).Invoke(), continuation);
                    }
                    catch (Exception exception)
                    {
                        AsyncServices.ThrowAsync(exception, null);
                    }
                });//, CancellationToken.None);, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                return;
            }

            TaskScheduler taskScheduler = Task.DefaultScheduler;
            if (task.IsCompleted)
            {
                //Task.Factory.StartNew(s => ((Action)s).Invoke(), continuation, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
                continuation();
                return;
            }

            //if (taskScheduler != TaskScheduler.Default)
            //{
            //    task.ContinueWith(_ => RunNoException(continuation), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, taskScheduler);
            //    return;
            //}

            task.ContinueWith(param0 =>
            {
                if (IsValidLocationForInlining)
                {
                    RunNoException(continuation);
                    return;
                }

                //Task.Factory.StartNew(s => RunNoException((Action)s), continuation, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                continuation();
            });//, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Invokes the delegate in a try/catch that will propagate the exception asynchronously on the thread pool.
        /// </summary>
        /// <param name="continuation"></param>
        private static void RunNoException(Action continuation)
        {
            try
            {
                continuation.Invoke();
            }
            catch (Exception exception)
            {
                AsyncServices.ThrowAsync(exception, null);
            }
        }

        /// <summary>Copies the exception's stack trace so its stack trace isn't overwritten.</summary>
        /// <param name="exc">The exception to prepare.</param>
        internal static Exception PrepareExceptionForRethrow(Exception exc)
        {
            return exc;
        }
    }
}
