using System.Collections;
using System.Collections.Generic;

namespace System.Disposables
{
    /// <summary>
    /// <see cref="IDisposable"/> 関連、ユーティリティ メソッドや拡張。
    /// </summary>
    /// <remarks>
    /// アセンブリの依存関係的にこのプロジェクト内にあるのが一番楽だった。
    ///
    /// Rx にほぼ同じ機能のクラスあり。
    /// こっちの方が、スレッド安全性確保をさぼってる。
    ///
    /// IteratorTasks (以下、IT) にも、Rx 互換機能の一環として同機能のクラスあり。
    ///
    /// IteratorTasks と System.Threading.Tasks(以下、TT。Rx の Disposables を使う) で呼び分けるのが大変すぎて、結局、IT と TT で別系統の Disposables を持ちたくなくて今にいたる。
    /// </remarks>
    public static class Disposable
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

    public class CompositeDisposable : IDisposable, IEnumerable<IDisposable>, IEnumerable
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
