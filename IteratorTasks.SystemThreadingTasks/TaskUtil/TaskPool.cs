using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskUtil
{
    // 同名のクラスが IteratorTasks の方にもあるけど、それの System.Threading.Tasks 版。
    // この名前空間以下でいいのかは微妙（もっと汎用）だけど、移すかどうかは必要になってから考える。
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
