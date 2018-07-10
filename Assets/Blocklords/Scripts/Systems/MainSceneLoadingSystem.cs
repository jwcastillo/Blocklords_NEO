using System.Collections;
using System.Collections.Generic;
using AlphaECS;
using AlphaECS.Unity;
using UnityEngine;
using Zenject;

public class MainSceneLoadingSystem : SystemBehaviour
{
    [Inject] private GameDataSystem GameDataSystem { get; set; }

    [SerializeField] private MultiSceneSetup mainSetup;
    [SerializeField] private MultiSceneSetup heroCreationSetup;

    private IEnumerable<string> scenesToUnload;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        scenesToUnload = SceneUtilities.GetScenesToUnload(mainSetup, heroCreationSetup);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        if(GameDataSystem.heroIDs.Length <= 0)
        {
            foreach(var scene in heroCreationSetup.Setups)
            {
                EventSystem.Publish(new LoadSceneEvent(scene.path, true));
            }
        }
    }

}
