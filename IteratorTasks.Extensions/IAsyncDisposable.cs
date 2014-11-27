using IteratorTasks;

namespace System
{
    public interface IAsyncDisposable
    {
        Task DisposeAsync();
    }
}
