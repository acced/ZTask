using System.Runtime.CompilerServices;

namespace AsyConsoleApp1;

    public class CustomAsyncMethodBuilder
    {
        private ZTaskCompletionSource _tcs;

        public CustomAsyncMethodBuilder()
        {
            Console.WriteLine("CustomAsyncMethodBuilder .ctor");
            _tcs = new ZTaskCompletionSource();
        }

        // 必需的 Create 方法
        public static CustomAsyncMethodBuilder Create() => new CustomAsyncMethodBuilder();

        // 必需的 Start 方法
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            Console.WriteLine("CustomAsyncMethodBuilder Start");
            stateMachine.MoveNext();
        }

        // 必需的 SetResult 方法
        public void SetResult()
        {
            Console.WriteLine("CustomAsyncMethodBuilder SetResult");
            _tcs.SetResult(null);
        }

        // 必需的 SetException 方法
        public void SetException(Exception exception)
        {
            Console.WriteLine($"CustomAsyncMethodBuilder SetException: {exception.Message}");
            //_tcs.SetException(exception);
        }

        // 必需的 AwaitOnCompleted 方法
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Console.WriteLine("CustomAsyncMethodBuilder AwaitOnCompleted");
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 必需的 AwaitUnsafeOnCompleted 方法
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter waiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Console.WriteLine("CustomAsyncMethodBuilder AwaitUnsafeOnCompleted");
            waiter.OnCompleted(stateMachine.MoveNext);
        }

        // 缺少的 SetStateMachine 方法
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // 通常不需要在这里执行任何操作
        }

        // 缺少的 Task 属性
        public ZTask Task => _tcs.Task;
    }

  public class ZTaskCompletionSource : IZTaskSource
    {
        private Action<object> continuation;
        private object state;
        private ZTaskStatus status = ZTaskStatus.Pending;
        private readonly object gate = new object();
        private CancellationToken cancellationToken;
        private Timer timer;

        public ZTask Task => new ZTask(this);

        public ZTaskStatus GetStatus() => status;

        public void OnCompleted(Action<object> continuation, object state)
        {
            lock (gate)
            {
                if (status == ZTaskStatus.Pending)
                {
                    this.continuation = continuation;
                    this.state = state;
                }
                else
                {
                    continuation(state);
                }
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
                    default:
                        throw new InvalidOperationException("任务状态无效");
                }
            }
        }

        public void SetResult(object state = null)
        {
            lock (gate)
            {
                status = ZTaskStatus.Succeeded;
                continuation?.Invoke(state);
            }
        }

        public void SetException(Exception exception)
        {
            lock (gate)
            {
                status = ZTaskStatus.Faulted;
            }
        }

        public void SetCanceled()
        {
            lock (gate)
            {
                status = ZTaskStatus.Canceled;
            }
        }
    }
