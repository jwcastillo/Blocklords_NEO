
public class HeroTransferedEvent {
    public bool isSuccess;
    public string message;

    public HeroTransferedEvent(bool iS, string m )
    {
        this.isSuccess = iS;
        this.message = m;
    }

    public HeroTransferedEvent(string m)
    {
        this.isSuccess = false;
        this.message = m;
    }
    public HeroTransferedEvent()
    {
        this.isSuccess = true;
        this.message = "";
    }
}
