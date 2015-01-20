// System.Threading.Tasks だけじゃなくて、Rx 系も互換ライブラリを

using System;

namespace System.Reactive.Disposables
{
    public static class Disposable
    {
        public static IDisposable Create(Action dispose)
        {
            return new ActionDisposer(dispose);
        }

        /// <summary>
        /// 何もしないダミー。
        /// </summary>
        public readonly static IDisposable Empty = new EmptyDisposer();
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

    internal class EmptyDisposer : IDisposable
    {
        public void Dispose() { }
    }
}
