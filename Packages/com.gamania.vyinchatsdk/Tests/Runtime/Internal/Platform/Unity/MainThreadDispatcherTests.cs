using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VyinChatSdk.Internal.Platform;

namespace VyinChatSdk.Tests.Runtime.Platform.Unity
{
    public class MainThreadDispatcherTests
    {
        [SetUp]
        public void SetUp()
        {
            MainThreadDispatcher.ClearQueue();
        }

        [UnityTest]
        public IEnumerator Enqueue_ExecutesActionOnMainThread()
        {
            bool actionExecuted = false;
            int? callbackThreadId = null;
            int mainThreadId = Thread.CurrentThread.ManagedThreadId;

            MainThreadDispatcher.Enqueue(() =>
            {
                actionExecuted = true;
                callbackThreadId = Thread.CurrentThread.ManagedThreadId;
            });

            yield return null;

            Assert.IsTrue(actionExecuted, "Action was not executed");
            Assert.AreEqual(mainThreadId, callbackThreadId, "Action was not executed on main thread");
        }

        [UnityTest]
        public IEnumerator Enqueue_ExecutesMultipleActionsInOrder()
        {
            var executionOrder = new List<int>();

            MainThreadDispatcher.Enqueue(() => executionOrder.Add(1));
            MainThreadDispatcher.Enqueue(() => executionOrder.Add(2));
            MainThreadDispatcher.Enqueue(() => executionOrder.Add(3));

            yield return null;

            Assert.AreEqual(3, executionOrder.Count);
            Assert.AreEqual(1, executionOrder[0]);
            Assert.AreEqual(2, executionOrder[1]);
            Assert.AreEqual(3, executionOrder[2]);
        }

        [UnityTest]
        public IEnumerator Enqueue_HandlesExceptionGracefully()
        {
            bool secondActionExecuted = false;

            MainThreadDispatcher.Enqueue(() => throw new Exception("Test exception"));
            MainThreadDispatcher.Enqueue(() => secondActionExecuted = true);

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*Test exception.*"));

            yield return null;

            Assert.IsTrue(secondActionExecuted, "Second action should still execute after first action throws");
        }

        [UnityTest]
        public IEnumerator Enqueue_FromBackgroundThread_ExecutesOnMainThread()
        {
            bool actionExecuted = false;
            int? callbackThreadId = null;
            int? backgroundThreadId = null;
            int mainThreadId = Thread.CurrentThread.ManagedThreadId;

            Task.Run(() =>
            {
                backgroundThreadId = Thread.CurrentThread.ManagedThreadId;
                MainThreadDispatcher.Enqueue(() =>
                {
                    actionExecuted = true;
                    callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                });
            });

            float timeout = 1f;
            while (!actionExecuted && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(actionExecuted, "Action was not executed");
            Assert.AreNotEqual(backgroundThreadId, callbackThreadId, "Background thread and callback thread should be different");
            Assert.AreEqual(mainThreadId, callbackThreadId, "Callback should execute on main thread");
        }

        [UnityTest]
        public IEnumerator Enqueue_NullAction_DoesNotThrow()
        {
            MainThreadDispatcher.Enqueue(null);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Enqueue_MultipleBackgroundThreads_AllExecuteOnMainThread()
        {
            int executionCount = 0;
            int mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var threadIds = new List<int>();
            object lockObj = new object();

            for (int i = 0; i < 5; i++)
            {
                Task.Run(() =>
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        lock (lockObj)
                        {
                            executionCount++;
                            threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                        }
                    });
                });
            }

            float timeout = 2f;
            while (executionCount < 5 && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.AreEqual(5, executionCount, "All 5 actions should execute");
            foreach (var threadId in threadIds)
            {
                Assert.AreEqual(mainThreadId, threadId, "All actions should execute on main thread");
            }
        }

        [UnityTest]
        public IEnumerator Instance_CreatesSingletonCorrectly()
        {
            var instance1 = MainThreadDispatcher.Instance;
            var instance2 = MainThreadDispatcher.Instance;

            yield return null;

            Assert.AreSame(instance1, instance2, "Should return same instance");

            var dispatchers = GameObject.FindObjectsOfType<MainThreadDispatcher>();
            Assert.AreEqual(1, dispatchers.Length, "Should only create one MainThreadDispatcher GameObject");
            Assert.AreEqual("VyinChatMainThreadDispatcher", instance1.gameObject.name, "GameObject should have correct name");
        }
    }
}
