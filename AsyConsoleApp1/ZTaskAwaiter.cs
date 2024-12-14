using System.Runtime.CompilerServices;

namespace AsyConsoleApp1;

public readonly struct ZTaskAwaiter : INotifyCompletion
{
    private readonly IZTaskSource task;

    public ZTaskAwaiter(IZTaskSource task)
    {
        this.task = task;
    }

    // 表示任务是否已完成
    public bool IsCompleted
    {
        get
        {
            var status = task.GetStatus();
            Console.WriteLine($"Task status: {status}");
            return status != ZTaskStatus.Pending;
        }
    }

    // 注册回调，当任务完成时调用
    // 注册回调，回调函数会在任务完成时执行
    public void OnCompleted(Action continuation)
    {
        var context = SynchronizationContext.Current;

        task.OnCompleted(state =>
        {
            // 确保回调在主线程或 UI 线程中执行
            if (context != null)
            {
                context.Post(_ => continuation(), null);
            }
            else
            {
                continuation();
            }
        }, null);
    }

    // 获取任务结果（如果有）
    public void GetResult() => task.GetResult();
}