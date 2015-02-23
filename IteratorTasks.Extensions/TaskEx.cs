using System;
using System.Linq;

namespace IteratorTasks
{
    public static partial class TaskEx
    {
        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルされる想定。
        /// (このオーバーロードでは、キャンセルは呼び出し元の債務。)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task<T> First<T>(params Task<T>[] tasks) { return First(null, tasks); }

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルする。
        /// (内部で作った <see cref="CancellationToken"/> を引数に渡してタスクを起動。)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task<T> First<T>(params AsyncFunc<T>[] tasks)
        {
            var cts = new CancellationTokenSource();
            var created = tasks.Select(x => x(cts.Token)).ToArray();
            return First(cts, created);
        }

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルする。
        /// (引数で渡した1つの <see cref="CancellationTokenSource.Token"/> を引数に渡してタスクを起動。)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cts"></param>
        /// <param name="tasks">最初の1つを待ちたいタスク一覧。</param>
        /// <returns>最初の1つだけ終わったら完了になるタスク。</returns>
        public static Task<T> First<T>(CancellationTokenSource cts, params Task<T>[] tasks)
        {
            return First(null, cts, tasks);
        }

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルする。
        /// (引数で渡した1つの <see cref="CancellationToken"/> を引数に渡してタスクを起動。)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ct"></param>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task<T> First<T>(CancellationToken ct, params Task<T>[] tasks)
        {
            var cts = ct.ToCancellationTokenSourceOneWay();
            return First(cts, tasks);
        }

        /// <summary>
        /// <see cref="First(CancellationTokenSource, Task[])"/>
        /// <see cref="TaskScheduler"/> 明示版。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scheduler"></param>
        /// <param name="cts"></param>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task<T> First<T>(TaskScheduler scheduler, CancellationTokenSource cts, params Task<T>[] tasks)
        {
            if (tasks.Length == 0)
                throw new ArgumentException("tasks must contain at least one task", "tasks");

            var tcs = new TaskCompletionSource<T>(scheduler);
            // Task.Firstに終わったタスクを渡すとtcs = null が先に呼ばれてnullぽ出るのでここで受けておく
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
                        tcs.Propagate(t);
                        tcs = null;
                        if (cts != null)
                            cts.Cancel();
                    }
                });
            }

            return task0;
        }

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルされる想定。
        /// (このオーバーロードでは、キャンセルは呼び出し元の債務。)
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task First(params Task[] tasks) { return First(null, tasks); }

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルする。
        /// (内部で作った <see cref="CancellationToken"/> を引数に渡してタスクを起動。)
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task First(params AsyncAction[] tasks)
        {
            var cts = new CancellationTokenSource();
            var created = tasks.Select(x => x(cts.Token)).ToArray();
            return First(cts, created);
        }

        /// <summary>
        /// <see cref="First(AsyncAction[])"/>
        /// <see cref="TaskScheduler"/> 明示版。
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task First(TaskScheduler scheduler, params AsyncAction[] tasks)
        {
            var cts = new CancellationTokenSource();
            var created = tasks.Select(x => x(cts.Token)).ToArray();
            return First(scheduler, cts, created);
        }

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルする。
        /// (引数で渡した1つの <see cref="CancellationTokenSource.Token"/> を引数に渡してタスクを起動。)
        /// </summary>
        /// <param name="cts"></param>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task First(CancellationTokenSource cts, params Task[] tasks)
        {
            return First(null, cts, tasks);
        }

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルする。
        /// (内部で作った <see cref="CancellationToken"/> を引数に渡してタスクを起動。)
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task First(CancellationToken ct, params Task[] tasks)
        {
            var cts = ct.ToCancellationTokenSourceOneWay();
            return First(cts, tasks);
        }

        /// <summary>
        /// <see cref="First(CancellationTokenSource, Task[])"/>
        /// <see cref="TaskScheduler"/> 明示版。
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="cts"></param>
        /// <param name="tasks">最初の1つを待ちたいタスク一覧。</param>
        /// <returns>最初の1つだけ終わったら完了になるタスク。</returns>
        // ※保留 <param name="onComplete">最初の1つのタスクが終了時に呼ばれる。Task.First().ContinueWith(onComplete) すると呼ばれるフレームが1フレーム遅れるけども、これならたぶん即呼ばれる。</param>
        public static Task First(TaskScheduler scheduler, CancellationTokenSource cts, params Task[] tasks)
        {
            if (tasks.Length == 0)
                throw new ArgumentException("tasks must contain at least one task", "tasks");

            var tcs = new TaskCompletionSource<object>(scheduler);
            // Task.Firstに終わったタスクを渡すとtcs = null が先に呼ばれてnullぽ出るのでここで受けておく
            var task0 = tcs.Task;

            if (cts != null)
                cts.Token.Register(() =>
                {
                    //if (onComplete != null)
                    //    onComplete();

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
                        tcs.Propagate(t);
                        tcs = null;
                        if (cts != null)
                            cts.Cancel();
                    }
                });
            }

            return task0;
        }

        internal static void Propagate<T>(this TaskCompletionSource<T> tcs, Task task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                var tt = task as Task<T>;
                tcs.SetResult(tt == null ? default(T) : tt.Result);
            }
            else if (task.IsFaulted)
            {
                tcs.SetException(task.Exception);
            }
            else
            {
                tcs.SetCanceled();
            }
        }
    }
}
