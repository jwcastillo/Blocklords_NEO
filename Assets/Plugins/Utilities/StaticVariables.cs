using UnityEngine;
using System;
using System.Collections.Generic;

public static partial class StaticVariables
{
	public static int Color = Shader.PropertyToID("_Color");

    public const string MainCamera = "MainCamera";

	public static TimeSpan TS_0_05 = TimeSpan.FromSeconds(0.05f);
	public static TimeSpan TS_0_25 = TimeSpan.FromSeconds(0.25f);
	public static TimeSpan TS_0_5 = TimeSpan.FromSeconds(0.5f);
	public static TimeSpan TS_0_75 = TimeSpan.FromSeconds(0.25f);

    public const string SceneFileExtension = ".unity";
}

public static class PanelStates
{
    public const int Default = 0;
    public const int Enabled = 1;
    public const int Disabled = 2;
}

public static class PanelParameters
{
    public static int State = Animator.StringToHash("State");
}
