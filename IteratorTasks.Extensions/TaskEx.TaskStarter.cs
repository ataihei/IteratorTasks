using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IteratorTasks
{
    /// <summary>
    /// 標準の System.Threading.Tasks.Task にない類の処理。
    /// </summary>
    public static partial class TaskEx
    {
        /// <summary>
        /// <see cref="Task"/> を1個1個順に実行する。
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task Sequencial(IEnumerable<TaskStarter> tasks)
        {
            return Task.Run(SequencialInternal(tasks));
        }

        /// <summary>
        /// <see cref="Sequencial(IEnumerable{TaskStarter})"/>
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task Sequencial(params TaskStarter[] tasks) { return Sequencial(tasks.AsEnumerable()); }

        private static IEnumerator SequencialInternal(IEnumerable<TaskStarter> tasks)
        {
            foreach (var t in tasks)
            {
                var task = t();
                if (!task.IsCompleted)
                    yield return task;
            }
        }
    }
}
