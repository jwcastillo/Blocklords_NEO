using BattleContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

/**
 *  Battle of the Blocklords game.
 *  
 *  Version: 1.0
 *  Author: Medet Ahmetson
 *
 */

namespace Blocklords
{
    class BattleContract : SmartContract
    {
        private static readonly int XP_1 = 500, XP_2 = 100;

        private static readonly int MY_WIN = 0,
            ENEMY_WIN = 1, BOTH_LOSE = 2;

        private static readonly int itemIdLength = 13;

        private static readonly int heroParametersLength = HeroParameters.GetIndex(HeroParameterType.Address)
            + HeroParameters.GetLength(HeroParameterType.Address);

        // Parameters of ItemContract
        private static readonly int itemParametersLength = ItemParameters.GetIndex(ItemParameterType.Address)
            + ItemParameters.GetLength(ItemParameterType.Address);

        [Appcall("1231231")]
        public static extern StorageContext GetItemContext();

        [Appcall("1231231")]
        public static extern StorageContext GetHeroContext();


        private static byte[] GetFalseByte()
        {
            return new BigInteger(0).AsByteArray();
        }
        private static byte[] GetTrueByte()
        {
            return new BigInteger(1).AsByteArray();
        }

        public static byte[] Main(string operation, object[] args)
        {

            /*if (operation == "put" || 
                  operation == "update" ||
                  operation == "setItemState" || 
                  operation == "transfer" )
            {
                if (!Runtime.CheckWitness((byte[])args[0]))
                {
                    Runtime.Log("Authorization failed!");
                    return GetFalseByte();
                }
            }*/
            // @Param Owner Address, Attacker ID, Attacker Items List, Defender Address, Defender ID, Defender Items List
            if (operation == "attackHero")
            {
                Hero attacker, defender;

                if (!args.Length.Equals(17)) return GetFalseByte();
                if (!IsValidHeroId((string)args[0])) return GetFalseByte();
                if (!IsValidHeroId((string)args[7])) return GetFalseByte();

                // Returns Hero Information: @Id, @parameter caller
                attacker = GetHeroData((string)args[0], (byte[])args[14]);
                if (attacker == null) return GetFalseByte();
                attacker.Troops = (int)args[1];

                string[] attackerItemIds = new string[]
                {
                    (string)args[2],
                    (string)args[3],
                    (string)args[4],
                    (string)args[5],
                    (string)args[6]
                };
                // Parameters To Items @Item Parameter Lists, @Item Owner
                attacker.Items = GetItems(attackerItemIds, (byte[])args[14]);
                //if (attacker.Items == null) return GetFalseByte();
                attacker.IsNPC = false;
                attacker.Owner = (byte[])args[14];

                defender = GetHeroData((string)args[7], (byte[])args[15]);
                if (defender == null) return GetFalseByte();
                defender.Troops = (int)args[8];

                string[] defenderItemIds = new string[]
                {
                    (string)args[9],
                    (string)args[10],
                    (string)args[11],
                    (string)args[12],
                    (string)args[13]
                };
                defender.Items = GetItems(defenderItemIds, (byte[])args[15]);
                //if (defender.Items == null) return GetFalseByte();
                defender.IsNPC = SetNPCMode((int)args[17]);
                defender.Owner = (byte[])args[15];

                return AttackHero(attacker, defender);
            }

            // @Param Hero ID
            if (operation == "attackCity") return AttackCity((string)args[0]);

            return GetFalseByte();
        }

        private static byte[] AttackCity(string itemId)
        {
            // Validate input
            if (!IsValidHeroId(itemId))
            {
                Runtime.Log("Invalid Item ID parameter!");
                return GetFalseByte();
            }
            byte[] item = Storage.Get(Storage.CurrentContext, itemId);
            Runtime.Log("Item is " + item.AsString());
            return item;
        }

        /* Battle Calculation for Hero VS. Hero: 
         * Involved in the Battle, the heroes, the number of troops, the items from both part
         * 
         * Passing Arguments
         * Hero #1 ID (13)              ARG #1
         * //Hero #1 Speed Stat (4)       
         * //Hero #1 Strength Stat (4)
         * Hero #1 Troops Number (4)    ARG #2
         * Hero #1 Item 1 Id (13)       ARG #3
         * Hero #1 Item 2 Id (13)       ARG #4
         * Hero #1 Item 3 Id (13)       ARG #5
         * Hero #1 Item 4 Id (13)       ARG #6
         * Hero #1 Item 5 Id (13)       ARG #7 
         * Hero #1 Address              ARG #15
         * Hero #2 ID (13)              ARG #8
         * //Hero #2 Speed Stat (4)
         * //Hero #2 Strength Stat (4)
         * //Hero #2 Defence Stat (4)
         * Hero #2 Troops Number (4)    ARG #9
         * Hero #2 Item 1 Id (13)       ARG #10
         * Hero #2 Item 2 Id (13)       ARG #11
         * Hero #2 Item 3 Id (13)       ARG #12
         * Hero #2 Item 4 Id (13)       ARG #13
         * Hero #2 Item 5 Id (13)       ARG #14
         * Hero #2 NPC (1)              ARG #16
         * Hero #2 Address              ARG #17
         * 
         * 
         * Steps of the Battle:
         * 1) Calculate the Damage
         * 2) Attack to both of heroes
         * 3) Check who win or both lose
         * 4) If non of them win nor lose, repeat the step 1)
         * 5) If someone win the battle, go to step 6)
         * 6) Decide how many item stats will increase
         * 7) Calculate the increase amount
         * 8) Increase the stats
         * 
         * TOTAL LENGTH OF PASSING ARGUMENTS ARE
         * 
         * Involved Attacker's Speed, Strength, HP, Troops number
         * Involved Defenser's Speed, Strength, HP, Troops number and Defense
         * 
         */

        private static BigInteger DamageCalculation(BigInteger myStrength, BigInteger myDefense, BigInteger mySpeed, BigInteger enemyDefense, BigInteger enemySpeed, int advantage, int cityDefence = 1)
        {
            BigInteger damage = myStrength * (1 - myDefense / (XP_1 + enemyDefense)) * (mySpeed / (enemySpeed + XP_2)) * advantage * (1 - cityDefence);
            return damage;
        }

        private static BigInteger CalculateDamage(Hero my, Hero enemy)
        {
            // Get the Hero Parameters
            // Get the Item Parameters
            BigInteger myStrength = Hero.CalculateStat(StatType.Strength, my);
            BigInteger myDefense = Hero.CalculateStat(StatType.Defense, my);
            BigInteger mySpeed = Hero.CalculateStat(StatType.Speed, my);
            BigInteger enemyDefense = Hero.CalculateStat(StatType.Defense, enemy);
            BigInteger enemySpeed = Hero.CalculateStat(StatType.Speed, enemy);
            int advantage = Class.GetAdvantage(my.Class, enemy.Class);

            return DamageCalculation(myStrength, myDefense, mySpeed, enemyDefense, enemySpeed, advantage);
        }
        private static byte[] AttackHero(Hero my, Hero enemy)
        {
            BigInteger myAttack = CalculateDamage(my, enemy);
            BigInteger enemyAttack = CalculateDamage(enemy, my);

            BigInteger enemyRemainTroops = AcceptDamage(myAttack, enemy.Troops);
            BigInteger myRemainTroops = AcceptDamage(enemyAttack, my.Troops);

            int battleResult = CalculateBattleResult(my.Troops, myRemainTroops, enemy.Troops, enemyRemainTroops);
            if (battleResult.Equals(BattleContract.MY_WIN))
            {
                return RewardWinner(my, myRemainTroops, enemy, enemyRemainTroops);
            }
            if (battleResult.Equals(BattleContract.ENEMY_WIN))
            {
                return RewardWinner(enemy, enemyRemainTroops, my, myRemainTroops);
            }
            LogBattleResultBothLose(my, myRemainTroops, enemy, enemyRemainTroops);
            //Storage.Put(Storage.CurrentContext, heroId, heroParams);
            //Runtime.Notify(new object[] { "Address has returned from the blockchain storage" });
            return GetFalseByte();
        }

        private static BigInteger AcceptDamage(BigInteger damage, int troops)
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
        private static int CalculateBattleResult(BigInteger myTroops, BigInteger myRemained, BigInteger enemyTroops, BigInteger enemyRemained)
        {
            BigInteger myPercent = BigInteger.Divide(myTroops, 100);
            BigInteger myRemainedPercents = BigInteger.Divide(myRemained, myPercent);

            BigInteger enemyPercent = BigInteger.Divide(myTroops, 100);
            BigInteger enemyRemainedPercents = BigInteger.Divide(enemyRemained, enemyPercent);

            if (enemyRemainedPercents < 30)
            {
                if (myRemainedPercents >= 30)
                {
                    return BattleContract.MY_WIN;
                } else
                {
                    return BattleContract.BOTH_LOSE;
                }
            }
            if (myRemainedPercents < 30)
            {
                if (enemyRemainedPercents >= 30)
                {
                    return BattleContract.ENEMY_WIN;
                }
                else
                {
                    return BattleContract.BOTH_LOSE;
                }
            }

            if (myRemainedPercents.Equals(enemyRemainedPercents))
            {
                return BattleContract.BOTH_LOSE;
            }

            if (myRemainedPercents > enemyRemainedPercents)
            {
                return BattleContract.MY_WIN;
            }
            // else
            return BattleContract.ENEMY_WIN;
        }

        private static byte[] RewardWinner(Hero winner, BigInteger winnerRemainedTroops, Hero loser, BigInteger loserRemainedTroops)
        {
            int[] randomItemIndexes = PickRandomItemIndexes(0, winner.Items.Length - 1);

        
            for(int i=0; i<randomItemIndexes.Length; i++)
            {
                int increaseValue = GetIncreaseValue(winner.Items[i]);
                IncreaseStat(winner.Items[i], increaseValue);
            }

            LogBattleResult(winner, winnerRemainedTroops, loser, loserRemainedTroops);
            return GetFalseByte();
        }
        private static int GetRandomItemsAmount(int itemsAmount)
        {
            Header header = Blockchain.GetHeader(Blockchain.GetHeight());
            ulong randomNumber = header.ConsensusData;
            return (int)(randomNumber % (uint)(itemsAmount - 1) + 1);
        }
        private static int[] PickRandomItemIndexes(int minIndex, int maxIndex)
        {
            int amount = GetRandomItemsAmount(maxIndex + 1);
            int[] numbers = new int[amount];
            return numbers;
        }
        private static int GetIncreaseValue(Item item)
        {
            return 0;
        }
        private static void IncreaseStat(Item item, int increaseValue)
        {

        }
        private static void LogBattleResult(Hero winner, BigInteger winnerRemainedTroops, Hero loser, BigInteger loserRemainedTroops)
        {

        }
        private static void LogBattleResultBothLose(Hero my, BigInteger myRemainedTroops, Hero enemy, BigInteger enemyRemainedTroops)
        {

        }

        /**
         *  Checks the item id. Item ID's length should be exactly 15.
         *  First 13 is represents the Unix timestamp in Milliseconds.
         *  And last 2 digits are representing the random number, just for case.
         */
        private static bool IsValidHeroId(string itemId)
        {
            return itemId.Length.Equals(HeroParameters.IdLength);
        }

        private static Item GetItem(string itemId, byte[] address)
        {
            StorageContext storageContext = GetItemContext();
            string parameters = Storage.Get(storageContext, itemId.AsByteArray()).AsString();
            if (!parameters.Length.Equals(BattleContract.itemParametersLength))
            {
                return null;
            }
            if (!parameters.EndsWith(address.ToString()))
            {
                return null;
            }

            Item item = new Item
            {
                Id = itemId,
                Stat = ItemParameters.GetValue(ItemParameterType.Stat, parameters),
                MaxStat = ItemParameters.GetValue(ItemParameterType.MaxStat, parameters),
                statType = ItemParameters.GetStat(parameters),
                Quality = ItemParameters.GetValue(ItemParameterType.Quality, parameters)
            };
            return item;
        }
        private static Item[] GetItems(string[] ids, byte[] address)
        {
            Item[] items;
            int itemsNum = 5;
            // Decide How Many Items were Attached to Hero
            for (int i=1; i<ids.Length; i++)
            {
                if (ids[i].Length.Equals(itemIdLength))
                {
                    itemsNum = i + 1;
                }
            }

            // Return Item By Id
            items = new Item[itemsNum];
            for (int i=0; i<itemsNum; i++)
            {
                Item item = GetItem(ids[i], address);
                if (item == null)
                {
                    return null;
                } else
                {
                    items[i] = item;
                }
            }

            return items;
        }

        private static Hero GetHeroData(string heroId, byte[] address)
        {
            StorageContext storageContext = GetHeroContext();
            string parameters = Storage.Get(storageContext, heroId.AsByteArray()).AsString();
            if (!parameters.Length.Equals(BattleContract.heroParametersLength))
            {
                return null;
            }
            if (!parameters.EndsWith(address.AsString()))
            {
                return null;
            }

            Hero hero = new Hero
            {
                Id              = heroId,
                Stats      = new Stat[5]{
                    new Stat(StatType.Leadership, HeroParameters.GetValue(HeroParameterType.Leadership, parameters)),
                    new Stat(StatType.Strength, HeroParameters.GetValue(HeroParameterType.Strength, parameters)),
                    new Stat(StatType.Strength, HeroParameters.GetValue(HeroParameterType.Speed, parameters)),
                    new Stat(StatType.Strength, HeroParameters.GetValue(HeroParameterType.Intelligence, parameters)),
                    new Stat(StatType.Strength, HeroParameters.GetValue(HeroParameterType.Defence, parameters))
                },
                Class           = HeroParameters.GetClass(parameters)
     
            };

            return hero;
        }

        private static bool SetNPCMode(int npcInt)
        {
            if (npcInt.Equals(0))
            {
                return false;
            }
            return true;
        }

    }
}
