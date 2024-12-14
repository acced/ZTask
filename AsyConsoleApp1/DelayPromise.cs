namespace AsyConsoleApp1;

    public sealed class DelayPromise : IZTaskSource
    {
        private Action<object> continuation;
        private object state;
        private ZTaskStatus status = ZTaskStatus.Pending;
        private readonly object gate = new object();
        private CancellationToken cancellationToken;
        private Timer timer;

        private readonly int totalDelay;

        private DelayPromise(int delayMilliseconds)
        {
            this.totalDelay = delayMilliseconds;
        }

        public static IZTaskSource Create(int delayMilliseconds, CancellationToken cancellationToken, bool cancelImmediately)
        {
            var promise = new DelayPromise(delayMilliseconds)
            {
                cancellationToken = cancellationToken
            };

            if (cancellationToken.IsCancellationRequested)
            {
                promise.SetCanceled();
                return promise;
            }

            // 设置一个定时器，在指定的延迟后调用 Complete 方法
            promise.timer = new Timer(_ => promise.Complete(), null, delayMilliseconds, Timeout.Infinite);

            // 注册取消回调
            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() => promise.SetCanceled());
            }

            return promise;
        }

        public ZTaskStatus GetStatus()
        {
            lock (gate)
            {
                return status;
            }
        }

        public void OnCompleted(Action<object> continuation, object state)
        {
            bool alreadyCompleted = false;
            lock (gate)
            {
                if (status != ZTaskStatus.Pending)
                {
                    alreadyCompleted = true;
                }
                else
                {
                    this.continuation = continuation;
                    this.state = state;
                }
            }

            if (alreadyCompleted)
            {
                // 如果任务已经完成，立即调用回调
                continuation(state);
            }
        }

        public void GetResult()
        {
            lock (gate)
            {
                switch (status)
                {
                    case ZTaskStatus.Succeeded:
                        return;
                    case ZTaskStatus.Faulted:
                        throw new InvalidOperationException("任务失败。");
                    case ZTaskStatus.Canceled:
                        throw new OperationCanceledException(cancellationToken);
                    case ZTaskStatus.Pending:
                        throw new InvalidOperationException("任务仍处于挂起状态。");
                    default:
                        throw new InvalidOperationException("未知任务状态。");
                }
            }
        }

        private void Complete()
        {
            Action<object> toInvoke = null;
            object toState = null;

            lock (gate)
            {
                if (status == ZTaskStatus.Pending)
                {
                    status = ZTaskStatus.Succeeded;
                    toInvoke = continuation;
                    toState = state;
                }
            }

            // 在锁外调用回调，防止死锁
            toInvoke?.Invoke(toState);
            timer?.Dispose();
        }

        private void SetCanceled()
        {
            Action<object> toInvoke = null;
            object toState = null;

            lock (gate)
            {
                if (status == ZTaskStatus.Pending)
                {
                    status = ZTaskStatus.Canceled;
                    toInvoke = continuation;
                    toState = state;
                }
            }

            // 在锁外调用回调，防止死锁
            toInvoke?.Invoke(toState);
            timer?.Dispose();
        }
    }