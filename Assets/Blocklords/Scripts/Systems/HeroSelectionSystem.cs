using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS;
using AlphaECS.Unity;
using System;
using UniRx;
using System.Linq;
using UniRx.Triggers;

public class HeroSelectionSystem : SystemBehaviour
{
    private IGroup selectableHeroes;
    private int isSelectedHash;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        selectableHeroes = this.CreateGroup(new HashSet<Type>() { typeof(HeroComponent), typeof(SelectableComponent), typeof(Animator) });
        isSelectedHash = Animator.StringToHash("IsSelected");
    }

    public override void OnEnable()
    {
        base.OnEnable();

        selectableHeroes.OnAdd().Subscribe(entity =>
        {
            var selectableComponent = entity.GetComponent<SelectableComponent>();
            var animator = entity.GetComponent<Animator>();

            selectableComponent.SelectionAreas.Select(t => t.OnPointerClickAsObservable()).Merge().Subscribe(_ =>
            {
                foreach(var e in selectableHeroes.Entities)
                {
                    if (e == entity) { continue; }
                    e.GetComponent<SelectableComponent>().IsSelected.Value = false;
                }
                selectableComponent.IsSelected.Value = true;
            }).AddTo(this.Disposer);

            selectableComponent.IsSelected.DistinctUntilChanged().Subscribe(value =>
            {
                animator.SetBool(isSelectedHash, value);
            }).AddTo(this.Disposer).AddTo(selectableComponent.Disposer);
                               
        }).AddTo(this.Disposer);
    }
}
