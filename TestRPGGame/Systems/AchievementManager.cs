using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TestRPGGame.Entities.Player;
using TestRPGGame.Interfaces;
using TestRPGGame.Systems.Conditions;

namespace TestRPGGame.Systems
{
    /// <summary>
    /// Manages achievement tracking, unlocking, and rewards.
    /// Follows same pattern as ProgressionManager and DungeonManager.
    /// </summary>
    public class AchievementManager
    {
        private readonly IGameInterface _gameInterface;
        private readonly List<Achievement> _achievements;
        private readonly HashSet<string> _unlockedAchievementIds;
        private readonly string _achievementsPath;

        public IReadOnlyList<Achievement> Achievements => _achievements.AsReadOnly();
        public int TotalPoints => _achievements.Sum(a => a.Points);
        public int EarnedPoints => _achievements.Where(a => a.IsUnlocked).Sum(a => a.Points);
        public double CompletionPercentage => _achievements.Count > 0
            ? (double)_achievements.Count(a => a.IsUnlocked) / _achievements.Count * 100
            : 0;

        public AchievementManager(IGameInterface gameInterface, string? customDataPath = null)
        {
            _gameInterface = gameInterface;
            _achievements = new List<Achievement>();
            _unlockedAchievementIds = new HashSet<string>();

            // Use custom path for testing, otherwise use default
            if (customDataPath != null)
            {
                _achievementsPath = customDataPath;
            }
            else
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                _achievementsPath = Path.Combine(baseDirectory, "Data", "achievements.json");
            }

            LoadAchievements();
        }

        /// <summary>
        /// Loads achievements from JSON and initializes unlock status.
        /// </summary>
        private void LoadAchievements()
        {
            try
            {
                if (!File.Exists(_achievementsPath))
                {
                    _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = $"Achievements file not found at: {_achievementsPath}",
                        Type = GameEvents.MessageType.Warning
                    });
                    return;
                }

                var json = File.ReadAllText(_achievementsPath);
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("achievements", out var achievementsArray))
                    return;

                foreach (var achievementElement in achievementsArray.EnumerateArray())
                {
                    var achievement = ParseAchievement(achievementElement);
                    if (achievement != null)
                    {
                        _achievements.Add(achievement);
                    }
                }

                _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = $"Loaded {_achievements.Count} achievements",
                    Type = GameEvents.MessageType.Info
                });
            }
            catch (Exception ex)
            {
                _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = $"Error loading achievements: {ex.Message}",
                    Type = GameEvents.MessageType.Error
                });
            }
        }

        /// <summary>
        /// Parses an achievement from JSON element.
        /// </summary>
        private Achievement? ParseAchievement(JsonElement element)
        {
            try
            {
                var achievement = new Achievement
                {
                    Id = element.GetProperty("id").GetString() ?? string.Empty,
                    Name = element.GetProperty("name").GetString() ?? string.Empty,
                    Description = element.GetProperty("description").GetString() ?? string.Empty,
                    Category = element.GetProperty("category").GetString() ?? string.Empty,
                    Points = element.TryGetProperty("points", out var points) ? points.GetInt32() : 10
                };

                // Parse rewards
                if (element.TryGetProperty("goldReward", out var goldReward))
                    achievement.GoldReward = goldReward.GetInt32();
                if (element.TryGetProperty("experienceReward", out var expReward))
                    achievement.ExperienceReward = expReward.GetInt32();
                if (element.TryGetProperty("titleReward", out var titleReward))
                    achievement.TitleReward = titleReward.GetString();
                if (element.TryGetProperty("isHidden", out var hidden))
                    achievement.IsHidden = hidden.GetBoolean();

                // Parse condition
                achievement.Condition = ParseCondition(element);

                return achievement;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses condition from JSON element based on conditionType.
        /// </summary>
        private IAchievementCondition? ParseCondition(JsonElement element)
        {
            if (!element.TryGetProperty("conditionType", out var conditionType))
                return null;

            var type = conditionType.GetString();

            return type switch
            {
                "StatThreshold" => new StatThresholdCondition
                {
                    StatName = element.GetProperty("statName").GetString() ?? string.Empty,
                    Threshold = element.GetProperty("threshold").GetInt32(),
                    DisplayName = element.TryGetProperty("displayName", out var displayName)
                        ? displayName.GetString() ?? string.Empty
                        : string.Empty
                },
                "KillEnemyType" => new KillEnemyTypeCondition
                {
                    EnemyType = element.GetProperty("enemyType").GetString() ?? string.Empty,
                    RequiredKills = element.GetProperty("requiredKills").GetInt32()
                },
                "DungeonCompletion" => new DungeonCompletionCondition
                {
                    DungeonName = element.TryGetProperty("dungeonName", out var dungeonName)
                        ? dungeonName.GetString()
                        : null,
                    RequiredCompletions = element.TryGetProperty("requiredCompletions", out var required)
                        ? required.GetInt32()
                        : 1
                },
                _ => null
            };
        }

        /// <summary>
        /// Checks all locked achievements and unlocks any that meet their conditions.
        /// Should be called after significant game events.
        /// </summary>
        public void CheckAchievements(PlayerStatistics statistics, Player player)
        {
            var lockedAchievements = _achievements.Where(a => !a.IsUnlocked).ToList();

            foreach (var achievement in lockedAchievements)
            {
                if (achievement.CheckCondition(statistics))
                {
                    UnlockAchievement(achievement, player);
                }
            }
        }

        /// <summary>
        /// Unlocks an achievement, awards rewards, and publishes event.
        /// </summary>
        private void UnlockAchievement(Achievement achievement, Player player)
        {
            achievement.Unlock();
            _unlockedAchievementIds.Add(achievement.Id);

            // Award rewards
            if (achievement.GoldReward > 0)
            {
                player.Gold += achievement.GoldReward;
            }

            if (achievement.ExperienceReward > 0)
            {
                player.GainExperience(achievement.ExperienceReward);
            }

            // Publish unlock event
            _gameInterface.OnEvent(new GameEvents.AchievementUnlockedEvent
            {
                AchievementId = achievement.Id,
                Name = achievement.Name,
                Description = achievement.Description,
                Category = achievement.Category,
                GoldReward = achievement.GoldReward,
                ExperienceReward = achievement.ExperienceReward,
                TitleReward = achievement.TitleReward,
                Points = achievement.Points
            });

            // Show notification
            var rewardText = BuildRewardText(achievement);
            _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
            {
                Message = $"\n🏆 ACHIEVEMENT UNLOCKED: {achievement.Name}\n{achievement.Description}\n{rewardText}",
                Type = GameEvents.MessageType.Success
            });
        }

        /// <summary>
        /// Builds reward text for achievement unlock notification.
        /// </summary>
        private string BuildRewardText(Achievement achievement)
        {
            var rewards = new List<string>();

            if (achievement.GoldReward > 0)
                rewards.Add($"+{achievement.GoldReward} gold");
            if (achievement.ExperienceReward > 0)
                rewards.Add($"+{achievement.ExperienceReward} XP");
            if (!string.IsNullOrEmpty(achievement.TitleReward))
                rewards.Add($"Title: {achievement.TitleReward}");
            if (achievement.Points > 0)
                rewards.Add($"{achievement.Points} points");

            return rewards.Count > 0 ? $"Rewards: {string.Join(", ", rewards)}" : string.Empty;
        }

        /// <summary>
        /// Gets achievements by category.
        /// </summary>
        public List<Achievement> GetAchievementsByCategory(string category)
        {
            return _achievements.Where(a => a.Category == category).ToList();
        }

        /// <summary>
        /// Gets all categories present in achievements.
        /// </summary>
        public List<string> GetCategories()
        {
            return _achievements.Select(a => a.Category).Distinct().OrderBy(c => c).ToList();
        }

        /// <summary>
        /// Restores unlock status from save data.
        /// </summary>
        public void RestoreUnlockStatus(HashSet<string> unlockedIds)
        {
            _unlockedAchievementIds.Clear();

            foreach (var id in unlockedIds)
            {
                var achievement = _achievements.FirstOrDefault(a => a.Id == id);
                if (achievement != null && !achievement.IsUnlocked)
                {
                    achievement.IsUnlocked = true;
                    _unlockedAchievementIds.Add(id);
                }
            }
        }

        /// <summary>
        /// Gets the set of unlocked achievement IDs for saving.
        /// </summary>
        public HashSet<string> GetUnlockedAchievementIds()
        {
            return new HashSet<string>(_unlockedAchievementIds);
        }
    }
}
