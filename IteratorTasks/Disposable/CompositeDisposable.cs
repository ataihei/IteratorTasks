// System.Threading.Tasks だけじゃなくて、Rx 系も互換ライブラリを

using System.Collections;
using System.Collections.Generic;

namespace System.Reactive.Disposables
{
    public class CompositeDisposable : IDisposable, IEnumerable<IDisposable>, IEnumerable
    {
        private List<IDisposable> _list = new List<IDisposable>();

        public void Add(IDisposable d)
        {
            _list.Add(d);
        }

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
