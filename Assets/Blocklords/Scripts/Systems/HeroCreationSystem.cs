using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS;
using AlphaECS.Unity;
using System;
using UniRx;
using Random = UnityEngine.Random;
using System.Linq;

public class HeroCreationSystem : SystemBehaviour
{
    private IGroup heroes;
    private IGroup players;

    [SerializeField] private GameObject heroPrefab;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        heroes = this.CreateGroup(new HashSet<Type>() { typeof(HeroComponent), typeof(ViewComponent), });
        //players = this.CreateGroup(new HashSet<Type>() { typeof(PlayerDataComponent), });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        //delay a frame to check whether it's a saved hero or new hero that requires setup
        heroes.OnAdd().DelayFrame(1).Where(e => !e.HasComponent<PlayerDataComponent>()).Subscribe(entity =>
        {
            var heroComponent = entity.GetComponent<HeroComponent>();

            heroComponent.ID.Value = Guid.NewGuid().ToString();
            heroComponent.Name.Value = "Hero " + heroComponent.ID.Value;

            heroComponent.Leadership.Value = Random.Range(0, 100);
            heroComponent.Intelligence.Value = Random.Range(0, 100);
            heroComponent.Strength.Value = Random.Range(0, 100);
            heroComponent.Defense.Value = Random.Range(0, 100);
            heroComponent.Speed.Value = Random.Range(0, 100);
        }).AddTo(this.Disposer);

        heroes.OnAdd().DelayFrame(1).Where(e => e.HasComponent<SelectableComponent>()).Subscribe(entity =>
        {
            var heroComponent = entity.GetComponent<HeroComponent>();
            var selectableComponent = entity.GetComponent<SelectableComponent>();

            var previousValue = selectableComponent.IsSelected.Value;
            selectableComponent.IsSelected.Subscribe(value =>
            {
                //double-click
                if(value && previousValue)
                {
                    //HACK
                    var json = JsonUtility.ToJson(heroComponent);
                    var hc = PrefabFactory.Instantiate(heroPrefab).GetComponent<HeroComponent>();
                    JsonUtility.FromJsonOverwrite(json, hc);
                }
                previousValue = value;
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
}
