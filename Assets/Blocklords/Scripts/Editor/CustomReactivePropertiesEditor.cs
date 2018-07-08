using UniRx;

[UnityEditor.CustomPropertyDrawer(typeof(FadeStateReactiveProperty))]
[UnityEditor.CustomPropertyDrawer(typeof(HeroComponentReactiveProperty))]
[UnityEditor.CustomPropertyDrawer(typeof(HeroClassReactiveProperty))]
[UnityEditor.CustomPropertyDrawer(typeof(ItemTypeReactiveProperty))]
[UnityEditor.CustomPropertyDrawer(typeof(ItemQualityReactiveProperty))]
public partial class ExtendedInspectorDisplayDrawer : InspectorDisplayDrawer
{
}