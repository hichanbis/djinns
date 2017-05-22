using UnityEngine;
using System.Collections;

public enum SwitchType
{
    LERP,
    CUT
}

public class ShotZone : MonoBehaviour
{
    public Shot targetShot;
    public SwitchType switchType = SwitchType.LERP;
    public bool lookAtPlayer = true;
    public float lerpDuration;
    public bool followPlayer = true;
    private Vector3 offset;
    private bool entered = false;
    private bool stillIn = false;
    private Transform playerTr;

    void Start()
    {
        offset = targetShot.GetOffset();
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("Player"))
        {
            entered = true;
            playerTr = c.transform;
        }
    }

    void OnTriggerStay(Collider c)
    {
        if (c.CompareTag("Player"))
        {
            stillIn = true;
            playerTr = c.transform;
        }
    }

    void OnTriggerExit(Collider c)
    {
        //il faut memoriser la derniere position de la caméra et lerper a cette position
    }

    void LookAtPlayer(Transform playerTr)
    {
        //Camera.main.transform.LookAt(c.transform);
        Quaternion rotation = Quaternion.LookRotation(playerTr.position - Camera.main.transform.position);
        Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, rotation, Time.deltaTime * 1.0f);
    }

    void LateUpdate()
    {
        if (entered)
        {
            targetShot.CutOrLerpToShot(switchType, lerpDuration);
        }

        if (stillIn)
        {
            if (lookAtPlayer && playerTr != null)
                LookAtPlayer(playerTr.transform);
            if (followPlayer && playerTr != null) //only follow if distance superior to offset (or not?)
            {
             //   if (Vector3.Distance(Camera.main.transform.position, playerTr.position) >= offset.magnitude)
                    Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, playerTr.transform.position + offset, Time.deltaTime * 1.0f);
            }
        }

        entered = false;
        stillIn = false;
    }
}