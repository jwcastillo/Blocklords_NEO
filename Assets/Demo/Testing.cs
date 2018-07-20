using UniRx;
using UnityEngine;
using AlphaECS.Unity;

public class Testing : ComponentBehaviour
{
    public bool requestWallet = false;
    public bool createWallet = false;
    public bool openWallet = false;
    public string privateKeyOrWIF;

	private void Start ()
    {
        // Listener for Gas Change event
        // Send Gas.
        // Request of Wallet
        EventSystem.OnEvent<GasChangeEvent>().Subscribe(_ =>
        {
            Debug.Log("GAS CHANGE EVENT");
            DebugWallet(_.Wallet);
        });

        // Listener for Blockchain Error event.
        // Debug.Error(Message);
        EventSystem.OnEvent<BlockchainErrorEvent>().Subscribe(_ =>
        {
            Debug.LogError("Blockchain middleware error: "+_.Message);
        });

        // Listener for Wallet Event
        // Debug Wallet information
        EventSystem.OnEvent<WalletEvent>().Subscribe(_ =>
        {
            Debug.Log("RECEIVE WALLET EVENT");
            DebugWallet(_.Wallet);
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
	}

    private void DebugWallet(Wallet wallet)
    {
        Debug.Log("The " + wallet.Address + " wallet has a " + wallet.GAS + " GAS!");
        Debug.Log("Private Key: " + wallet.PrivateKey);
    }
}
