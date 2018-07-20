using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BattleContract
{
    class Hero
    {
        public Stat[] Stats = new Stat[5];

        public string Id;
        public ClassType Class;
        public int Troops;
        public Item[] Items;
        public bool IsNPC;

        public byte[] Owner;

        public static BigInteger CalculateStat(StatType statType, Hero hero)
        {
            BigInteger stat = hero.GetStat(statType).value;

            for(int i=0; i< hero.Items.Length; i++)
            {
                if (hero.Items[i].statType.Equals(statType))
                {
                    stat = stat + hero.Items[i].Stat;
                }
            }

            return stat;
        }

        public Stat GetStat(StatType statType)
        {
            for (int i = 0; i<Stats.Length; i++)
            {
                if (Stats[i].statType.Equals(statType))
                {
                    return Stats[i];
                }
            }

            return new Stat();
        }
    }
}
