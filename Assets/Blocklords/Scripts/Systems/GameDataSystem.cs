using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using AlphaECS;
using CodeStage.AntiCheat.ObscuredTypes;
using UniRx;
using System;
using System.Linq;
using Zenject;

public static class PlayerKeys
{
    public static string PlayerID = "PlayerID";
    public static string PlayerIDs = "PlayerIDs";

    public static string SelectedPlayerID = "SelectedPlayerID";

    public static string HeroID = "HeroID";
    public static string HeroIDs = "HeroIDs";

    public static string SelectedHeroID = "SelectedHeroID";

    public static string ItemID = "ItemID";
    public static string ItemIDs = "ItemIDs";
}

public class GameDataSystem : SystemBehaviour
{
    [Inject] private CorePlayers CorePlayers { get; set; }
    [Inject] private Heroes Heroes { get; set; }
    [Inject] private SerializableHeroes SerializableHeroes { get; set; }

    [SerializeField] private GameObject playerDataPrefab;
    [SerializeField] private GameObject heroDataPrefab;
    [SerializeField] private GameObject itemDataPrefab;

    //for bootstrapping player entities on load
    //[SerializeField] private string[] playerIDs;
    //[SerializeField] private string[] heroIDs;
    public string[] playerIDs;
    public string[] heroIDs;
    public string[] itemIDs;

    //private IGroup serializableItems;

    public PlayerDataComponentReactiveProperty SelectedPlayerData;
    public EntityReactiveProperty SelectedHero;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        var json = ObscuredPrefs.GetString(PlayerKeys.PlayerIDs);
        playerIDs = string.IsNullOrEmpty(json) || JsonHelper.FromJson<string>(json).ToArray().Length <= 0 ? new[] { "default" } : JsonHelper.FromJson<string>(json).ToArray();

        json = ObscuredPrefs.GetString(PlayerKeys.HeroIDs);
        heroIDs = string.IsNullOrEmpty(json) ? new[] { "" } : JsonHelper.FromJson<string>(json).ToArray();

        json = ObscuredPrefs.GetString(PlayerKeys.ItemIDs);
        itemIDs = string.IsNullOrEmpty(json) ? new[] { "" } : JsonHelper.FromJson<string>(json).ToArray();

        //serializableItems = this.CreateGroup(new HashSet<Type>() { typeof(HeroComponent), typeof(PlayerDataComponent), });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        //HACK + TODO -> remove the delay and check
        CorePlayers.OnAdd().Subscribe(entity =>
        {
            var playerDataComponent = entity.GetComponent<PlayerDataComponent>();

            var previousValue = playerDataComponent.ID.Value;
            playerDataComponent.ID.DistinctUntilChanged().Subscribe(value =>
            {
                //check we're not on the initial value, ie, setting up a playerDataComponent that's just been instantiated from the list...
                //... and ensure the id isn't already being used
                //if(value != previousValue && players.Entities.Where(e => e != entity).Any(e => e.GetComponent<PlayerDataComponent>().ID.Value == value))
                //{
                //    Debug.LogWarning("ID " + value + " ALREADY EXISTS - REVERTING!");
                //    playerDataComponent.ID.Value = previousValue;
                //}

                playerIDs = CorePlayers.Entities.Select(e => e.GetComponent<PlayerDataComponent>().ID.Value).Distinct().ToArray();
                var json = JsonHelper.ToJson<string>(playerIDs);
                ObscuredPrefs.SetString(PlayerKeys.PlayerIDs, json);
                ObscuredPrefs.SetString(PlayerKeys.PlayerID + playerDataComponent.ID.Value, entity.Serialize());

                previousValue = playerDataComponent.ID.Value;
            }).AddTo(this.Disposer).AddTo(playerDataComponent.Disposer);

        }).AddTo(this.Disposer);

        CorePlayers.OnRemove().Subscribe(entity =>
        {
            playerIDs = CorePlayers.Entities.Select(e => e.GetComponent<PlayerDataComponent>().ID.Value).Distinct().ToArray();
            var json = JsonHelper.ToJson(playerIDs.ToArray());
            ObscuredPrefs.SetString(PlayerKeys.PlayerIDs, json);
        }).AddTo(this.Disposer);

        foreach (var id in playerIDs)
        { CreatePlayer(id); }

        foreach (var id in heroIDs)
        { CreateHero(id); }

        //foreach (var id in itemIDs)
        //{ CreateItem(id); } 

        var selectedPlayerID = ObscuredPrefs.GetString(PlayerKeys.SelectedPlayerID);
        var playerCount = 0;
        CorePlayers.OnAdd().TakeWhile(_ => SelectedPlayerData.Value == null).Subscribe(entity =>
        {
            var playerDataComponent = entity.GetComponent<PlayerDataComponent>();
            //if the ids match, or we're at the last player
            if(playerDataComponent.ID.Value == selectedPlayerID || playerCount == playerIDs.Length - 1)
            {
                SelectedPlayerData.Value = playerDataComponent;
            }
            playerCount += 1;
        }).AddTo(this.Disposer);

        IDisposable getPlayerIDAsync = null;
        SelectedPlayerData.DistinctUntilChanged().Subscribe(pdc =>
        {
            if(getPlayerIDAsync != null)
            { getPlayerIDAsync.Dispose(); }

            getPlayerIDAsync = pdc.ID.DistinctUntilChanged().Subscribe(id =>
            {
                ObscuredPrefs.SetString(PlayerKeys.SelectedPlayerID, id);
            }).AddTo(this.Disposer).AddTo(pdc.Disposer);
        }).AddTo(this.Disposer);

        var selectedHeroID = ObscuredPrefs.GetString(PlayerKeys.SelectedHeroID);
        var heroCount = 0;
        SerializableHeroes.OnAdd().Subscribe(entity =>
        {
            var heroComponent = entity.GetComponent<HeroComponent>();
            var playerDataComponent = entity.GetComponent<PlayerDataComponent>();

            if(string.IsNullOrEmpty(playerDataComponent.ID.Value))
            {
                playerDataComponent.ID.Value = SelectedPlayerData.Value.ID.Value;
            }

            heroComponent.ID.DistinctUntilChanged().Subscribe(_ =>
            {
                heroIDs = SerializableHeroes.Entities.Select(e => e.GetComponent<HeroComponent>().ID.Value).Distinct().ToArray();
                var json = JsonHelper.ToJson<string>(heroIDs);
                ObscuredPrefs.SetString(PlayerKeys.HeroIDs, json);
                ObscuredPrefs.SetString(PlayerKeys.HeroID + heroComponent.ID.Value, entity.Serialize());
            }).AddTo(this.Disposer);

            //if the ids match, or we're at the last hero
            if (heroComponent.ID.Value == selectedHeroID || heroCount == heroIDs.Length - 1)
            {
                SelectedHero.Value = entity;
            }
            heroCount += 1;

        }).AddTo(this.Disposer);

        IDisposable getHeroIDAsync = null;
        SelectedHero.DistinctUntilChanged().Subscribe(heroEntity =>
        {
            if (getHeroIDAsync != null)
            { getHeroIDAsync.Dispose(); }

            var heroComponent = heroEntity.GetComponent<HeroComponent>();

            getHeroIDAsync = heroComponent.ID.DistinctUntilChanged().Subscribe(id =>
            {
                ObscuredPrefs.SetString(PlayerKeys.SelectedHeroID, id);
            }).AddTo(this.Disposer).AddTo(heroComponent.Disposer);
        }).AddTo(this.Disposer);

        SerializableHeroes.OnRemove().Subscribe(entity =>
        {
            heroIDs = SerializableHeroes.Entities.Select(e => e.GetComponent<HeroComponent>().ID.Value).Distinct().ToArray();
            var json = JsonHelper.ToJson<string>(heroIDs);
            ObscuredPrefs.SetString(PlayerKeys.HeroIDs, json);
        }).AddTo(this.Disposer);
    }

    private void CreatePlayer(string id)
    {
        var json = ObscuredPrefs.GetString(PlayerKeys.PlayerID + id);
        var entity = PoolManager.GetPool().CreateEntity();
        PrefabFactory.Instantiate(entity, playerDataPrefab, this.transform);
        entity.Deserialize(json);

        //HACK -> force a change here, because we get an OnChanged() fired when the entity is instantiated...
        //... but not on Deserialization, causing things to get out of sync on initial load
        entity.GetComponent<PlayerDataComponent>().ID.SetValueAndForceNotify(entity.GetComponent<PlayerDataComponent>().ID.Value);
    }

    private void CreateHero(string id)
    {
        var json = ObscuredPrefs.GetString(PlayerKeys.HeroID + id);
        var entity = PoolManager.GetPool().CreateEntity();
        PrefabFactory.Instantiate(entity, heroDataPrefab, this.transform);
        entity.Deserialize(json);

        //HACK -> force a change here, because we get an OnChanged() fired when the entity is instantiated...
        //... but not on Deserialization, causing things to get out of sync on initial load
        entity.GetComponent<HeroComponent>().ID.SetValueAndForceNotify(entity.GetComponent<HeroComponent>().ID.Value);
    }

    private void CreateItem(string id)
    {
        var json = ObscuredPrefs.GetString(PlayerKeys.ItemID + id);
        var entity = PoolManager.GetPool().CreateEntity();
        PrefabFactory.Instantiate(entity, itemDataPrefab, this.transform);
        entity.Deserialize(json);

        //HACK -> force a change here, because we get an OnChanged() fired when the entity is instantiated...
        //... but not on Deserialization, causing things to get out of sync on initial load
        //entity.GetComponent<ItemCollectionComponent>().ID.SetValueAndForceNotify(entity.GetComponent<ItemCollectionComponent>().ID.Value);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        //playerIDs.Clear();
    }
}
