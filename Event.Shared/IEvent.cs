namespace System
{
    /// <summary>
    /// Rx でいう IObservable。
    /// Rx が使えるならこんなクラス自前で用意しないのに。的なやつ。
    /// 標準の event が使いにくいので。
    /// <seealso cref="HandlerList{TArg}"/>
    /// </summary>
    /// <typeparam name="TArg">イベント引数の型。</typeparam>
    public interface IEvent<TArg>
    {
        void Add(Handler<TArg> action);
        void Remove(Handler<TArg> action);
    }
}
