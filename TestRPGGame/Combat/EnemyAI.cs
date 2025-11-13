using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Entities.Player;
using TestRPGGame.DataLoading;
using TestRPGGame.Abilities.Effects;
using TestRPGGame.Abilities.Applicators;
using TestRPGGame.Utils;

namespace TestRPGGame.Combat
{
    /// <summary>
    /// AI decision engine for enemies. Evaluates combat state and selects optimal actions.
    /// Uses behavior patterns, boss phases, and context awareness for intelligent decisions.
    /// </summary>
    public class EnemyAI
    {

        // Memory system - tracks recent actions
        private Queue<string> recentPlayerActions = new Queue<string>(5);
        private Dictionary<string, int> playerAbilityUsage = new Dictionary<string, int>();
        private int turnsSinceLastHeal = 0;

        /// <summary>
        /// Current behavior controlling this enemy's decisions.
        /// </summary>
        public EnemyBehaviorData? CurrentBehavior { get; set; }

        /// <summary>
        /// Enemy phases (if this enemy has phases).
        /// </summary>
        public List<EnemyPhaseData>? Phases { get; set; }

        /// <summary>
        /// Index of the current active phase.
        /// </summary>
        private int currentPhaseIndex = 0;

        public EnemyAI(EnemyBehaviorData? behavior = null, List<EnemyPhaseData>? phases = null)
        {
            CurrentBehavior = behavior;
            Phases = phases;
        }

        /// <summary>
        /// Select the best ability for the enemy to use based on current combat state.
        /// Returns null if enemy should use basic attack.
        /// </summary>
        public EnemyAbility? SelectAbility(Enemy enemy, Player player)
        {
            // Check for phase transitions (bosses only)
            CheckPhaseTransition(enemy);

            // Get available abilities
            var availableAbilities = enemy.Abilities
                .Where(a => CanUseAbility(a, enemy))
                .ToList();

            if (availableAbilities.Count == 0)
            {
                return null; // Use basic attack
            }

            // Score each ability based on context
            var scoredAbilities = availableAbilities
                .Select(ability => new
                {
                    Ability = ability,
                    Score = ScoreAbility(ability, enemy, player)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            // Select ability with weighted randomness (best abilities more likely)
            double totalScore = scoredAbilities.Sum(x => x.Score);
            double roll = RandomProvider.NextDouble() * totalScore;
            double cumulative = 0;

            foreach (var scored in scoredAbilities)
            {
                cumulative += scored.Score;
                if (roll <= cumulative)
                {
                    return scored.Ability;
                }
            }

            // Fallback to highest scored ability
            return scoredAbilities.First().Ability;
        }

        /// <summary>
        /// Check if enemy can use this ability based on cooldown, HP threshold, and phase restrictions.
        /// </summary>
        private bool CanUseAbility(EnemyAbility ability, Enemy enemy)
        {
            // Check cooldown and HP threshold
            if (!ability.CanUse(enemy.CurrentHP, enemy.MaxHP))
            {
                return false;
            }

            // Check phase restrictions (bosses only)
            if (Phases != null && currentPhaseIndex < Phases.Count)
            {
                var phase = Phases[currentPhaseIndex];

                // If specific abilities are enabled, check if this one is
                if (phase.EnabledAbilities.Count > 0 &&
                    !phase.EnabledAbilities.Contains(ability.Ability.Name))
                {
                    return false;
                }

                // Check if this ability is disabled
                if (phase.DisabledAbilities.Contains(ability.Ability.Name))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Score an ability based on current combat context and AI behavior.
        /// Higher score = more likely to use.
        /// </summary>
        private double ScoreAbility(EnemyAbility ability, Enemy enemy, Player player)
        {
            double score = 1.0;

            if (CurrentBehavior == null)
            {
                return score; // No behavior = equal weighting
            }

            // Base score from ability type weights
            score *= GetAbilityTypeWeight(ability);

            // Apply contextual decision factors
            score *= GetContextualModifiers(ability, enemy, player);

            // Apply phase priority boosts (bosses only)
            if (Phases != null && currentPhaseIndex < Phases.Count)
            {
                var phase = Phases[currentPhaseIndex];
                if (phase.AbilityPriorityBoost.TryGetValue(ability.Ability.Name, out double boost))
                {
                    score *= boost;
                }
            }

            // Add some randomness (±20%)
            score *= (0.8 + RandomProvider.NextDouble() * 0.4);

            return Math.Max(score, 0.1); // Minimum score
        }

        /// <summary>
        /// Get the weight for this ability based on its effect types.
        /// </summary>
        private double GetAbilityTypeWeight(EnemyAbility ability)
        {
            if (CurrentBehavior == null || CurrentBehavior.AbilityWeights.Count == 0)
            {
                return 1.0;
            }

            double weight = 1.0;

            foreach (var effect in ability.Ability.Effects)
            {
                string effectType = GetEffectCategory(effect);
                if (CurrentBehavior.AbilityWeights.TryGetValue(effectType, out double typeWeight))
                {
                    weight *= typeWeight;
                }
            }

            return weight;
        }

        /// <summary>
        /// Categorize an effect for behavior weighting.
        /// </summary>
        private string GetEffectCategory(IAbilityEffect effect)
        {
            return effect switch
            {
                DamageEffect => "damage",
                RestoreEffect => "heal",
                LifeStealEffect => "damage",
                EffectApplicator => GetEffectApplicatorCategory((EffectApplicator)effect),
                _ => "other"
            };
        }

        /// <summary>
        /// Categorize an EffectApplicator based on its EffectKind.
        /// </summary>
        private string GetEffectApplicatorCategory(EffectApplicator effect)
        {
            return effect.EffectKind switch
            {
                Constants.EffectKind.AttackBoost => "buff",
                Constants.EffectKind.DefenseBoost => "defensive",
                Constants.EffectKind.SpeedBoost => "buff",
                Constants.EffectKind.DamageBoost => "buff",
                Constants.EffectKind.EvasionBoost => "defensive",
                Constants.EffectKind.AttackReduction => "control",
                Constants.EffectKind.DefenseReduction => "control",
                Constants.EffectKind.SpeedReduction => "control",
                Constants.EffectKind.AccuracyReduction => "control",
                Constants.EffectKind.Regeneration => "heal",
                Constants.EffectKind.Shield => "defensive",
                Constants.EffectKind.Stun => "control",
                Constants.EffectKind.DamageOverTime => "damage",
                Constants.EffectKind.Thorns => "defensive",
                Constants.EffectKind.LifeSteal => "damage",
                Constants.EffectKind.Dodge => "defensive",
                Constants.EffectKind.Composite => "buff",
                _ => "other"
            };
        }

        /// <summary>
        /// Apply contextual modifiers based on combat state.
        /// </summary>
        private double GetContextualModifiers(EnemyAbility ability, Enemy enemy, Player player)
        {
            if (CurrentBehavior == null || CurrentBehavior.DecisionFactors.Count == 0)
            {
                return 1.0;
            }

            double modifier = 1.0;

            int enemyHPPercent = (int)((enemy.CurrentHP / (double)enemy.MaxHP) * 100);
            int playerHPPercent = (int)((player.CurrentHP / (double)player.MaxHP) * 100);

            // Apply decision factors based on context
            if (CurrentBehavior.DecisionFactors.TryGetValue("playerLowHP", out double playerLowHPFactor))
            {
                if (playerHPPercent < 30)
                {
                    string effectType = GetEffectCategory(ability.Ability.Effects.FirstOrDefault());
                    if (effectType == "damage")
                    {
                        modifier *= playerLowHPFactor; // Finish them off!
                    }
                }
            }

            if (CurrentBehavior.DecisionFactors.TryGetValue("selfLowHP", out double selfLowHPFactor))
            {
                if (enemyHPPercent < 30)
                {
                    string effectType = GetEffectCategory(ability.Ability.Effects.FirstOrDefault());
                    if (effectType == "heal" || effectType == "defensive")
                    {
                        modifier *= selfLowHPFactor; // Prioritize survival
                    }
                }
            }

            if (CurrentBehavior.DecisionFactors.TryGetValue("playerHasBuffs", out double playerBuffsFactor))
            {
                if (player.Effects.GetBuffs().Count > 0)
                {
                    modifier *= playerBuffsFactor;
                }
            }

            // Avoid spamming heal abilities
            if (GetEffectCategory(ability.Ability.Effects.FirstOrDefault()) == "heal")
            {
                if (turnsSinceLastHeal < 3)
                {
                    modifier *= 0.3; // Heavily penalize recent healing
                }
                if (enemyHPPercent > 70)
                {
                    modifier *= 0.5; // Don't heal at high HP
                }
            }

            return modifier;
        }

        /// <summary>
        /// Check if boss should transition to a new phase.
        /// </summary>
        private void CheckPhaseTransition(Enemy enemy)
        {
            if (Phases == null || Phases.Count == 0)
            {
                return; // Not a boss
            }

            int hpPercent = (int)((enemy.CurrentHP / (double)enemy.MaxHP) * 100);

            // Check if we should move to next phase
            for (int i = currentPhaseIndex + 1; i < Phases.Count; i++)
            {
                var phase = Phases[i];
                if (hpPercent <= phase.HPThreshold && !phase.HasTriggered)
                {
                    // Transition to new phase
                    currentPhaseIndex = i;
                    phase.HasTriggered = true;

                    // Update current behavior if phase specifies one
                    if (!string.IsNullOrEmpty(phase.BehaviorId))
                    {
                        var behavior = DataLoader.GetBehavior(phase.BehaviorId);
                        if (behavior != null)
                        {
                            CurrentBehavior = behavior;
                        }
                    }

                    break; // Only one phase transition per check
                }
            }
        }

        /// <summary>
        /// Record a player action for AI memory/learning.
        /// </summary>
        public void RecordPlayerAction(string actionName)
        {
            recentPlayerActions.Enqueue(actionName);
            if (recentPlayerActions.Count > 5)
            {
                recentPlayerActions.Dequeue();
            }

            if (!playerAbilityUsage.ContainsKey(actionName))
            {
                playerAbilityUsage[actionName] = 0;
            }
            playerAbilityUsage[actionName]++;
        }

        /// <summary>
        /// Notify AI that enemy used a heal ability.
        /// </summary>
        public void RecordHealUsed()
        {
            turnsSinceLastHeal = 0;
        }

        /// <summary>
        /// Update turn counter for AI memory.
        /// </summary>
        public void OnTurnEnd()
        {
            turnsSinceLastHeal++;
        }

        /// <summary>
        /// Get the phase message for the current phase, if any.
        /// </summary>
        public string? GetCurrentPhaseMessage(string enemyName)
        {
            if (Phases == null || currentPhaseIndex >= Phases.Count)
            {
                return null;
            }

            var phase = Phases[currentPhaseIndex];
            if (!string.IsNullOrEmpty(phase.PhaseMessage) && phase.HasTriggered)
            {
                return phase.PhaseMessage.Replace("{name}", enemyName);
            }

            return null;
        }
    }
}
