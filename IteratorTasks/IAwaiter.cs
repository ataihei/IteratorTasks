using System;

namespace IteratorTasks
{
	public interface IAwaiter
	{
		bool IsCompleted { get; }
		void OnCompleted(Action continuation);
	}
}
