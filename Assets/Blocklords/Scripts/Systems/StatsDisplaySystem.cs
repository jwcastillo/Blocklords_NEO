using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using Zenject;
using UniRx;

public class StatsDisplaySystem : SystemBehaviour
{
    [Inject] private StreamSystem StreamSystem { get; set; }
    [Inject] private SerializableHeroes SerializableHeroes { get; set; }

    public override void OnEnable()
    {
        base.OnEnable();

        StreamSystem.ItemEquippedStream.Subscribe(item =>
        {
            foreach(var entity in SerializableHeroes.Entities)
            {
                var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>();
                var heroComponent = entity.GetComponent<HeroComponent>();


            }
        }).AddTo(this.Disposer);

        StreamSystem.ItemUnequippedStream.Subscribe(item =>
        {

        }).AddTo(this.Disposer);
    }
}
