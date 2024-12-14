namespace AsyConsoleApp1;

public interface IZTaskSource
{
    ZTaskStatus GetStatus();
    void OnCompleted(Action<object> continuation, object state);
    void GetResult();
}