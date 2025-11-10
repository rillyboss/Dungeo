using System.Collections.Generic;
using PlayerEntity = TestRPGGame.Entities.Player.Player;

namespace TestRPGGame.Entities.Dungeon
{
    public class DungeonRequirements
    {
        public int MinLevel { get; set; }
        public int GoldCost { get; set; }
        public string? PreviousDungeonRequired { get; set; }

        public bool MeetsRequirements(PlayerEntity player, Dictionary<string, bool> completedDungeons)
        {
            if (player.Level < MinLevel) return false;
            if (player.Gold < GoldCost) return false;
            if (!string.IsNullOrEmpty(PreviousDungeonRequired) &&
                !completedDungeons.GetValueOrDefault(PreviousDungeonRequired, false))
            {
                return false;
            }
            return true;
        }
    }
}
