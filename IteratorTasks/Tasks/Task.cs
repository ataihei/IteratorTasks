using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IteratorTasks
{
	/// <summary>
	/// .NET 4 の Task 的にコルーチンを実行するためのクラス。
	/// 戻り値なし版。
	/// </summary>
	public partial class Task : IAwaiter, IEnumerator, IDisposable
	{
		/// <summary>
		/// TaskScheduler で使うフラグ。
		/// RunTask の直前で落として、RunTask 実行後に立てる。
		/// あんまり internal にしたくないけども妥協。
		/// </summary>
		internal bool _updated;

		internal Task() { }

		public Task(IEnumerator routine)
		{
			if (routine == null)
				throw new ArgumentNullException("routine");

			Status = TaskStatus.Created;
			Routine = routine;
		}

		public Task(Func<IEnumerator> routine)
		{
			Status = TaskStatus.Created;
			try
			{
				Routine = routine();
			}
			catch (Exception e)
			{
				AddError(e);
				Complete();
			}
		}

		protected void RunOnce()
		{
			try
			{
				if (!Routine.MoveNext())
					Complete();
				Status = TaskStatus.Running;
			}
			catch (Exception e)
			{
				AddError(e);
				Complete();
			}
		}

		/// <summary>
		/// 中で動いているコルーチン。
		/// </summary>
		protected IEnumerator Routine { get; set; }

		/// <summary>
		/// タスク実行中に発生した例外。
		/// </summary>
		public AggregateException Error { get; private set; }

		protected void AddError(Exception exc)
		{
			var agg = exc as AggregateException;
			if (agg != null)
			{
				if (Error == null)
					Error = agg;
				else
					Error.Merge(agg);
			}
			else
			{
				if (Error == null)
					Error = new AggregateException(exc);
				else
					Error.Merge(exc);
			}
		}

		/// <summary>
		/// タスクの現在の状態。
		/// </summary>
		public TaskStatus Status { get; protected internal set; }

		public bool IsDone { get { return IsCompleted || IsCanceled || IsFaulted; } }
		public bool IsCompleted { get { return Status == TaskStatus.RanToCompletion; } }
		public bool IsCanceled { get { return Status == TaskStatus.Canceled; } }
		public bool IsFaulted { get { return Status == TaskStatus.Faulted; } }

		void IAwaiter.OnCompleted(Action continuation)
		{
			if (IsDone)
				Scheduler.Post(continuation);
			else
			{
				lock (_callback)
				{
					_callback.Add(continuation);
				}
			}
		}

		List<Action> _callback = new List<Action>();

		object IEnumerator.Current
		{
			get
			{
				if (IsCanceled) return null;
				return Routine == null ? null : Routine.Current;
			}
		}

		bool IEnumerator.MoveNext()
		{
			if (Status == TaskStatus.Created)
			{
				Status = TaskStatus.Running;

				// 最初の一回、Start 時に RunOnce して、そこで MoveNext が false ならもう RanToEnd になってるはずなので。
				return true;
			}

			if (Status != TaskStatus.Running) return false;
			if (Routine == null) return false;

			bool hasNext;

			try
			{
				hasNext = Routine.MoveNext();
			}
			catch (Exception exc)
			{
				AddError(exc);
				hasNext = false;
			}

			if (!hasNext)
			{
				Complete();
			}
			return hasNext;
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException();
		}

		void IDisposable.Dispose()
		{
			var d = Routine as IDisposable;
			if (d != null) d.Dispose();
			Routine = null;
		}

		protected void Complete()
		{
			if (Status == TaskStatus.Canceled)
				return;

			if (Error != null)
				Status = TaskStatus.Faulted;
			else
				Status = TaskStatus.RanToCompletion;

			var d = _beforeCallback;
			if (d != null) d();

			lock (_callback)
			{
				if (_callback.Count != 0)
				{
					foreach (var c in _callback)
					{
						Scheduler.Post(c);
					}
				}
				_callback.Clear();
			}
		}

		internal event Action BeforeCallback { add { _beforeCallback += value; } remove { _beforeCallback -= value; } }
		private Action _beforeCallback;

		/// <summary>
		/// このタスクをキャンセルするためのトークン。
		/// </summary>
		public CancellationTokenSource Cancellation { get; private set; }

		/// <summary>
		/// タスクをキャンセルします。
		/// CancellationToken 越しにキャンセルするので、コルーチン側が対応していない場合には即座にはタスク終了しない。
		/// </summary>
		public void Cancel()
		{
			if (Cancellation == null)
				throw new InvalidOperationException("Can't cancel Task.");

			Cancellation.Cancel();

			((IEnumerator)this).MoveNext();
		}

		/// <summary>
		/// Cancel と同様。
		/// キャンセル理由を指定できるバージョン。
		/// </summary>
		/// <param name="e">キャンセル理由。</param>
		public void Cancel(Exception e)
		{
			if (Cancellation == null)
				throw new InvalidOperationException("Can't cancel Task.");

			Cancellation.Cancel(e);

			((IEnumerator)this).MoveNext();
		}

		/// <summary>
		/// タスクの強制終了。
		/// CancellationToken を介さないので、タスクの中身のルーチン側はキャンセルされたことを知るすべがなく、途中で止まる。
		/// </summary>
		public void ForceCancel()
		{
			ForceCancel(new TaskCanceledException("Task force canceled."));
		}

		/// <summary>
		/// タスクを強制キャンセルします。OnCompleteは呼ばれません。
		/// 例外も処理済み扱い（IsHandled が true に）。
		/// </summary>
		public void ForceCancel(Exception e)
		{
			Status = TaskStatus.Canceled;
			AddError(e);
			Error.IsHandled = true;
			((IDisposable)this).Dispose();
		}

		/// <summary>
		/// 既定のタスクスケジューラー。
		/// </summary>
		public static TaskScheduler DefaultScheduler { get { return ___xxx; } } private static TaskScheduler ___xxx = new TaskScheduler(-1);

		/// <summary>
		/// このタスクに紐づいているスケジューラー。
		/// </summary>
		public TaskScheduler Scheduler { get { return _scheduler ?? DefaultScheduler; } }
		internal TaskScheduler _scheduler;

		/// <summary>
		/// タスク開始。
		/// new Task(ルーチン) しただけだと開始してない状態（cold start）なので、Startを呼ぶ必要がある。
		/// Task.Run(ルーチン) なら介した状態（hot start）のタスクが返る。
		/// </summary>
		public void Start()
		{
			Start(DefaultScheduler);
		}

		/// <summary>
		/// スケジューラーを明示してタスク開始。
		/// </summary>
		/// <param name="scheduler"></param>
		public void Start(TaskScheduler scheduler)
		{
			if (Status == TaskStatus.Created)
			{
				if (scheduler == null)
					throw new ArgumentNullException();

				RunOnce();
				scheduler.QueueTask(this);
				_scheduler = scheduler;
			}
			else
				throw new InvalidOperationException();
		}

		/// <summary>
		/// 開始済みのタスクを作る。
		/// </summary>
		/// <param name="routine"></param>
		/// <returns></returns>
		public static Task Run(IEnumerator routine) { return Run(routine, null); }

		/// <summary>
		/// スケジューラーを指定して開始済みのタスクを作る。
		/// </summary>
		/// <param name="routine"></param>
		/// <param name="scheduler"></param>
		/// <returns></returns>
		public static Task Run(IEnumerator routine, TaskScheduler scheduler)
		{
			var t = new Task(routine);
			t.Start(scheduler ?? DefaultScheduler);
			return t;
		}

		/// <summary>
		/// 開始済みのタスクを作る。
		/// </summary>
		/// <param name="routine"></param>
		/// <returns></returns>
		public static Task Run(Func<IEnumerator> routine) { return Run(routine, null); }

		/// <summary>
		/// 開始済みのタスクを作る。
		/// キャンセル可能。
		/// </summary>
		/// <param name="routine"></param>
		/// <param name="cts"></param>
		/// <returns></returns>
		public static Task Run(Func<CancellationToken, IEnumerator> routine, CancellationTokenSource cts) { return Run(routine, cts, null); }

		public static Task Run(Func<CancellationToken, IEnumerator> routine, CancellationTokenSource cts, TaskScheduler scheduler)
		{
			var t = Run(() => routine(cts.Token), scheduler);
			t.Cancellation = cts;
			return t;
		}

		/// <summary>
		/// スケジューラーを指定して開始済みのタスクを作る。
		/// </summary>
		/// <param name="routine"></param>
		/// <param name="scheduler"></param>
		/// <returns></returns>
		public static Task Run(Func<IEnumerator> routine, TaskScheduler scheduler)
		{
			var t = new Task(routine);
			if (!t.IsDone) t.Start(scheduler ?? DefaultScheduler);
			return t;
		}

		public static Task<T> Run<T>(Func<Action<T>, IEnumerator> routine) { return Run(routine, null); }

		public static Task<T> Run<T>(Func<Action<T>, CancellationToken, IEnumerator> routine, CancellationTokenSource cts) { return Run(routine, cts, null); }

		public static Task<T> Run<T>(Func<Action<T>, CancellationToken, IEnumerator> routine, CancellationTokenSource cts, TaskScheduler scheduler)
		{
			var t = Run<T>(callback => routine(callback, cts.Token), scheduler);
			t.Cancellation = cts;
			return t;
		}

		public static Task<T> Run<T>(Func<Action<T>, IEnumerator> routine, TaskScheduler scheduler)
		{
			var t = new Task<T>(routine);
			if (!t.IsDone) t.Start(scheduler ?? DefaultScheduler);
			return t;
		}

		/// <summary>
		/// ただの値をタスク化。
		/// 作ったタスクは、最初から完了済みで、Result で値を取れる。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Task<T> Return<T>(T value) { return new Task<T>(value); }

		/// <summary>
		/// 完了しているタスク
		/// </summary>
		/// <returns></returns>
		public static Task Return() { return new Task<object>(default(object)); }

		/// <summary>
		/// キャンセルを待つだけのタスク
		/// </summary>
		public static Task<T> WaitCancel<T>(CancellationToken ct)
		{
			var tcs = new TaskCompletionSource<T>();
			if (ct != CancellationToken.None)
				ct.Register(() => tcs.SetCanceled());
			return tcs.Task;
		}

		/// <summary>
		/// 例外を直接渡して、最初から完了済みのタスクを作る。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="error"></param>
		/// <returns></returns>
		public static Task<T> Exception<T>(Exception error)
		{
			var tcs = new TaskCompletionSource<T>();
			tcs.SetException(error);
			return tcs.Task;
		}

		/// <summary>
		/// 例外を直接渡して、最初から完了済みのタスクを作る。
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		public static Task Exception(Exception error)
		{
			return Exception<object>(error);
		}

		/// <summary>
		/// 複数のタスクが完了するのを待つタスクを作る。
		/// </summary>
		/// <param name="tasks"></param>
		/// <returns></returns>
		/// <remarks>
		/// foreach しかしてないのに IEnumerable 版がないのは、遅延評価されたくないから。
		/// </remarks>
		public static Task WhenAll(params Task[] tasks)
		{
			int count = tasks.Length;
			if (count == 0) return Task.Return<object>(null);

			var tcs = new TaskCompletionSource<object>();
			AggregateException ex = new AggregateException();
			var isAllErrorHandled = true;
			foreach (var t in tasks)
			{
				var tLocal = t;
				Action callback = null;
				callback = () =>
				{
					tLocal.BeforeCallback -= callback;
				};
				tLocal.BeforeCallback += callback;

				var a = (IAwaiter)tLocal;
				a.OnCompleted(() =>
				{
					if (tLocal.IsFaulted)
					{
						if (!tLocal.Error.IsHandled)
							isAllErrorHandled = false;
						tLocal.Error.IsHandled = true;
						lock (ex) ex.Merge(tLocal.Error);
					}

					System.Threading.Interlocked.Decrement(ref count);

					if (count == 0)
					{
						if (ex.Exceptions.Any())
						{
							ex.IsHandled = isAllErrorHandled;
							tcs.SetException(ex);
						}
						else
							tcs.SetResult(null);
					}
				});
			}

			return tcs.Task;
		}

		public static Task<T> First<T>(Task<T>[] tasks) { return First(tasks, null); }

		public static Task<T> First<T>(params AsyncFunc<T>[] tasks)
		{
			var cts = new CancellationTokenSource();
			var created = tasks.Select(x => x(cts.Token)).ToArray();
			return First(created, cts);
		}

		/// <summary>
		/// 複数のタスクのうち、最初に終わったものの値を返す。
		/// 残りのタスクは内部でキャンセルする。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tasks">最初の1つを待ちたいタスク一覧。</param>
		/// <param name="cts"></param>
		/// <returns>最初の1つだけ終わったら完了になるタスク。</returns>
		public static Task<T> First<T>(Task<T>[] tasks, CancellationTokenSource cts)
		{
			return First(tasks, null, cts);
		}

		public static Task<T> First<T>(Task<T>[] tasks, TaskScheduler scheduler, CancellationTokenSource cts)
		{
			if (tasks.Length == 0)
				throw new ArgumentException("tasks must contain at least one task", "tasks");

			var tcs = new TaskCompletionSource<T>(scheduler);
			// Task.Firstに終わったタスクを渡すとtcs = null が先に呼ばれてnullぽ出るのでここで受けておく
			var task0 = tcs.Task;

			if (cts != null)
				cts.Token.Register(() =>
				{
					if (tcs != null)
					{
						tcs.SetCanceled();
						tcs = null;
					}
				});

			foreach (var task in tasks)
			{
				if (task == null)
					throw new ArgumentException("task must not null", "tasks");

				task.ContinueWith(t =>
				{
					if (tcs != null)
					{
						tcs.Propagate(t);
						tcs = null;
						if (cts != null)
							cts.Cancel();
					}
				});
			}

			return task0;
		}

		public static Task First(Task[] tasks) { return First(null, tasks, null); }

		public static Task First(Action onComplete, params AsyncAction[] tasks)
		{
			var cts = new CancellationTokenSource();
			var created = tasks.Select(x => x(cts.Token)).ToArray();
			return First(onComplete, created, cts);
		}

		public static Task First(params AsyncAction[] tasks) { return First(default(TaskScheduler), tasks); }

		public static Task First(TaskScheduler scheduler, params AsyncAction[] tasks)
		{
			var cts = new CancellationTokenSource();
			var created = tasks.Select(x => x(cts.Token)).ToArray();
			return First(null, created, scheduler, cts);
		}

		public static Task First(Task[] tasks, CancellationTokenSource cts)
		{
			return First(null, tasks, cts);
		}

		public static Task First(Action onComplete, Task[] tasks, CancellationTokenSource cts)
		{
			return First(onComplete, tasks, null, cts);
		}

		/// <summary>
		/// 複数のタスクのうち、最初に終わったものの値を返す。
		/// 残りのタスクは内部でキャンセルする。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="onComplete">最初の1つのタスクが終了時に呼ばれる。Task.First().ContinueWith(onComplete) すると呼ばれるフレームが1フレーム遅れるけども、これならたぶん即呼ばれる。</param>
		/// <param name="tasks">最初の1つを待ちたいタスク一覧。</param>
		/// <param name="cts"></param>
		/// <returns>最初の1つだけ終わったら完了になるタスク。</returns>
		public static Task First(Action onComplete, Task[] tasks, TaskScheduler scheduler, CancellationTokenSource cts)
		{
			if (tasks.Length == 0)
				throw new ArgumentException("tasks must contain at least one task", "tasks");

			var tcs = new TaskCompletionSource<object>(scheduler);
			// Task.Firstに終わったタスクを渡すとtcs = null が先に呼ばれてnullぽ出るのでここで受けておく
			var task0 = tcs.Task;

			if (cts != null)
				cts.Token.Register(() =>
				{
					if (onComplete != null)
						onComplete();

					if (tcs != null)
					{
						tcs.SetCanceled();
						tcs = null;
					}
				});

			foreach (var task in tasks)
			{
				if(task == null)
					throw new ArgumentException("task must not null", "tasks");

				task.ContinueWith(t =>
				{
					if (tcs != null)
					{
						tcs.Propagate(t);
						tcs = null;
						if (cts != null)
							cts.Cancel();
					}
				});
			}

			return task0;
		}

		protected Task<T> ContinueWithInternal<T>(Func<Task> starter)
		{
			var tcs = new TaskCompletionSource<T>();

			var a1 = (IAwaiter)this;

			a1.OnCompleted(() =>
			{
				try
				{
					var continuation = starter();
					var a2 = (IAwaiter)continuation;

					a2.OnCompleted(() => tcs.Propagate(continuation));
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});

			return tcs.Task;
		}

		protected Task<T> ContinueWithInternal<T>(Func<T> func)
		{
			var tcs = new TaskCompletionSource<T>(Scheduler);

			var a1 = (IAwaiter)this;

			a1.OnCompleted(() =>
			{
				if (this.IsCanceled)
				{
					tcs.SetCanceled();
					return;
				}

				try
				{
					var result = func();
					tcs.SetResult(result);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});

			return tcs.Task;
		}

		/// <summary>
		/// タスク完了後に別の処理を行う。
		/// </summary>
		/// <param name="func"></param>
		/// <returns></returns>
		public Task ContinueWith(Action<Task> func)
		{
			return ContinueWithInternal<object>(() => { func(this); return default(object); });
		}

		public Task<T> ContinueWith<T>(Func<Task, T> func)
		{
			return ContinueWithInternal<T>(() => func(this));
		}

		/// <summary>
		/// タスク完了後に別の処理を行う。
		/// イテレーターを渡して新しいタスクを起動。
		/// </summary>
		/// <param name="routine"></param>
		/// <returns></returns>
		public Task ContinueWithIterator(Func<Task, IEnumerator> routine)
		{
			return ContinueWithInternal<object>(() => Task.Run(() => routine(this), this.Scheduler));
		}

		/// <summary>
		/// タスク完了後に別の処理を行う。
		/// イテレーターを渡して新しいタスクを起動。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="routine"></param>
		/// <returns></returns>
		public Task<T> ContinueWithIterator<T>(Func<Task, Action<T>, IEnumerator> routine)
		{
			return ContinueWithInternal<T>(() => Task.Run<T>(callback => routine(this, callback), this.Scheduler));
		}

		/// <summary>
		/// タスク完了後に別の処理を行う。
		/// タスク開始用のメソッドを渡して、新しいタスクを起動。
		/// </summary>
		/// <param name="starter"></param>
		/// <returns></returns>
		public Task ContinueWithTask(Func<Task, Task> starter)
		{
			return ContinueWithInternal<object>(() => starter(this));
		}

		/// <summary>
		/// タスク完了後に別の処理を行う。
		/// タスク開始用のメソッドを渡して、新しいタスクを起動。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="starter"></param>
		/// <returns></returns>
		public Task<T> ContinueWithTask<T>(Func<Task, Task<T>> starter)
		{
			return ContinueWithInternal<T>(() => starter(this));
		}

		[Obsolete]
		public bool IsHandled { get { return Error == null || Error.IsHandled; } set { if (Error != null) Error.IsHandled = value; } }
	}
}
