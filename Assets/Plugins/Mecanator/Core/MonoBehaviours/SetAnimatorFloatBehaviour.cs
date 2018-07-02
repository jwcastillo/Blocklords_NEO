using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using SubjectNerd.Utilities;

public class SetAnimatorFloatBehaviour : MonoBehaviour
{
    [Reorderable] [SerializeField] private List<AnimatorFloat> floats = new List<AnimatorFloat>();
    [SerializeField] private AnimatorSetFloatEvent delegates;

    private void Awake()
    {
        foreach (var af in floats)
        {
            af.ParameterHash = Animator.StringToHash(af.ParameterName);
        }
    }

    public void Execute()
    {
        foreach (var af in floats)
        {
            delegates.Invoke(af.ParameterHash, af.Value);
        }
    }
}

[Serializable]
public class AnimatorSetFloatEvent : UnityEvent<int, float> { }

[SerializeField]
public class AnimatorFloat
{
    public string ParameterName;
    public float Value;
    [HideInInspector] public int ParameterHash;
}