public class GasChangeEvent
{
    public Wallet Wallet { get; set; }

    public GasChangeEvent(Wallet wallet)
    {
        Wallet = wallet;
    }
}