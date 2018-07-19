using AlphaECS.Unity;
using System;
using System.Collections.Generic;
using AlphaECS;

public class BlockchainSystem : SystemBehaviour {
    enum Net
    {
        MainNet,
        TestNet,
        PrivateNet
    }

    List<String> RequireFee = new List<String>();
    List<String> RequireTransactionEnd = new List<String>();

    readonly string ItemContractAddress = "";
    readonly string MerchantContractAddress = "";
    readonly string BattleContractAddress = "";
    readonly string HeroContractAddress = "";

    readonly string neoNetUrl = "azure.com/asdasdasd:4000";
    readonly string neoNetPort = "30333";

    readonly string itemContract = "";
}
