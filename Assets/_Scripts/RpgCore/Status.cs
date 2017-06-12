using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[System.Serializable]
public class Status
{
    [XmlAttribute("id")]
    public string id;
    public string name;
    public string type;
    public int successRatePercent;


    public Status()
    {
    }

    public override string ToString()
    {
        return string.Format("[Status: id={0}, name={1}, type={2}], successRate={3}", id, name, type, successRatePercent);
    }

}


