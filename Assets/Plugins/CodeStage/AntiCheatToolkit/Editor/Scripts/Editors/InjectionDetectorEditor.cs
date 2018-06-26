#if UNITY_EDITOR
namespace CodeStage.AntiCheat.EditorCode.Editors
{
	using Detectors;
	using Windows;

	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof (InjectionDetector))]
	internal class InjectionDetectorEditor : ActDetectorEditor
	{
		protected override void DrawUniqueDetectorProperties()
		{
			if (!EditorPrefs.GetBool(ActEditorGlobalStuff.PrefsInjectionEnabled))
			{
				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("Injection Detector is not enabled!", EditorStyles.boldLabel);
				if (GUILayout.Button("Enable in Settings..."))
				{
					ActSettings.ShowWindow();
				}
			}
			else if (!ActPostprocessor.IsInjectionDetectorTargetCompatible())
			{
				EditorGUILayout.LabelField("Injection Detector disabled for this platform.", EditorStyles.boldLabel);
			}
		}
	}
}
#endif