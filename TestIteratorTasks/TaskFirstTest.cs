using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using IteratorTasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;


namespace TestIteratorTasks
{
    [TestClass]
    public class TaskFirstTest
    {
        private int?[] iteratorCount = new int?[3];

        [TestMethod()]
        public void First後にContinueWithでフレームが進むかどうか()
        {
            int?[] continueWithCount = new int?[3];
            int? completedCount = null;
            int? runCompletedCount = null;

            var t0 = Task.Run(TestIteratorZero);
            var t1 = Task.Run(TestIteratorOne);
            var t2 = Task.Run(TestIteratorTwo);

            // yield return の後のUpdateCountの値を確かめる。
            TaskEx.First(t0).ContinueWith(_ => continueWithCount[0] = Task.DefaultScheduler.UpdateCount);
            TaskEx.First(t1).ContinueWith(_ => continueWithCount[1] = Task.DefaultScheduler.UpdateCount);
            TaskEx.First(t2).ContinueWith(_ => continueWithCount[2] = Task.DefaultScheduler.UpdateCount);

            // CompletedTaskのcontinueWithは、Updatecountを挟まず処理できる。
            TaskEx.First(Task.CompletedTask).ContinueWith(_ => completedCount = Task.DefaultScheduler.UpdateCount);

            // Task.Run(CompletedTask)の場合でも、UpdateCountを挟まず処理できる。
            TaskEx.First(Task.Run(Task.CompletedTask)).ContinueWith(_ => runCompletedCount = Task.DefaultScheduler.UpdateCount);


            Task.DefaultScheduler.Update(10);

            Assert.AreEqual(iteratorCount[0], continueWithCount[0]);
            Assert.AreEqual(iteratorCount[1], continueWithCount[1]);
            Assert.AreEqual(iteratorCount[2], continueWithCount[2]);
            Assert.AreEqual(0, completedCount);
            Assert.AreEqual(0, runCompletedCount);
        }
        private IEnumerator TestIteratorZero()
        {
            // yield return より前の値が正しいか確認のテスト。
            Assert.AreEqual(0, Task.DefaultScheduler.UpdateCount);
            iteratorCount[0] = Task.DefaultScheduler.UpdateCount;
            yield break;
        }

        private IEnumerator TestIteratorOne()
        {
            Assert.AreEqual(0, Task.DefaultScheduler.UpdateCount);
            yield return null;

            iteratorCount[1] = Task.DefaultScheduler.UpdateCount;
        }

        private IEnumerator TestIteratorTwo()
        {
            Assert.AreEqual(0, Task.DefaultScheduler.UpdateCount);
            yield return null;

            Assert.AreEqual(1, Task.DefaultScheduler.UpdateCount);
            yield return null;

            iteratorCount[2] = Task.DefaultScheduler.UpdateCount;
        }

    }
}
