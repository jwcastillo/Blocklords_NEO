using AlphaECS;
using Neo.SmartContract.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

public class Testing : MonoBehaviour {

    public bool requestWallet = false;
    public bool createWallet = false;
    public bool openWallet = false;
    public string privateKeyOrWIF;

    public bool putHero = false;
    public string heroId = "";

    public bool createItem = false;
    public string itemId = "";
    public ItemType itemType;
    public ItemQuality itemQuality;
    public Stats statValue;
    public Stats maxStatValue;

    public bool buyItem = false;
    public string buyingItemId = "";
    public double itemPrice = 0;

    public readonly byte[] itemOwner = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y".ToScriptHash();

    [Inject]
    public IEventSystem eventSystem;

	// Use this for initialization
	void Start () {
        Debug.Log(eventSystem);
        // Listener for Gas Change event
        // Send Gas.
        // Request of Wallet
        eventSystem.OnEvent<GasChangeEvent>().Subscribe(_ =>
        {
            Debug.Log("GAS CHANGE EVENT");
            DebugWallet(_.wallet);
        });

        // Listener for Blockchain Error event.
        // Debug.Error(Message);
        eventSystem.OnEvent<WalletTransferedEvent>().Subscribe(_ =>
        {
            Debug.LogError("Wallet error: "+_.message);
        });

        // Listener for Wallet Event
        // Debug Wallet information
        eventSystem.OnEvent<WalletEvent>().Subscribe(_ =>
        {
            Debug.Log("RECEIVE WALLET EVENT");
            DebugWallet(_.wallet);
        });

        eventSystem.OnEvent<HeroTransferedEvent>().Subscribe(e =>
        {
            if (e.isSuccess)
            {
                Debug.Log("Hero was put on Blockchain successfully");

            } else {
                Debug.LogError(e.message);
            }
            
        });
        eventSystem.OnEvent<ItemTransferedEvent>().Subscribe(e =>
        {
            if (e.isSuccess)
            {
                Debug.Log("Item was put on Blockchain successfully");

            }
            else
            {
                Debug.LogError(e.message);
            }

        });
    }
	
	// Update is called once per frame
	void Update () {
		if (this.requestWallet)
        {
            this.requestWallet = false;
            Debug.Log("Requesting Wallet Information");
            this.eventSystem.Publish(new RequestWalletEvent());
        } else if (this.createWallet)
        {
            this.createWallet = false;
            Debug.Log("Creating Wallet");
            this.eventSystem.Publish(new CreateWalletEvent());
        }
        else if (this.openWallet)
        {
            this.openWallet = false;

            if (this.privateKeyOrWIF.Length < 1)
            {
                Debug.LogWarning("Please provide the Private key or WIF");
                return;
            }

            this.eventSystem.Publish(new OpenWalletEvent(this.privateKeyOrWIF));

            Debug.Log("Open the Wallet");
            this.privateKeyOrWIF = "";
        } else if (this.putHero)
        {
            this.putHero = false;
            HeroComponent heroComponent = new HeroComponent();
            heroComponent.ID = new StringReactiveProperty(heroId.PadLeft(13, '0'));
            Stats stats = new Stats();
            stats.Intelligence = new IntReactiveProperty(1);
            stats.Leadership = new IntReactiveProperty(1);
            stats.Speed = new IntReactiveProperty(1);
            stats.Strength = new IntReactiveProperty(1);
            stats.Defense = new IntReactiveProperty(1);
            heroComponent.BaseStats = stats;
            heroComponent.Class = new HeroClassReactiveProperty(HeroClass.Ranger);

            this.eventSystem.Publish(new PutHeroEvent(heroComponent, true));
        } else if (this.createItem)
        {
            this.createItem = false;
            Item item = new Item();
            item.ID = new StringReactiveProperty(itemId.PadLeft(13, '0'));
            item.BaseStats = new StatsReactiveProperty();
            item.BaseStats.Value = statValue;
            item.MaxStats = new StatsReactiveProperty();
            item.MaxStats.Value = maxStatValue;
            item.Name = new StringReactiveProperty("Item #" + itemId.ToString());
            item.ItemType = new ItemTypeReactiveProperty(itemType);
            item.ItemQuality = new ItemQualityReactiveProperty(itemQuality);

            this.eventSystem.Publish(new CreateItemEvent(item));
        } else if (this.buyItem)
        {
            this.buyItem = false;
            Item item = new Item();
            item.ID = new StringReactiveProperty(buyingItemId.PadLeft(13, '0'));
            MarketItem marketItem = new MarketItem(item, new Decimal(itemPrice), itemOwner);

            this.eventSystem.Publish(new BuyItemEvent(marketItem));
        }
    }

    private void DebugWallet(Wallet wallet)
    {
        Debug.Log("The " + wallet.keys.address + " wallet has a " + wallet.GAS + " GAS!");
        Debug.Log("Private Key: " + wallet.keys.PrivateKey.ToString());
    }
}
