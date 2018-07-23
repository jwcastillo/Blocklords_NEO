using BattleContract.Character;
using BattleContract.GameComponents;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace BattleContract.StorageData
{
    public class ItemDataHelper
    {
        public static readonly int MaxItems = 5;
        public static readonly ItemData[] dataPositions = new ItemData[5]   // <-- 5 is MAXITEM
        {
            new ItemData(ItemDataType.Stat, 0, 4),
            new ItemData(ItemDataType.MaxStat, 4, 4),
            new ItemData(ItemDataType.StatType, 8, 1),
            new ItemData(ItemDataType.Quality, 9, 1),
            new ItemData(ItemDataType.Address, 10, 20)
        };

        public static int GetIndex(ItemDataType type)
        {
            int i = 0;  // index
            if (type.Equals(ItemDataType.Stat))
                i = 0;
            if (type.Equals(ItemDataType.MaxStat))
                i = 1;
            if (type.Equals(ItemDataType.StatType))
                i = 2;
            if (type.Equals(ItemDataType.Quality))
                i = 3;
            if (type.Equals(ItemDataType.Address))
                i = 4;
            return dataPositions[i].Index;
        }
        public static int GetLength(ItemDataType type)
        {
            int i = 0;  // index
            if (type.Equals(ItemDataType.Stat))
                i = 0;
            if (type.Equals(ItemDataType.MaxStat))
                i = 1;
            if (type.Equals(ItemDataType.StatType))
                i = 2;
            if (type.Equals(ItemDataType.Quality))
                i = 3;
            if (type.Equals(ItemDataType.Address))
                i = 4;
            return dataPositions[i].Length;
        }

        public static readonly int IdLength = 13;

        public static BigInteger GetValue(ItemDataType type, string parameters)
        {
            return new BigInteger(parameters.Substring(GetIndex(type), GetLength(type)).AsByteArray());
        }

        public static StatType GetStatType(string parameters)
        {
            ItemDataType type = ItemDataType.StatType;
            string param = parameters.Substring(GetIndex(type), GetLength(type));

            return Character.Helper.StringToStatType(param);
        }

        public static QualityType GetQualityType(string parameters)
        {
            ItemDataType type = ItemDataType.Quality;
            string param = parameters.Substring(GetIndex(type), GetLength(type));

            return GameComponents.Helper.StringToQualityType(param);
        }
        public static string GetAddress(string parameters)
        {
            ItemDataType type = ItemDataType.Address;
            string param = parameters.Substring(GetIndex(type), GetLength(type));

            return param;
        }

        // Parameters of ItemContract
        public static readonly int ItemDataLength = GetIndex(ItemDataType.Address)
            + GetLength(ItemDataType.Address);
        public static readonly int ItemIdLength = 13;
        
        public static void PutItem(Item item, byte[] owner)
        {
            StorageContext storageContext = BattleContract.GetItemContext();

            string parameters = StringAndArray.Helper.GetZeroPrefixedString(item.Stat.AsString(), 4) +
                StringAndArray.Helper.GetZeroPrefixedString(item.MaxStat.AsString(), 4) +
                GameComponents.Helper.QualityTypeToString(item.Quality) +
                Character.Helper.StatTypeToString(item.statType) +
                owner.AsString();

            Storage.Put(storageContext, item.Id.AsByteArray(), parameters);
        }
        public static Item GetItem(string itemId, byte[] address)
        {
            StorageContext storageContext = BattleContract.GetItemContext();
            string parameters = Storage.Get(storageContext, itemId.AsByteArray()).AsString();
            if (!parameters.Length.Equals(ItemDataHelper.ItemDataLength))
            {
                return new Item();
            }
            string itemOwner = ItemDataHelper.GetAddress(parameters);
            if (!itemOwner.Equals(address.AsString()))
            {
                return new Item();
            }

            Item item = new Item
            {
                Id = itemId,
                Stat = ItemDataHelper.GetValue(ItemDataType.Stat, parameters).AsByteArray(),
                MaxStat = ItemDataHelper.GetValue(ItemDataType.MaxStat, parameters).AsByteArray(),
                statType = ItemDataHelper.GetStatType(parameters),
                Quality = ItemDataHelper.GetQualityType(parameters)
            };
            return item;
        }
        public static Item[] GetItems(string[] ids, byte[] address)
        {
            Item[] items;
            int itemsNum = 5;
            // Decide How Many Items were Attached to Hero
            for (int i = 1; i < ids.Length; i++)
            {
                if (ids[i].Length.Equals(ItemIdLength))
                {
                    itemsNum = i + 1;
                }
            }

            // Return Item By Id
            items = new Item[itemsNum];
            for (int i = 0; i < itemsNum; i++)
            {
                Item item = GetItem(ids[i], address);
                if (item.Id == null)
                {
                    return null;
                }
                else
                {
                    items[i] = item;
                }
            }

            return items;
        }
    }
}
