using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


public class TransformInfo
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;


    public TransformInfo(Transform t = null)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        scale = new Vector3(1, 1, 1);
        SetWithTransform(t);
    }

    public void SetWithTransform(Transform t)
    {
        if (!t)
            return;
        position = t.position;
        rotation = t.rotation;
        scale = t.localScale;
    }

    public static bool Equals(TransformInfo a, TransformInfo b)
    {
        return (a.position == b.position && a.rotation == b.rotation && a.scale == b.scale);
    }


    #region Storage

    public void WriteToFile(StreamWriter writer)
    {
        try
        {
            writer.WriteLine(position.x);
            writer.WriteLine(position.y);
            writer.WriteLine(position.z);

            writer.WriteLine(rotation.x);
            writer.WriteLine(rotation.y);
            writer.WriteLine(rotation.z);
            writer.WriteLine(rotation.w);

            writer.WriteLine(scale.x);
            writer.WriteLine(scale.y);
            writer.WriteLine(scale.z);
        }
        catch (Exception e)
        {
            Debug.LogError("The TransformInfo could not be written to file: " + e.Message);
        }
    }

    public void InitializeFromFile(StreamReader reader)
    {
        try
        {
            float floatVal;

            if (float.TryParse(reader.ReadLine(), out floatVal))
                position.x = floatVal;
            if (float.TryParse(reader.ReadLine(), out floatVal))
                position.y = floatVal;
            if (float.TryParse(reader.ReadLine(), out floatVal))
                position.z = floatVal;

            if (float.TryParse(reader.ReadLine(), out floatVal))
                rotation.x = floatVal;
            if (float.TryParse(reader.ReadLine(), out floatVal))
                rotation.y = floatVal;
            if (float.TryParse(reader.ReadLine(), out floatVal))
                rotation.z = floatVal;
            if (float.TryParse(reader.ReadLine(), out floatVal))
                rotation.w = floatVal;

            if (float.TryParse(reader.ReadLine(), out floatVal))
                scale.x = floatVal;
            if (float.TryParse(reader.ReadLine(), out floatVal))
                scale.y = floatVal;
            if (float.TryParse(reader.ReadLine(), out floatVal))
                scale.z = floatVal;
        }
        catch (Exception e)
        {
            Debug.LogError("The TransformInfo could not be read from file: " + e.Message);
        }
    }

    public static TransformInfo TransformInfoFromFile(StreamReader reader)
    {
        TransformInfo tInfo = new TransformInfo();
        tInfo.InitializeFromFile(reader);
        return tInfo;
    }

    #endregion
}


public static class TransformExensions_TransformInfo
{
    public static TransformInfo GetTransformInfo(this Transform t)
    {
        return new TransformInfo(t);
    }

    public static void SetTransformInfo(this Transform t, TransformInfo info)
    {
        t.position = info.position;
        t.rotation = info.rotation;
        t.localScale = info.scale;
    }

    public static Dictionary<string, TransformInfo> GetTransformInfoForAllChildren(this Transform t, Dictionary<string, TransformInfo> dict = null, bool recursive = true)
    {
        if (dict == null)
            dict = new Dictionary<string, TransformInfo>();

        foreach (Transform child in t)
        {
            dict.Add(child.name, child.transform.GetTransformInfo());

            if (recursive && child.childCount > 0)
            {
                child.GetTransformInfoForAllChildren(dict, true);
            }
        }

        return dict;
    }
}