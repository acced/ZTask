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
        public static CustomAsyncMethodBuilder Create()
            => new CustomAsyncMethodBuilder();

        // 必需的 Start 方法
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
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
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Console.WriteLine("CustomAsyncMethodBuilder AwaitOnCompleted");
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        // 必需的 AwaitUnsafeOnCompleted 方法
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            Console.WriteLine("CustomAsyncMethodBuilder AwaitUnsafeOnCompleted");
            awaiter.OnCompleted(stateMachine.MoveNext);
        }


        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // 通常不需要在这里执行任何操作
        }


        public ZTask Task => _tcs.Task;
    }

   public class ZTaskCompletionSource : IZTaskSource
   {
       CancellationToken cancellationToken;
       Action<object> singleContinuation;
       List<(Action<object>, object)> secondaryContinuationList;

       int intStatus; // ZTaskStatus
       bool handled = false;

       public ZTaskCompletionSource()
       {

       }

       internal void MarkHandled()
       {
           if (!handled)
           {
               handled = true;

           }
       }

       public ZTask Task
       {
           get { return new ZTask(this); }
       }


       public void GetResult()
       {
           MarkHandled();

           var status = (ZTaskStatus)intStatus;
           switch (status)
           {
               case ZTaskStatus.Succeeded:
                   return;
               case ZTaskStatus.Faulted:
                   //exception.GetException().Throw();
                   return;
               case ZTaskStatus.Canceled:
                   throw new OperationCanceledException(cancellationToken);
               default:
               case ZTaskStatus.Pending:
                   //throw new InvalidOperationException("not yet completed.");
                   return;
           }
       }

       public ZTaskStatus GetStatus()
       {
           return (ZTaskStatus)intStatus;
       }




       public void OnCompleted(Action<object> continuation, object state)
       {
           continuation(state);
       }


       public void SetResult(object o)
       {

       }
   }