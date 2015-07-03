namespace IteratorTasks.Runtime.CompilerServices
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>Provides an awaitable context for switching into a target environment.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct YieldAwaitable
    {
        /// <summary>Gets an awaiter for this <see cref="YieldAwaitable"/>.</summary>
        /// <returns>An awaiter for this awaitable.</returns>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public YieldAwaiter GetAwaiter()
        {
            return default(YieldAwaiter);
        }

        /// <summary>Provides an awaiter that switches into a target environment.</summary>
        /// <remarks>This type is intended for compiler use only.</remarks>
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct YieldAwaiter : ICriticalNotifyCompletion, INotifyCompletion
        {
            /// <summary>A completed task.</summary>
            /// <remarks>
            /// This does not use <see cref="CompletedTask.Default"/> to ensure this awaiter behaves properly even if
            /// the default completed task is disposed.
            /// </remarks>
            private static readonly Task _completed = Task.CompletedTask;

            /// <summary>Gets whether a yield is not required.</summary>
            /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
            public bool IsCompleted
            {
                get
                {
                    return false;
                }
            }

            /// <summary>Posts the <paramref name="continuation"/> back to the current context.</summary>
            /// <param name="continuation">The action to invoke asynchronously.</param>
            /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
            public void OnCompleted(Action continuation)
            {
                _completed.GetAwaiter().OnCompleted(continuation);
            }

            /// <summary>Posts the <paramref name="continuation"/> back to the current context.</summary>
            /// <param name="continuation">The action to invoke asynchronously.</param>
            /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
            public void UnsafeOnCompleted(Action continuation)
            {
                _completed.GetAwaiter().UnsafeOnCompleted(continuation);
            }

            /// <summary>Ends the await operation.</summary>
            public void GetResult()
            {
            }
        }
    }
}
