
namespace BattleContract.StorageData
{
    public struct ItemData
    {
        public int Index;
        public int Length;
        public ItemDataType parameterType;

        public ItemData(ItemDataType p, int i, int l)
        {
            parameterType = p;
            Index = i;
            Length = l;
        }
    }
}
