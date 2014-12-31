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
