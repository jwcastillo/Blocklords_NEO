using UnityEngine.SceneManagement;
using AlphaECS.Unity;
using UniRx;
using AlphaECS;
using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using DG.Tweening;
using System.Collections.Generic;
using Zenject;

public class SceneLoadingSystem : SystemBehaviour
{
    [SerializeField]
    private MultiSceneSetup defaultSetup;

    [SerializeField]
    private CanvasGroup canvasRoot;

    [SerializeField]
    private ReactiveCollection<string> scenesToLoad = new ReactiveCollection<string>();
    [SerializeField]
    private ReactiveCollection<string> scenesToUnload = new ReactiveCollection<string>();

    private BoolReactiveProperty readyToLoad = new BoolReactiveProperty();

    private Tween fadingSequence;
    public FadeStateReactiveProperty FadingState;

    [SerializeField]
    private float fadeTime = 1.25f;
    [SerializeField]
    private float fadeDelay = 1f;

    private Scene activeScene;

    private IGroup sceneSetups;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        sceneSetups = this.CreateGroup(new Type[] { typeof(SceneSetupComponent) });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        //loading
        scenesToLoad.ObserveAdd().Subscribe(e =>
        {
            //defer until the loading screen is active
            readyToLoad.StartWith(readyToLoad.Value).Where(isReady => isReady).FirstOrDefault().Subscribe(_ =>
            {
                SceneManager.LoadSceneAsync(e.Value, LoadSceneMode.Additive);
            }).AddTo(this.Disposer);

            //if(QualitySettingsSystem.Performance.Value <= DevicePerformance.Low)
            //{
            //  //defer until the loading screen is active and we're not loading any other scenes
            //  if(ScenesToLoad.Count <= 1)
            //  {
            //      ReadyToLoad.StartWith(ReadyToLoad.Value).Where(isReady => isReady).FirstOrDefault().Subscribe(_ =>
            //      {
            //          SceneManager.LoadSceneAsync(e.Value, LoadSceneMode.Additive);
            //      }).AddTo(this.Disposer);
            //  }
            //  else
            //  {
            //      ScenesToLoad.ObserveRemove().Where(_ => ScenesToLoad.Count > 0 && ScenesToLoad[0] == e.Value).FirstOrDefault().Subscribe(_ =>
            //      {
            //          ReadyToLoad.StartWith(ReadyToLoad.Value).Where(isReady => isReady).FirstOrDefault().Subscribe(_2 =>
            //          {
            //              SceneManager.LoadSceneAsync(e.Value, LoadSceneMode.Additive);
            //          }).AddTo(this.Disposer);
            //      }).AddTo(this.Disposer);
            //  }
            //}
            //else
            //{
            //  //defer until the loading screen is active
            //  ReadyToLoad.StartWith(ReadyToLoad.Value).Where(isReady => isReady).FirstOrDefault().Subscribe(_ =>
            //  {
            //      SceneManager.LoadSceneAsync(e.Value, LoadSceneMode.Additive);
            //  }).AddTo(this.Disposer);
            //}
        }).AddTo(this.Disposer);

        //unloading
        scenesToUnload.ObserveAdd().Subscribe(e =>
        {
            var scene = SceneManager.GetSceneByPath(e.Value).IsValid() ? SceneManager.GetSceneByPath(e.Value) : SceneManager.GetSceneByName(e.Value);
            if (scene == activeScene)
            {
                var defaultScene = SceneManager.GetSceneByPath(defaultSetup.Setups[0].path).IsValid() ? SceneManager.GetSceneByPath(defaultSetup.Setups[0].path) : SceneManager.GetSceneByName(defaultSetup.Setups[0].path);
                SceneManager.SetActiveScene(defaultScene);
            }

            //defer until the loading screen is active
            readyToLoad.StartWith(readyToLoad.Value).Where(isReady => isReady).FirstOrDefault().Subscribe(_ =>
            {
                SceneManager.UnloadSceneAsync(e.Value);
            }).AddTo(this.Disposer);
        }).AddTo(this.Disposer);

        //fade in
        scenesToLoad.ObserveRemove().Merge(scenesToUnload.ObserveRemove()).Subscribe(_ =>
        {
            if (scenesToLoad.Count == 0 && scenesToUnload.Count == 0)
            {
                readyToLoad.Value = false;

                Resources.UnloadUnusedAssets();

                var mainScene = sceneSetups.Entities.Select(e => e.GetComponent<SceneSetupComponent>())
                                           .FirstOrDefault().MultiSceneSetup.Setups.Select(s => s).FirstOrDefault();
                activeScene = SceneManager.GetSceneByPath(mainScene.path);
                if (activeScene.IsValid() && activeScene.isLoaded)
                {
                    SceneManager.SetActiveScene(activeScene);
                }

                FadingState.Value = FadeState.FadingIn;

                if (fadingSequence != null)
                { fadingSequence.Kill(); }

                fadingSequence = canvasRoot.DOFade(0f, fadeTime).SetDelay(fadeDelay).OnComplete(() =>
                {
                    SetLoadingScreenActive(false);
                    FadingState.Value = FadeState.FadedIn;
                });
            }
        }).AddTo(this.Disposer);


#if UNITY_EDITOR //delay to account for multi-scene editing so we don't get scenes loaded multiple times
        sceneSetups.OnAdd().DelayFrame(1).Subscribe(entity =>
#else
        SceneSetups.OnAdd().Subscribe(entity =>
#endif
        {
            var setupComponent = entity.GetComponent<SceneSetupComponent>();
            setupComponent.MultiSceneSetup.Setups.Skip(1).ForEachRun(sceneSetup =>
            {
                EventSystem.Publish(new LoadSceneEvent(sceneSetup.path));
            });

        }).AddTo(this.Disposer);

        EventSystem.OnEvent<LoadSceneEvent>().Subscribe(e =>
        {
            if (SceneManager.GetSceneByPath(e.SceneName).isLoaded || SceneManager.GetSceneByName(e.SceneName).isLoaded || scenesToLoad.Any(x => x == e.SceneName))
            { return; }

            if (e.ShouldFade && FadingState.Value != FadeState.FadedOut)
            {
                readyToLoad.Value = false;
                scenesToLoad.Add(e.SceneName);

                if (FadingState.Value != FadeState.FadingOut)
                {
                    if (fadingSequence != null)
                    { fadingSequence.Kill(); }

                    FadingState.Value = FadeState.FadingOut;
                    SetLoadingScreenActive(true);

                    fadingSequence = canvasRoot.DOFade(1f, fadeTime).OnComplete(() =>
                    {
                        readyToLoad.Value = true;
                        FadingState.Value = FadeState.FadedOut;
                    });
                }
            }
            else
            {
                readyToLoad.Value = true;
                scenesToLoad.Add(e.SceneName);
            }
        }).AddTo(this.Disposer);

        EventSystem.OnEvent<UnloadSceneEvent>().Subscribe(e =>
        {
            var scene = SceneManager.GetSceneByPath(e.SceneName).IsValid() ? SceneManager.GetSceneByPath(e.SceneName) : SceneManager.GetSceneByName(e.SceneName);
            if (!scene.IsValid() || !scene.isLoaded || scenesToUnload.Any(x => x == e.SceneName))
            { return; }

            if (e.ShouldFade && FadingState.Value != FadeState.FadedOut)
            {
                readyToLoad.Value = false;
                scenesToUnload.Add(e.SceneName);

                if (FadingState.Value != FadeState.FadingOut)
                {
                    if (fadingSequence != null)
                    { fadingSequence.Kill(); }

                    FadingState.Value = FadeState.FadingOut;
                    SetLoadingScreenActive(true);

                    fadingSequence = canvasRoot.DOFade(1f, fadeTime).OnComplete(() =>
                    {
                        readyToLoad.Value = true;
                        FadingState.Value = FadeState.FadedOut;
                    });
                }
            }
            else
            {
                readyToLoad.Value = true;
                scenesToUnload.Add(e.SceneName);
            }
        }).AddTo(this.Disposer);

        defaultSetup.Setups.ForEachRun(sceneSetup =>
        {
            EventSystem.Publish(new LoadSceneEvent(sceneSetup.path));
        });
    }

    public override void OnDisable()
    {
        base.OnDisable();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scenesToLoad.Any(x => x == scene.path))
        {
            scenesToLoad.Remove(scene.path);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scenesToUnload.Any(x => x == scene.path))
        {
            scenesToUnload.Remove(scene.path);
        }
    }

    private void SetLoadingScreenActive(bool isActive)
    {
        canvasRoot.gameObject.SetActive(isActive);
    }
}

public static class SceneUtilities
{
    public static IEnumerable<string> GetScenesToUnload(MultiSceneSetup sourceSetup, MultiSceneSetup targetSetup)
    {
        var targetScenes = targetSetup.Setups.Select(s => s.path);
        return sourceSetup.Setups.Select(s => s.path).Where(path => targetScenes.All(x => x != path));
    }
}

public enum FadeState
{
    FadedIn, //scene is fully loaded in
    FadingOut, //transitioning to loading screen
    FadedOut, //fully faded in the loading screen
    FadingIn //fading out the loading screen / fading in the scene
}

[Serializable]
public class FadeStateReactiveProperty : ReactiveProperty<FadeState>
{
    public FadeStateReactiveProperty() { }
    public FadeStateReactiveProperty(FadeState initialValue) : base(initialValue) { }
}
