using System;
using System.Linq;

namespace IteratorTasks
{
    public static partial class TaskEx
    {

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルする。
        /// (内部で作った <see cref="CancellationToken"/> を引数に渡してタスクを起動。)
        /// </summary>
        public static Task<T> First<T>(params AsyncFunc<T>[] tasks)
        {
            var cts = new CancellationTokenSource();
            return First<T>(cts, tasks);
        }

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルする。
        /// (引数で渡した1つの <see cref="CancellationTokenSource.Token"/> を引数に渡してタスクを起動。)
        /// </summary>
        public static Task<T> First<T>(CancellationTokenSource cts, params AsyncFunc<T>[] tasks)
        {
            var t = tasks.Select(x => x(cts.Token)).ToArray();
            return Task.WhenAny<T>(t, cts).OnSuccessWithTask(x =>
            {
                cts.Cancel();
                if (x.Exception == null)
                    return Task.FromResult(x.Result);
                return Task.FromException<T>(x.Exception);
            });
        }

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルする。
        /// (内部で作った <see cref="CancellationToken"/> を引数に渡してタスクを起動。)
        /// </summary>
        public static Task First(params AsyncAction[] tasks)
        {
            var cts = new CancellationTokenSource();
            return First(cts, tasks);
        }

        /// <summary>
        /// 複数のタスクのうち、どれか1つが終わるのを待つ。
        /// その1つのタスクが終わったら、残りのタスクはキャンセルする。
        /// (引数で渡した1つの <see cref="CancellationTokenSource.Token"/> を引数に渡してタスクを起動。)
        /// </summary>
        public static Task First(CancellationTokenSource cts, params AsyncAction[] tasks)
        {
            var t = tasks.Select(x => x(cts.Token)).ToArray();
            return Task.WhenAny(t, cts).OnSuccessWithTask(x =>
            {
                cts.Cancel();
                if (x.Exception == null)
                    return Task.CompletedTask;
                return Task.FromException(x.Exception);
            });
        }
    }
}
