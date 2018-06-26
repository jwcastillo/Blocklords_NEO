using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities;
using SubjectNerd.Utilities;

public class MultiSceneSetup : ScriptableObject
{
	[Reorderable]
	public SceneSetup[] Setups;
}

namespace Utilities
{
	[System.Serializable]
	public class SceneSetup
	{
		public bool isActive;
		public bool isLoaded;
		public string path;
	}
}