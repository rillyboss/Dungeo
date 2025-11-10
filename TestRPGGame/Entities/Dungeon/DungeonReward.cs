namespace TestRPGGame.Entities.Dungeon
{
    public class DungeonReward
    {
        public int GoldMin { get; set; }
        public int GoldMax { get; set; }
        public int Experience { get; set; }
        public int GuaranteedLootCount { get; set; }
        public int MinLootRarity { get; set; } // 0=Common, 1=Uncommon, 2=Rare, 3=Epic, 4=Legendary
    }
}
