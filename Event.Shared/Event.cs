using System;
using System.Collections.Generic;
using System.Text;

namespace System.Events
{
    public static class Event
    {
        /// <summary>
        /// <see cref="IEvent{TArg}"/> をデリゲートから作る。
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        public static IEvent<TArg> Create<TArg>(Func<HandlerList<TArg>, IDisposable> subscribe) { return new DelegateEvent<TArg>(subscribe); }

        /// <summary>
        /// イベント引数の型を変換。
        /// </summary>
        public static IEvent<TResult> Select<TSource, TResult>(this IEvent<TSource> source, Func<TSource, TResult> selector)
        {
            return Create<TResult>(e =>
            {
                return source.Subscribe((sender, arg) => e.Invoke(sender, selector(arg)));
            });
        }

        /// <summary>
        /// イベント引数を条件フィルタリング。
        /// </summary>
        public static IEvent<TSource> Where<TSource>(this IEvent<TSource> source, Func<TSource, bool> predicate)
        {
            return Create<TSource>(e =>
            {
                return source.Subscribe((sender, arg) => { if (predicate(arg)) e.Invoke(sender, arg); });
            });
        }

        /// <summary>
        /// object から具体的な型へのキャスト。
        /// 型が合わないイベントが来た時は例外。
        /// </summary>
        public static IEvent<T> Cast<T>(this IEvent<object> source)
        {
            return Create<T>(e =>
            {
                return source.Subscribe((sender, arg) => e.Invoke(sender, (T)arg));
            });
        }

        /// <summary>
        /// object から具体的な型への変換。
        /// <see cref="Cast{T}(IEvent{object})"/> と違って、型が合わないものは無視(イベントを起こさない)。
        /// </summary>
        public static IEvent<T> OfType<T>(this IEvent<object> source)
        {
            return Create<T>(e =>
            {
                return source.Subscribe((sender, arg) => e.Invoke(sender, (T)arg));
            });
        }
    }

    internal class DelegateEvent<TArg> : IEvent<TArg>
    {
        private Func<HandlerList<TArg>, IDisposable> _subscribe;
        public DelegateEvent(Func<HandlerList<TArg>, IDisposable> subscribe) { _subscribe = subscribe; }

        public IDisposable Subscribe(Handler<TArg> action)
        {
            var e = new HandlerList<TArg>();
            e.Subscribe(action);
            return _subscribe(e);
        }
    }
}
