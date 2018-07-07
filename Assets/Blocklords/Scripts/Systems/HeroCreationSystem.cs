using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS;
using AlphaECS.Unity;
using System;
using UniRx;
using Random = UnityEngine.Random;
using System.Linq;
using Zenject;
using TMPro;
using UniRx.Triggers;

public class HeroCreationSystem : SystemBehaviour
{
    [Inject] GameDataSystem GameDataSystem { get; set; }

    private IGroup heroes;
    private IGroup players;

    [SerializeField] private GameObject heroPrefab;

    [SerializeField] private Animator heroCreationPanelAnimator; //the root canvas, contains the graphic raycaster for the hero cards and confirmation panel
    [SerializeField] private Animator heroCardsPanelAnimator;
    [SerializeField] private Animator confirmationPanelAnimator;

    [SerializeField] private Transform confirmationButton;

    private int isShowingHash;

    [SerializeField] private List<IntList> modifierGroups = new List<IntList>();
    private List<IntList> selectedGroups = new List<IntList>();

    private HeroComponentReactiveProperty selectedHero = new HeroComponentReactiveProperty();

    [SerializeField] private TextMeshProUGUI leadershipText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI troopsText;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        heroes = this.CreateGroup(new HashSet<Type>() { typeof(HeroComponent), typeof(ViewComponent), });
        //players = this.CreateGroup(new HashSet<Type>() { typeof(PlayerDataComponent), });

        isShowingHash = Animator.StringToHash("IsShowing");
    }

    public override void OnEnable()
    {
        base.OnEnable();

        selectedHero.DistinctUntilChanged().Where(hc => hc != null).Subscribe(heroComponent =>
        {
            leadershipText.text = (heroComponent.BaseStats.Leadership.Value + heroComponent.ModifierStats.Select(stat => stat.Leadership.Value).Sum()).ToString();
            intelligenceText.text = (heroComponent.BaseStats.Intelligence.Value + heroComponent.ModifierStats.Select(stat => stat.Intelligence.Value).Sum()).ToString();
            strengthText.text = (heroComponent.BaseStats.Strength.Value + heroComponent.ModifierStats.Select(stat => stat.Strength.Value).Sum()).ToString();
            defenseText.text = (heroComponent.BaseStats.Defense.Value + heroComponent.ModifierStats.Select(stat => stat.Defense.Value).Sum()).ToString();
            speedText.text = (heroComponent.BaseStats.Speed.Value + heroComponent.ModifierStats.Select(stat => stat.Speed.Value).Sum()).ToString();
            //troopsText.text = heroComponent.BaseStats.Troops.Value.ToString(); //TODO -> this is a computed value

        }).AddTo(this.Disposer);

        confirmationButton.OnPointerClickAsObservable().Subscribe(_ =>
        {
            ConfirmHero();
        }).AddTo(this.Disposer);

        //delay a frame to check whether it's a saved hero or new hero that requires setup
        heroes.OnAdd().DelayFrame(1).Where(e => !e.HasComponent<PlayerDataComponent>()).Subscribe(entity =>
        {
            var heroComponent = entity.GetComponent<HeroComponent>();

            heroComponent.ID.Value = Guid.NewGuid().ToString();
            heroComponent.Name.Value = "Hero " + heroComponent.ID.Value;
        }).AddTo(this.Disposer);


        heroes.OnAdd().DelayFrame(1).Where(e => e.HasComponent<SelectableComponent>() && !e.HasComponent<PlayerDataComponent>()).Subscribe(entity =>
        {
            var heroComponent = entity.GetComponent<HeroComponent>();
            var selectableComponent = entity.GetComponent<SelectableComponent>();

            //var modifier = ScriptableObject.CreateInstance<Stats>();
            var modifier = new Stats();
            modifier.Leadership.Value = GetModifier();
            modifier.Strength.Value = GetModifier();
            modifier.Defense.Value = GetModifier();
            modifier.Speed.Value = GetModifier();
            heroComponent.ModifierStats.Add(modifier);

            //var previousValue = selectableComponent.IsSelected.Value;

            selectableComponent.IsSelected.Subscribe(value =>
            {
                if(value)
                {
                    selectedHero.Value = heroComponent;
                    //heroCreationPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Disabled);
                    heroCardsPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Disabled);
                    confirmationPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Enabled);
                }

                //double-click
                //if(value && previousValue)
                //{
                //    ConfirmHero();
                //}
                //previousValue = value;
            }).AddTo(this.Disposer);

        }).AddTo(this.Disposer);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        heroes.Entities.Where(e => !e.HasComponent<PlayerDataComponent>()).ForEachRun(e =>
        {
            Destroy(e.GetComponent<ViewComponent>().Transforms[0]);
        });
    }

    private void ConfirmHero()
    {
        //HACK
        var json = JsonUtility.ToJson(selectedHero.Value);

        Debug.Log(json);
        Debug.Log(JsonUtility.ToJson(selectedHero.Value.ModifierStats[0]));

        var hc = PrefabFactory.Instantiate(heroPrefab, GameDataSystem.transform).GetComponent<HeroComponent>();
        JsonUtility.FromJsonOverwrite(json, hc);

        //HACK to trigger data subscribers until we've got a better saving and loading scheme
        hc.ID.SetValueAndForceNotify(selectedHero.Value.ID.Value);

        //can use tweens later, for now just animate out
        heroCreationPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Disabled);
        heroCardsPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Disabled);
        confirmationPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Disabled);

        //HACK - transition to the main scene
        //Observable.TimerFrame(30).Subscribe(_ =>
        //{
        //    EventSystem.Publish(new LoadSceneEvent() {})
        //}).AddTo(this.Disposer);
    }

    private int GetModifier()
    {
        if (modifierGroups.Count <= 0)
        {
            modifierGroups.Clear();
            foreach (var group in selectedGroups)
            { modifierGroups.Add(group); }
            selectedGroups.Clear();
        }

        var selectedGroup = modifierGroups[UnityEngine.Random.Range(0, modifierGroups.Count - 1)];
        var selectedModifier = selectedGroup.Values[UnityEngine.Random.Range(0, selectedGroup.Values.Count)];

        selectedGroups.Add(selectedGroup);
        modifierGroups.Remove(selectedGroup);

        return selectedModifier;
    }
}
