using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System;

//had wanted to use SOs for stats, but when serializing objects which use them as fields, they use the instanceID rather than fully serializing the object
//so we'll split here and mostly use Stats as POCO, with a separate wrapper class to be used as needed
[CreateAssetMenu(fileName = "Stats Wrapper", menuName = "Stats Wrapper", order = 1)]
public class StatsWrapper : ScriptableObject
{
    public Stats Stats;
}
