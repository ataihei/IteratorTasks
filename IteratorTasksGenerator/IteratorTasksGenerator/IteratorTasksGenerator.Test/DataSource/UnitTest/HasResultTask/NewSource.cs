using System.Collections;
using IteratorTasks;

namespace ConsoleApplication1
{
    class TypeName
    {
        private Task<int> _task = Task<int>.CompletedTask;

        public IEnumerator MethodName()
        {
            var _t = _task;
            if (!_t.IsCompleted)
                yield return _t;
            _t.ThrowIfException();
            var result = t.Result;
        }
    }
}