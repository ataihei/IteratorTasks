using System;

namespace IteratorTasks
{
    public partial class Task
    {
        public static Task<Task<T>> WhenAny<T>(params Task<T>[] tasks) { return WhenAny(tasks, null); }

        /// <summary>
        /// 複数のタスクのうち、最初に終わったものの値を返す。
        /// 残りのタスクは内部でキャンセルする。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks">最初の1つを待ちたいタスク一覧。</param>
        /// <param name="cts"></param>
        /// <returns>最初の1つだけ終わったら完了になるタスク。</returns>
        public static Task<Task<T>> WhenAny<T>(Task<T>[] tasks, CancellationTokenSource cts)
        {
            return WhenAny(tasks, null, cts);
        }

        public static Task<Task<T>> WhenAny<T>(Task<T>[] tasks, TaskScheduler scheduler, CancellationTokenSource cts)
        {
            if (tasks.Length == 0)
                throw new ArgumentException("tasks must contain at least one task", "tasks");

            var tcs = new TaskCompletionSource<Task<T>>(scheduler);
            // Task.WhenAnyに終わったタスクを渡すとtcs = null が先に呼ばれてnullぽ出るのでここで受けておく
            var task0 = tcs.Task;

            if (cts != null)
                cts.Token.Register(() =>
                {
                    if (tcs != null)
                    {
                        tcs.SetCanceled();
                        tcs = null;
                    }
                });

            foreach (var task in tasks)
            {
                if (task == null)
                    throw new ArgumentException("task must not null", "tasks");

                task.ContinueWith(t =>
                {
                    if (tcs != null)
                    {
                        tcs.SetResult(t);
                        tcs = null;
                        if (cts != null)
                            cts.Cancel();
                    }
                });
            }

            return task0;
        }

        public static Task<Task> WhenAny(params Task[] tasks) { return WhenAny(null, tasks, null); }

        public static Task<Task> WhenAny(Task[] tasks, CancellationTokenSource cts)
        {
            return WhenAny(null, tasks, cts);
        }

        public static Task<Task> WhenAny(Action onComplete, Task[] tasks, CancellationTokenSource cts)
        {
            return WhenAny(onComplete, tasks, null, cts);
        }

        /// <summary>
        /// 複数のタスクのうち、最初に終わったものの値を返す。
        /// 残りのタスクは内部でキャンセルする。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onComplete">最初の1つのタスクが終了時に呼ばれる。Task.WhenAny().ContinueWith(onComplete) すると呼ばれるフレームが1フレーム遅れるけども、これならたぶん即呼ばれる。</param>
        /// <param name="tasks">最初の1つを待ちたいタスク一覧。</param>
        /// <param name="cts"></param>
        /// <returns>最初の1つだけ終わったら完了になるタスク。</returns>
        public static Task<Task> WhenAny(Action onComplete, Task[] tasks, TaskScheduler scheduler, CancellationTokenSource cts)
        {
            if (tasks.Length == 0)
                throw new ArgumentException("tasks must contain at least one task", "tasks");

            var tcs = new TaskCompletionSource<Task>(scheduler);
            // Task.WhenAnyに終わったタスクを渡すとtcs = null が先に呼ばれてnullぽ出るのでここで受けておく
            var task0 = tcs.Task;

            if (cts != null)
                cts.Token.Register(() =>
                {
                    if (onComplete != null)
                        onComplete();

                    if (tcs != null)
                    {
                        tcs.SetCanceled();
                        tcs = null;
                    }
                });

            foreach (var task in tasks)
            {
                if (task == null)
                    throw new ArgumentException("task must not null", "tasks");

                task.ContinueWith(t =>
                {
                    if (tcs != null)
                    {
                        tcs.SetResult(t);
                        tcs = null;
                        if (cts != null)
                            cts.Cancel();
                    }
                });
            }

            return task0;
        }
    }
}
