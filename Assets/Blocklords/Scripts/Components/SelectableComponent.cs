using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using UniRx;

public class SelectableComponent : ComponentBehaviour
{
    public Transform[] SelectionAreas;
    public BoolReactiveProperty IsSelected = new BoolReactiveProperty();
}
