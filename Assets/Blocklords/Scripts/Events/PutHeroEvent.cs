public class PutHeroEvent {

    public HeroComponent heroComponent;
    public bool firstHero = false;
	
    public PutHeroEvent(HeroComponent hc, bool fh)
    {
        this.heroComponent = hc;
        this.firstHero = fh;
    }
}
