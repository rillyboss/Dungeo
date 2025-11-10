using System;
using TestRPGGame.Entities.Player;

namespace TestRPGGame.Systems
{
    public class SaveSlotInfo
    {
        public int SlotNumber { get; set; }
        public string Name { get; set; } = "";
        public PlayerClass Class { get; set; }
        public int Level { get; set; }
        public DateTime SaveTime { get; set; }
        public bool IsEmpty { get; set; }
    }
}
