namespace AsyConsoleApp1;

public sealed class DelayPromise : IZTaskSource
    {
        private Action<object> continuation;
        private object state;
        private ZTaskStatus status = ZTaskStatus.Pending;
        private CancellationToken cancellationToken;
        private CancellationTokenRegistration cancellationTokenRegistration;

        private Timer timer;
        private Timer progressTimer;
        private readonly int totalDelay;
        private int elapsedDelay;
        private readonly Action<float> progressCallback;

        private DelayPromise(int delayMilliseconds, Action<float> progressCallback)
        {
            this.totalDelay = delayMilliseconds;
            this.progressCallback = progressCallback;
        }

        public static IZTaskSource Create(int delayMilliseconds, CancellationToken cancellationToken,
            bool cancelImmediately, Action<float> progressCallback = null)
        {
            var promise = new DelayPromise(delayMilliseconds, progressCallback)
            {
                cancellationToken = cancellationToken
            };

            if (cancellationToken.IsCancellationRequested)
            {
                promise.SetCanceled();
                return promise;
            }

            // 设置一个计时器，在指定的延迟后调用 Complete 方法
            promise.timer = new Timer(_ => { promise.Complete(); }, null, delayMilliseconds, Timeout.Infinite);

            // 注册取消回调
            if (cancellationToken.CanBeCanceled)
            {
                promise.cancellationTokenRegistration = cancellationToken.Register(() =>
                {
                    promise.timer?.Dispose();
                    promise.SetCanceled();
                });
            }

            // 启动进度报告计时器
            if (progressCallback != null)
            {
                promise.progressTimer = new Timer(state =>
                {
                    Interlocked.Add(ref promise.elapsedDelay, 100);
                    float progress = Math.Min((float)promise.elapsedDelay / promise.totalDelay, 1f);
                    promise.progressCallback?.Invoke(progress);
                }, null, 0, 100); // 每100毫秒报告一次进度
            }

            return promise;
        }

        public ZTaskStatus GetStatus()
        {
            
                return status;
            
        }

        public void OnCompleted(Action<object> continuation, object state)
        {
            bool alreadyCompleted = false;
           
                if (status != ZTaskStatus.Pending)
                {
                    alreadyCompleted = true;
                }
                else
                {
                    this.continuation = continuation;
                    this.state = state;
                }
            

            if (alreadyCompleted)
            {
                // 如果任务已经完成，立即调用回调
                continuation(state);
            }
        }

        public void GetResult()
        {
            
                switch (status)
                {
                    case ZTaskStatus.Succeeded:
                        return;
                    case ZTaskStatus.Faulted:
                        throw new InvalidOperationException("Task Faulted.");
                    case ZTaskStatus.Canceled:
                        throw new OperationCanceledException(cancellationToken);
                    case ZTaskStatus.Pending:
                        throw new InvalidOperationException("Task is still pending.");
                    default:
                        throw new InvalidOperationException("Unknown task status.");
                }
            
        }

        private void Complete()
        {
            Action<object> toInvoke = null;
            object toState = null;

            
                if (status == ZTaskStatus.Pending)
                {
                    status = ZTaskStatus.Succeeded;
                    toInvoke = continuation;
                    toState = state;
                }
            

            // 在锁外调用回调，防止死锁
            if (toInvoke != null)
            {
                Console.WriteLine("Task completed successfully.");
                toInvoke.Invoke(toState);
            }

            // 释放资源
            timer?.Dispose();
            progressTimer?.Dispose();
            cancellationTokenRegistration.Dispose();
        }

        private void SetCanceled()
        {
            Action<object> toInvoke = null;
            object toState = null;

           
            if (status == ZTaskStatus.Pending)
            {
                    status = ZTaskStatus.Canceled;
                    toInvoke = continuation;
                    toState = state;
            }
            

            // 在锁外调用回调，防止死锁
            if (toInvoke != null)
            {
                Console.WriteLine("Task was canceled.");
                toInvoke.Invoke(toState);
            }

            // 释放资源
            timer?.Dispose();
            progressTimer?.Dispose();
            cancellationTokenRegistration.Dispose();
        }
    }