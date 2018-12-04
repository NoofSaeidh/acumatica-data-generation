namespace DataGeneration.Core.Logging
{
    public interface IStopwatchLogger
    {
        IStopwatchLogger Log(string description, params object[] args);
        IStopwatchLogger Start();
        IStopwatchLogger Stop();
        IStopwatchLogger Reset();
        IStopwatchLogger Restart();
    }
}