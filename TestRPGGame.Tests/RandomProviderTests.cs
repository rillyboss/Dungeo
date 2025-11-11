using Xunit;
using TestRPGGame.Utils;
using System.Collections.Generic;
using System.Linq;

namespace TestRPGGame.Tests
{
    public class RandomProviderTests : TestBase
    {
        [Fact]
        public void RandomProvider_Next_ReturnsValueInRange()
        {
            // Arrange
            int max = 10;

            // Act
            int result = RandomProvider.Next(max);

            // Assert
            Assert.InRange(result, 0, max - 1);
        }

        [Fact]
        public void RandomProvider_NextMinMax_ReturnsValueInRange()
        {
            // Arrange
            int min = 5;
            int max = 15;

            // Act
            int result = RandomProvider.Next(min, max);

            // Assert
            Assert.InRange(result, min, max - 1);
        }

        [Fact]
        public void RandomProvider_NextDouble_ReturnsValueBetweenZeroAndOne()
        {
            // Act
            double result = RandomProvider.NextDouble();

            // Assert
            Assert.InRange(result, 0.0, 1.0);
        }

        [Fact]
        public void RandomProvider_NextBool_ReturnsBooleanValue()
        {
            // Act
            bool result = RandomProvider.NextBool();

            // Assert
            Assert.True(result == true || result == false);
        }

        [Fact]
        public void RandomProvider_NextBoolWithProbability_RespectsZeroProbability()
        {
            // Arrange
            double probability = 0.0;

            // Act - Test multiple times to ensure consistency
            bool anyTrue = false;
            for (int i = 0; i < 100; i++)
            {
                if (RandomProvider.NextBool(probability))
                {
                    anyTrue = true;
                    break;
                }
            }

            // Assert
            Assert.False(anyTrue);
        }

        [Fact]
        public void RandomProvider_NextBoolWithProbability_RespectsOneProbability()
        {
            // Arrange
            double probability = 1.0;

            // Act - Test multiple times to ensure consistency
            bool allTrue = true;
            for (int i = 0; i < 100; i++)
            {
                if (!RandomProvider.NextBool(probability))
                {
                    allTrue = false;
                    break;
                }
            }

            // Assert
            Assert.True(allTrue);
        }

        [Fact]
        public void RandomProvider_Next_GeneratesVariedResults()
        {
            // Arrange
            int sampleSize = 1000;
            int max = 100;
            var results = new HashSet<int>();

            // Act
            for (int i = 0; i < sampleSize; i++)
            {
                results.Add(RandomProvider.Next(max));
            }

            // Assert - Should have generated many different values
            Assert.True(results.Count > 50, $"Expected varied results, got {results.Count} unique values");
        }

        [Fact]
        public void RandomProvider_NextDouble_GeneratesVariedResults()
        {
            // Arrange
            int sampleSize = 1000;
            var results = new HashSet<double>();

            // Act
            for (int i = 0; i < sampleSize; i++)
            {
                results.Add(RandomProvider.NextDouble());
            }

            // Assert - Should have generated many different values
            Assert.True(results.Count > 900, $"Expected varied results, got {results.Count} unique values");
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(5, 15)]
        [InlineData(-10, 10)]
        [InlineData(100, 200)]
        public void RandomProvider_NextMinMax_StaysWithinBounds(int min, int max)
        {
            // Arrange
            int iterations = 100;

            // Act & Assert
            for (int i = 0; i < iterations; i++)
            {
                int result = RandomProvider.Next(min, max);
                Assert.InRange(result, min, max - 1);
            }
        }

        [Fact]
        public void RandomProvider_NextBoolWithProbability_ApproximatesDistribution()
        {
            // Arrange
            double probability = 0.5;
            int sampleSize = 10000;
            int trueCount = 0;

            // Act
            for (int i = 0; i < sampleSize; i++)
            {
                if (RandomProvider.NextBool(probability))
                {
                    trueCount++;
                }
            }

            // Assert - With 50% probability, expect roughly 5000 trues (allow 10% margin)
            double actualProbability = (double)trueCount / sampleSize;
            Assert.InRange(actualProbability, 0.45, 0.55);
        }

        [Fact]
        public void RandomProvider_NextDouble_CoversFullRange()
        {
            // Arrange
            int sampleSize = 10000;
            bool hasLow = false;  // < 0.1
            bool hasMid = false;  // 0.4-0.6
            bool hasHigh = false; // > 0.9

            // Act
            for (int i = 0; i < sampleSize; i++)
            {
                double value = RandomProvider.NextDouble();
                if (value < 0.1) hasLow = true;
                if (value >= 0.4 && value <= 0.6) hasMid = true;
                if (value > 0.9) hasHigh = true;

                if (hasLow && hasMid && hasHigh) break;
            }

            // Assert - Should cover low, mid, and high ranges
            Assert.True(hasLow, "Should generate values < 0.1");
            Assert.True(hasMid, "Should generate values in 0.4-0.6");
            Assert.True(hasHigh, "Should generate values > 0.9");
        }

        [Fact]
        public void RandomProvider_IsThreadSafe_MultipleConcurrentCalls()
        {
            // Arrange
            int threadCount = 10;
            int callsPerThread = 1000;
            var results = new System.Collections.Concurrent.ConcurrentBag<int>();

            // Act
            System.Threading.Tasks.Parallel.For(0, threadCount, _ =>
            {
                for (int i = 0; i < callsPerThread; i++)
                {
                    results.Add(RandomProvider.Next(100));
                }
            });

            // Assert - Should complete without exceptions and generate varied results
            Assert.Equal(threadCount * callsPerThread, results.Count);
            Assert.True(results.Distinct().Count() > 50, "Should generate varied results even with concurrent access");
        }
    }
}
