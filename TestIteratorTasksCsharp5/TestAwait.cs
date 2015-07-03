using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using TT = System.Threading.Tasks;
using IteratorTasks;
using System.Collections;

namespace TestIteratorTasksCsharp5
{
    [TestClass]
    public class TestAwait
    {
        private bool _isDone;
        private Thread _updateThread;

        [TestInitialize]
        public void Init()
        {
            _isDone = false;
            _updateThread = new Thread(UpdateLoop);
            _updateThread.Start(Task.DefaultScheduler);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _isDone = true;
            _updateThread.Join();
        }

        private void UpdateLoop(object state)
        {
            var scheduler = (TaskScheduler)state;

            while (!_isDone)
            {
                try
                {
                    var delayMilliseconds = scheduler.IsActive ? 5 : 50;
                    try
                    {
                        Thread.Sleep(delayMilliseconds);
                    }
                    catch (TaskCanceledException) { }
                    // ↑Delay なし、専用スレッドで回りっぱなしとかがいいかもしれないし。

                    scheduler.Update();
                }
                catch (Exception)
                {
                }
            }
        }

        private const int N = 10;

        [TestMethod]
        public void AsyncAwaitIsAvailable()
        {
            var t = TestAsync();
            t.Wait();
            var x = t.Result;
            Assert.AreEqual(N, x.Length);

            for (int i = 0; i < N; i++)
            {
                Assert.AreEqual(i * 5, x[i]);
            }
        }

        private async TT.Task<int[]> TestAsync()
        {
            var x = new int[N];
            for (int i = 0; i < N; i++)
            {
                x[i] = await TestAsync(i);
            }
            return x;
        }

        private async TT.Task<int> TestAsync(int i)
        {
            await Task.Delay(10);
            var x = await Task.Run<int>(TestIterator);
            return x * i;
        }

        private IEnumerator TestIterator(Action<int> callback)
        {
            var i = 1;
            yield return null;
            ++i;
            yield return null;
            ++i;
            yield return null;
            ++i;
            yield return null;
            ++i;
            callback(i);
        }
    }
}
