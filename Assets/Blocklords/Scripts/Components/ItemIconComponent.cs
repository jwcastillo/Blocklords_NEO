using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using UnityEngine.UI;
using TMPro;

public class ItemIconComponent : ComponentBehaviour
{
    public ItemReactiveProperty Item = new ItemReactiveProperty();
    public Image Icon;
    public TextMeshProUGUI Text;
}
