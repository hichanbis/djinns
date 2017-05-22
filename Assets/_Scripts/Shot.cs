using UnityEngine;
using System.Collections;
public class Shot : MonoBehaviour
{
    public Vector3 focalPoint;

    public Vector3 GetOffset()
    {
        return transform.position - focalPoint;
    }

    public void CutOrLerpToShot(SwitchType switchType, float lerpDuration)
    {
        if (switchType.Equals(SwitchType.CUT))
        {
            Camera.main.transform.localPosition = transform.position;
            Camera.main.transform.localRotation = transform.rotation;
        }
        else
        {
            StartCoroutine(LerpToShot(lerpDuration));
            Debug.Log("lerp done");
            
        }
    }

    IEnumerator LerpToShot(float lerpDuration)
    {
        float t = 0.0f;
        Vector3 startingPos = Camera.main.transform.position;
        Quaternion startingRot = Camera.main.transform.rotation;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (Time.timeScale / lerpDuration);
            Camera.main.transform.position = Vector3.Lerp(startingPos, transform.position, t);
            Camera.main.transform.rotation = Quaternion.Slerp(startingRot, transform.rotation, t);
            yield return null;
        }
        yield return null;
        transform.position = Camera.main.transform.position;
        transform.rotation = Camera.main.transform.rotation;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            transform.LookAt(focalPoint);
            CutOrLerpToShot(SwitchType.CUT, 0);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, focalPoint);
    }
}