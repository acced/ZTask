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
public struct ZTask(IZTaskSource source)
{
    readonly IZTaskSource source = source;

    // 实现 GetAwaiter 方法
    public ZTaskAwaiter GetAwaiter()
    {
        return new ZTaskAwaiter(source);
    }

    public static ZTask Delay(int millisecondsDelay, CancellationToken cancellationToken = default,
        bool cancelImmediately = false)
    {
        if (millisecondsDelay < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(millisecondsDelay),
                "Delay does not allow negative delay time.");
        }

        var source = DelayPromise.Create(millisecondsDelay, cancellationToken, cancelImmediately);
        return new ZTask(source);
    }
}