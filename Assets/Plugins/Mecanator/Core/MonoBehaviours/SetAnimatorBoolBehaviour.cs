using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using SubjectNerd.Utilities;

public class SetAnimatorBoolBehaviour : MonoBehaviour
{
    [Reorderable] [SerializeField] private List<AnimatorBool> bools = new List<AnimatorBool>();
    [SerializeField] private AnimatorSetBoolEvent delegates;

    private void Awake()
    {
        foreach(var ab in bools)
        {
            ab.ParameterHash = Animator.StringToHash(ab.ParameterName);
        }
    }

    public void Execute()
    {
        foreach (var ab in bools)
        {
            delegates.Invoke(ab.ParameterHash, ab.Value);
        }
    }
}

[Serializable]
public class AnimatorSetBoolEvent : UnityEvent<int, bool> { }

[Serializable]
public class AnimatorBool
{
    public string ParameterName;
    public bool Value;
    [HideInInspector] public int ParameterHash;
}
