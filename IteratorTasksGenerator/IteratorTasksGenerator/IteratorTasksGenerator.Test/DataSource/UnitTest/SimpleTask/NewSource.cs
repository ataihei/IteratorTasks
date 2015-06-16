using System.Collections;
using IteratorTasks;

namespace ConsoleApplication1
{
    class TypeName
    {
        private Task _task = Task.CompletedTask;

        public IEnumerator MethodName()
        {
            var _t = _task;
            if (!_t.IsCompleted)
                yield return _t;
            _t.ThrowIfException();
        }
    }
}