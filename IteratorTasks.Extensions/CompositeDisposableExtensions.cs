using System.Collections.Generic;

namespace System.Reactive.Disposables
{
    public static class CompositeDisposableExtensions
    {
        public static void AddRange(this CompositeDisposable composite, IEnumerable<IDisposable> items)
        {
            foreach (var i in items)
            {
                composite.Add(i);
            }
        }
    }
}
