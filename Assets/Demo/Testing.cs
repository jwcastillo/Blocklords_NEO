using AlphaECS;
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
        eventSystem.OnEvent<BlockchainErrorEvent>().Subscribe(_ =>
        {
            Debug.LogError("Blockchain middleware error: "+_.Message);
        });

        // Listener for Wallet Event
        // Debug Wallet information
        eventSystem.OnEvent<WalletEvent>().Subscribe(_ =>
        {
            Debug.Log("RECEIVE WALLET EVENT");
            DebugWallet(_.wallet);
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
        }
	}

    private void DebugWallet(Wallet wallet)
    {
        Debug.Log("The " + wallet.Address + " wallet has a " + wallet.GAS + " GAS!");
        Debug.Log("Private Key: " + wallet.PrivateKey);
    }
}
