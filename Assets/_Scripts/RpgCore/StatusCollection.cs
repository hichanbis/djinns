using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("StatusCollection")]
public class StatusCollection
{
    private static StatusCollection instance;

    private StatusCollection() { }

    [XmlArray("Statuses"), XmlArrayItem("Status")]
    public List<Status> statuses;

    public static StatusCollection Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Database.Load<StatusCollection>(Application.dataPath + "/_Databases/StatusCollection.xml");
            }
            return instance;
        }
    }

    public Status FindStatusFromId(string id)
    {
        return statuses.Find(t => t.id.Equals(id));
    }


}

