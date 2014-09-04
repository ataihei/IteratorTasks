using System;
using System.Collections;

namespace IteratorTasks
{
    public static class TaskExtensions
    {
        /// <summary>
        /// タスク正常終了時にのみ呼ばれる。
        /// </summary>
        /// <param name="t"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Task Then(this Task t, Action a)
        {
            t.ContinueWith(x =>
            {
                if (x.Status == TaskStatus.RanToCompletion)
                    a();
            });
            return t;
        }

        /// <summary>
        /// タスク正常終了時にのみ呼ばれる。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Task<T> Then<T>(this Task<T> t, Action<T> a)
        {
            Action<Task<T>> continuation = x =>
            {
                if (x.Status == TaskStatus.RanToCompletion)
                    a(x.Result);
            };
            t.ContinueWith(continuation);
            return t;
        }

        /// <summary>
        /// タスクがエラー終了時、かつ、指定した例外が出た場合にのみ呼ばれる。
        /// </summary>
        /// <typeparam name="T">指定したい例外の型</typeparam>
        /// <param name="t"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Task OnError<TEx>(this Task t, Action<TEx> a)
            where TEx : Exception
        {
            t.ContinueWith(x =>
            {
                if (t.Error != null)
                    foreach (var e in t.Error.Exceptions)
                    {
                        var et = e as TEx;
                        if (et != null)
                            a(et);
                    }
            });
            return t;
        }

        /// <summary>
        /// タスクがエラー終了時、かつ、指定した例外が出た場合にのみ呼ばれる。
        /// </summary>
        /// <typeparam name="T">指定したい例外の型</typeparam>
        /// <param name="t"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Task<T> OnError<T, TEx>(this Task<T> t, Action<TEx> a)
            where TEx : Exception
        {
            t.ContinueWith(x =>
            {
                if (t.Error != null)
                    foreach (var e in t.Error.Exceptions)
                    {
                        var et = e as TEx;
                        if (et != null)
                            a(et);
                    }
            });
            return t;
        }

        /// <summary>
        /// 前段のタスクでエラー出てたらそれを伝搬するだけで、実際にはContinueWithしないバージョン。
        /// </summary>
        /// <param name="t"></param>
        /// <param name="func"></param>
        /// <param name="rethrowOnError"></param>
        /// <returns></returns>
        public static Task ContinueWith(this Task t, Action<Task> func, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWith(func);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception(x.Error);
                else return x.ContinueWith(func);
            });
        }
        public static Task<T> ContinueWith<T>(this Task t, Func<Task, T> func, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWith(func);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception<T>(x.Error);
                else return x.ContinueWith(func);
            });
        }
        public static Task ContinueWithIterator(this Task t, Func<Task, IEnumerator> routine, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWithIterator(routine);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception(x.Error);
                else return x.ContinueWithIterator(routine);
            });
        }
        public static Task ContinueWithTask(this Task t, Func<Task, Task> starter, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWithTask(starter);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception(x.Error);
                else return x.ContinueWithTask(starter);
            });
        }
        public static Task<T> ContinueWithTask<T>(this Task t, Func<Task, Task<T>> starter, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWithTask(starter);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception<T>(x.Error);
                else return x.ContinueWithTask(starter);
            });
        }

        public static Task ContinueWith<T>(this Task<T> t, Action<Task<T>> func, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWith(func);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception(x.Error);
                else return x.ContinueWith(func);
            });
        }
        public static Task<U> ContinueWith<T, U>(this Task<T> t, Func<Task<T>, U> func, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWith(func);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception<U>(x.Error);
                else return x.ContinueWith(func);
            });
        }
        public static Task ContinueWithIterator<T>(this Task<T> t, Func<Task<T>, IEnumerator> routine, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWithIterator(routine);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception(x.Error);
                else return x.ContinueWithIterator(routine);
            });
        }
        public static Task<U> ContinueWithIterator<T, U>(this Task<T> t, Func<Task<T>, Action<U>, IEnumerator> routine, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWithIterator(routine);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception<U>(x.Error);
                else return x.ContinueWithIterator(routine);
            });
        }
        public static Task ContinueWithTask<T>(this Task<T> t, Func<Task<T>, Task> starter, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWithTask(starter);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception(x.Error);
                else return x.ContinueWithTask(starter);
            });
        }
        public static Task<U> ContinueWithTask<T, U>(this Task<T> t, Func<Task<T>, Task<U>> starter, bool rethrowOnError)
        {
            if (!rethrowOnError) return t.ContinueWithTask(starter);
            return t.ContinueWithTask(x =>
            {
                if (x.Error != null) return Task.Exception<U>(x.Error);
                else return x.ContinueWithTask(starter);
            });
        }

        /// <summary>
        /// 一方通行にキャンセルを伝搬するCancellationTokenSourceを作ります
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static CancellationTokenSource ToCancellationTokenSourceOneWay(this CancellationToken ct)
        {
            var cts = new CancellationTokenSource();
            if (ct != CancellationToken.None)
                ct.Register(() => cts.Cancel());
            return cts;
        }
    }
}
