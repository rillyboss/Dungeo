namespace TestRPGGame.Interfaces
{
    /// <summary>
    /// Logger that discards all messages. Useful for testing when you don't want console output.
    /// </summary>
    public class NullLogger : ILogger
    {
        public void LogInfo(string message)
        {
            // No-op
        }

        public void LogWarning(string message)
        {
            // No-op
        }

        public void LogError(string message)
        {
            // No-op
        }
    }
}
