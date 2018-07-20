
using Neo.SmartContract.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BattleContract
{
    enum ItemParameterType
    {
        Stat = 0,
        MaxStat = 1,
        StatType = 2,
        Quality = 3,
        Address = 4
    }

    class ItemParameter
    {
        public int Index;
        public int Length;
        public ItemParameterType parameterType;

        public ItemParameter(ItemParameterType p, int i, int l)
        {
            parameterType = p;
            Index = i;
            Length = l;
        }
    }

    class ItemParameters
    {

        public static readonly ItemParameter[] Parameters = new ItemParameter[5]
        {
            new ItemParameter(ItemParameterType.Stat, 0, 4),
            new ItemParameter(ItemParameterType.MaxStat, 4, 4),
            new ItemParameter(ItemParameterType.StatType, 8, 1),
            new ItemParameter(ItemParameterType.Quality, 9, 1),
            new ItemParameter(ItemParameterType.Address, 10, 20)
        };

        public static int GetIndex(ItemParameterType type)
        {
            int i = 0;  // index
            if (type.Equals(ItemParameterType.Stat))
                i = 0;
            if (type.Equals(ItemParameterType.MaxStat))
                i = 1;
            if (type.Equals(ItemParameterType.StatType))
                i = 2;
            if (type.Equals(ItemParameterType.Quality))
                i = 3;
            if (type.Equals(ItemParameterType.Address))
                i = 4;
            return Parameters[i].Index;
        }
        public static int GetLength(ItemParameterType type)
        {
            int i = 0;  // index
            if (type.Equals(ItemParameterType.Stat))
                i = 0;
            if (type.Equals(ItemParameterType.MaxStat))
                i = 1;
            if (type.Equals(ItemParameterType.StatType))
                i = 2;
            if (type.Equals(ItemParameterType.Quality))
                i = 3;
            if (type.Equals(ItemParameterType.Address))
                i = 4;
            return Parameters[i].Length;
        }

        public static readonly int IdLength = 13;

        public static BigInteger GetValue(ItemParameterType type, string parameters)
        {
            return new BigInteger(parameters.Substring(GetIndex(type), GetLength(type)).AsByteArray());
        }

        public static StatType GetStat(string parameters)
        {
            ItemParameterType type = ItemParameterType.StatType;
            string param = parameters.Substring(GetIndex(type), GetLength(type));

            return Stat.StringToEnum(param);
        }
    }
}
