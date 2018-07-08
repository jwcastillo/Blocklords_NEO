using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using Zenject;

public class ItemSetupSystem : SystemBehaviour
{
    [Inject] private SerializableHeroes SerializableHeroes { get; set; }

    public override void OnEnable()
    {
        base.OnEnable();


    }
}