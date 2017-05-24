using UnityEngine;

public class CameraFollowPlayerPath : MonoBehaviour
{
    public enum OrientationModes
    {
        none,
        custom,
        lookAtTarget,
        lookAtPathDirection
    }

    [SerializeField]
    private OrientationModes _orientationMode = OrientationModes.lookAtTarget;

    [SerializeField]
    private CameraPath path;

    [SerializeField]
    private float pathLag = 0.0f;

    [SerializeField]
    private float lookAtLerp = 0.9f;

    private Transform player;
    private float lastPercent = 0;
    private bool ignoreNormalise = false;

    private int accuracy = 3;
//the higher the more accurate by an order of magnitude but doesn't cost an order of magnitude! :o)

    //Set the initial position of the cam so we don't jump at the start of the demo
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>().transform;

        float nearestPercent = path.GetNearestPoint(player.position, ignoreNormalise, 5);
        lastPercent = nearestPercent;

        Vector3 nearestPoint = path.GetPathPosition(nearestPercent, ignoreNormalise);
        transform.position = nearestPoint;
        switch (_orientationMode)
        {
            case OrientationModes.none:
                //none
                break;

            case OrientationModes.custom:
                transform.rotation = path.GetPathRotation(nearestPercent, ignoreNormalise);
                break;

            case OrientationModes.lookAtTarget:
                transform.rotation = Quaternion.LookRotation(player.position - transform.position);
                break;

            case OrientationModes.lookAtPathDirection:
                transform.rotation = Quaternion.LookRotation(path.GetPathDirection(nearestPercent));
                break;
        }
    }

    //Update the cam animation
    void LateUpdate()
    {
        float nearestPercent = path.GetNearestPoint(player.position, ignoreNormalise, accuracy);
        float theta = nearestPercent - lastPercent;
        if (theta > 0.5f)
            lastPercent += 1;
        else if (theta < -0.5f)
            lastPercent += -1;

        float usePercent = Mathf.Lerp(lastPercent, nearestPercent, 0.4f);
        lastPercent = usePercent;
        Vector3 nearestPoint = path.GetPathPosition(usePercent, ignoreNormalise);
        Vector3 backwards = -path.GetPathDirection(usePercent, !ignoreNormalise);

        transform.position = Vector3.Lerp(transform.position, nearestPoint + backwards * pathLag, 0.4f);

        switch (_orientationMode)
        {
            case OrientationModes.none:
                //none
                break;

            case OrientationModes.custom:
                transform.rotation = path.GetPathRotation(nearestPercent, ignoreNormalise);
                break;

            case OrientationModes.lookAtTarget:
                //transform.rotation = Quaternion.LookRotation(player.position - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(player.position - transform.position), Time.deltaTime * lookAtLerp);
                break;
            case OrientationModes.lookAtPathDirection:
                transform.rotation = Quaternion.LookRotation(path.GetPathDirection(usePercent));
                break;
        }
    }
}
