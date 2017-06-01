using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
    public float speed = 7f;
    public float walkingSnappyness = 50;
    public float turnSmoothing = 0.3f;

    private Rigidbody playerRigidbody;
    private Animator anim;
    private Vector3 movementDirection;
    private Vector3 facingDirection;
    private Quaternion screenMovementSpace;
    private Vector3 screenMovementForward;
    private Vector3 screenMovementRight;
    private float horizontal;
    private float vertical;

    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        movementDirection = Vector2.zero;
        facingDirection = Vector2.zero;
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        //KeepPlayerOnGround();

        screenMovementSpace = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace * Vector3.forward;
        screenMovementRight = screenMovementSpace * Vector3.right;

        Vector3 movementDirection = horizontal * screenMovementRight + vertical * screenMovementForward;
        if (movementDirection.sqrMagnitude > 1)
            movementDirection.Normalize();

        Vector3 targetVelocity = movementDirection * speed;
        Vector3 deltaVelocity = targetVelocity - playerRigidbody.velocity;
        if (playerRigidbody.useGravity)
            deltaVelocity.y = 0;
        playerRigidbody.AddForce(deltaVelocity * walkingSnappyness, ForceMode.Acceleration);

        bool running = movementDirection.sqrMagnitude > 0;
        anim.SetBool("IsRunning", running);

        Vector3 faceDir = facingDirection;
        if (faceDir == Vector3.zero)
            faceDir = movementDirection;

        transform.LookAt(transform.position + movementDirection);

        /* //Make the character rotate progressively towards the target rotation
        if (faceDir == Vector3.zero)
        {
           playerRigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            float rotationAngle = AngleAroundAxis(transform.forward, faceDir, Vector3.up);
            playerRigidbody.angularVelocity = (Vector3.up * rotationAngle * turnSmoothing);
        }*/

    }


    // The angle between dirA and dirB around axis
    static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
    {
        // Project A and B onto the plane orthogonal target axis
        dirA = dirA - Vector3.Project(dirA, axis);
        dirB = dirB - Vector3.Project(dirB, axis);

        // Find (positive) angle between A and B
        float angle = Vector3.Angle(dirA, dirB);

        // Return angle multiplied with 1 or -1
        return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
    }

    void KeepPlayerOnGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hitInfo;
        //terrain should have mesh collider and be on custom terrain 
        //layer so we don't hit other objects with our raycast
        LayerMask layer = 1 << LayerMask.NameToLayer("Terrain");
        //cast ray
        if (Physics.Raycast(ray, out hitInfo, layer))
        {
            //get where on the z axis our raycast hit the ground
            float groundYPos = hitInfo.point.y;
            //copy current position into temporary container
            Vector3 pos = transform.position;
            pos.y = groundYPos;
            //override our position with the new adjusted position.
            transform.position = pos;
        }
    }
}

