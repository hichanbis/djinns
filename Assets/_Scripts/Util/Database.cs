using System.Xml.Serialization;
using System.IO;
using System;

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

    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static T LoadFromText<T>(string text)
    {
        var serializer = new XmlSerializer(typeof(T));
        return (T)Convert.ChangeType(serializer.Deserialize(new StringReader(text)), typeof(T));
    }
}
