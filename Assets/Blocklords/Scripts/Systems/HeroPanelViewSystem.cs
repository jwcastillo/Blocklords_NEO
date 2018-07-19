using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using Zenject;
using UniRx;

public class HeroPanelViewSystem : SystemBehaviour
{
    [Inject] private GameDataSystem GameDataSystem { get; set; }
    [Inject] private StreamSystem StreamSystem { get; set; }

    [SerializeField] private GameObject heroCardPrefab;
    [SerializeField] private Transform heroCardParent;
    private GameObject heroCardInstance;

    public override void OnEnable()
    {
        base.OnEnable();

        //HACK + TODO -> make better use of HeroViewSystem, or consider merging these 
        StreamSystem.ItemEquippedStream.Subscribe(evt =>
        {
            var entity = evt.Collection;
            var heroComponent = entity.GetComponent<HeroComponent>();
            var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>();

            if (heroCardInstance != null)
            {
                Destroy(heroCardInstance);
            }

            var cardEntity = PoolManager.GetPool().CreateEntity();
            heroCardInstance = PrefabFactory.Instantiate(cardEntity, heroCardPrefab, heroCardParent);

            var targetItemCollectionComponent = heroCardInstance.AddComponent<ItemCollectionComponent>();
            var itemData = JsonUtility.ToJson(itemCollectionComponent);

            JsonUtility.FromJsonOverwrite(itemData, targetItemCollectionComponent);

            cardEntity.AddComponent(targetItemCollectionComponent);
        }).AddTo(this.Disposer);

        //GameDataSystem.SelectedHero.DistinctUntilChanged().Where(entity => entity != null).Subscribe(entity =>
        //{
        //    var heroComponent = entity.GetComponent<HeroComponent>();
        //    var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>();

        //    if (heroCardInstance != null)
        //    {
        //        Destroy(heroCardInstance);
        //    }

        //    //var cardEntity = PoolManager.GetPool().CreateEntity();
        //    heroCardInstance = PrefabFactory.Instantiate(entity, heroCardPrefab, heroCardParent);

        //    var targetItemCollectionComponent = heroCardInstance.AddComponent<ItemCollectionComponent>();
        //    var itemData = JsonUtility.ToJson(itemCollectionComponent);

        //    JsonUtility.FromJsonOverwrite(itemData, targetItemCollectionComponent);

        //    entity.AddComponent(targetItemCollectionComponent);

        //}).AddTo(this.Disposer);
    }
}
