using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleContract
{
    enum ClassType
    {
        Rider = 0,
        Archer = 1,
        Soldier = 2
    }
    class Class
    {
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

        public static ClassType StringToEnum(string value)
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

        public static string EnumToString(ClassType classType)
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
    }
}
