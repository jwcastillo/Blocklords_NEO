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
    public static string IDs = "PlayerIDs";
    public static string SelectedID = "SelectedID";
    public static string Hero = "Hero";
}

public class GameDataSystem : SystemBehaviour
{
    [SerializeField] private GameObject playerDataPrefab;

    //for bootstrapping player entities on load
    [SerializeField] private string[] playerIDs;

    private IGroup players;
    private IGroup serializableHeroes;

    [SerializeField] private PlayerDataComponentReactiveProperty SelectedPlayerData;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        var json = ObscuredPrefs.GetString(PlayerKeys.IDs);
        if(string.IsNullOrEmpty(json))
        {
            playerIDs = new[] { "default" };
        }
        else
        {
            playerIDs = JsonHelper.FromJson<string>(json);
        }

        players = this.CreateGroup(new HashSet<Type>() { typeof(PlayerDataComponent), });
        serializableHeroes = this.CreateGroup(new HashSet<Type>() { typeof(HeroComponent), typeof(PlayerDataComponent), });
    }

    public override void OnEnable()
    {
        base.OnEnable();

        players.OnAdd().Subscribe(entity =>
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
                ObscuredPrefs.SetString(PlayerKeys.IDs, json);
                ObscuredPrefs.SetString(PlayerKeys.IDs + playerDataComponent.ID.Value, entity.Serialize());

                previousValue = playerDataComponent.ID.Value;
            }).AddTo(this.Disposer).AddTo(playerDataComponent.Disposer);

        }).AddTo(this.Disposer);

        players.OnRemove().Subscribe(entity =>
        {
            playerIDs = players.Entities.Select(e => e.GetComponent<PlayerDataComponent>().ID.Value).Distinct().ToArray();
            var json = JsonHelper.ToJson(playerIDs.ToArray());
            ObscuredPrefs.SetString(PlayerKeys.IDs, json);
        }).AddTo(this.Disposer);

        //instantiate a default player if none exists
        if(playerIDs == null || playerIDs.Length <= 0)
        {
            CreatePlayer("default");
        }
        else
        {
            foreach (var id in playerIDs)
            {
                CreatePlayer(id);
            } 
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

            playerDataComponent.ID.Value = SelectedPlayerData.Value.ID.Value;

        }).AddTo(this.Disposer);
    }

    private void CreatePlayer(string id)
    {
        var json = ObscuredPrefs.GetString(PlayerKeys.IDs + id);
        var entity = PoolManager.GetPool().CreateEntity();
        PrefabFactory.Instantiate(entity, playerDataPrefab, this.transform);
        entity.Deserialize(json);

        //HACK -> force a change here, because we get an OnChanged() fired when the entity is instantiated...
        //... but not on Deserialization, causing things to get out of sync on initial load
        entity.GetComponent<PlayerDataComponent>().ID.SetValueAndForceNotify(entity.GetComponent<PlayerDataComponent>().ID.Value);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        //playerIDs.Clear();
    }
}

//[Serializable]
//public class PlayerDataTable 
//{
//    public EntityStringTable Data = new EntityStringTable();
//}

//[Serializable]
//public class EntityStringTable : SerializableDictionary<IEntity, string> { }
