using BattleContract.GameComponents;

namespace BattleContract.Character
{
    public struct Hero
    {
        public Stat[] Stats;

        public string Id;
        public ClassType Class;
        public int Troops;
        public Item[] Items;
        public bool IsNPC;

        public byte[] Owner;
    }
}
