using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

// This serves as a "template" for XML Serialization

public class XmlSerializationTemplate
{
    public const string DefaultFilePath = "Assets/mySerializedClass.txt";


    #region Save/Load

    public static void SaveToFile(XmlSerializationTemplate ob) { SaveToFile(ob, DefaultFilePath); }
    public static void SaveToFile(XmlSerializationTemplate ob, string filename)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(XmlSerializationTemplate));
        TextWriter writer = new StreamWriter(filename);

        serializer.Serialize(writer, ob);

        writer.Close();
    }

    public static XmlSerializationTemplate LoadFromFile() { return LoadFromFile(DefaultFilePath); }
    public static XmlSerializationTemplate LoadFromFile(string filename)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(XmlSerializationTemplate));
        FileStream fs = new FileStream(filename, FileMode.Open);

        return (XmlSerializationTemplate)serializer.Deserialize(fs);
    }

    #endregion

}
