using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnimatorIntDelegateTable))]
[CustomPropertyDrawer(typeof(ClassStatsTable))]
[CustomPropertyDrawer(typeof(ClassStatsWrapperTable))]
[CustomPropertyDrawer(typeof(ItemsTransformsTable))]
public class CustomSerializableDictionaries : SerializableDictionaryPropertyDrawer { }