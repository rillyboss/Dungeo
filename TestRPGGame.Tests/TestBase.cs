using System;
using Xunit;
using TestRPGGame.DataLoading;

namespace TestRPGGame.Tests
{
    /// <summary>
    /// Base class for all tests that ensures game data is loaded.
    /// All test classes should inherit from this to ensure data is available.
    /// </summary>
    public class TestBase : IDisposable
    {
        private static bool _dataLoaded = false;
        private static readonly object _lock = new object();

        public TestBase()
        {
            // Ensure data is loaded once for all tests
            lock (_lock)
            {
                if (!_dataLoaded)
                {
                    DataLoader.LoadAllData();
                    _dataLoaded = true;
                }
            }
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
