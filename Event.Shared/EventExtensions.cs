#if UseIteratorTasks
using IteratorTasks;
#else
using System.Threading;
using System.Threading.Tasks;
#endif

namespace System
{
    public static partial class EventExtensions
    {
        /// <summary>
        /// イベントを1回だけ受け取る。
        /// </summary>
        public static Task<TArg> FirstAsync<TArg>(this IEvent<TArg> e, CancellationToken ct)
        {
            return FirstAsync(e, _ => true, ct);
        }
        public static Task<TArg> FirstAsync<TArg>(this IEvent<TArg> e)
        {
            return FirstAsync(e, CancellationToken.None);
        }

        /// <summary>
        /// イベントを1回だけ受け取る。
        /// predicate を満たすまでは何回でもイベントを受け取る。
        /// </summary>
        /// <typeparam name="TArg">イベント引数の型。</typeparam>
        /// <param name="e">イベント発生元。</param>
        /// <param name="predicate">受け取り条件。</param>
        /// <param name="ct">キャンセル用。</param>
        /// <returns>イベントが1回起きるまで待つタスク。</returns>
        public static Task<TArg> FirstAsync<TArg>(this IEvent<TArg> e, Func<TArg, bool> predicate, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TArg>();

            Handler<TArg> a = null;
            a = (sender, args) =>
            {
                if (predicate(args))
                {
                    e.Remove(a);
                    tcs.TrySetResult(args);
                }
            };

            e.Add(a);

            if (ct != CancellationToken.None)
            {
                ct.Register(() =>
                {
                    e.Remove(a);
                    tcs.TrySetCanceled();
                });
            }

            return tcs.Task;
        }

        /// <summary>
        /// イベントを1回だけ受け取る。
        /// predicate を満たすまでは何回でもイベントを受け取る。
        /// predicate が非同期処理なバージョン。
        /// </summary>
        public static Task<TArg> FirstAsync<TArg>(this IEvent<TArg> e, Func<TArg, Task<bool>> predicate, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TArg>();

            Handler<TArg> a = null;
            a = (sender, args) =>
            {
                predicate(args).ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        e.Remove(a);
                        tcs.TrySetResult(args);
                    }
                });
            };

            e.Add(a);

            if (ct != CancellationToken.None)
            {
                ct.Register(() =>
                {
                    e.Remove(a);
                    tcs.TrySetCanceled();
                });
            }

            return tcs.Task;
        }
    }
}
