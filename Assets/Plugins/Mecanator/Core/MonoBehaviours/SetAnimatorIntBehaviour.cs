using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using SubjectNerd.Utilities;

public class SetAnimatorIntBehaviour : MonoBehaviour
{
    //[Reorderable] [SerializeField] private List<AnimatorInt> ints = new List<AnimatorInt>();
    //[SerializeField] private AnimatorSetIntEvent delegates;

    [SerializeField] private AnimatorIntDelegateTable actions;

    private void Awake()
    {
        foreach (var ai in actions.Keys)
        {
            ai.ParameterHash = Animator.StringToHash(ai.ParameterName);
        }
    }

    public void Execute()
    {
        foreach (var kvp in actions)
        {
            kvp.Value.Invoke(kvp.Key.ParameterHash, kvp.Key.Value);
        }
    }
}

[Serializable]
public class AnimatorSetIntEvent : UnityEvent<int, int> { }

[Serializable]
public class AnimatorInt
{
    public string ParameterName;
    public int Value;
    [HideInInspector] public int ParameterHash;
}

[Serializable]
public class AnimatorIntDelegateTable : SerializableDictionary<AnimatorInt, AnimatorSetIntEvent> { }