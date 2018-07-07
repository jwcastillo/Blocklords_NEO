using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlphaECS.Unity;
using AlphaECS;
using CodeStage.AntiCheat.ObscuredTypes;
using UniRx;
using System;
using System.Linq;

public static class PlayerKeys
{
    public static string PlayerID = "PlayerID";
    public static string PlayerIDs = "PlayerIDs";

    public static string SelectedID = "SelectedID";

    public static string HeroID = "HeroID";
    public static string HeroIDs = "HeroIDs";
}

public class GameDataSystem : SystemBehaviour
{
    [SerializeField] private GameObject playerDataPrefab;
    [SerializeField] private GameObject heroDataPrefab;

    //for bootstrapping player entities on load
    //[SerializeField] private string[] playerIDs;
    //[SerializeField] private string[] heroIDs;
    public string[] playerIDs;
    public string[] heroIDs;

    private IGroup players;
    private IGroup serializableHeroes;

    [SerializeField] private PlayerDataComponentReactiveProperty SelectedPlayerData;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        var json = ObscuredPrefs.GetString(PlayerKeys.PlayerIDs);
        if(string.IsNullOrEmpty(json))
        {
            playerIDs = new[] { "default" };
        }
        else
        {
            playerIDs = JsonHelper.FromJson<string>(json).ToArray();
        }

        json = ObscuredPrefs.GetString(PlayerKeys.HeroIDs);
        heroIDs = JsonHelper.FromJson<string>(json).ToArray();
        //if (string.IsNullOrEmpty(json))
        //{
        //    heroIDs = new[] { "default" };
        //}
        //else
        //{
        //    heroIDs = JsonHelper.FromJson<string>(json).ToArray();
        //}

        players = this.CreateGroup(new HashSet<Type>() { typeof(PlayerDataComponent), });
        serializableHeroes = this.CreateGroup(new HashSet<Type>() { typeof(HeroComponent), typeof(PlayerDataComponent), });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        //HACK + TODO -> remove the delay and check
        players.OnAdd().DelayFrame(1).Where(entity => !entity.HasComponent<HeroComponent>()).Subscribe(entity =>
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

                playerIDs = players.Entities.Select(e => e.GetComponent<PlayerDataComponent>().ID.Value).Distinct().ToArray();
                var json = JsonHelper.ToJson<string>(playerIDs);
                ObscuredPrefs.SetString(PlayerKeys.PlayerIDs, json);
                ObscuredPrefs.SetString(PlayerKeys.PlayerID + playerDataComponent.ID.Value, entity.Serialize());

                previousValue = playerDataComponent.ID.Value;
            }).AddTo(this.Disposer).AddTo(playerDataComponent.Disposer);

        }).AddTo(this.Disposer);

        players.OnRemove().Subscribe(entity =>
        {
            playerIDs = players.Entities.Select(e => e.GetComponent<PlayerDataComponent>().ID.Value).Distinct().ToArray();
            var json = JsonHelper.ToJson(playerIDs.ToArray());
            ObscuredPrefs.SetString(PlayerKeys.PlayerIDs, json);
        }).AddTo(this.Disposer);

        foreach (var id in playerIDs)
        {
            CreatePlayer(id);
        }

        foreach (var id in heroIDs)
        {
            CreateHero(id);
        } 

        var selectedID = ObscuredPrefs.GetString(PlayerKeys.SelectedID);
        var count = 0;
        IDisposable getSelectedPlayerAsync = null;
        getSelectedPlayerAsync = players.OnAdd().TakeWhile(_ => SelectedPlayerData.Value == null).Subscribe(entity =>
        {
            var playerDataComponent = entity.GetComponent<PlayerDataComponent>();
            //if the ids match, or we're at the last player
            if(playerDataComponent.ID.Value == selectedID || count == playerIDs.Length - 1)
            {
                SelectedPlayerData.Value = playerDataComponent;
            }
            count += 1;
        }).AddTo(this.Disposer);

        IDisposable getPlayerIDAsync = null;
        SelectedPlayerData.DistinctUntilChanged().Subscribe(pdc =>
        {
            if(getPlayerIDAsync != null)
            { getPlayerIDAsync.Dispose(); }

            getPlayerIDAsync = pdc.ID.DistinctUntilChanged().Subscribe(id =>
            {
                selectedID = id;
                ObscuredPrefs.SetString(PlayerKeys.SelectedID, id);
            }).AddTo(this.Disposer).AddTo(pdc.Disposer);
        }).AddTo(this.Disposer);

        serializableHeroes.OnAdd().Subscribe(entity =>
        {
            var heroComponent = entity.GetComponent<HeroComponent>();
            var playerDataComponent = entity.GetComponent<PlayerDataComponent>();

            if(string.IsNullOrEmpty(playerDataComponent.ID.Value))
            {
                playerDataComponent.ID.Value = SelectedPlayerData.Value.ID.Value;
            }

            heroComponent.ID.DistinctUntilChanged().Subscribe(_ =>
            {
                heroIDs = serializableHeroes.Entities.Select(e => e.GetComponent<HeroComponent>().ID.Value).Distinct().ToArray();
                var json = JsonHelper.ToJson<string>(heroIDs);
                ObscuredPrefs.SetString(PlayerKeys.HeroIDs, json);
                ObscuredPrefs.SetString(PlayerKeys.HeroID + heroComponent.ID.Value, entity.Serialize());
            }).AddTo(this.Disposer);

        }).AddTo(this.Disposer);

        serializableHeroes.OnRemove().Subscribe(entity =>
        {
            heroIDs = serializableHeroes.Entities.Select(e => e.GetComponent<HeroComponent>().ID.Value).Distinct().ToArray();
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

    public override void OnDisable()
    {
        base.OnDisable();

        //playerIDs.Clear();
    }
}
