using System;
using System.IO;
using Xunit;
using TestRPGGame.Interfaces;
using TestRPGGame.DataLoading;
using TestRPGGame.Systems;
using TestRPGGame.Entities.Enemy;

namespace TestRPGGame.Tests
{
    /// <summary>
    /// Tests for ILogger implementations and their integration with system classes.
    /// </summary>
    public class LoggerTests
    {
        [Fact]
        public void NullLogger_DoesNotThrow_OnLogInfo()
        {
            var logger = new NullLogger();
            var exception = Record.Exception(() => logger.LogInfo("Test message"));
            Assert.Null(exception);
        }

        [Fact]
        public void NullLogger_DoesNotThrow_OnLogWarning()
        {
            var logger = new NullLogger();
            var exception = Record.Exception(() => logger.LogWarning("Test warning"));
            Assert.Null(exception);
        }

        [Fact]
        public void NullLogger_DoesNotThrow_OnLogError()
        {
            var logger = new NullLogger();
            var exception = Record.Exception(() => logger.LogError("Test error"));
            Assert.Null(exception);
        }

        [Fact]
        public void ConsoleLogger_DoesNotThrow_OnLogInfo()
        {
            var logger = new ConsoleLogger();
            var exception = Record.Exception(() => logger.LogInfo("Test message"));
            Assert.Null(exception);
        }

        [Fact]
        public void ConsoleLogger_DoesNotThrow_OnLogWarning()
        {
            var logger = new ConsoleLogger();
            var exception = Record.Exception(() => logger.LogWarning("Test warning"));
            Assert.Null(exception);
        }

        [Fact]
        public void ConsoleLogger_DoesNotThrow_OnLogError()
        {
            var logger = new ConsoleLogger();
            var exception = Record.Exception(() => logger.LogError("Test error"));
            Assert.Null(exception);
        }

        [Fact]
        public void ConsoleLogger_WritesToConsole_OnLogInfo()
        {
            var logger = new ConsoleLogger();
            var originalOut = Console.Out;

            try
            {
                using (var writer = new StringWriter())
                {
                    Console.SetOut(writer);
                    logger.LogInfo("Test message");

                    var output = writer.ToString();
                    Assert.Contains("Test message", output);
                }
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void ConsoleLogger_WritesToConsole_OnLogWarning()
        {
            var logger = new ConsoleLogger();
            var originalOut = Console.Out;

            try
            {
                using (var writer = new StringWriter())
                {
                    Console.SetOut(writer);
                    logger.LogWarning("Test warning");

                    var output = writer.ToString();
                    Assert.Contains("Warning:", output);
                    Assert.Contains("Test warning", output);
                }
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void ConsoleLogger_WritesToConsole_OnLogError()
        {
            var logger = new ConsoleLogger();
            var originalOut = Console.Out;

            try
            {
                using (var writer = new StringWriter())
                {
                    Console.SetOut(writer);
                    logger.LogError("Test error");

                    var output = writer.ToString();
                    Assert.Contains("ERROR:", output);
                    Assert.Contains("Test error", output);
                }
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void DataLoader_SetLogger_AcceptsNullLogger()
        {
            var exception = Record.Exception(() => DataLoader.SetLogger(new NullLogger()));
            Assert.Null(exception);
        }

        [Fact]
        public void DataLoader_SetLogger_AcceptsConsoleLogger()
        {
            var exception = Record.Exception(() => DataLoader.SetLogger(new ConsoleLogger()));
            Assert.Null(exception);
        }

        [Fact]
        public void GameConfig_SetLogger_AcceptsNullLogger()
        {
            var exception = Record.Exception(() => GameConfig.SetLogger(new NullLogger()));
            Assert.Null(exception);
        }

        [Fact]
        public void GameConfig_SetLogger_AcceptsConsoleLogger()
        {
            var exception = Record.Exception(() => GameConfig.SetLogger(new ConsoleLogger()));
            Assert.Null(exception);
        }

        [Fact]
        public void EnemyFactory_SetLogger_AcceptsNullLogger()
        {
            var exception = Record.Exception(() => EnemyFactory.SetLogger(new NullLogger()));
            Assert.Null(exception);
        }

        [Fact]
        public void EnemyFactory_SetLogger_AcceptsConsoleLogger()
        {
            var exception = Record.Exception(() => EnemyFactory.SetLogger(new ConsoleLogger()));
            Assert.Null(exception);
        }

        [Fact]
        public void DataLoader_LoadAllData_UsesConfiguredLogger()
        {
            // Set NullLogger to suppress output during test
            DataLoader.SetLogger(new NullLogger());

            // LoadAllData should not throw and should use the configured logger
            var exception = Record.Exception(() => DataLoader.LoadAllData());
            Assert.Null(exception);

            // Restore default logger
            DataLoader.SetLogger(new ConsoleLogger());
        }

        [Fact]
        public void GameConfig_LoadConfig_UsesConfiguredLogger()
        {
            // Set NullLogger to suppress output during test
            GameConfig.SetLogger(new NullLogger());

            // Access Config to trigger LoadConfig
            var exception = Record.Exception(() => { var config = GameConfig.Config; });
            Assert.Null(exception);

            // Restore default logger
            GameConfig.SetLogger(new ConsoleLogger());
        }

        [Fact]
        public void NullLogger_SuppressesOutput_ForDataLoader()
        {
            var originalOut = Console.Out;

            try
            {
                using (var writer = new StringWriter())
                {
                    Console.SetOut(writer);

                    // Set NullLogger
                    DataLoader.SetLogger(new NullLogger());
                    DataLoader.LoadAllData();

                    var output = writer.ToString();
                    // NullLogger should not write anything to console
                    Assert.Equal("", output.Trim());

                    // Restore default logger
                    DataLoader.SetLogger(new ConsoleLogger());
                }
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}
