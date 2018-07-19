using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using AlphaECS;
using Zenject;
using UniRx;
using TMPro;
using System;

public class StatsDisplaySystem : SystemBehaviour
{
    [Inject] private GameDataSystem GameDataSystem { get; set; }
    [Inject] private StreamSystem StreamSystem { get; set; }

    [SerializeField] private StatsText statsText;

    public override void OnEnable()
    {
        base.OnEnable();

        IDisposable updateText = null;
        GameDataSystem.SelectedHero.DistinctUntilChanged().Where(e => e != null).Subscribe(entity =>
        {
            if(updateText != null)
            { updateText.Dispose(); }

            var heroComponent = entity.GetComponent<HeroComponent>();
            updateText = heroComponent.ModifierStats.ObserveAdd().Select(_ => true)
                                      .Merge(heroComponent.ModifierStats.ObserveRemove().Select(_ => true))
                                      .StartWith(true)
            .Subscribe(_ =>
            {
                UpdateText();
            }).AddTo(this.Disposer).AddTo(heroComponent.Disposer);
        }).AddTo(this.Disposer);
    }

    private void UpdateText()
    {
        var heroComponent = GameDataSystem.SelectedHero.Value.GetComponent<HeroComponent>();
        statsText.Update(heroComponent.BaseStats, heroComponent.ModifierStats);
    }
}
