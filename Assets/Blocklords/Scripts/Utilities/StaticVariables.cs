using UnityEngine;
using System;
using System.Collections.Generic;

public static class StaticVariables
{
	public static int Color = Shader.PropertyToID("_Color");

    public const string MainCamera = "MainCamera";

	public static TimeSpan TS_0_05 = TimeSpan.FromSeconds(0.05f);
	public static TimeSpan TS_0_25 = TimeSpan.FromSeconds(0.25f);
	public static TimeSpan TS_0_5 = TimeSpan.FromSeconds(0.5f);
	public static TimeSpan TS_0_75 = TimeSpan.FromSeconds(0.25f);

    public const string SceneFileExtension = ".unity";
}
