using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IteratorTasks
{
	/// <summary>
	/// Task を状態遷移のトリガーとする状態マシン。
	/// </summary>
	public class StateManager : IEnumerable<State>
	{
		/// <summary>
		/// 終了状態のID。
		/// これを設定していないと一生 Await が終わらないので注意。
		/// </summary>
		public const int EndStateId = -1;

		public int InitialStateId { get; private set; }
		private List<State> _states = new List<State>();

		public StateManager(int initialStateId)
		{
			InitialStateId = initialStateId;
		}
		public Task Await()
		{
			return Await(null);
		}

		public Task Await(int initialOption)
		{
			return Await(null, initialOption);
		}

		public Task Await(TaskScheduler scheduler)
		{
			return Await(scheduler, 0);
		}

		public Task Await(CancellationToken ct)
		{
			return Await(null, ct, 0);
		}

		public Task Await(TaskScheduler scheduler, int initialOption)
		{
			return Await(scheduler, CancellationToken.None, initialOption);
		}

		public Task Await(TaskScheduler scheduler, CancellationToken ct)
		{
			return Await(scheduler, ct, 0);
		}

		public Task Await(TaskScheduler scheduler, CancellationToken ct, int initialOption)
		{
			return Task.Run(AwaitIterator(scheduler, ct, initialOption), scheduler);
		}

		public IEnumerator AwaitIterator(TaskScheduler scheduler, CancellationToken ct, int initialOption)
		{
			if (ct != CancellationToken.None && ct.IsCancellationRequested)
				yield break;

			var stateId = InitialStateId;
			var option = initialOption;

			OnStateChanged(stateId, option);

			while (true)
			{
				if (ct != CancellationToken.None && ct.IsCancellationRequested)
					break;

				var state = _states.First(s => s.Id == stateId);
				var t = state.AwaitTrigger(scheduler, ct, option);

				if (!t.IsDone)
					yield return t.ContinueWith(_ =>
					{
						// フレーム遅らせたくないのでここと else 内で同じ処理
						stateId = t.Result.NextStateId;
						option = t.Result.Option;
						OnStateChanged(stateId, option);
					});
				else
				{
					stateId = t.Result.NextStateId;
					option = t.Result.Option;
					OnStateChanged(stateId, option);
				}

				if (stateId == EndStateId)
					break;

				// ステート数が多くなった時のことを見越して辞書にしておく方が無難？
				// 今のところ、辞書にした方が遅そうな程度のステート数の予定。
			}

			//OnStateChanged(EndStateId, option);
		}

		public event Action<int, int> StateChanged { add { _stateChanged += value; } remove { _stateChanged -= value; } }
		private Action<int, int> _stateChanged;

		private void OnStateChanged(int stateId, int option)
		{
			var d = _stateChanged;
			if (d != null) d(stateId, option);
		}

		#region コレクション初期化子用 
		public void Add(State s) { _states.Add(s); }
		IEnumerator<State> IEnumerable<State>.GetEnumerator() { return _states.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return _states.GetEnumerator(); }
		#endregion
	}

	/// <summary>
	/// 状態マシンの1つ1つの状態。
	/// ID ＋ 遷移条件。
	/// </summary>
	public class State : IEnumerable<Transition>
	{
		/// <summary>
		/// 状態ID。
		/// </summary>
		/// <returns></returns>
		public int Id { get; private set; }

		public State(int id) { Id = id; }

		/// <summary>
		/// 遷移条件一覧。
		/// </summary>
		public IEnumerable<Transition> Transitions { get; private set; }
		private List<Transition> _transisions = new List<Transition>();

		internal Task<TransitionOption> AwaitTrigger(TaskScheduler scheduler, CancellationToken ct, int option)
		{
			var ctsInternal = ct.ToCancellationTokenSourceOneWay();
			// Register 解除できるように作って、↓のFirstが終わったら解除しないとだめかも。挙動はいいけど、無駄にインスタンス握りっぱなしでメモリ効率悪い予感。
			return Task.First(_transisions.Select(t => t.AwaitTrigger(ctsInternal.Token, option)).ToArray(), scheduler, ctsInternal);
		}

		#region コレクション初期化子用 
		public void Add(Transition t) { _transisions.Add(t); }
		public IEnumerator<Transition> GetEnumerator() { return Transitions.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return Transitions.GetEnumerator(); }
		#endregion
	}

	/// <summary>
	/// 状態マシンの遷移条件。
	/// </summary>
	public class Transition
	{
		/// <summary>
		/// First で最初に終わったタスクを見て、それの戻り値(int で次状態のIDを返す)を見て状態遷移する。
		/// </summary>
		public AsyncFunc<int, TransitionOption> Trigger { get; private set; }

		public Transition(AsyncAction trigger)
			: this(trigger, StateManager.EndStateId) { }

		public Transition(AsyncAction<int> trigger)
			: this(trigger, StateManager.EndStateId) { }

		/// <param name="trigger">次状態IDを返すTask。</param>
		public Transition(AsyncFunc<TransitionOption> trigger)
		{
			Trigger = (option, ct) => trigger(ct);
		}

		/// <param name="trigger">次状態IDを返すTask。</param>
		public Transition(AsyncFunc<int, TransitionOption> trigger)
		{
			Trigger = trigger;
		}

		/// <param name="trigger">遷移のきっかけとなる Task。</param>
		/// <param name="nextStateId">次状態のID。</param>
		public Transition(AsyncAction trigger, int nextStateId)
		{
			Trigger = (option, ct) => trigger(ct).ContinueWith(_ => new TransitionOption { NextStateId = nextStateId });
		}

		/// <param name="trigger">遷移のきっかけとなる Task。</param>
		/// <param name="nextStateId">次状態のID。</param>
		public Transition(AsyncAction<int> trigger, int nextStateId)
		{
			Trigger = (option, ct) => trigger(option, ct).ContinueWith(_ => new TransitionOption { NextStateId = nextStateId });
		}

		/// <param name="trigger">遷移のきっかけとなる Task。次状態への引数を返す。</param>
		/// <param name="nextStateId">次状態のID。</param>
		public Transition(AsyncFunc<int> trigger, int nextStateId)
		{
			Trigger = (option, ct) => trigger(ct).ContinueWith(t => new TransitionOption { NextStateId = nextStateId, Option = t.Result });
		}

		/// <param name="trigger">遷移のきっかけとなる Task。次状態への引数を返す。</param>
		/// <param name="nextStateId">次状態のID。</param>
		public Transition(AsyncFunc<int, int> trigger, int nextStateId)
		{
			Trigger = (option, ct) => trigger(option, ct).ContinueWith(t => new TransitionOption { NextStateId = nextStateId, Option = t.Result });
		}

		internal Task<TransitionOption> AwaitTrigger(CancellationToken ct, int option)
		{
			return Trigger(option, ct);
		}
	}

	public class TransitionOption
	{
		public int NextStateId { get; set; }
		public int Option { get; set; }
	}
}
