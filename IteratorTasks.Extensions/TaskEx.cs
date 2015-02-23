using System;
using System.Linq;

namespace IteratorTasks
{
    public static partial class TaskEx
    {
        /// <summary>
        /// タスクの結果をとれる版のWhenAny。
        /// </summary>
        /// <remarks>
        /// 旧Task.First
        /// </remarks>
        public static Task<T> WhenAny<T>(params Task<T>[] tasks)
        {
            return Task.WhenAny<T>(tasks).OnSuccessWithTask(x =>
            {
                if (x.Exception == null)
                    return Task.FromResult(x.Result);
                return Task.FromException<T>(x.Exception);
            });
        }

        /// <summary>
        /// タスクの結果をとれる版のWhenAny。
        /// </summary>
        /// <remarks>
        /// 旧Task.First
        /// </remarks>
        public static Task<T> WhenAny<T>(params AsyncFunc<T>[] tasks)
        {
            var cts = new CancellationTokenSource();
            return WhenAny<T>(cts, tasks);
        }

        /// <summary>
        /// タスクの結果をとれる版のWhenAny。
        /// </summary>
        /// <remarks>
        /// 旧Task.First
        /// </remarks>
        public static Task<T> WhenAny<T>(CancellationTokenSource cts, params AsyncFunc<T>[] tasks)
        {
            var t = tasks.Select(x => x(cts.Token)).ToArray();
            return Task.WhenAny<T>(t, cts).OnSuccessWithTask(x =>
            {
                if (x.Exception == null)
                    return Task.FromResult(x.Result);
                return Task.FromException<T>(x.Exception);
            });
        }

        /// <summary>
        /// タスクの結果をとれる版のWhenAny。
        /// </summary>
        /// <remarks>
        /// 旧Task.First
        /// </remarks>
        public static Task WhenAny(params Task[] tasks)
        {
            return Task.WhenAny(tasks).OnSuccessWithTask(x =>
            {
                if (x.Exception == null)
                    return Task.CompletedTask;
                return Task.FromException(x.Exception);
            });
        }

        /// <summary>
        /// タスクの結果をとれる版のWhenAny。
        /// </summary>
        /// <remarks>
        /// 旧Task.First
        /// </remarks>
        public static Task WhenAny(params AsyncAction[] tasks)
        {
            var cts = new CancellationTokenSource();
            return WhenAny(cts, tasks);
        }

        /// <summary>
        /// タスクの結果をとれる版のWhenAny。
        /// </summary>
        /// <remarks>
        /// 旧Task.First
        /// </remarks>
        public static Task WhenAny(CancellationTokenSource cts, params AsyncAction[] tasks)
        {
            var t = tasks.Select(x => x(cts.Token)).ToArray();
            return Task.WhenAny(t, cts).OnSuccessWithTask(x =>
            {
                if (x.Exception == null)
                    return Task.CompletedTask;
                return Task.FromException(x.Exception);
            });
        }
    }
}
