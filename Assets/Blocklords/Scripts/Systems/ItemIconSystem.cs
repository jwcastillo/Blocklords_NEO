using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using AlphaECS;
using System;
using UniRx;

public class ItemIconSystem : SystemBehaviour
{
    private IGroup itemIcons;

    [SerializeField] private string iconResourcePrefix;
    [SerializeField] private string iconResourceSuffix;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        itemIcons = this.CreateGroup(new HashSet<Type>() { typeof(ItemIconComponent) });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        itemIcons.OnAdd().Subscribe(entity =>
        {
            var itemIconComponent = entity.GetComponent<ItemIconComponent>();

            itemIconComponent.Item.DistinctUntilChanged().Where(item => item != null && !string.IsNullOrEmpty(item.ID.Value)).Subscribe(item =>
            {
                var path = iconResourcePrefix + item.ID + iconResourceSuffix;
                var sprite = Resources.Load<Sprite>(path);
                itemIconComponent.Icon.sprite = sprite;
                itemIconComponent.Text.text = item.Name.Value + "\n" + item.Description.Value;
            }).AddTo(this.Disposer);
        }).AddTo(this.Disposer);
    }
}
