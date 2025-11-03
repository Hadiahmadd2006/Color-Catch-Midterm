using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;             // Assigned automatically when a Player spawns
    private Rigidbody targetRb;          // Rigidbody of the player

    [Header("Framing")]
    public float followDistance = 7f;    
    public float height = 3f;            
    public float lookUpOffset = 1.2f;    

    [Header("Smoothing")]
    public float positionSmoothTime = 0.12f; 
    public float rotationSmoothSpeed = 10f;  

    [Header("Heading Detection")]
    public float minSpeedForHeading = 0.15f; 
    private Vector3 lastForward = Vector3.forward;

    [Header("Collision (Optional)")]
    public LayerMask collisionMask;      
    public float camCollisionRadius = 0.2f;

    private Vector3 _posVel;             

    void Start()
    {
        // Try to find a player automatically on start
        FindNewTarget();
    }

    void LateUpdate()
    {
        // If the player was destroyed or changed (e.g., new theme), re-acquire
        if (target == null)
        {
            FindNewTarget();
            if (target == null) return; // no player in scene yet
        }

        if (targetRb == null) targetRb = target.GetComponent<Rigidbody>();

        // Determine forward based on the ball's horizontal velocity
        Vector3 flatVel = Vector3.zero;
        #if UNITY_6000_OR_NEWER
        if (targetRb != null) flatVel = Vector3.ProjectOnPlane(targetRb.linearVelocity, Vector3.up);
        #else
        if (targetRb != null) flatVel = Vector3.ProjectOnPlane(targetRb.linearVelocity, Vector3.up);
        #endif

        Vector3 desiredForward;
        if (flatVel.sqrMagnitude > minSpeedForHeading * minSpeedForHeading)
        {
            desiredForward = flatVel.normalized;
            lastForward = desiredForward;
        }
        else
        {
            desiredForward = lastForward;
        }

        // Desired camera position
        Vector3 rawDesiredPos = target.position - desiredForward * followDistance + Vector3.up * height;

        // Collision handling
        Vector3 focusPoint = target.position + Vector3.up * lookUpOffset;
        Vector3 camDir = (rawDesiredPos - focusPoint);
        float camDist = camDir.magnitude;
        Vector3 desiredPos = rawDesiredPos;

        if (camDist > 0.001f)
        {
            camDir /= camDist;
            if (Physics.SphereCast(focusPoint, camCollisionRadius, camDir, out RaycastHit hit, camDist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                desiredPos = hit.point - camDir * 0.05f;
            }
        }

        // Smooth follow
        Vector3 smoothedPos = Vector3.SmoothDamp(transform.position, desiredPos, ref _posVel, positionSmoothTime);
        transform.position = smoothedPos;

        // Smooth rotation
        Quaternion targetRot = Quaternion.LookRotation(focusPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothSpeed * Time.deltaTime);
    }

    // ------------------ NEW METHODS ------------------

    // Automatically finds the player in the scene
    public void FindNewTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            target = playerObj.transform;
            targetRb = playerObj.GetComponent<Rigidbody>();
            Debug.Log("[CameraFollow] Found new player: " + playerObj.name);
        }
    }

    // Called by GameManager after spawning a new player (optional)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        targetRb = newTarget.GetComponent<Rigidbody>();
        Debug.Log("[CameraFollow] Target manually set to " + newTarget.name);
    }
}