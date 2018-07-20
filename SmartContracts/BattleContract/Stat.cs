using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BattleContract
{
    public enum StatType
    {
        Leadership = 0,     // Head,
        Defense = 1,        // Body, Shield,
        Speed = 2,          // Hands
        Intelligence = 3, 
        Strength = 4        // Weapon
    }

    class Stat
    {
        public StatType statType;
        public BigInteger value;

        public Stat(){        }
        public Stat(StatType s, BigInteger v)
        {
            statType = s;
            value = v;
        }

        public static StatType StringToEnum(string value)
        {
            StatType statType = StatType.Defense;

            if (value.Equals("0"))
            {
                statType = StatType.Leadership;
            }
            if (value.Equals("1"))
            {
                statType = StatType.Defense;
            }
            if (value.Equals("2"))
            {
                statType = StatType.Speed;
            }
            if (value.Equals("4"))
            {
                statType = StatType.Strength;
            }
            return statType;
        }
        public static string EnumToString(StatType statType)
        {
            string value = "0";

            if (statType.Equals(StatType.Leadership))
            {
                value = "0";
            }
            if (statType.Equals(StatType.Defense))
            {
                value = "1";
            }
            if (statType.Equals(StatType.Speed))
            {
                value = "2";
            }
            if (statType.Equals(StatType.Strength))
            {
                value = "4";
            }
            return value;
        }
    }
}
