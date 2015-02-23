using System;

namespace IteratorTasks
{
    /// <summary>
    /// 任意のタイミングで完了するタスクを生成するクラス。
    /// 主に View 側のイベント待ちなどに用いる。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaskCompletionSource<T>
    {
        private Task<T> _task = new Task<T> { Status = TaskStatus.Running };

        /// <summary>
        /// 任意のタイミングで完了するタスク。
        /// </summary>
        public Task<T> Task { get { return _task; } }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TaskCompletionSource() : this(null) { }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="scheduler"></param>
        public TaskCompletionSource(TaskScheduler scheduler)
        {
            if (scheduler == null) scheduler = IteratorTasks.Task.DefaultScheduler;
            scheduler.QueueTask(this);
            Task._scheduler = scheduler;
        }

        /// <summary>
        /// キャンセルを試行する。すでにタスクが完了済みであれば何も起きない。
        /// </summary>
        public void TrySetCanceled()
        {
            if (_task.IsCompleted)
                return;
            ((ITaskInternal)_task).Cancel();
        }

        /// <summary>
        /// 例外を出すことを試行する。すでにタスクが完了済みであれば何も起きない。
        /// </summary>
        /// <param name="exception"></param>
        public void TrySetException(Exception exception)
        {
            if (_task.IsCompleted)
                return;
            ((ITaskInternal)_task).SetException(exception);
        }

        /// <summary>
        /// タスクを正常終了させることを試行する。すでにタスクが完了済みであれば何も起きない。
        /// </summary>
        /// <param name="result"></param>
        public void TrySetResult(T result)
        {
            if (_task.IsCompleted)
                return;
            ((ITaskInternal)_task).SetResult(result);
        }

        /// <summary>
        /// キャンセルする。すでにタスクが完了済みであれば InvalidOperationException を出す。
        /// </summary>
        public void SetCanceled()
        {
            if (_task.IsCompleted)
                throw new InvalidOperationException();
            ((ITaskInternal)_task).Cancel();
        }

        /// <summary>
        /// 例外を出す。すでにタスクが完了済みであれば InvalidOperationException を出す。
        /// </summary>
        public void SetException(Exception exception)
        {
            if (_task.IsCompleted)
                throw new InvalidOperationException();
            ((ITaskInternal)_task).SetException(exception);
        }

        /// <summary>
        /// タスクを正常終了させる。すでにタスクが完了済みであれば InvalidOperationException を出す。
        /// </summary>
        public void SetResult(T result)
        {
            if (_task.IsCompleted)
                throw new InvalidOperationException();
            ((ITaskInternal)_task).SetResult(result);
        }

        internal void Propagate(Task task)
        {
            if (_task.IsCompleted)
                return;

            if (task.Status == TaskStatus.RanToCompletion)
            {
                var tt = task as Task<T>;
                SetResult(tt == null ? default(T) : tt.Result);
            }
            else if (task.IsFaulted)
            {
                SetException(task.Exception);
            }
            else
            {
                SetCanceled();
            }
        }

        // 外から SetResult とかされたくないので、internal なインターフェイスを作って、明示的実装。
        internal interface ITaskInternal
        {
            void Cancel();
            void SetException(Exception e);
            void SetResult(T result);
        }
    }

    public partial class Task<T> : TaskCompletionSource<T>.ITaskInternal
    {
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
