
namespace IteratorTasks
{
    public static class TaskCompletionSourceExtensions
    {
        public static TaskCompletionSource<T> WithCancellation<T>(this TaskCompletionSource<T> tcs, CancellationToken ct)
        {
            if (ct != CancellationToken.None)
                ct.Register(tcs.SetCanceled);

            return tcs;
        }
    }
}
