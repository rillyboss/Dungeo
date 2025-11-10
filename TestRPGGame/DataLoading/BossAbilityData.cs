using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class BossAbilityData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Cooldown { get; set; }

        // Support multiple effects per ability
        public List<BossAbilityEffectData> Effects { get; set; } = new();

        // Legacy single effect support for backward compatibility
        public BossAbilityEffectData? Effect
        {
            get => Effects.Count > 0 ? Effects[0] : null;
            set
            {
                if (value != null)
                {
                    if (Effects.Count == 0)
                        Effects.Add(value);
                    else
                        Effects[0] = value;
                }
            }
        }
    }
}
