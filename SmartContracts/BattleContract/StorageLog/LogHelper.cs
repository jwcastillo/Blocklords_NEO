using BattleContract.Battle;
using BattleContract.Character;
using BattleContract.StorageData;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace BattleContract.StorageLog
{
    public static class Helper
    {
        public static void AddIncreasedItem(BattleLog log, string id, string stat, int increasing)
        {
            log.increasingsNumber++;
            string value = id + StringAndArray.Helper.GetStringByDigit(increasing) + StringAndArray.Helper.GetZeroPrefixedString(stat, 4);
            log.Increasings[log.increasingsNumber - 1] = value;
        }

        public static string GetIncreasingsAsLogParameter(BattleLog battleLog)
        {
            string parameter = "";

            for(int i=0; i<battleLog.increasingsNumber; i++)
            {
                parameter = parameter + battleLog.Increasings[0];
            }
            if (battleLog.increasingsNumber.Equals(ItemDataHelper.MaxItems)) return parameter;

            // Add Empty Parameters
            int parameterLength = 13 + 1 + 4;   // ID (13) INCREASING (1) + STAT (4)
            for(int i= battleLog.increasingsNumber; i< ItemDataHelper.MaxItems; i++)
            {
                parameter = parameter + StringAndArray.Helper.GetZeroPrefixedString("", parameterLength);
            }

            return parameter;
        }

        public static string GetBattleResult(BattleLog battleLog)
        {
            return StringAndArray.Helper.GetStringByDigit((int)(battleLog.battleResult));
        }
        public static string GetBattleType(BattleLog battleLog)
        {
            return StringAndArray.Helper.GetStringByDigit((int)battleLog.battleType);
        }
        public static string GetRemainedTroops (BattleLog battleLog, bool isMy)
        {
            if (isMy)
                return StringAndArray.Helper.GetZeroPrefixedString(battleLog.MyRemainedTroops.ToByteArray().AsString(), 4);
            return StringAndArray.Helper.GetZeroPrefixedString(battleLog.EnemyRemainedTroops.ToByteArray().AsString(), 4);
        }
        public static string GetIncreasingsNumber(BattleLog battleLog)
        {
            return StringAndArray.Helper.GetStringByDigit((int)battleLog.increasingsNumber);
        }

        public static string GetStorageParameters(BattleLog battleLog, Hero my, Hero enemy)
        {
            return GetBattleResult(battleLog) + GetBattleType(battleLog) + my.Id + GetRemainedTroops(battleLog, true) + enemy.Id + GetRemainedTroops(battleLog, false) +
                GetIncreasingsNumber(battleLog) + GetIncreasingsAsLogParameter(battleLog) + my.Owner.AsString() + enemy.Owner.AsString();
        }

        public static void AddToLog(BattleLog battleLog, BattleResult battleResult)
        {
            battleLog.battleResult = battleResult;
        }
        public static void AddToLog(BattleLog battleLog, BigInteger troops, bool isMy)
        {
            if (isMy)
                battleLog.MyRemainedTroops = troops;
            battleLog.EnemyRemainedTroops = troops;
        }
        public static void AddToLog(BattleLog battleLog, BigInteger myTroops, BigInteger enemyTroops)
        {
            AddToLog(battleLog, myTroops, true);
            AddToLog(battleLog, myTroops, false);
        }
        public static void AddToLog(BattleLog battleLog,  BattleResult battleResult, BigInteger myTroops, BigInteger enemyTroops)
        {
            AddToLog(battleLog, battleResult);
            AddToLog(battleLog, myTroops, enemyTroops);
        }

        // LOG RECORDS ON STORAGE IS:
        //  BATTLE ID (13)  <= KEY
        //
        //  BATTLE_RESULT (1)   HERO #1 ID (13) HERO #1 TROOPS (4)  HERO #1 REMAINED TROOPS (4),
        //  HERO #2 ID (13) HERO #2 TROOPS (4)  HERO #2 REMAINED TROOPS (4), STAT INCREASED ITEMS NO (1)
        //  ( ITEM ID (13)  INCREASED VALUE (1) STAT BEFORE INCREASING (4) ) x5, BATTLE TYPE (1)
        //  HERO #1 OWNER
        //  HERO #2 OWNER
        public static void LogBattleResult(BattleLog battleLog, Hero my, Hero enemy)
        {
            string parameters = StorageLog.Helper.GetStorageParameters(battleLog, my, enemy);
            Storage.Put(Storage.CurrentContext, battleLog.BattleId, parameters);
        }
    }
}
