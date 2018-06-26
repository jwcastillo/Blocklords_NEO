using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS;
using AlphaECS.Unity;
using System;
using UniRx;
using Random = UnityEngine.Random;

public class HeroCreationSystem : SystemBehaviour
{
    private IGroup heroes;
    private IGroup players;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        heroes = this.CreateGroup(new HashSet<Type>() { typeof(HeroComponent), });
        //players = this.CreateGroup(new HashSet<Type>() { typeof(PlayerDataComponent), });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        heroes.OnAdd().Subscribe(entity =>
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
    }
}
