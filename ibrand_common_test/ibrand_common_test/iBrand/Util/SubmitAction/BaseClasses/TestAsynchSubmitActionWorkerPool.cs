using System;
using System.Threading;
using NLog;
using NomadLogic.iBrandDesktop;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Util.SubmitAction.BaseClasses
{
    internal class SubmitActionMock : AbstractSubmitAction
    {
        protected override void InstanciateSubmitActionPools()
        {
            // rien;
        }

        public override bool SubmitRespondent(Respondent respondent)
        {
            return true;
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePoll = true)
        {
            return null;
        }

        public override void TransformNotReadyToSyncRespondents(CancellationToken? ct)
        {
            // Rien
        }
    }

    internal class AsynchSubmitActionWorker : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected int _id;
        protected SubmitActionMock _submitAction;
        protected static readonly object Lock = new object();
        protected int _waitTime;
        protected SubmitActionWorkerPool _pool;

        public AsynchSubmitActionWorker(int id, SubmitActionMock submitAction, int waitTime,
            SubmitActionWorkerPool secondPool)
        {
            _id = id;
            _submitAction = submitAction;
            _waitTime = waitTime;
            _pool = secondPool;
            Finished = false;
        }

        public bool Finished
        {
            get;
            protected set;
        }

        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            log.Debug("Worker (" + _id  + ") currently executing step 1");
            Thread.Sleep((int)(_waitTime * 3 * (new Random().NextDouble())));

            var workerStep2 = new AsynchSubmitActionWorkerStep2(_id, _submitAction, _waitTime);

            _pool.AddWorker(workerStep2);

            Finished = true;
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }

        public ISubmitAction GetSubmitAction()
        {
            return _submitAction;
        }

        public int GetRespId()
        {
            return _id;
        }
    }

    internal class AsynchSubmitActionWorkerStep2 : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected int _id;
        protected SubmitActionMock _submitAction;
        protected static readonly object Lock = new object();
        protected int _waitTime;

        public AsynchSubmitActionWorkerStep2(int id, SubmitActionMock submitAction, int waitTime)
        {
            _id = id;
            _submitAction = submitAction;
            _waitTime = waitTime;
            Finished = false;
        }

        public bool Finished
        {
            get;
            protected set;
        }

        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            for (var i = 0; i < 10; i++)
            {
                log.Debug("Worker (" + _id + ") currently executing step 2." + i);
                Thread.Yield();
                Thread.Sleep((int)(_waitTime * (new Random().NextDouble())));
            }

            Finished = true;
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }

        public ISubmitAction GetSubmitAction()
        {
            return _submitAction;
        }

        public int GetRespId()
        {
            return _id;
        }
    }

    public class TestAsynchSubmitActionWorkerPool : TestBaseNunit
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        [Test]
        public void TestAsychPool()
        {
            const int nbrWorker = 30;

            var pool = new SubmitActionWorkerPool
            {
                MaxWorkerThread = 1,
                PoolName = "First"
            };

            var pool2 = new SubmitActionWorkerPool
            {
                MaxWorkerThread = 10,
                PoolName = "Second"
            };

            SubmitActionWorkerPool.StartPoolInstance(pool);
            SubmitActionWorkerPool.StartPoolInstance(pool2);

            while (!(pool.IsRunning() && pool2.IsRunning())) {
                log.Debug("Waiting for pools to start.");
                Thread.Sleep(200);
            }

            var mock = new SubmitActionMock();

            for (var i = 0; i < nbrWorker; i++)
            {
                pool.AddWorker(new AsynchSubmitActionWorker(i, mock, 600, pool2));
            }

            Assert.IsTrue(pool.GetQueueSize() > 0);

            while (pool.GetQueueSize() > 0 || pool2.GetQueueSize() > 0) {
                log.Debug("Waiting for all workers...");
                Thread.Sleep(5000);
            }

            pool.Stop();
            pool2.Stop();

            log.Debug("Asynch test for worker pools finished.");
        }
    }
}
