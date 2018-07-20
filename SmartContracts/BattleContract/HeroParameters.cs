
using Neo.SmartContract.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BattleContract
{
    enum HeroParameterType
    {
        Leadership = 0,
        Strength = 1,
        Speed = 2,
        Intelligence = 3,
        Defence = 4,
        Nation = 5,
        Class = 6,
        OptionalData = 7,
        Address = 8
    }

    class HeroParameter
    {
        public int Index;
        public int Length;
        public HeroParameterType parameterType;

        public HeroParameter(HeroParameterType p, int i, int l)
        {
            parameterType = p;
            Index = i;
            Length = l;
        }
    }

    class HeroParameters
    {

        public static readonly HeroParameter[] Parameters = new HeroParameter[9]
        {
            new HeroParameter(HeroParameterType.Leadership, 0, 4),
            new HeroParameter(HeroParameterType.Strength, 4, 4),
            new HeroParameter(HeroParameterType.Speed, 8, 4),
            new HeroParameter(HeroParameterType.Intelligence, 12, 4),
            new HeroParameter(HeroParameterType.Defence, 16, 4),
            new HeroParameter(HeroParameterType.Nation, 20, 1),
            new HeroParameter(HeroParameterType.Class, 0, 1),
            new HeroParameter(HeroParameterType.OptionalData, 0, 1),
            new HeroParameter(HeroParameterType.Address, 0, 20)
        };

        public static int GetIndex(HeroParameterType type)
        {
            int i = 0;  // index
            if (type == HeroParameterType.Leadership)
                i = 0;
            if (type == HeroParameterType.Strength)
                i = 1;
            if (type == HeroParameterType.Speed)
                i = 2;
            if (type == HeroParameterType.Intelligence)
                i = 3;
            if (type == HeroParameterType.Defence)
                i = 4;
            if (type == HeroParameterType.Nation)
                i = 5;
            if (type == HeroParameterType.Class)
                i = 6;
            if (type == HeroParameterType.OptionalData)
                i = 7;
            if (type == HeroParameterType.Address)
                i = 8;
            return Parameters[i].Index;
        }
        public static int GetLength(HeroParameterType type)
        {
            int i = 0;  // index
            if (type == HeroParameterType.Leadership)
                i = 0;
            if (type == HeroParameterType.Strength)
                i = 1;
            if (type == HeroParameterType.Speed)
                i = 2;
            if (type == HeroParameterType.Intelligence)
                i = 3;
            if (type == HeroParameterType.Defence)
                i = 4;
            if (type == HeroParameterType.Nation)
                i = 5;
            if (type == HeroParameterType.Class)
                i = 6;
            if (type == HeroParameterType.OptionalData)
                i = 7;
            if (type == HeroParameterType.Address)
                i = 8;
            return Parameters[i].Length;
        }


        public static readonly int IdLength = 13;

        public static BigInteger GetValue(HeroParameterType type, string parameters)
        {
            return new BigInteger(parameters.Substring(GetIndex(type), GetLength(type)).AsByteArray());
        }

        public static ClassType GetClass(string parameters)
        {
            HeroParameterType type = HeroParameterType.Class;
            string param = parameters.Substring(GetIndex(type), GetLength(type));

            return Class.StringToEnum(param);
        }
    }
}
