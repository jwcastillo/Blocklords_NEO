using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using UniRx;

public class PlayerDataComponent : ComponentBehaviour
{
    public StringReactiveProperty ID = new StringReactiveProperty();
}
