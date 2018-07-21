using UnityEngine;
using Neo.Lux.Core;
using Neo.Lux.Utils;
using LunarParser;
using Neo.Lux.Cryptography;

public class NeoBlocklordsRpc : NeoRPC
{
    public NeoBlocklordsRpc(int port, string neoscanURL) : base(port, neoscanURL)
    {
    }

    protected override string GetRPCEndpoint()
    {
        return $"http://localhost:{port}";
    }

    private void LogData(DataNode node, int ident = 0)
    {
        var tabs = new string('\t', ident);
        Logger($"{tabs}{node}");
        foreach (DataNode child in node.Children)
            LogData(child, ident + 1);
    }

    public new DataNode QueryRPC(string method, object[] _params, int id = 1)
    {
        var paramData = DataNode.CreateArray("params");
        string paramUrl = "";
        for (int i=0; i<_params.Length; ++i)
        {
            var entry = _params[i];
            paramUrl += "\"" + entry + "\"";
            if (i + 1 != _params.Length)
            {
                paramUrl += ", ";
            }
            paramData.AddField(null, entry);
        }
        paramUrl = "[" + paramUrl + "]";

        var jsonRpcData = DataNode.CreateObject(null);
        jsonRpcData.AddField("method", method);
        jsonRpcData.AddNode(paramData);
        jsonRpcData.AddField("id", id);
        jsonRpcData.AddField("jsonrpc", "2.0");

        Logger("NeoDB QueryRPC: " + method);
        LogData(jsonRpcData);

        int retryCount = 0;
        do
        {
            if (rpcEndpoint == null)
            {
                rpcEndpoint = GetRPCEndpoint();
                Logger("Update RPC Endpoint: " + rpcEndpoint);
            }

            string url = rpcEndpoint + "?jsonrpc=2.0&method=getstorage&params=" + paramUrl + "&id=" + id;

            var response = RequestUtils.Request(RequestType.GET, url, jsonRpcData);

            if (response != null && response.HasNode("result"))
            {
                return response;
            }
            else
            {
                if (response != null && response.HasNode("error"))
                {
                    var error = response["error"];
                    Logger("RPC Error: " + error.GetString("message"));
                }
                else
                {
                    Logger("No answer");
                }
                rpcEndpoint = null;
                retryCount++;
            }

        } while (retryCount < 10);

        return null;
    }

    public override byte[] GetStorage(string scriptHash, byte[] key)
    {
        var response = QueryRPC("getstorage", new object[] { key.ByteToHex() });
        if (response == null)
        {
            response = QueryRPC("getstorage", new object[] { scriptHash, key.ByteToHex() });
            if (response == null)
            {
                Debug.Log("Failed the QueryRPC");
                return null;
            }
        }
        var result = response.GetString("result");
        if (string.IsNullOrEmpty(result))
        {
            return null;
        }
        return result.HexToBytes();
    }


}
