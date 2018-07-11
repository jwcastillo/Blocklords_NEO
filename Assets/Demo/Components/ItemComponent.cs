using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using UniRx;

public class ItemComponent : ComponentBehaviour
{
    public StringReactiveProperty ID = new StringReactiveProperty();
    public StringReactiveProperty Name = new StringReactiveProperty();

    public IntReactiveProperty Leadership = new IntReactiveProperty();
    public IntReactiveProperty Intelligence = new IntReactiveProperty();
    public IntReactiveProperty Strength = new IntReactiveProperty();
    public IntReactiveProperty Defense = new IntReactiveProperty();
    public IntReactiveProperty Speed = new IntReactiveProperty();
}
