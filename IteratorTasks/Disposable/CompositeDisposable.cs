// System.Threading.Tasks だけじゃなくて、Rx 系も互換ライブラリを

using System.Collections;
using System.Collections.Generic;

namespace System.Reactive.Disposables
{
    /// <summary>
    /// 複数の<see cref="IDisposable"/>をまとめてDisposeすることができる<see cref="IDisposable"/>のコレクション。
    /// </summary>
    public class CompositeDisposable : IDisposable, IEnumerable<IDisposable>, IEnumerable
    {
        private List<IDisposable> _list = new List<IDisposable>();

        /// <summary>
        /// <see cref="IDisposable"/>を追加。
        /// </summary>
        /// <param name="d"></param>
        public void Add(IDisposable d)
        {
            _list.Add(d);
        }

        /// <summary>
        /// 登録されてるすべての<see cref="IDisposable"/>を Dispose する。
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
