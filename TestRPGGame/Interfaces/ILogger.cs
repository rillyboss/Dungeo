namespace TestRPGGame.Interfaces
{
    /// <summary>
    /// Interface for logging diagnostic and error messages.
    /// Enables testability by allowing different logging implementations (console, null, file, etc.)
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an informational message (e.g., "Loaded 10 abilities")
        /// </summary>
        void LogInfo(string message);

        /// <summary>
        /// Logs a warning message (e.g., "Failed to load optional data")
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Logs an error message (e.g., "Critical failure loading data")
        /// </summary>
        void LogError(string message);
    }
}
