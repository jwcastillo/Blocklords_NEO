
public class BlockchainErrorEvent {
    public string Message { get; private set; }

    public BlockchainErrorEvent(string message)
    {
        this.Message = message;
    }
}
