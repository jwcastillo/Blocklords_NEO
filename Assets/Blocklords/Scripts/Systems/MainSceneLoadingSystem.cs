using System.Collections;
using System.Collections.Generic;
using AlphaECS;
using AlphaECS.Unity;
using UnityEngine;
using Zenject;

public class MainSceneLoadingSystem : SystemBehaviour
{
    [Inject] private GameDataSystem GameDataSystem { get; set; }

    [SerializeField] private MultiSceneSetup MainSetup;
    [SerializeField] private MultiSceneSetup CharacterCreationSetup;

    private IEnumerable<string> scenesToUnload;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        scenesToUnload = SceneUtilities.GetScenesToUnload(MainSetup, CharacterCreationSetup);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        if(GameDataSystem.heroIDs.Length <= 0)
        {
            foreach(var scene in CharacterCreationSetup.Setups)
            {
                EventSystem.Publish(new LoadSceneEvent(scene.path, true));
            }
        }
    }

}
