using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;

public class HeroCollectionComponent : ComponentBehaviour
{
    public HeroTable Heroes = new HeroTable();
}

public class HeroTable : SerializableDictionary<string, HeroComponent> { }
