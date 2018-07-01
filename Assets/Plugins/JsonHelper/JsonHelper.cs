using System;
using System.Collections.Generic;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    //public static List<T> FromJson<T>(string json)
    {
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] items)
    //public static string ToJson<T>(List<T> items)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = items;
        return UnityEngine.JsonUtility.ToJson(wrapper);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
        //public List<T> Items;
    }
}