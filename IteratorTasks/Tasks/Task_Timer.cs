using System;
using System.Threading;

namespace IteratorTasks
{
    public partial class Task
    {
        /// <summary>
        /// 遅延後に完了するタスクを作成します。
        /// </summary>
        /// <param name="span">返されたタスクを完了する前に待機する時間。</param>
        /// <returns></returns>
        public static Task Delay(TimeSpan span) => Delay(span, CancellationToken.None);

        /// <summary>
        /// 遅延後に完了するタスクを作成します。
        /// </summary>
        /// <param name="delayMilliseconds">返されたタスクを完了する前までのミリ秒単位の待機時間。</param>
        /// <returns></returns>
        public static Task Delay(int delayMilliseconds) => Delay(delayMilliseconds, CancellationToken.None, null);

        /// <summary>
        /// 遅延後に完了するタスクを作成します。
        /// </summary>
        /// <param name="delayMilliseconds">返されたタスクを完了する前までのミリ秒単位の待機時間。</param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static Task Delay(int delayMilliseconds, TaskScheduler scheduler) => Delay(delayMilliseconds, CancellationToken.None, scheduler);

        /// <summary>
        /// 遅延後に完了するタスクを作成します。
        /// </summary>
        /// <param name="span">返されたタスクを完了する前に待機する時間。</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task Delay(TimeSpan span, CancellationToken ct)
        {
            var ms = span.TotalMilliseconds;
            if (ms < 0)
                return Task.CompletedTask;

            // Delayの最大待ち時間(約４年)より長い場合はキャンセルされるまで待つタスクになる
            if(int.MaxValue < ms)
            {
                var tcs = new TaskCompletionSource<object>();
                ct.Register(() => tcs.SetResult(default(object)));
                return tcs.Task;
            }
            return Delay((int)ms, ct, null);
        }

        /// <summary>
        /// 遅延後に完了するタスクを作成します。
        /// </summary>
        /// <param name="delayMilliseconds">返されたタスクを完了する前までのミリ秒単位の待機時間。</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task Delay(int delayMilliseconds, CancellationToken ct) => Delay(delayMilliseconds, ct, null);

        /// <summary>
        /// 遅延後に完了するタスクを作成します。
        /// </summary>
        /// <param name="delayMilliseconds">返されたタスクを完了する前までのミリ秒単位の待機時間。</param>
        /// <param name="ct"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static Task Delay(int delayMilliseconds, CancellationToken ct, TaskScheduler scheduler)
        {
            var tcs = new TaskCompletionSource<object>(scheduler);

            Timer timer = null;

            timer = new Timer(state =>
            {
                timer.Dispose();
                tcs.TrySetResult(null);
            }, null, delayMilliseconds, Timeout.Infinite);

            if (ct != CancellationToken.None)
                ct.Register(() => { timer.Dispose(); tcs.TrySetCanceled(); });

            return tcs.Task;
        }
    }
}
