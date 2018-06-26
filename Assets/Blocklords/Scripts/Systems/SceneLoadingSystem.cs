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
    private MultiSceneSetup DefaultSetup;
    [SerializeField]
    private CanvasGroup CanvasRoot;

    [SerializeField]
    private ReactiveCollection<string> ScenesToLoad = new ReactiveCollection<string>();
    [SerializeField]
    private ReactiveCollection<string> ScenesToUnload = new ReactiveCollection<string>();

    private BoolReactiveProperty ReadyToLoad = new BoolReactiveProperty();

    private Tween FadingSequence;
    public FadeStateReactiveProperty FadingState;

    [SerializeField]
    private float FadeTime = 1.25f;
    [SerializeField]
    private float FadeDelay = 1f;

    private Scene ActiveScene;

    private IGroup SceneSetups;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        SceneSetups = this.CreateGroup(new Type[] { typeof(SceneSetupComponent) });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        //loading
        ScenesToLoad.ObserveAdd().Subscribe(e =>
        {
            //defer until the loading screen is active
            ReadyToLoad.StartWith(ReadyToLoad.Value).Where(isReady => isReady).FirstOrDefault().Subscribe(_ =>
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
        ScenesToUnload.ObserveAdd().Subscribe(e =>
        {
            var scene = SceneManager.GetSceneByPath(e.Value).IsValid() ? SceneManager.GetSceneByPath(e.Value) : SceneManager.GetSceneByName(e.Value);
            if (scene == ActiveScene)
            {
                var defaultScene = SceneManager.GetSceneByPath(DefaultSetup.Setups[0].path).IsValid() ? SceneManager.GetSceneByPath(DefaultSetup.Setups[0].path) : SceneManager.GetSceneByName(DefaultSetup.Setups[0].path);
                SceneManager.SetActiveScene(defaultScene);
            }

            //defer until the loading screen is active
            ReadyToLoad.StartWith(ReadyToLoad.Value).Where(isReady => isReady).FirstOrDefault().Subscribe(_ =>
            {
                SceneManager.UnloadSceneAsync(e.Value);
            }).AddTo(this.Disposer);
        }).AddTo(this.Disposer);

        //fade in
        ScenesToLoad.ObserveRemove().Merge(ScenesToUnload.ObserveRemove()).Subscribe(_ =>
        {
            if (ScenesToLoad.Count == 0 && ScenesToUnload.Count == 0)
            {
                ReadyToLoad.Value = false;

                Resources.UnloadUnusedAssets();

                if (ActiveScene != null && ActiveScene.IsValid())
                {
                    SceneManager.SetActiveScene(ActiveScene);
                }

                FadingState.Value = FadeState.FadingIn;

                if (FadingSequence != null)
                { FadingSequence.Kill(); }

                FadingSequence = CanvasRoot.DOFade(0f, FadeTime).SetDelay(FadeDelay).OnComplete(() =>
                {
                    SetLoadingScreenActive(false);
                    FadingState.Value = FadeState.FadedIn;
                });
            }
        }).AddTo(this.Disposer);


#if UNITY_EDITOR //delay to account for multi-scene editing so we don't get scenes loaded multiple times
        SceneSetups.OnAdd().DelayFrame(1).Subscribe(entity =>
#else
        SceneSetups.OnAdd().Subscribe(entity =>
#endif
        {

            var setupComponent = entity.GetComponent<SceneSetupComponent>();

            var mainScene = setupComponent.MultiSceneSetup.Setups.Select(s => s).First();
            ActiveScene = SceneManager.GetSceneByPath(mainScene.path);
            setupComponent.MultiSceneSetup.Setups.Skip(1).ForEachRun(sceneSetup =>
            {
                EventSystem.Publish(new LoadSceneEvent(sceneSetup.path));
            });

        }).AddTo(this.Disposer);

        EventSystem.OnEvent<LoadSceneEvent>().Subscribe(e =>
        {
            if (SceneManager.GetSceneByPath(e.SceneName).isLoaded || SceneManager.GetSceneByName(e.SceneName).isLoaded || ScenesToLoad.Any(x => x == e.SceneName))
            { return; }

            if (e.ShouldFade && FadingState.Value != FadeState.FadedOut)
            {
                ReadyToLoad.Value = false;
                ScenesToLoad.Add(e.SceneName);

                if (FadingState.Value != FadeState.FadingOut)
                {
                    if (FadingSequence != null)
                    { FadingSequence.Kill(); }

                    FadingState.Value = FadeState.FadingOut;
                    SetLoadingScreenActive(true);

                    FadingSequence = CanvasRoot.DOFade(1f, FadeTime).OnComplete(() =>
                    {
                        ReadyToLoad.Value = true;
                        FadingState.Value = FadeState.FadedOut;
                    });
                }
            }
            else
            {
                ReadyToLoad.Value = true;
                ScenesToLoad.Add(e.SceneName);
            }
        }).AddTo(this.Disposer);

        EventSystem.OnEvent<UnloadSceneEvent>().Subscribe(e =>
        {
            var scene = SceneManager.GetSceneByPath(e.SceneName).IsValid() ? SceneManager.GetSceneByPath(e.SceneName) : SceneManager.GetSceneByName(e.SceneName);
            if (!scene.IsValid() || !scene.isLoaded || ScenesToUnload.Any(x => x == e.SceneName))
            { return; }

            if (e.ShouldFade && FadingState.Value != FadeState.FadedOut)
            {
                ReadyToLoad.Value = false;
                ScenesToUnload.Add(e.SceneName);

                if (FadingState.Value != FadeState.FadingOut)
                {
                    if (FadingSequence != null)
                    { FadingSequence.Kill(); }

                    FadingState.Value = FadeState.FadingOut;
                    SetLoadingScreenActive(true);

                    FadingSequence = CanvasRoot.DOFade(1f, FadeTime).OnComplete(() =>
                    {
                        ReadyToLoad.Value = true;
                        FadingState.Value = FadeState.FadedOut;
                    });
                }
            }
            else
            {
                ReadyToLoad.Value = true;
                ScenesToUnload.Add(e.SceneName);
            }
        }).AddTo(this.Disposer);

        DefaultSetup.Setups.ForEachRun(sceneSetup =>
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
        if (ScenesToLoad.Any(x => x == scene.path))
        {
            ScenesToLoad.Remove(scene.path);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (ScenesToUnload.Any(x => x == scene.path))
        {
            ScenesToUnload.Remove(scene.path);
        }
    }

    private void SetLoadingScreenActive(bool isActive)
    {
        CanvasRoot.gameObject.SetActive(isActive);
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
