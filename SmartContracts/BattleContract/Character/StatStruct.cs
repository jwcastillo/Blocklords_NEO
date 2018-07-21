
namespace BattleContract.Character
{
    public struct Stat
    {
        public StatType statType;
        public byte[] Value;

        public Stat(StatType s, byte[] v)
        {
            statType = s;
            Value = v;
        }
    }
}
