﻿[System.Serializable]
public class Wallet
{
    public string PrivateKey { get; private set; }
    public string Address { get; set; }
    public string WIF { get; set; }
    public double GAS { get; set; }

    public Wallet(string privateKey, string wif, string address, double gas)
    {
        PrivateKey = privateKey;
        WIF = wif;
        Address = address;
        GAS = gas;
    }
}