using UnityEngine;
using UnityEngine.Events;
using System;

public class SetAnimatorTriggerBehaviour : MonoBehaviour
{
    public AnimatorSetTriggerEvent SetTrigger;

    public void Execute()
    {

    }
}

[Serializable]
public class AnimatorSetTriggerEvent : UnityEvent<string> { }
