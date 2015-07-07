#if UseIteratorTasks
using IteratorTasks;
#else
using System.Threading;
using System.Threading.Tasks;
#endif

namespace System
{
    /// <summary>
    /// <see cref="IAsyncEvent{T}"/> と <see cref="Task"/> 相互運用がらみ拡張メソッド。
    /// </summary>
    public static partial class AysncEventTaskExtensions
    {
        #region Subscribe 非同期版

        /// <summary>
        /// イベントを購読する。
        /// </summary>
        public static IDisposable Subscribe<T>(this IAsyncEvent<T> e, AsyncHandler<T> handler)
        {
            e.Add(handler);
            return Disposable.Create(() => e.Remove(handler));
        }

        /// <summary>
        /// イベントを購読する。
        /// </summary>
        public static IDisposable Subscribe<T>(this IAsyncEvent<T> e, Func<T, Task> handler) => Subscribe(e, (_1, arg) => handler(arg));

        /// <summary>
        /// イベントを購読する。
        /// </summary>
        public static IDisposable Subscribe<T>(this IAsyncEvent<T> e, Func<Task> handler) => Subscribe(e, (_1, _2) => handler());

        /// <summary>
        /// イベントを購読する。
        /// </summary>
        public static IDisposable Subscribe<T>(this IAsyncEvent<T> e, Action handler) => Subscribe(e, (_1, _2) => { handler(); return Task.FromResult(default(object)); });

        /// <summary>
        /// イベントを購読する。
        /// </summary>
        public static IDisposable Subscribe<T>(this IAsyncEvent<T> e, Action<T> handler) => Subscribe(e, (_1, args) => { handler(args); return Task.FromResult(default(object)); });

        /// <summary>
        /// キャンセルされるまでの間イベントを購読する。
        /// </summary>
        public static void SubscribeUntil<T>(this IAsyncEvent<T> e, CancellationToken ct, AsyncHandler<T> handler)
        {
            var d = e.Subscribe(handler);
            ct.Register(d.Dispose);
        }

        /// <summary>
        /// キャンセルされるまでの間イベントを購読する。
        /// </summary>
        public static void SubscribeUntil<T>(this IAsyncEvent<T> e, CancellationToken ct, Func<T, Task> handler)
        {
            var d = e.Subscribe(handler);
            ct.Register(d.Dispose);
        }

        /// <summary>
        /// キャンセルされるまでの間イベントを購読する。
        /// </summary>
        public static void SubscribeUntil<T>(this IAsyncEvent<T> e, CancellationToken ct, Func<Task> handler)
        {
            var d = e.Subscribe(handler);
            ct.Register(d.Dispose);
        }

        #endregion
    }

    internal static class Disposable
    {
        /// <summary>
        /// Action から <see cref="IDisposable"/> を作る。
        /// </summary>
        /// <param name="dispose"><see cref="IDisposable.Dispose"/> で呼びたい処理。</param>
        /// <returns><see cref="IDisposable"/> 化したもの。</returns>
        public static IDisposable Create(Action dispose) => new ActionDisposer(dispose);
    }

    internal class ActionDisposer : IDisposable
    {
        Action _onDispose;

        public ActionDisposer(Action onDispose)
        {
            if (onDispose == null)
                throw new ArgumentNullException();
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }
    }
}
