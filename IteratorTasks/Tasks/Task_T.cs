using System;
using System.Collections;

namespace IteratorTasks
{
    /// <summary>
    /// .NET 4 の Task 的にコルーチンを実行するためのクラス。
    /// 戻り値あり版。
    /// </summary>
    /// <typeparam name="T">最終的に返す型。</typeparam>
    public partial class Task<T> : Task
    {
        /// <summary>
        /// 最終的に返したい値。
        /// </summary>
        virtual public T Result
        {
            get
            {
                if (Exception != null) throw Exception;
                return _result;
            }
        }
        private T _result = default(T);

        internal Task() { }

        /// <summary>
        /// コルーチンのスタート用関数を渡して初期化。
        /// </summary>
        /// <param name="routine">コルーチンのスタート用関数。</param>
        public Task(Func<Action<T>, IEnumerator> routine)
        {
            Status = TaskStatus.Created;
            Routine = routine(SetResult);
        }

        internal Task(T result)
        {
            Status = TaskStatus.RanToCompletion;
            _result = result;
        }

        private void SetResult(T result)
        {
            _result = result;
        }

        /// <summary>
        /// ターゲットの <see cref="Task"/> が完了したときに非同期に実行する継続タスクを作成します。 
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public Task ContinueWith(Action<Task<T>> func) => ContinueWithInternal<object>(() => { func(this); return default(object); });

        /// <summary>
        /// ターゲットの <see cref="Task"/> が完了したときに非同期に実行する継続タスクを作成します。 
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public Task<U> ContinueWith<U>(Func<Task<T>, U> func) => ContinueWithInternal<U>(() => func(this));

        /// <summary>
        /// ターゲットの <see cref="Task"/> が完了したときに非同期に実行する継続タスクを作成します。 
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public Task ContinueWithIterator(Func<Task<T>, IEnumerator> routine) => ContinueWithInternal<object>(() => Task.Run(() => routine(this), this.Scheduler));

        /// <summary>
        /// ターゲットの <see cref="Task"/> が完了したときに非同期に実行する継続タスクを作成します。 
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="routine"></param>
        /// <returns></returns>
        public Task<U> ContinueWithIterator<U>(Func<Task<T>, Action<U>, IEnumerator> routine) => ContinueWithInternal<U>(() => Task.Run<U>(callback => routine(this, callback), this.Scheduler));

        /// <summary>
        /// ターゲットの <see cref="Task"/> が完了したときに非同期に実行する継続タスクを作成します。 
        /// </summary>
        /// <param name="starter"></param>
        /// <returns></returns>
        public Task ContinueWithTask(Func<Task<T>, Task> starter) => ContinueWithInternal<object>(() => starter(this));

        /// <summary>
        /// ターゲットの <see cref="Task"/> が完了したときに非同期に実行する継続タスクを作成します。 
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="starter"></param>
        /// <returns></returns>
        public Task<U> ContinueWithTask<U>(Func<Task<T>, Task<U>> starter) => ContinueWithInternal<U>(() => starter(this));
    }
}
