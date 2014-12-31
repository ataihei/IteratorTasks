#if UseIteratorTasks
using IteratorTasks;
#else
using System.Threading;
using System.Threading.Tasks;
#endif

using System.Linq;
using System.Reactive.Disposables;

namespace System
{
    /// <summary>
    /// Rx でいう Subject : IObserver, IObservable。
    /// <see cref="IEvent{TArg}"/> の実装。
    /// <see cref="Handler{TArg}"/> を束ねるクラス。
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// public IEvent<T> SomeEvent { get { return _someEvent; } }
    /// private HandlerList<T> _someEvent = new HandlerList<T>();
    /// ]]>
    /// </example>
    /// <typeparam name="TArg">イベント引数の型。</typeparam>
    public class HandlerList<TArg> : IEvent<TArg>
    {
        private Handler<TArg>[] _list;
        private object _sync = new object();

        public bool HasAny { get { return _list != null && _list.Length != 0; } }

        public void Invoke(object sender, TArg args)
        {
            // ↓これだけでいいはずなんだけども、たぶん iOS AOT で動かない。
            // var actions = System.Threading.Interlocked.Exchange(ref _list, null);
            // _sync が要らなかったら、単なる配列の薄いラッパーだし、struct にしたいんだけども。
            Handler<TArg>[] actions;
            lock (_sync)
            {
                actions = _list;
            }

            if (actions == null) return;
            foreach (var a in actions)
            {
                if (a != null) a(sender, args);
            }
        }

        public IDisposable Subscribe(Handler<TArg> action)
        {
            Add(action);
            return Disposable.Create(() => Remove(action));
        }

        public void Add(Handler<TArg> action)
        {
            // こっちは CompareExchage
            lock (_sync)
            {
                _list = _list == null
                    ? new[] { action }
                    : _list.Concat(new[] { action }).ToArray();
            }
        }

        public void Remove(Handler<TArg> action)
        {
            lock (_sync)
            {
                _list = _list == null
                    ? new Handler<TArg>[] { }
                    : _list.Where(x => x != action).ToArray();
            }
        }
    }

    /// <summary>
    /// 非ジェネリック版。
    /// </summary>
    /// <remarks>
    /// <see cref="Handler{TArg}"/> とは別型にしたいものの、実装がほぼコピペの似て非なるコードになるのがつらすぎて断念した結果。
    /// void はジェネリックの敵。
    /// </remarks>
    public class HandlerList : HandlerList<Null>
    {
        public void Invoke(object sender) { Invoke(sender, null); }
    }
}
