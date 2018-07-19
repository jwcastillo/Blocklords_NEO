using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using Zenject;
using UniRx;
using AlphaECS;
using System.Linq;

public class DefaultItemSetupSystem : SystemBehaviour
{
    [Inject] private GameDataSystem GameDataSystem { get; set; }

    [Inject] private CorePlayers CorePlayers { get; set; }
    [Inject] private SerializableHeroes SerializableHeroes { get; set; }

    public override void OnEnable()
    {
        base.OnEnable();

        CorePlayers.OnAdd().Subscribe(entity =>
        {
            var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>();
            foreach(var itemWrapper in GameDataSystem.ItemWrappers)
            {
                itemCollectionComponent.Items.Add(itemWrapper.Item);
            }
        }).AddTo(this.Disposer);

        //SerializableHeroes.OnAdd().Subscribe(entity =>
        //{
        //    var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>();
        //    foreach (var itemWrapper in GameDataSystem.ItemWrappers)
        //    {
        //        itemCollectionComponent.Items.Add(itemWrapper.Item);
        //    }
        //}).AddTo(this.Disposer);
    }
}
