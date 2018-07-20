public class OpenWalletEvent {

    public string PrivateKeyOrWIF { get; set; }

    public OpenWalletEvent(string privateKeyOrWIF)
    {
        this.PrivateKeyOrWIF = privateKeyOrWIF;
    }
}
