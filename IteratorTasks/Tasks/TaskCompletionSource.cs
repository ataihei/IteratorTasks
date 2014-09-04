using System;

namespace IteratorTasks
{
    public class TaskCompletionSource<T>
    {
        private Task<T> _task = new Task<T> { Status = TaskStatus.Running };

        public Task<T> Task { get { return _task; } }

        public TaskCompletionSource() : this(null) { }
        public TaskCompletionSource(TaskScheduler scheduler)
        {
            if (scheduler == null) scheduler = IteratorTasks.Task.DefaultScheduler;
            scheduler.QueueTask(this);
            Task._scheduler = scheduler;
        }

        public void SetForceCanceled()
        {
            ((ITaskInternal)_task).ForceCancel();
        }

        public void SetCanceled()
        {
            if (_task.IsDone)
                return;
            ((ITaskInternal)_task).Cancel();
        }

        public void SetException(Exception exception)
        {
            if (_task.IsDone)
                return;
            ((ITaskInternal)_task).SetException(exception);
        }

        public void SetResult(T result)
        {
            if (_task.IsDone)
                return;
            ((ITaskInternal)_task).SetResult(result);
        }

        internal void Propagate(Task task)
        {
            if (_task.IsDone)
                return;

            if (task.IsCompleted)
            {
                var tt = task as Task<T>;
                SetResult(tt == null ? default(T) : tt.Result);
            }
            else if (task.IsFaulted)
            {
                SetException(task.Error);
            }
            else
            {
                SetCanceled();
            }
        }

        // 外から SetResult とかされたくないので、internal なインターフェイスを作って、明示的実装。
        internal interface ITaskInternal
        {
            void ForceCancel();
            void Cancel();
            void SetException(Exception e);
            void SetResult(T result);
        }
    }

    public partial class Task<T> : TaskCompletionSource<T>.ITaskInternal
    {
        void TaskCompletionSource<T>.ITaskInternal.ForceCancel()
        {
            ForceCancel();
            Complete();
        }

        void TaskCompletionSource<T>.ITaskInternal.Cancel()
        {
            if (Status == TaskStatus.Running || Status == TaskStatus.Created)
            {
                AddError(new TaskCanceledException());
                Complete();
            }
        }

        void TaskCompletionSource<T>.ITaskInternal.SetException(Exception e)
        {
            if (Status == TaskStatus.Running || Status == TaskStatus.Created)
            {
                AddError(e);
                Complete();
            }
        }

        void TaskCompletionSource<T>.ITaskInternal.SetResult(T result)
        {
            if (Status == TaskStatus.Running || Status == TaskStatus.Created)
            {
                _result = result;
                Complete();
            }
        }
    }
}
