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

public class WalletSystem : SystemBehaviour {

    readonly string walletKey = "wallet";
    readonly int wifLength = 52;

    private Wallet wallet = new Wallet();
    private IDisposable checkBalance, gasChanged;

    private string privateNetUrl = "http://localhost";

    readonly int blockDelay = 20;       // In Seconds
    private NeoAPI api;

    // Use this for initialization
    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        this.api = new NeoBlocklordsRpc(30333, this.privateNetUrl+":4000");

        // Request of Wallet
        EventSystem.OnEvent<CreateWalletEvent>().Subscribe(_ =>
            CreateWallet()
        );
        EventSystem.OnEvent<OpenWalletEvent>().Subscribe(_ =>
            OpenWallet(_.PrivateKeyOrWIF)
        );


        EventSystem.OnEvent<RequestWalletEvent>().Subscribe(_ =>
        {
            if (this.wallet == null)
            {
                EventSystem.Publish(new WalletTransferedEvent("null_wallet"));
            } else
            {
                EventSystem.Publish(new WalletEvent(this.wallet));
            }
        });

        if (LoadWallet())
        {
            checkBalance = Observable.FromCoroutine(CheckBalance).Subscribe();
            gasChanged = wallet.GAS.DistinctUntilChanged().Subscribe(value =>
                EventSystem.Publish(new GasChangeEvent(wallet))
            );
        }
    }

    // Create Wallet
    private void CreateWallet()
    {
        RemoveWallet();

        byte[] privateKey = new byte[32];

        // generate a new private key
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(privateKey);
        }

        // generate a key pair
        this.wallet = new Wallet(privateKey);

        SaveWallet();

        gasChanged = wallet.GAS.DistinctUntilChanged().Subscribe(value =>
            EventSystem.Publish(new GasChangeEvent(wallet))
        );
        checkBalance = Observable.FromCoroutine(CheckBalance).Subscribe();

        EventSystem.Publish(new WalletEvent(this.wallet));
    }

    // Open Wallet
    private void OpenWallet(string privateKeyOrWIF)
    {
        if (privateKeyOrWIF.Length == 52)                   // WIF
        {
            this.wallet = new Wallet(privateKeyOrWIF);
        }
        else if (privateKeyOrWIF.Length == 64)              // Private Key
        {
            this.wallet = new Wallet(privateKeyOrWIF.HexToBytes());
        }
        else
        {
            Debug.LogWarning("Invalid key input, must be 52 or 64 hexdecimal characters.");
            EventSystem.Publish(new WalletTransferedEvent("invalid_private_key_or_wif"));
            return;
        }

        if (Wallet.IsNull(wallet))
        {
            Debug.LogWarning("Invalid key input, must be 52 or 64 hexdecimal characters.");
            EventSystem.Publish(new WalletTransferedEvent("invalid_private_key_or_wif"));
            return;
        }

        SaveWallet();

        if (checkBalance != null)
        {
            checkBalance.Dispose();
            checkBalance = null;
        }
        if (gasChanged != null)
        {
            gasChanged.Dispose();
            gasChanged = null;
        }
        gasChanged = wallet.GAS.DistinctUntilChanged().Subscribe(value =>
            EventSystem.Publish(new GasChangeEvent(wallet))
        );
        checkBalance = Observable.FromCoroutine(CheckBalance).Subscribe();

        Debug.Log("Getting the Balance");
        EventSystem.Publish(new WalletEvent(this.wallet));
    }

    // Couritine that check the balance on every 20 seconds
    private IEnumerator CheckBalance()
    {
        while (true)
        {
            Debug.Log("Balance checking");
            if (Wallet.IsNull(this.wallet))
            {
                break;
            }

            yield return new WaitForSeconds(2);

            try
            {
                var balances = api.GetAssetBalancesOf(this.wallet.keys);

                decimal balance = balances.ContainsKey("GAS") ? balances["GAS"] : 0;

                //if (wallet.GAS.Value != balance)
                //{
                    Debug.Log("GAS amount is " + wallet.GAS);

                    wallet.GAS.Value = balance;
                    SaveWallet();
                //}
            }
            catch (NullReferenceException exception)
            {
                Debug.LogWarning("Failed get balance: "+this.wallet.keys.address+exception.ToString());
                EventSystem.Publish(new WalletTransferedEvent("get_balance_fail"));
                break;
            }
            yield return new WaitForSeconds(blockDelay-2);
        }
        
    }

    private bool LoadWallet()
    {
        String wif = PlayerPrefs.GetString(this.walletKey, null);

        if (String.IsNullOrEmpty(wif))
        {
            return false;
        }
        if (!wif.Length.Equals(this.wifLength))
        {
            RemoveWallet();
            return false;
        }

        this.wallet = new Wallet(wif);

        if (Wallet.IsNull(this.wallet))
        {
            RemoveWallet();
            return false;
        }

        return true;
    }
    private void SaveWallet()
    {
        if (Wallet.IsNull(wallet))
        {
            Debug.LogWarning("Can not save null as a wallet!");
            return;
        }

        //String walletData = JsonUtility.ToJson();
        PlayerPrefs.SetString(walletKey, wallet.keys.WIF);
    }

    private void RemoveWallet()
    {
        PlayerPrefs.DeleteKey(this.walletKey);

        if (checkBalance != null) checkBalance.Dispose();
        wallet = null;
        checkBalance = null;
        if (gasChanged != null) gasChanged.Dispose();
        gasChanged = null;
    }
}
