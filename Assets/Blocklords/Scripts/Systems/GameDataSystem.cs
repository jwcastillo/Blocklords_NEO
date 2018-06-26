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
}

public class GameDataSystem : SystemBehaviour
{
    [SerializeField] private GameObject playerDataPrefab;

    //for bootstrapping / caching data locally
    [SerializeField] private PlayerIDs playerIDs = new PlayerIDs();

    private IGroup players;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        var json = ObscuredPrefs.GetString(PlayerKeys.IDs);
        JsonUtility.FromJsonOverwrite(json, playerIDs);

        players = this.CreateGroup(new HashSet<Type>() { typeof(PlayerDataComponent), });
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
                if(value != previousValue && playerIDs.IDs.Any(id => id == value))
                {
                    Debug.LogWarning("ID " + value + " ALREADY EXISTS - REVERTING!");
                    playerDataComponent.ID.Value = previousValue;
                }
                else if(playerIDs.IDs.All(id => id != value))
                {
                    playerIDs.IDs.Remove(previousValue);
                    playerIDs.IDs.Add(value);

                    var json = JsonUtility.ToJson(playerIDs);
                    ObscuredPrefs.SetString(PlayerKeys.IDs, json);   
                }

                previousValue = playerDataComponent.ID.Value;
            }).AddTo(this.Disposer).AddTo(playerDataComponent.Disposer);

        }).AddTo(this.Disposer);

        //instantiate a default player if none exists
        if(playerIDs.IDs == null || playerIDs.IDs.Count <= 0)
        {
            CreatePlayer("default");
        }
        else
        {
            foreach (var id in playerIDs.IDs)
            {
                CreatePlayer(id);
            } 
        }
    }

    private void CreatePlayer(string id)
    {
        //HACK + TODO -> move this over to POCO style? setup / initialization is sloppy
        var player = PrefabFactory.Instantiate(playerDataPrefab, this.transform);
        var entity = player.GetComponent<EntityBehaviour>().Entity;
        var playerDataComponent = player.AddComponent<PlayerDataComponent>();
        playerDataComponent.ID.Value = id;
        entity.AddComponent(playerDataComponent);
    }
}

[Serializable]
public class PlayerIDs 
{
    public List<string> IDs = new List<string>();
}
