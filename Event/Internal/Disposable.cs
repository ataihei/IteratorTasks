using System.Collections;
using System.Collections.Generic;

namespace System
{
    /// <summary>
    /// Rx とかに同機能のクラスがあるんだけども、依存関係の都合(IteratorTasks と System.Threading.Tasks で呼び分けるの面倒)で、同じものをここに持つ。
    /// IteratorTasks 内にも同様のクラスがあるので、public にはそっちを使ってほしい。
    /// </summary>
    internal static class Disposable
    {
        /// <summary>
        /// Action から <see cref="IDisposable"/> を作る。
        /// </summary>
        /// <param name="dispose"><see cref="IDisposable.Dispose"/> で呼びたい処理。</param>
        /// <returns><see cref="IDisposable"/> 化したもの。</returns>
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

    internal class CompositeDisposable : IDisposable, IEnumerable<IDisposable>, IEnumerable
    {
        private List<IDisposable> _list = new List<IDisposable>();

        /// <summary>
        /// 追加。
        /// </summary>
        /// <param name="d"></param>
        public void Add(IDisposable d)
        {
            _list.Add(d);
        }

        /// <summary>
        /// 追加。
        /// </summary>
        /// <param name="onDipose">破棄処理。</param>
        public void Add(Action onDipose)
        {
            _list.Add(new ActionDisposer(onDipose));
        }

        /// <summary>
        /// グループ内の disposable をまとめて <see cref="IDisposable.Dispose"/>。
        /// </summary>
        public void Dispose()
        {
            foreach (var x in _list)
            {
                if (x != null)
                    x.Dispose();
            }
            _list.Clear();
        }

        IEnumerator<IDisposable> IEnumerable<IDisposable>.GetEnumerator() { return _list.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return _list.GetEnumerator(); }
    }
}
