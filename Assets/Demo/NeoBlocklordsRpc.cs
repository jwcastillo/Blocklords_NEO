using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neo.Lux.Core;

public class NeoBlocklordsRpc : NeoRPC
{
    public NeoBlocklordsRpc(int port, string neoscanURL) : base(port, neoscanURL)
    {
    }

    protected override string GetRPCEndpoint()
    {
        return $"http://privatenet.ahmetson.com:{port}";
    }

}
