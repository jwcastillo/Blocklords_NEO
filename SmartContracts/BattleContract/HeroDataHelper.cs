using BattleContract.Character;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

namespace BattleContract.StorageData
{
    class HeroDataHelper
    {
        public static readonly HeroData[] dataPositions = new HeroData[9]
        {
            new HeroData(HeroDataType.Leadership, 0, 4),
            new HeroData(HeroDataType.Strength, 4, 4),
            new HeroData(HeroDataType.Speed, 8, 4),
            new HeroData(HeroDataType.Intelligence, 12, 4),
            new HeroData(HeroDataType.Defence, 16, 4),
            new HeroData(HeroDataType.Nation, 20, 1),
            new HeroData(HeroDataType.Class, 0, 1),
            new HeroData(HeroDataType.OptionalData, 0, 1),
            new HeroData(HeroDataType.Address, 0, 20)
        };

        public static int GetIndex(HeroDataType type)
        {
            int i = 0;  // index
            if (type == HeroDataType.Leadership)
                i = 0;
            if (type == HeroDataType.Strength)
                i = 1;
            if (type == HeroDataType.Speed)
                i = 2;
            if (type == HeroDataType.Intelligence)
                i = 3;
            if (type == HeroDataType.Defence)
                i = 4;
            if (type == HeroDataType.Nation)
                i = 5;
            if (type == HeroDataType.Class)
                i = 6;
            if (type == HeroDataType.OptionalData)
                i = 7;
            if (type == HeroDataType.Address)
                i = 8;
            return dataPositions[i].Index;
        }
        public static int GetLength(HeroDataType type)
        {
            int i = 0;  // index
            if (type == HeroDataType.Leadership)
                i = 0;
            if (type == HeroDataType.Strength)
                i = 1;
            if (type == HeroDataType.Speed)
                i = 2;
            if (type == HeroDataType.Intelligence)
                i = 3;
            if (type == HeroDataType.Defence)
                i = 4;
            if (type == HeroDataType.Nation)
                i = 5;
            if (type == HeroDataType.Class)
                i = 6;
            if (type == HeroDataType.OptionalData)
                i = 7;
            if (type == HeroDataType.Address)
                i = 8;
            return dataPositions[i].Length;
        }


        public static readonly int IdLength = 13;

        public static byte[] GetValue(HeroDataType type, string parameters)
        {
            return parameters.Substring(GetIndex(type), GetLength(type)).AsByteArray();
        }

        public static ClassType GetClass(string parameters)
        {
            HeroDataType type = HeroDataType.Class;
            string param = parameters.Substring(GetIndex(type), GetLength(type));

            return Character.Helper.StringToClassType(param);
        }

        public static string GetAddress(string parameters)
        {
            HeroDataType type = HeroDataType.Address;
            return parameters.Substring(GetIndex(type), GetLength(type));
        }

        public static readonly int HeroDataLength = GetIndex(HeroDataType.Address)
            + GetLength(HeroDataType.Address);

        public static Hero GetHeroData(string heroId, byte[] address)
        {
            StorageContext storageContext = BattleContract.GetHeroContext();
            string parameters = Storage.Get(storageContext, heroId.AsByteArray()).AsString();
            if (!parameters.Length.Equals(HeroDataHelper.HeroDataLength))
            {
                return new Hero();
            }

            string heroOwner = HeroDataHelper.GetAddress(parameters);
            if (!heroOwner.Equals(address.AsString()))
            {
                return new Hero();
            }

            Hero hero = new Hero
            {
                Id = heroId,
                Stats = new Stat[5]{
                    new Stat(StatType.Leadership, HeroDataHelper.GetValue(HeroDataType.Leadership, parameters)),
                    new Stat(StatType.Strength, HeroDataHelper.GetValue(HeroDataType.Strength, parameters)),
                    new Stat(StatType.Speed, HeroDataHelper.GetValue(HeroDataType.Speed, parameters)),
                    new Stat(StatType.Intelligence, HeroDataHelper.GetValue(HeroDataType.Intelligence, parameters)),
                    new Stat(StatType.Defense, HeroDataHelper.GetValue(HeroDataType.Defence, parameters))
                },
                Class = HeroDataHelper.GetClass(parameters)

            };

            return hero;
        }
    }
}
