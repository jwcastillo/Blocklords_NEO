public class LoadSceneEvent
{
	public string SceneName;
    public bool ShouldFade;

	public LoadSceneEvent(string sceneName, bool shouldFade = true)
	{
		this.SceneName = sceneName;
        this.ShouldFade = shouldFade;
	}
}
