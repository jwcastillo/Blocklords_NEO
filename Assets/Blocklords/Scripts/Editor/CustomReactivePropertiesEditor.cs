using UniRx;

[UnityEditor.CustomPropertyDrawer(typeof(FadeStateReactiveProperty))]
[UnityEditor.CustomPropertyDrawer(typeof(HeroComponentReactiveProperty))]
[UnityEditor.CustomPropertyDrawer(typeof(HeroClassReactiveProperty))]
public partial class ExtendedInspectorDisplayDrawer : InspectorDisplayDrawer
{
}