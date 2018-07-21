using UniRx;
using AlphaECS.Unity;
using AlphaECS;
using UnityEngine;
using System.Collections;
using System;
using Neo.Lux.Core;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class HeroTransferSystem : SystemBehaviour
{
    readonly string walletKey = "wallet";
    string heroContractAddress = "5ec209256128b2ffa437d6866bc3101db6e3e28a";
    UInt160 heroContract;

    private DoubleReactiveProperty GAS = new DoubleReactiveProperty(0.0);
    private Wallet wallet;

    private string privateNetUrl = "http://localhost";

    readonly int blockDelay = 20;       // In Seconds
    private NeoAPI api;

    private HeroComponent heroComponent;

    // Use this for initialization
    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        this.heroContract = UInt160.Parse("0x" + this.heroContractAddress);

        this.api = new NeoBlocklordsRpc(30333, this.privateNetUrl+":4000");

        Debug.Log("Loading Wallet System");

        // Get the saved wallet on computer

        // Request of Wallet
        EventSystem.OnEvent<PutHeroEvent>().Subscribe(evt =>
        {
            PutHero(evt.heroComponent, evt.firstHero);
        });

        EventSystem.OnEvent<WalletEvent>().Subscribe(evt =>
        {
            this.wallet = evt.Wallet;

            if (this.heroComponent == null)
                {
                    Debug.LogError("Hero Component is Empty");
                }
                else
                {
                    Debug.LogWarning("Hero component is Not Empty");
                    this.wallet = evt.Wallet;

                    string heroParameters = HeroComponentToContractParameters(heroComponent);
                    string heroId = heroComponent.ID.Value;

                    StartCoroutine(PutFirstHeroOnBlockchain(heroId, heroParameters));
                }
        });

        this.heroComponent = null;
        EventSystem.Publish(new RequestWalletEvent());

        /*Observable.EveryApplicationFocus().Subscribe(_ =>
        {
            if (_ == true)
            {
                if (checkBalance != null && wallet != null)
                {
                    checkBalance = Observable.FromCoroutine(CheckBalance).Subscribe();
                }
            }
        });
        Observable.EveryApplicationPause().Subscribe(_ =>
        {
            if (_ == true)
            {
                if (checkBalance != null)
                {
                    checkBalance.Dispose();
                    checkBalance = null;
                }
            }
        });
        Observable.OnceApplicationQuit().Subscribe(_ =>
        {
            if (checkBalance != null)
                {
                    checkBalance.Dispose();
                    checkBalance = null;
                }
            SaveWallet();
        });*/

    }

    // Put Hero on Server and Blockchain
    private void PutHero(HeroComponent heroComponent, bool firstHero)
    {
        if (!firstHero)
        {
            EventSystem.Publish(new HeroTransferedEvent(false, "Only First Hero Putting method supported!"));
            return;
        }

        if (this.wallet == null)
        {
            this.heroComponent = heroComponent;
            Debug.Log("Wallet Request");
            EventSystem.Publish(new RequestWalletEvent());
        }
        else
        {
            Debug.Log("Non Wallet Request");
            string heroParameters = HeroComponentToContractParameters(heroComponent);
            string heroId = heroComponent.ID.Value;

            StartCoroutine(PutFirstHeroOnBlockchain(heroId, heroParameters));
        }
    }

    private void PutHeroOnServer()
    {
        heroComponent = null;
        EventSystem.Publish(new HeroTransferedEvent());
    }

    // Helpers
    private HeroComponent ContractParametersToHeroComponent(string heroParameters)
    {
        HeroComponent c = new HeroComponent();
        return c;
    }
    private string HeroComponentToContractParameters(HeroComponent heroComponent)
    {
        string heroParameters = "";

        // Leadership Stat (4) 
        // Strength Stat (4) 
        // Speed Stat (4) 
        // Intelligence Stat (4) 
        // Defense Stat (4) 
        // Hero Nation (1) 
        // Hero Class (1) 
        // Optional Value (1)
        string leadership = heroComponent.BaseStats.Leadership.Value.ToString().PadLeft(4, '0');
        string strength = heroComponent.BaseStats.Strength.Value.ToString().PadLeft(4, '0');
        string speed = heroComponent.BaseStats.Speed.Value.ToString().PadLeft(4, '0');
        string intelligence = heroComponent.BaseStats.Intelligence.Value.ToString().PadLeft(4, '0');
        string defense = heroComponent.BaseStats.Defense.Value.ToString().PadLeft(4, '0');
        string nation = "0";
        string Class = ((int)heroComponent.Class.Value).ToString();
        string optional = "0";

        heroParameters = leadership + strength + speed + intelligence + defense + nation + Class + optional;

        return heroParameters;
    }

    IEnumerator PutFirstHeroOnBlockchain(string heroId, string heroParameters)
    {
        yield return new WaitForSeconds(5);
        try
        {
            List<string> argsString = new List<string> { };
            object[] args = argsString.Cast<object>().ToArray();
            Debug.Log(args.ToString());
            byte[] address = this.wallet.keys.address.AddressToScriptHash();
            Debug.Log( address.ToHexString()); 

            Transaction tx = api.CallContract(this.wallet.keys, this.heroContract, "putFirstHero", new object[]
            {
                address,
                heroId,
                heroParameters
            });

            if (tx == null)
            {
                EventSystem.Publish(new HeroTransferedEvent("Failed to put the hero on the Blockchain!"));
            }
            else
            {
                api.WaitForTransaction(this.wallet.keys, tx);
                //byte[] heroIdBytes = heroId.HexToBytes();

                byte[] heroIdBytes = Encoding.Default.GetBytes(heroId);

                byte[] result = api.GetStorage(this.heroContractAddress, heroIdBytes);
                if (result == null)
                {
                    EventSystem.Publish(new HeroTransferedEvent("Storage Result is NULL!"));
                }
                else
                {
                    string resultString = result.ByteToHex();

                    if (resultString.Length.Equals(0))
                    {
                        EventSystem.Publish(new HeroTransferedEvent("Storage returns NULL!"));
                    }
                    else
                    {
                        PutHeroOnServer();
                    }
                }
            }
        }
        catch (NullReferenceException exception)
        {
            EventSystem.Publish(new HeroTransferedEvent("Exception while Putting the Hero on Blockchain!"));
        }
    }
}
