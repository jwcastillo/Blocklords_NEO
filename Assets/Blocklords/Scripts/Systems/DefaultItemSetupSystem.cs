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
    [Inject] private CorePlayers CorePlayers { get; set; }

    [SerializeField] private List<ItemWrapper> defaultItemWrappers = new List<ItemWrapper>();

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        defaultItemWrappers = Resources.LoadAll<ItemWrapper>("Items").ToList();
    }

    public override void OnEnable()
    {
        base.OnEnable();

        CorePlayers.OnAdd().Subscribe(entity =>
        {
            var itemCollectionComponent = entity.GetComponent<ItemCollectionComponent>();
            foreach(var itemWrapper in defaultItemWrappers)
            {
                itemCollectionComponent.Items.Add(itemWrapper.Item);
            }
        }).AddTo(this.Disposer);
    }
}
