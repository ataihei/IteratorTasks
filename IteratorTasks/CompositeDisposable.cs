// System.Threading.Tasks だけじゃなくて、Rx 系も互換ライブラリを

using System;
using System.Collections.Generic;

namespace IteratorTasks
{
    public class CompositeDisposable : IDisposable
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
                x.Dispose();
            }
            _list.Clear();
        }
    }
}
