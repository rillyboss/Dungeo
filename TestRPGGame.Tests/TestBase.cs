using System;
using Xunit;
using TestRPGGame.DataLoading;
using TestRPGGame.Systems;

namespace TestRPGGame.Tests
{
    /// <summary>
    /// Base class for all tests that ensures game data is loaded and config is reset.
    /// All test classes should inherit from this to ensure consistent test environment.
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

            // Reset config to defaults for each test to ensure isolation
            // Tests can override with GameConfig.SetConfig() if needed
            GameConfig.ResetConfig();
        }

        public void Dispose()
        {
            // Reset config after test completes
            GameConfig.ResetConfig();
        }
    }
}
