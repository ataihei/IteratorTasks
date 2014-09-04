using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IteratorTasks
{
	//Task が終了しない問題あり。問題が解決するまで使用停止。
	//public static class StreamExtensions
	//{
	//	public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count)
	//	{
	//		return ReadAsync(stream, buffer, offset, count, Task.DefaultScheduler);
	//	}
	//	public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count, TaskScheduler scheduler)
	//	{
	//		var tcs = new TaskCompletionSource<int>(scheduler);
	//		var async = stream.BeginRead(buffer, 0, count, r =>
	//		{
	//			try
	//			{
	//				tcs.SetResult(stream.EndRead(r));
	//			}
	//			catch (Exception ex)
	//			{
	//				tcs.SetException(ex);
	//			}

	//		}, null);

	//		return tcs.Task;
	//	}

	//	public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count)
	//	{
	//		return WriteAsync(stream, buffer, offset, count, Task.DefaultScheduler);
	//	}
	//	public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count, TaskScheduler scheduler)
	//	{
	//		var tcs = new TaskCompletionSource<object>(scheduler);
	//		var async = stream.BeginWrite(buffer, offset, count, r =>
	//		{
	//			try
	//			{
	//				stream.EndWrite(r);
	//				tcs.SetResult(null);
	//			}
	//			catch (Exception ex)
	//			{
	//				tcs.SetException(ex);
	//			}
	//		}, null);

	//		return tcs.Task;
	//	}
	//}
}
