using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using IT = IteratorTasks;
using ST = System.Threading;
using TT = System.Threading.Tasks;

namespace IteratorTasks
{
    /// <summary>
    /// 複数のスケジューラーを、同時に1個ずつしか動かさない保証付で Update を掛けるクラス。
    /// あくまで、このランナーに登録したスケジューラーは1度に1個しか動かないというだけで、実行スレッドはころころ変わるので注意(Sleep じゃなくて await Delay してるので)。
    /// もちろん、別ランナーに登録したスケジューラーは同時に複数動く。
    /// </summary>
    public class MultiTaskRunner
    {
        private TT.Task _task;
        private volatile bool _isAlive;
        private ST.CancellationTokenSource _cts = new ST.CancellationTokenSource();

        /// <summary>
        /// 管理下にあるスケジューラー一覧。
        /// </summary>
        public IEnumerable<IT.TaskScheduler> Schedulers { get { return _schedulers; } }
        private ImmutableList<IT.TaskScheduler> _schedulers = ImmutableList<IT.TaskScheduler>.Empty;

        /// <summary>
        /// 一斉に止めるためのキャンセル トークン。
        /// </summary>
        public ST.CancellationToken Token { get { return _cts.Token; } }

        /// <summary>
        /// エラーが起きていているかどうか。
        /// </summary>
        public bool HasError { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MultiTaskRunner()
        {
            _task = UpdateLoop();
        }

        private async TT.Task UpdateLoop()
        {
            _isAlive = true;
            while (_isAlive)
            {
                var any = false;

                foreach (var s in _schedulers)
                {
                    try
                    {
                        s.Update();
                    }
                    catch (Exception ex)
                    {
                        HasError = true;
                        OnError(ex, s);
                    }
                    any |= s.IsActive;
                }

                var delayMilliseconds = any ? 1 : 50;
                await TT.Task.Delay(delayMilliseconds).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 停止させる。
        /// </summary>
        /// <returns></returns>
        public TT.Task Stop()
        {
            _cts.Cancel();
            _isAlive = false;
            return _task;
        }

        /// <summary>
        /// 追加。
        /// </summary>
        /// <param name="s"></param>
        public void Add(IT.TaskScheduler s)
        {
            _schedulers = _schedulers.Add(s);
        }

        /// <summary>
        /// 削除。
        /// </summary>
        /// <param name="s"></param>
        public void Remove(IT.TaskScheduler s)
        {
            _schedulers = _schedulers.Remove(s);
        }

        /// <summary>
        /// このランナーが止まった時に Completed になるタスク。
        /// </summary>
        public TT.Task Task { get { return _task; } }

        /// <summary>
        /// エラーがあった場合に起こすイベント。
        /// </summary>
        public event EventHandler<ErrorHandlerAgs> Error;

        /// <summary>
        /// <see cref="Error"/> イベントの引数。
        /// </summary>
        public class ErrorHandlerAgs
        {
            /// <summary>
            /// 起きた例外。
            /// </summary>
            public Exception Error { get; private set; }

            /// <summary>
            /// 起きたタスクを実行していたスケジューラー。
            /// </summary>
            public IT.TaskScheduler Scheduler { get; private set; }

            internal ErrorHandlerAgs(Exception error, IT.TaskScheduler scheduler)
            {
                Error = error;
                Scheduler = scheduler;
            }
        }

        private void OnError(Exception ex, IT.TaskScheduler scheduler)
        {
            var d = Error;
            if (d != null) d(this, new ErrorHandlerAgs(ex, scheduler));
        }
    }
}
