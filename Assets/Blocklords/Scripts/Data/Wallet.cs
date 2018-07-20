
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;

[System.Serializable]
public class Wallet {
    public DecimalReactiveProperty GAS = new DecimalReactiveProperty();
    public KeyPair keys;

    public Wallet(string privateKey, decimal gas)
    {
        this.keys = new KeyPair(privateKey.HexToBytes());
        GAS.Value = gas;
    }

    public Wallet(KeyPair keys)
    {
        this.keys = keys;
        GAS.Value = 0;
    }

    public Wallet (byte[] privateKey)
    {
        keys = new KeyPair(privateKey);
        GAS.Value = 0;
    }

    public Wallet(string wif)
    {
        this.keys = KeyPair.FromWIF(wif);
        GAS.Value = 0;
    }

    public Wallet()
    {
        keys = null;
        GAS.Value = 0;
    }

    public static bool IsNull(Wallet wallet)
    {
        if (wallet == null) return true;
        return (wallet.keys == null);
    }
    public void SetNull()
    {
        keys = null;
        GAS.Value = 0;
    }
}
