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

public class ItemTransferSystem : SystemBehaviour {

    readonly string walletKey = "wallet";
    string itemContractAddress = "0463e00d9200830853c33ce16fe804e86af814aa";
    string itemWalletAddress = "AZUE49z4N3H2yky8UkgDNBj4VUM2UA259d";
    string assetID = "602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
    UInt160 itemContract;

    private readonly decimal delimeter = 100000000;
    private readonly decimal transactionFee = 0.01m;
    private Wallet wallet;

    private string privateNetUrl = "http://localhost";

    readonly int blockDelay = 20;       // In Seconds
    private NeoAPI api;

    private MarketItem marketItem;
    private Item creatingItem;

    IDisposable blockEnd;

    Transaction tx;

    // Use this for initialization
    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory);

        this.itemContract = UInt160.Parse("0x" + this.itemContractAddress);

        this.api = new NeoBlocklordsRpc(30333, this.privateNetUrl+":4000");

        // Require the Wallet
        // Check the Item from Market or not.
        // Calculate the Price of Item
        // Does player has an amount of money required to send them
        // Apply money to the Owner of Item
        // Apply Transaction fee to the Buying

        // Call the Buying of item

        Debug.Log("Loading Wallet System");

        // Get the saved wallet on computer

        EventSystem.OnEvent<BuyItemEvent>().Subscribe(_ =>
        {
            StartCoroutine(BuyItem(_.marketItem));
        });

        EventSystem.OnEvent<CreateItemEvent>().Subscribe(_ =>
        {
            StartCoroutine(CreateItem(_.item));
        });

        EventSystem.OnEvent<WalletEvent>().Subscribe(_ =>
        {
            this.wallet = _.wallet;

            if (this.creatingItem == null)
            {
                if (this.marketItem != null)
                {
                    StartCoroutine(BuyItem(this.marketItem));
                }
            } else
            {
                StartCoroutine(CreateItem(this.creatingItem));
            }
        });

        this.marketItem = null;
        this.creatingItem = null;
        if (Wallet.IsNull(this.wallet))
        {
            EventSystem.Publish(new RequestWalletEvent());
        }
    }

    IEnumerator BuyItem(MarketItem marketItem)
    {
        yield return new WaitForSeconds(5);
        try
        {
            if (Wallet.IsNull(this.wallet))
            {
                this.marketItem = marketItem;
                EventSystem.Publish(new RequestWalletEvent());
            } else
            {
                byte[] buyerAddress = this.wallet.keys.address.AddressToScriptHash();
                //byte[] ownerAddress = marketItem.ownerAddress.Value.AddressToScriptHash();
                byte[] ownerAddress = marketItem.ownerAddress.Value;
                decimal requiredGas = marketItem.price.Value + transactionFee;

                if (requiredGas > wallet.GAS.Value)
                {
                    this.marketItem = null;
                    EventSystem.Publish(new ItemTransferedEvent("not_enough_gas"));
                }
                else
                {

                    // Blocklords Revenue :)
                    Transaction.Output transactionOutput = new Transaction.Output
                    {
                        assetID = LuxUtils.ReverseHex(assetID).HexToBytes(),
                        value = transactionFee,
                        scriptHash = itemWalletAddress.AddressToScriptHash().ToScriptHash()
                    };
                    // Item owner revenue
                    Transaction.Output transactionOutput1 = new Transaction.Output
                    {
                        assetID = LuxUtils.ReverseHex(assetID).HexToBytes(),
                        value = marketItem.price.Value,
                        scriptHash = ownerAddress.ToScriptHash()
                    };
                    List<Transaction.Output> outputs = new List<Transaction.Output>();
                    outputs.Add(transactionOutput);
                    outputs.Add(transactionOutput1);

                    tx = api.CallContract(this.wallet.keys, this.itemContract, "transfer", new object[] {
                    buyerAddress,
                    MarketItem.GetItemId(marketItem),
                    ownerAddress
                }, "GAS", outputs);
                    if (tx == null)
                    {
                        this.marketItem = null;
                        EventSystem.Publish(new ItemTransferedEvent("Failed to buy Item!"));
                    }
                    else
                    {
                        api.WaitForTransaction(this.wallet.keys, tx);
                        //byte[] heroIdBytes = heroId.HexToBytes();

                        byte[] itemId = Encoding.ASCII.GetBytes(MarketItem.GetItemId(marketItem));

                        byte[] result = api.GetStorage(this.itemContractAddress, itemId);
                        if (result == null)
                        {
                            this.marketItem = null;
                            EventSystem.Publish(new ItemTransferedEvent("Storage Result is NULL!"));
                        }
                        else
                        {
                            //string resultString = Encoding.ASCII.GetString(result);

                            if (!result.Length.Equals(30))
                            {
                                this.marketItem = null;
                                EventSystem.Publish(new ItemTransferedEvent("Storage has no Correct answer!"));
                            }
                            else if (result.ToString().EndsWith(ownerAddress.ToString()))
                            {
                                this.marketItem = null;
                                LogOnServer("Item was bought successfully");
                            }
                            else
                            {
                                this.marketItem = null;
                                Debug.LogError(result);
                                EventSystem.Publish(new ItemTransferedEvent("Item owner doesn't changed"));
                            }
                        }
                    }
                }
            }
        }
        catch (NullReferenceException exception)
        {
            this.marketItem = null;
            EventSystem.Publish(new ItemTransferedEvent("Exception while Putting the Transfering Item!"));
        }
    }

    IEnumerator CreateItem(Item item)
    {
        yield return new WaitForSeconds(5);
        try
        {
            if (Wallet.IsNull(this.wallet))
            {
                this.creatingItem = item;
                EventSystem.Publish(new RequestWalletEvent());
            }
            else
            {
                //ITEM ID(13)    STAT VALUE(4)  MAX STAT VALUE(4)   STAT TYPE(1) QUALITY(1)  OWNER ADDRESS(33)

                byte[] owner = this.wallet.keys.address.AddressToScriptHash();
                Debug.Log("Owner's length: " + owner.Length);
                string itemId = item.ID.Value.PadLeft(13, '0');
                string itemParameters = item.BaseStats.Value.ToString().PadLeft(4, '0');
                itemParameters += item.MaxStats.Value.ToString().PadLeft(4, '0');
                itemParameters += ((int)item.ItemType.Value).ToString();
                itemParameters += ((int)item.ItemQuality.Value).ToString();

                tx = api.CallContract(this.wallet.keys, this.itemContract, "put", new object[] {
                    owner,
                    itemId,
                    itemParameters
                });
                if (tx == null)
                {
                    this.creatingItem = null;
                    EventSystem.Publish(new ItemTransferedEvent("Failed to put Item on the Blockchain!"));
                }
                else
                {
                    blockEnd = Observable.FromCoroutine(WaitForTransaction).Subscribe(_ =>
                    {
                        api.WaitForTransaction(this.wallet.keys, tx);
                        //byte[] heroIdBytes = heroId.HexToBytes();

                        byte[] itemIdBytes = Encoding.Default.GetBytes(itemId);

                        byte[] result = api.GetStorage(this.itemContractAddress, itemIdBytes);
                        if (result == null)
                        {
                            this.creatingItem = null;
                            EventSystem.Publish(new ItemTransferedEvent("Storage Result is NULL!"));
                        }
                        else
                        {
                            //string resultString = Encoding.Default.GetString(result);

                            //if (String.IsNullOrEmpty(resultString))
                            //{
                            //    this.creatingItem = null;
                            //    EventSystem.Publish(new ItemTransferedEvent("Storage returns NULL!"));
                            //}
                            //else
                            //{
                            this.creatingItem = null;
                            LogOnServer("Item was created successfully");
                            //}
                        }
                        blockEnd.Dispose();
                    });
                    
                }
            }
        }
        catch (NullReferenceException exception)
        {
            this.creatingItem = null;
            EventSystem.Publish(new ItemTransferedEvent("Exception while Putting the Transfering Item!"));
        }
    }

    private void LogOnServer(string message)
    {
        Debug.LogWarning(message);
        this.creatingItem = null;
        this.marketItem = null;
    }

    IEnumerator WaitForTransaction()
    {
        if (tx == null)
        {
            Debug.Log("Value is null!");
            throw new ArgumentNullException();
        }

        uint newBlock;

        uint oldBlock = 0;
        oldBlock = api.GetBlockHeight();
        do
        {
            yield return new WaitForSeconds(5);
            newBlock = api.GetBlockHeight();
        } while (newBlock == oldBlock);

        while (oldBlock < newBlock)
        {
            var other = api.GetBlock(oldBlock);

            if (other != null)
            {
                foreach (var entry in other.transactions)
                {
                    if (entry.Hash == tx.Hash)
                    {
                        oldBlock = newBlock;
                        break;
                    }
                }

                oldBlock++;
            }
            else
            {
                yield return new WaitForSeconds(5);
            }

        }
    }

}
