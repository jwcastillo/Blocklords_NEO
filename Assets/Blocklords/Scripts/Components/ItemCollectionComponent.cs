using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using SubjectNerd.Utilities;
using UniRx;
using System.Linq;

public class ItemCollectionComponent : ComponentBehaviour
{
    //[HideInInspector] public ReactiveCollection<Item> Items = new ReactiveCollection<Item>();
    [Reorderable] public List<Item> Items = new List<Item>(); 

//#if UNITY_EDITOR
//    [SerializeField] [Reorderable] private List<Item> items = new List<Item>();

//    //public ItemCollection ItemsRx = new ItemCollection();

//    //NOTE -> want to use awake here but for some reason the inspector does not refresh correctly when doing this on startup
//    //private void Awake()
//    private void Start()
//    {
//        Items.ObserveAdd().Select(_ => true).Merge(Items.ObserveRemove().Select(_ => true)).StartWith(true).Subscribe(_ =>
//        {
//            items.Clear();
//            foreach(var item in Items)
//            {
//                items.Add(item);
//            }
//        }).AddTo(this.Disposer);
//    }
//#endif
}

//[System.Serializable]
//public class ItemCollection : ReactiveCollection<Item> { }
