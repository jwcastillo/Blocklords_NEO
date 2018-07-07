using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System;

[Serializable]
public class Stats
{
    public IntReactiveProperty Leadership = new IntReactiveProperty();
    public IntReactiveProperty Intelligence = new IntReactiveProperty();
    public IntReactiveProperty Strength = new IntReactiveProperty();
    public IntReactiveProperty Defense = new IntReactiveProperty();
    public IntReactiveProperty Speed = new IntReactiveProperty();
}
