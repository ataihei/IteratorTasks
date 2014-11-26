namespace System
{
    /// <summary>
    /// <see cref="IEvent{TArg}"/> の非同期対応亜種。
    /// イベントの Invoke を await しないと行けない場面で使う。
    /// </summary>
    /// <typeparam name="TArg">イベント引数の型。</typeparam>
    public interface IAsyncEvent<TArg>
    {
        void Add(AsyncHandler<TArg> action);
        void Remove(AsyncHandler<TArg> action);
    }
}
