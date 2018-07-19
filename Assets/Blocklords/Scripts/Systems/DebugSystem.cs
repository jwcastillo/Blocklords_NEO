using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugSystem : SystemBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Utilities/Clear Data")]
    public static void ClearData()
    {
        PlayerPrefs.DeleteAll();
    }
#endif
}
