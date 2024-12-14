using System.Runtime.CompilerServices;

namespace AsyConsoleApp1;


public enum ZTaskStatus
{
    /// <summary>The operation has not yet completed.</summary>
    Pending = 0,

    /// <summary>The operation completed successfully.</summary>
    Succeeded = 1,

    /// <summary>The operation completed with an error.</summary>
    Faulted = 2,

    /// <summary>The operation completed due to cancellation.</summary>
    Canceled = 3
}


[AsyncMethodBuilder(typeof(CustomAsyncMethodBuilder))]
public  struct ZTask(IZTaskSource source)
{
    // 实现 GetAwaiter 方法
    public ZTaskAwaiter GetAwaiter() => new ZTaskAwaiter(source);

    // 延迟任务
    public static ZTask Delay(int millisecondsDelay, CancellationToken cancellationToken = default, bool cancelImmediately = false)
    {
        if (millisecondsDelay < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(millisecondsDelay), "Delay 不允许负延迟时间。");
        }
        var source = DelayPromise.Create(millisecondsDelay, cancellationToken, cancelImmediately);
        return new ZTask(source);
    }
}