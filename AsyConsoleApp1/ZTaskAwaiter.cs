using System.Runtime.CompilerServices;

namespace AsyConsoleApp1;

public readonly struct ZTaskAwaiter(IZTaskSource task) : INotifyCompletion
{
    // 表示任务是否已完成
    public bool IsCompleted
    {
        get
        {
            Console.WriteLine(task.GetStatus());
            return task.GetStatus() != ZTaskStatus.Pending;
        }
    }

    // 注册回调，当任务完成时调用
    public void OnCompleted(Action continuation)
    {
        // 这里使用一个状态对象，可以传递给 OnCompleted 方法
        task.OnCompleted(state =>
        {
            if (state is Action action)
            {
                action();
            }
        }, continuation);
    }

    // 获取任务结果（如果有）
    public void GetResult()
    {
        task.GetResult();
    }
}