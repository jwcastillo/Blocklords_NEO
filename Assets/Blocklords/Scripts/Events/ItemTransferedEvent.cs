
public class ItemTransferedEvent {
    public bool isSuccess;
    public string message;

    public ItemTransferedEvent(bool iS, string m )
    {
        this.isSuccess = iS;
        this.message = m;
    }

    public ItemTransferedEvent(string m)
    {
        this.isSuccess = false;
        this.message = m;
    }
    public ItemTransferedEvent()
    {
        this.isSuccess = true;
        this.message = "";
    }
}
