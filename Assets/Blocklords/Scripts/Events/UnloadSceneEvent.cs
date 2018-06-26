public class UnloadSceneEvent
{
	public string SceneName;
	public bool ShouldFade;

	public UnloadSceneEvent(string sceneName, bool shouldFade = true)
	{
		this.SceneName = sceneName;
		this.ShouldFade = shouldFade;
	}
}
