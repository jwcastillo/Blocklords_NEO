
using BattleContract.Character;
using BattleContract.GameComponents;
using BattleContract.Math;
using BattleContract.StorageData;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace BattleContract.Battle
{
    public class Helper
    {
        private static readonly int XP_1 = 500, XP_2 = 100;


        public static BattleType GetBattleType(bool isMyNpc, bool isEnemyNpc)
        {
            if (isMyNpc) return BattleType.PVE;
            if (isEnemyNpc) return BattleType.PVE;
            return BattleType.PVP;
        }

        

        public static TotalRate Create(ItemIncreasing[] Increasings, Range idsRange)
        {
            TotalRate totalRate = new TotalRate
            {
                Rates = new Range[Math.Helper.GetLength(idsRange)],
                Total = 0,
                Ids = Math.Helper.AsArray(idsRange)
            };

            for (int i = 0, rangeMin = 1, idsNum = Math.Helper.GetLength(idsRange); i < idsNum; i++)
            {
                totalRate.Rates[i] = new Range(rangeMin, Increasings[totalRate.Ids[i]].Rate);
                rangeMin = Increasings[totalRate.Ids[i]].Rate + 1;    // The Min of Next Rate is the next number after the current rates Max

                totalRate.Total = totalRate.Total + totalRate.Rates[i].Max;
            }

            return totalRate;
        }

        public static int GetIdByRate(TotalRate TotalRate, int rate)
        {
            for (int i = 0; i < TotalRate.Rates.Length; i++)
            {
                if (Math.Helper.InRange(TotalRate.Rates[i], rate))
                {
                    return TotalRate.Ids[i];
                }
            }

            return 0;
        }


        public static BigInteger DamageCalculation(BigInteger myStrength, BigInteger myDefense, BigInteger mySpeed, BigInteger enemyDefense, BigInteger enemySpeed, int advantage, int cityDefence = 1)
        {
            BigInteger damage = myStrength * (1 - myDefense / (Battle.Helper.XP_1 + enemyDefense)) * (mySpeed / (enemySpeed + Battle.Helper.XP_2)) * advantage * (1 - cityDefence);
            return damage;
        }

        public static BigInteger CalculateDamage(Hero my, Hero enemy)
        {
            // Get the Hero Parameters
            // Get the Item Parameters
            BigInteger myStrength = Character.Helper.GetStatValueByType(StatType.Strength, my);
            BigInteger myDefense = Character.Helper.GetStatValueByType(StatType.Defense, my);
            BigInteger mySpeed = Character.Helper.GetStatValueByType(StatType.Speed, my);
            BigInteger enemyDefense = Character.Helper.GetStatValueByType(StatType.Defense, enemy);
            BigInteger enemySpeed = Character.Helper.GetStatValueByType(StatType.Speed, enemy);
            int advantage = Character.Helper.GetAdvantage(my.Class, enemy.Class);

            return DamageCalculation(myStrength, myDefense, mySpeed, enemyDefense, enemySpeed, advantage);
        }

        public static BigInteger AcceptDamage(BigInteger damage, int troops)
        {
            BigInteger remainedTroops = new BigInteger(troops) - damage;
            if (troops < 0) return 0;

            return remainedTroops;
        }
        /**
         * Return Result:
         * 0 - no one win or lose
         * 1 - win first hero
         * 2 - win second hero
         * 3 - both of the heroes lose
         */
        public static BattleResult CalculateBattleResult(BigInteger myTroops, BigInteger myRemained, BigInteger enemyTroops, BigInteger enemyRemained)
        {
            BigInteger myPercent = myTroops / 100;
            BigInteger myRemainedPercents = myRemained / myPercent;

            BigInteger enemyPercent = myTroops / 100;
            BigInteger enemyRemainedPercents = enemyRemained / enemyPercent;

            if (enemyRemainedPercents < 30)
            {
                if (myRemainedPercents >= 30)
                {
                    return BattleResult.MY_WIN;
                }
                else
                {
                    return BattleResult.BOTH_LOSE;
                }
            }
            if (myRemainedPercents < 30)
            {
                if (enemyRemainedPercents >= 30)
                {
                    return BattleResult.ENEMY_WIN;
                }
                else
                {
                    return BattleResult.BOTH_LOSE;
                }
            }

            if (myRemainedPercents.Equals(enemyRemainedPercents))
            {
                return BattleResult.BOTH_LOSE;
            }

            if (myRemainedPercents > enemyRemainedPercents)
            {
                return BattleResult.MY_WIN;
            }
            // else
            return BattleResult.ENEMY_WIN;
        }

        


        public static int GetIncreaseValue(ItemIncreasing[] increasingItems, Item item, BattleType battleType)
        {
            Range ids = GetBattleTypeRange(battleType);
            ids = GetQualityTypeRange(ids, item.Quality);
            ids = GetStatTypeRange(ids, item.statType);
            TotalRate rates = Battle.Helper.Create(increasingItems, ids);       // Build the total weight for each ids

            int randomRate = Math.Helper.GetRandomNumber(rates.Total); // Call GetRandomNumber(rates.total);
            int id = Battle.Helper.GetIdByRate(rates, randomRate);              // Returns assigned ID for rate

            return increasingItems[id].Increasing;
        }
        public static void IncreaseStat(Item item, int increaseValue, byte[] owner)
        {
            BigInteger stat = item.Stat.AsBigInteger() + increaseValue;
            BigInteger maxStat = item.MaxStat.AsBigInteger();

            if (stat > maxStat)
            {
                Runtime.Log("over_max_stat_value");
                if (!item.Stat.Equals(item.MaxStat))
                    return;
                Runtime.Log("Set to Maximum");
                item.Stat = item.MaxStat;
            }
            else
            {
                item.Stat = stat.AsByteArray();
            }

            ItemDataHelper.PutItem(item, owner);
        }

        private static Range GetBattleTypeRange(BattleType battleType)
        {
            if (battleType.Equals(BattleType.PVP))
            {
                return new Range(1, 75);
            }
            return new Range(76, 150);
        }
        private static Range GetQualityTypeRange(Range range, QualityType qualityType)
        {
            int qualityRangeDistance = 25;
            int quality = 0;

            if (qualityType == QualityType.SSR)
            {
                quality = 0;
            }
            else if (qualityType == QualityType.SR)
            {
                quality = 1;
            }
            else
            {
                quality = 2;
            }

            Range newRange = range;
            newRange.Min = range.Min + (quality * qualityRangeDistance);
            newRange.Max = newRange.Min + qualityRangeDistance - 1;
            return newRange;
        }
        private static Range GetStatTypeRange(Range range, StatType statType)
        {
            int statRangeDistance = 5;
            int stat = 0;

            if (statType.Equals(StatType.Leadership))
            {
                stat = 0;
            }
            else if (statType.Equals(StatType.Defense))
            {
                stat = 1;
            }
            else if (statType.Equals(StatType.Speed))
            {
                stat = 2;
            }
            else if (statType.Equals(StatType.Strength))
            {
                stat = 3;
            }
            else
            {
                stat = 4;
            }
            Range newRange = range;
            newRange.Min = range.Min + (stat * statRangeDistance);
            newRange.Max = newRange.Min + statRangeDistance - 1;        // -1 at the end is added by newRange.Min itself. Which is always equal to 1
            return newRange;
        }

    }
}
