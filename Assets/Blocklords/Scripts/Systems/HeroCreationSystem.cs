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

    [Inject] private NonSerializableHeroes NonSerializableHeroes { get; set; }

    [SerializeField] private MultiSceneSetup heroCreationSetup;

    [SerializeField] private GameObject heroPrefab;

    [SerializeField] private Animator heroCreationPanelAnimator; //the root canvas, contains the graphic raycaster for the hero cards and confirmation panel
    [SerializeField] private Animator heroCardsPanelAnimator;
    [SerializeField] private Animator confirmationPanelAnimator;

    [SerializeField] private Transform confirmationButton;

    [SerializeField] private List<IntList> modifierGroups = new List<IntList>();
    private List<IntList> selectedGroups = new List<IntList>();

    private HeroComponentReactiveProperty selectedHero = new HeroComponentReactiveProperty();

    [SerializeField] private StatsText statsText;

    public override void OnEnable()
    {
        base.OnEnable();

        selectedHero.DistinctUntilChanged().Where(hc => hc != null).Subscribe(heroComponent =>
        {
            statsText.Update(heroComponent.BaseStats, heroComponent.ModifierStats);
        }).AddTo(this.Disposer);

        confirmationButton.OnPointerClickAsObservable().Subscribe(_ =>
        {
            ConfirmHero();
        }).AddTo(this.Disposer);

        var itemTypes = Enum.GetValues(typeof(ItemType)).Cast<ItemType>();
        //selectable hero cards
        NonSerializableHeroes.OnAdd().Where(e => e.HasComponent<SelectableComponent>()).Subscribe(entity =>
        {
            var heroComponent = entity.GetComponent<HeroComponent>();
            var selectableComponent = entity.GetComponent<SelectableComponent>();
            var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>(); //HACK + TODO -> remove this dependency from hero creation

            heroComponent.ID.Value = Guid.NewGuid().ToString();
            heroComponent.Name.Value = "Hero " + heroComponent.ID.Value;

            var modifier = new Stats();
            modifier.Leadership.Value = GetModifier();
            modifier.Strength.Value = GetModifier();
            modifier.Defense.Value = GetModifier();
            modifier.Speed.Value = GetModifier();
            heroComponent.ModifierStats.Add(modifier);

            foreach(var itemType in itemTypes)
            {
                var itemWrappers = GameDataSystem.ItemWrappers.Where(iw => iw.Item.ItemType.Value == itemType).ToList();
                var item = itemWrappers[Random.Range(0, itemWrappers.Count)].Item;
                var clone = item.Clone();
                itemCollectionComponent.Items.Add(clone);
            }

            selectableComponent.IsSelected.Subscribe(value =>
            {
                if(value)
                {
                    selectedHero.Value = heroComponent;
                    heroCardsPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Disabled);
                    confirmationPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Enabled);
                }
            }).AddTo(this.Disposer);

        }).AddTo(this.Disposer);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        NonSerializableHeroes.Entities.ForEachRun(e =>
        {
            Destroy(e.GetComponent<ViewComponent>().Transforms[0]);
        });
    }

    private void ConfirmHero()
    {
        //HACK
        var json = JsonUtility.ToJson(selectedHero.Value);
        var go = PrefabFactory.Instantiate(heroPrefab, GameDataSystem.transform);
        var hc = go.GetComponent<HeroComponent>();
        JsonUtility.FromJsonOverwrite(json, hc);

        //HACK to trigger data subscribers until we've got a better saving and loading scheme
        hc.ID.SetValueAndForceNotify(selectedHero.Value.ID.Value);

        //can use tweens later, for now just animate out
        heroCreationPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Disabled);
        heroCardsPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Disabled);
        confirmationPanelAnimator.SetInteger(PanelParameters.State, PanelStates.Disabled);

        //HACK - transition to the main scene
        Observable.TimerFrame(30).Subscribe(_ =>
        {
            EventSystem.Publish(new UnloadSceneEvent(heroCreationSetup.Setups[0].path, false));
        }).AddTo(this.Disposer);
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
