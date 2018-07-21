using System.Numerics;

namespace BattleContract.Character
{
    public class Helper
    {
        public static BigInteger GetStatValueByType(StatType statType, Hero hero)
        {
            BigInteger stat = new BigInteger(GetStat(hero, statType).Value);

            for (int i = 0; i < hero.Items.Length; i++)
            {
                if (hero.Items[i].statType.Equals(statType))
                {
                    stat = stat + new BigInteger(hero.Items[i].Stat);
                }
            }

            return stat;
        }
        private static Stat GetStat(Hero hero, StatType statType)
        {
            for (int i = 0; i < hero.Stats.Length; i++)
            {
                if (hero.Stats[i].statType.Equals(statType))
                {
                    return hero.Stats[i];
                }
            }

            return new Stat();
        }

        public static int GetAdvantage(ClassType my, ClassType enemy)
        {
            if (my.Equals(ClassType.Rider))
            {
                if (enemy.Equals(ClassType.Archer)) return 2;
            }
            if (my.Equals(ClassType.Archer))
            {
                if (enemy.Equals(ClassType.Soldier)) return 2;
            }
            if (my.Equals(ClassType.Soldier))
            {
                if (enemy.Equals(ClassType.Rider)) return 2;
            }
            return 1;
        }
        public static ClassType StringToClassType(string value)
        {
            ClassType classType = ClassType.Archer;

            if (value.Equals("0"))
            {
                classType = ClassType.Rider;
            }
            if (value.Equals("1"))
            {
                classType = ClassType.Archer;
            }
            if (value.Equals("2"))
            {
                classType = ClassType.Soldier;
            }
            return classType;
        }
        public static string ClassTypeToString(ClassType classType)
        {
            string value = "0";

            if (classType.Equals(ClassType.Rider))
            {
                value = "0";
            }
            if (classType.Equals(ClassType.Archer))
            {
                value = "1";
            }
            if (classType.Equals(ClassType.Soldier))
            {
                value = "2";
            }
            return value;
        }

        public static StatType StringToStatType(string value)
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
        public static string StatTypeToString(StatType statType)
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
