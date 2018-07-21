using AlphaECS;
using Neo.SmartContract.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
using AlphaECS.Unity;

public class Testing : ComponentBehaviour
{
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

	private void Start ()
    {
        Debug.Log(EventSystem);
        // Listener for Gas Change event
        // Send Gas.
        // Request of Wallet
        EventSystem.OnEvent<GasChangeEvent>().Subscribe(evt =>
        {
            Debug.Log("GAS CHANGE EVENT");
            DebugWallet(evt.Wallet);
        });

        // Listener for Blockchain Error event.
        // Debug.Error(Message);
        EventSystem.OnEvent<WalletTransferedEvent>().Subscribe(evt =>
        {
            Debug.LogError("Wallet error: "+evt.message);
        });

        // Listener for Wallet Event
        // Debug Wallet information
        EventSystem.OnEvent<WalletEvent>().Subscribe(evt =>
        {
            Debug.Log("RECEIVE WALLET EVENT");
            DebugWallet(evt.Wallet);
        });

        EventSystem.OnEvent<HeroTransferedEvent>().Subscribe(evt =>
        {
            if (evt.isSuccess)
            {
                Debug.Log("Hero was put on Blockchain successfully");

            } else {
                Debug.LogError(evt.message);
            }
            
        });
        EventSystem.OnEvent<ItemTransferedEvent>().Subscribe(evt =>
        {
            if (evt.isSuccess)
            {
                Debug.Log("Item was put on Blockchain successfully");

            }
            else
            {
                Debug.LogError(evt.message);
            }

        });
    }
	
	private void Update ()
    {
		if (this.requestWallet)
        {
            this.requestWallet = false;
            Debug.Log("Requesting Wallet Information");
            EventSystem.Publish(new RequestWalletEvent());
        }
        else if (this.createWallet)
        {
            this.createWallet = false;
            Debug.Log("Creating Wallet");
            EventSystem.Publish(new CreateWalletEvent());
        }
        else if (this.openWallet)
        {
            this.openWallet = false;

            if (this.privateKeyOrWIF.Length < 1)
            {
                Debug.LogWarning("Please provide the Private key or WIF");
                return;
            }

            EventSystem.Publish(new OpenWalletEvent(this.privateKeyOrWIF));

            Debug.Log("Open the Wallet");
            this.privateKeyOrWIF = "";
        }
        else if (this.putHero)
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

            EventSystem.Publish(new PutHeroEvent(heroComponent, true));
        }
        else if (this.createItem)
        {
            this.createItem = false;
            Item item = new Item();
            item.ID = new StringReactiveProperty(itemId.PadLeft(13, '0'));
            item.BaseStats = statValue;
            item.MaxStats = maxStatValue;
            item.Name = new StringReactiveProperty("Item #" + itemId.ToString());
            item.ItemType = new ItemTypeReactiveProperty(itemType);
            item.ItemQuality = new ItemQualityReactiveProperty(itemQuality);

            EventSystem.Publish(new CreateItemEvent(item));
        }
        else if (this.buyItem)
        {
            this.buyItem = false;
            Item item = new Item();
            item.ID = new StringReactiveProperty(buyingItemId.PadLeft(13, '0'));
            MarketItem marketItem = new MarketItem(item, new Decimal(itemPrice), itemOwner);

            EventSystem.Publish(new BuyItemEvent(marketItem));
        }
    }

    private void DebugWallet(Wallet wallet)
    {
        //Debug.Log("The " + wallet.keys.address + " wallet has a " + wallet.GAS + " GAS!");
        //Debug.Log("Private Key: " + wallet.keys.PrivateKey.ToString());
    }
}
