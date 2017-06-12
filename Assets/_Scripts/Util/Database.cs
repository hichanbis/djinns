using System.Xml.Serialization;
using System.IO;
using System;
using System.Runtime.Serialization;

public class Database
{
    public static T Load<T>(string path)
    {
        var serializer = new XmlSerializer(typeof(T));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return (T)Convert.ChangeType(serializer.Deserialize(stream), typeof(T));
        }
    }

    public static T LoadWithReferences<T>(string path)
    {
        var serializer = new DataContractSerializer(typeof(T), null, 
            0x7FFF /*maxItemsInObjectGraph*/, 
            false /*ignoreExtensionDataObject*/, 
            true /*preserveObjectReferences : this is where the magic happens */, 
            null /*dataContractSurrogate*/);


        using (var stream = new FileStream(path, FileMode.Open))
        {
            return (T)Convert.ChangeType(serializer.ReadObject(stream), typeof(T));
        }
    }


    public static void SaveWithReferences<T>(AbilityCollection a, string path)
    {
        var serializer = new DataContractSerializer(typeof(T), null, 
            0x7FFF /*maxItemsInObjectGraph*/, 
            false /*ignoreExtensionDataObject*/, 
            true /*preserveObjectReferences : this is where the magic happens */, 
            null /*dataContractSurrogate*/);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.WriteObject(stream, a);
        }
    }



    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static T LoadFromText<T>(string text)
    {
        var serializer = new XmlSerializer(typeof(T));
        return (T)Convert.ChangeType(serializer.Deserialize(new StringReader(text)), typeof(T));
    }
}
