public class WalletEvent
{
    public Wallet Wallet { get; private set; }

    public WalletEvent(Wallet wallet)
    {
        Wallet = wallet;
    }
}