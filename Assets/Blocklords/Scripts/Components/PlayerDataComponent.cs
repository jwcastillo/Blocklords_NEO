using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using UniRx;
using System;
using AlphaECS;

public class PlayerDataComponent : ComponentBehaviour
{
    public StringReactiveProperty ID = new StringReactiveProperty();
    public StringReactiveProperty Name = new StringReactiveProperty();
}

[Serializable]
public class PlayerDataComponentReactiveProperty : ReactiveProperty<PlayerDataComponent>
{
    public PlayerDataComponentReactiveProperty() { }
    public PlayerDataComponentReactiveProperty(PlayerDataComponent initialValue) : base(initialValue) { }
}
