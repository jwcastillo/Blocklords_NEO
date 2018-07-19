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

public class WalletSystem : SystemBehaviour
{
    readonly string walletKey = "wallet";

    private DoubleReactiveProperty GAS = new DoubleReactiveProperty(0.0);
    private Wallet wallet;
    private KeyPair keys;
    private IDisposable checkBalance;

    readonly int blockDelay = 20;       // In Seconds
    private NeoAPI api;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        this.api = new NeoBlocklordsRpc(30333, "http://privatenet.ahmetson.com:4000");

        Debug.Log("Loading Wallet System");

        GAS.DistinctUntilChanged().Subscribe(value =>
        {
            EventSystem.Publish(new GasChangeEvent(wallet));
        });
        // Get the saved wallet on computer

        // Request of Wallet
        EventSystem.OnEvent<CreateWalletEvent>().Subscribe(_ =>
        {
            CreateWallet();
        });
        EventSystem.OnEvent<OpenWalletEvent>().Subscribe(_ =>
        {
            string privateKeyOrWIF = _.PrivateKeyOrWIF;
            OpenWallet(privateKeyOrWIF);
        });
        EventSystem.OnEvent<RequestWalletEvent>().Subscribe(_ =>
        {
            if (this.wallet == null)
            {
                EventSystem.Publish(new BlockchainErrorEvent("null_wallet"));
            } else
            {
                EventSystem.Publish(new WalletEvent(this.wallet));
            }
        });

        if (LoadWallet())
        {
            checkBalance = Observable.FromCoroutine(CheckBalance).Subscribe();
        }

        Observable.EveryApplicationFocus().Subscribe(_ =>
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
        });
    }

    private void CreateWallet()
    {
        if (wallet != null)
        {
            checkBalance.Dispose();
            wallet = null;
            checkBalance = null;
        }

        //string password = createWalletPassword.text;
        byte[] privateKey = new byte[32];

        // generate a new private key
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(privateKey);
        }

        // generate a key pair
        this.keys = new KeyPair(privateKey);
        this.wallet = KeyPairToWallet(this.keys);
        this.wallet.GAS = 0.0;
        this.GAS.Value = this.wallet.GAS;

        SaveWallet();

        checkBalance = Observable.FromCoroutine(CheckBalance).Subscribe();

        EventSystem.Publish(new WalletEvent(this.wallet));
    }

    private void OpenWallet(string privateKeyOrWIF)
    {
        if (privateKeyOrWIF.Length == 52)
        {
            this.keys = KeyPair.FromWIF(privateKeyOrWIF);
        }
        else if (privateKeyOrWIF.Length == 64)
        {
            this.keys = new KeyPair(privateKeyOrWIF.HexToBytes());
        }
        else
        {
            Debug.LogWarning("Invalid key input, must be 52 or 64 hexdecimal characters.");
            EventSystem.Publish(new BlockchainErrorEvent("invalid_private_key_or_wif"));
            return;
        }

        this.wallet = KeyPairToWallet(this.keys);
        this.wallet.GAS = 0.0;
        SaveWallet();

        if (checkBalance != null)
        {
            checkBalance.Dispose();
            checkBalance = null;
        }
        checkBalance = Observable.FromCoroutine(CheckBalance).Subscribe();

        Debug.Log("Getting the Balance");

        EventSystem.Publish(new WalletEvent(this.wallet));
    }

    // Delete Wallet on the Computer
    // Indicate change of GAS amount
    // Invoke Smart Contract
    // ~~ All methods of all smart contracts ~~
    // Wait For Transaction

    // Couritine that check the balance on every 20 seconds
    private IEnumerator CheckBalance()
    {
        while (true)
        {
            if (wallet == null)
            {
                break;
            }

            try
            {
                var balances = api.GetAssetBalancesOf(this.keys);

                decimal balance = balances.ContainsKey("GAS") ? balances["GAS"] : 0;
                wallet.GAS = (double)balance;

                Debug.Log("GAS amount is " + wallet.GAS);

                if (this.GAS.Value != wallet.GAS)
                {
                    this.GAS.Value = wallet.GAS;
                    SaveWallet();
                }
            }
            catch (NullReferenceException exception)
            {
                    Debug.LogWarning("Failed get balance");
                    EventSystem.Publish(new BlockchainErrorEvent("get_balance_fail"));
            }
        }
        yield return new WaitForSeconds(blockDelay);
    }

    private bool LoadWallet()
    {
        String walletData = PlayerPrefs.GetString(walletKey, null);
        if (walletData == null)
        {
            return false;
        }

        wallet = JsonUtility.FromJson<Wallet>(walletData);

        return true;
    }

    private void SaveWallet()
    {
        if (wallet == null)
        {
            Debug.LogWarning("Can not save null as a wallet!");
            return;
        }

        String walletData = JsonUtility.ToJson(wallet);
        PlayerPrefs.SetString(walletKey, walletData);
    }

    private Wallet KeyPairToWallet(KeyPair keyPair)
    {
        Wallet w = new Wallet(keyPair.PrivateKey.ByteToHex(), keyPair.WIF, keyPair.address, 0.0);

        return w;
    }
}
