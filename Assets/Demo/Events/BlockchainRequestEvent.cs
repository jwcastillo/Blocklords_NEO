using System;
using System.Collections.Generic;

public class BlockchainRequestEvent
{
    public string contractName;
    public string methodName;

    public List<HeroComponent> heroArguments;
    public List<ItemComponent> itemArguments;
    public List<String> stringArguments;
    public List<Boolean> booleanArguments;
    public List<String> walletAddressArguments;
    public List<Int32> integerArguments;
    public List<Double> doubleArguments;
}