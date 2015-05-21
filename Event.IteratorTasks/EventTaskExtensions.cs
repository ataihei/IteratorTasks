using System.Threading;

namespace System.Events
{
    /// <summary>
    /// <see cref="IEvent{TArg}"/> と <see cref="IteratorTasks.Task"/> つなぎこみ関連拡張メソッド。
    /// </summary>
    public static class EventTaskExtensions
    {
        /// <summary>
        /// スケジューラー上で Subscribe する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static IEvent<T> SubscribeOn<T>(this IEvent<T> e, IteratorTasks.TaskScheduler scheduler)
        {
            return Event.Create<T>(handler =>
                e.Subscribe((sender, x) =>
                    scheduler.Post(() =>
                        handler.Invoke(sender, x)
                    )
                )
            );
        }
    }
}
