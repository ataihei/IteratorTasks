using T = System.Threading;
using TT = System.Threading.Tasks;
using System.Linq;

namespace IteratorTasks
{
    /// <summary>
    /// IteratorTasks と .NET 標準 Task の interop 用拡張メソッド群。
    /// </summary>
    public static class TaskExtensions
    {
        public static TaskRunner BeginUpdate(this TaskScheduler scheduler)
        {
#if false
            // 複数個所で同じスケジューラーに対してこいつを呼ばれるとやばいので、変な挙動した場合、ここの ifdef true にして呼び出し元探してみる。
            var s = new System.Diagnostics.StackTrace();
            var sid = scheduler.Id;
            System.Diagnostics.Debug.WriteLine("scheduler " + sid + "runs on " + s.ToString());
#endif

            return new TaskRunner(scheduler);
        }

        public static void DebugWriteOnUnhandledException(this TaskScheduler scheduler)
        {
            scheduler.UnhandledException += scheduler_UnhandledException;
        }

        private static void scheduler_UnhandledException(Task obj)
        {
            var e = obj.Error;
            var a = e as AggregateException;

            if (a == null || !a.Exceptions.Any(x => x is TaskCanceledException))
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        public static Task ToIterator(this TT.Task t, TaskScheduler scheduler)
        {
            var tcs = new TaskCompletionSource<object>(scheduler);
            t.ContinueWith(x =>
                {
                    if (x.IsCompleted)
                        tcs.SetResult(null);
                    else if (x.IsFaulted)
                        tcs.SetException(x.Exception);
                    else
                        tcs.SetCanceled();
                });
            return tcs.Task;
        }

        public static Task<T> ToIterator<T>(this TT.Task<T> t, TaskScheduler scheduler)
        {
            var tcs = new TaskCompletionSource<T>(scheduler);
            t.ContinueWith(x =>
                {
                    if (x.IsCanceled)
                        tcs.SetCanceled();
                    else if (x.IsFaulted)
                        tcs.SetException(x.Exception);
                    else
                        tcs.SetResult(x.Result);
                });
            return tcs.Task;
        }

        public static TT.Task ToTpl(this Task t)
        {
            var tcs = new TT.TaskCompletionSource<bool>();
            t.ContinueWith(x =>
                {
                    if (x.IsCanceled)
                        tcs.SetCanceled();
                    else if (x.IsFaulted)
                        tcs.SetException(x.Error);
                    else
                        tcs.SetResult(false);
                });
            return tcs.Task;
        }

        public static TT.Task<T> ToTpl<T>(this Task<T> t)
        {
            var tcs = new TT.TaskCompletionSource<T>();
            t.ContinueWith(x =>
                {
                    if (x.IsCanceled)
                        tcs.SetCanceled();
                    else if (x.IsFaulted)
                        tcs.SetException(x.Error);
                    else
                        tcs.SetResult(t.Result);
                });
            return tcs.Task;
        }

        public static CancellationToken ToIterator(this T.CancellationToken ct)
        {
            var cts = new CancellationTokenSource();
            ct.Register(() => cts.Cancel());
            return cts.Token;
        }

        public static T.CancellationToken ToTpl(this CancellationToken ct)
        {
            var cts = new T.CancellationTokenSource();
            ct.Register(() => cts.Cancel());
            return cts.Token;
        }
    }

}
