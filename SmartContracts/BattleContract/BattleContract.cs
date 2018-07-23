using BattleContract.Battle;
using BattleContract.Character;
using BattleContract.StorageLog;
using BattleContract.StorageData;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;
using BattleContract.Data;


/**
 *  Battle of the Blocklords game.
 *  
 *  Version: 1.0
 *  Author: Medet Ahmetson
 *
 */

namespace BattleContract
{
    class BattleContract : SmartContract
    {
        [Appcall("9b82f30aa0f23231d028cfd3d153ea485fcb5344")]
        public static extern StorageContext GetItemContext();

        [Appcall("3ebd4f8add4bf73b70c014bb563b58e92322c096")]
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

                if (!args.Length.Equals(18)) return GetFalseByte();
                if (!IsValidHeroId((string)args[0])) return GetFalseByte();
                if (!IsValidHeroId((string)args[7])) return GetFalseByte();

                // Returns Hero Information: @Id, @parameter caller
                attacker = HeroDataHelper.GetHeroData((string)args[0], (byte[])args[14]);
                if (attacker.Id == null) return GetFalseByte();
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
                attacker.Items = ItemDataHelper.GetItems(attackerItemIds, (byte[])args[14]);
                //if (attacker.Items == null) return GetFalseByte();
                attacker.IsNPC = false;
                attacker.Owner = (byte[])args[14];

                defender = HeroDataHelper.GetHeroData((string)args[7], (byte[])args[15]);
                if (defender.Id == null) return GetFalseByte();
                defender.Troops = (int)args[8];

                string[] defenderItemIds = new string[]
                {
                    (string)args[9],
                    (string)args[10],
                    (string)args[11],
                    (string)args[12],
                    (string)args[13]
                };
                defender.Items = ItemDataHelper.GetItems(defenderItemIds, (byte[])args[15]);
                //if (defender.Items == null) return GetFalseByte();
                defender.IsNPC = SetNPCMode((int)args[17]);
                defender.Owner = (byte[])args[15];

                string battleId = (string)args[18];

                BattleLog battleLog = new BattleLog
                {
                    BattleId = battleId
                };

                return AttackHero(battleLog, attacker, defender);
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

        
        private static byte[] AttackHero(BattleLog battleLog, Hero my, Hero enemy)
        {
            BigInteger myAttack = Battle.Helper.CalculateDamage(my, enemy);
            BigInteger enemyAttack = Battle.Helper.CalculateDamage(enemy, my);

            BigInteger enemyRemainTroops = Battle.Helper.AcceptDamage(myAttack, enemy.Troops);
            BigInteger myRemainTroops = Battle.Helper.AcceptDamage(enemyAttack, my.Troops);

            BattleResult battleResult = Battle.Helper.CalculateBattleResult(my.Troops, myRemainTroops, enemy.Troops, enemyRemainTroops);

            StorageLog.Helper.AddToLog(battleLog, battleResult, myRemainTroops, enemyRemainTroops);

            if (battleResult.Equals(BattleResult.BOTH_LOSE))
            {
                StorageLog.Helper.LogBattleResult(battleLog, my, enemy);
                Runtime.Log("both_lose");
                return GetFalseByte();
            }

            return RewardWinner(battleLog, my, enemy);
        }

        public static byte[] RewardWinner(BattleLog battleLog, Hero my, Hero enemy)
        {
            Hero winner = my, loser = enemy;

            if (battleLog.battleResult.Equals(BattleResult.ENEMY_WIN))
            {
                winner = enemy;
                loser = my;
            }

            int[] randomItemIndexes = new int[0];
            if (!winner.IsNPC)
            {
                randomItemIndexes = Math.Helper.SelectRandomNumbers(winner.Items.Length);
                ItemIncreasing[] itemIncreasings = IncreasingTable.Get();
                BattleType battleType = Battle.Helper.GetBattleType(winner.IsNPC, loser.IsNPC);
                battleLog.battleType = battleType;

                for (int i = 0; i < randomItemIndexes.Length; i++)
                {
                    int increaseValue = Battle.Helper.GetIncreaseValue(itemIncreasings, winner.Items[i], battleType);

                    StorageLog.Helper.AddIncreasedItem(battleLog, winner.Items[i].Id, winner.Items[i].Stat.AsString(), increaseValue);

                    Battle.Helper.IncreaseStat(winner.Items[i], increaseValue, winner.Owner);
                }
            }

            StorageLog.Helper.LogBattleResult(battleLog, my, enemy);
            return GetFalseByte();
        }

        

        /**
         *  Checks the item id. Item ID's length should be exactly 15.
         *  First 13 is represents the Unix timestamp in Milliseconds.
         *  And last 2 digits are representing the random number, just for case.
         */
        private static bool IsValidHeroId(string itemId)
        {
            return itemId.Length.Equals(HeroDataHelper.IdLength);
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
