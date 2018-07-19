using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using AlphaECS;
using System;
using UniRx;
using Zenject;

public class HeroViewSystem : SystemBehaviour
{
    [Inject] private StreamSystem StreamSystem { get; set; }
    private IGroup heroViews;

    [SerializeField] private string iconResourcePrefix;
    [SerializeField] private string iconResourceSuffix;

    private Dictionary<string, Sprite> headIDSpritesTable = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> bodyIDSpritesTable = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> leftArmIDSpritesTable = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> rightArmIDSpritesTable = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> shieldIDSpritesTable = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> weaponIDSpritesTable = new Dictionary<string, Sprite>();

    //private Dictionary<ItemType, string> itemTypePaths = new Dictionary<ItemType, string>()
    //{
    //    { ItemType.Head, "head_0" },
    //    { ItemType.Body, "body_0" },
    //    { ItemType.Hands, "hands_0" },
    //    { ItemType.Shield, "shield_0" },
    //    { ItemType.Weapon, "weapon_0" },
    //};

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        heroViews = this.CreateGroup(new HashSet<Type> { typeof(HeroComponent), typeof(ItemCollectionComponent), typeof(HeroViewComponent) });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        heroViews.OnAdd().Subscribe(entity =>
        {
            var itemCollection = entity.GetComponent<ItemCollectionComponent>();
            //itemCollection.Items.
        }).AddTo(this.Disposer);

        StreamSystem.ItemEquippedStream.Subscribe(evt =>
        {
            var heroView = evt.Collection.GetComponent<HeroViewComponent>();
            if (heroView == null) { return; }

            var index = GetIndex(evt.Item.ID.Value);
            if(evt.Item.ItemType.Value == ItemType.Head)
            {
                var path = iconResourcePrefix + "head_0" + index + iconResourceSuffix;
                SetImage(path, headIDSpritesTable, heroView.HeadImage);
            }
            else if(evt.Item.ItemType.Value == ItemType.Body)
            {
                var path = iconResourcePrefix + "body_0" + index + iconResourceSuffix;
                SetImage(path, bodyIDSpritesTable, heroView.BodyImage); 
            }
            else if (evt.Item.ItemType.Value == ItemType.Hands)
            {
                var path = iconResourcePrefix + "arm_left_0" + index + iconResourceSuffix;
                SetImage(path, leftArmIDSpritesTable, heroView.LeftArmImage);

                path = iconResourcePrefix + "arm_right_0" + index + iconResourceSuffix;
                SetImage(path, rightArmIDSpritesTable, heroView.RightArmImage);
            }
            else if (evt.Item.ItemType.Value == ItemType.Shield)
            {
                var path = iconResourcePrefix + "shield_0" + index + iconResourceSuffix;
                SetImage(path, shieldIDSpritesTable, heroView.ShieldImage);
            }
            else if (evt.Item.ItemType.Value == ItemType.Weapon)
            {
                var path = iconResourcePrefix + "weapon_0" + index + iconResourceSuffix;
                SetImage(path, weaponIDSpritesTable, heroView.WeaponImage);
            }
        }).AddTo(this.Disposer);
    }

    //HACK!!!
    private int GetIndex(string id)
    {
        var index = int.Parse(id.Substring(4, id.Length - 4));
        index = index % 4;
        if (index == 0) { index = 4; }
        return index;
    }

    private void SetImage(string path, Dictionary<string, Sprite> table, UnityEngine.UI.Image image)
    {
        if (!table.ContainsKey(path))
        {
            var sprite = Resources.Load<Sprite>(path);
            table.Add(path, sprite);
        }
        image.sprite = table[path];
    }
}
