using System.Collections.Generic;

namespace IteratorTasks
{
    /// <summary>
    /// いったんタスクを投げっぱなし(await しない)でほったらかして、
    /// 後からまとめて WhenAll するためのクラス。
    /// </summary>
    public class TaskPool
    {
        private readonly List<Task> _list = new List<Task>();

        public void Register(Task t) { _list.Add(t); }

        public Task Await()
        {
            var copy = _list.ToArray();
            _list.Clear();
            return Task.WhenAll(copy);
        }
    }
}
