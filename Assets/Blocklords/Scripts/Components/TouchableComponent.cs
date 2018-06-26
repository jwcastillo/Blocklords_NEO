using UnityEngine;
using AlphaECS.Unity;
using UniRx;

public class TouchableComponent : ComponentBehaviour
{
    public Transform[] TouchAreas;
    public BoolReactiveProperty IsTouchable = new BoolReactiveProperty(true);
}
