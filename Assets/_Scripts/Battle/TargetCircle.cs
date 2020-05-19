using UnityEngine;
using System.Collections;

public class TargetCircle : MonoBehaviour
{
    public int segments;
    public float xradius = 1f;
    public float zradius = 1f;
    public float yheight = 0.3f;
  
    private LineRenderer line;

    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();


        line.useWorldSpace = false;
        line.loop = true;
    }



    public void DisplayCircle()
    {
        line.positionCount = segments + 1;

        float x;
        float y = yheight;
        float z;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * zradius;

            line.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }

    public void HideCircle()
    {
        line.positionCount = 0;
    }

}