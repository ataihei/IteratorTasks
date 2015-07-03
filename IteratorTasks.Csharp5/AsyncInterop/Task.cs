#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.Threading.Tasks.Task))]
[assembly: TypeForwardedTo(typeof(System.Threading.Tasks.Task<>))]

#else

using I = IteratorTasks;

namespace System.Threading.Tasks
{
    public class Task
    {
        protected readonly I.Task _t;

        internal Task(I.Task t) { _t = t; }

        public I.Task AsIterator() => _t;

        public static implicit operator I.Task(Task t) => t._t;

        public void Wait()
        {
            while (!_t.IsCompleted)
                Thread.Sleep(10);
        }

        public I.Runtime.CompilerServices.TaskAwaiter GetAwaiter() => _t.GetAwaiter();
    }

    public class Task<T> : Task
    {
        internal Task(I.Task<T> t) : base(t) { }

        public new I.Task<T> AsIterator() => (I.Task<T>)_t;

        public T Result => AsIterator().Result;

        public static implicit operator I.Task<T>(Task<T> t) => t.AsIterator();

        public new I.Runtime.CompilerServices.TaskAwaiter<T> GetAwaiter() => AsIterator().GetAwaiter();
    }

    internal class TaskCompletionSource<TResult>
    {
        I.TaskCompletionSource<TResult> _tcs = new I.TaskCompletionSource<TResult>();
        public Task<TResult> Task => new Task<TResult>(_tcs.Task);

        public bool TrySetResult(TResult result)
        {
            _tcs.TrySetResult(result);
            return true;
        }

        public bool TrySetCanceled()
        {
            _tcs.TrySetCanceled();
            return true;
        }

        public bool TrySetException(Exception exception)
        {
            _tcs.TrySetException(exception);
            return true;
        }
    }
}

#endif
