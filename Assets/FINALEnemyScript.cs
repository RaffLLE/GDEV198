using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For AIPath
using Pathfinding;

public class FINALEnemyScript : MonoBehaviour
{
    // Automatic Variables
    protected private new Rigidbody2D rigidbody;
    protected private Animator animator;    
    protected private AIPath aipath;
    protected private AIDestinationSetter destinationSetter;
    protected private Transform lastSeenLocation;
    protected private GameObject player;
    protected private PatrolPath path;

    // Movement Variables
    protected private float movementSpeed;
    protected private float rotationSpeed;
    protected private float movementAcceleration;

    [Header("Patrol Movement")]
    public float patrolMovementSpeed;
    public float patrolRotationSpeed;
    public float patrolMovementAcceleration;
    
    [Header("Chase Movement")]
    public float chaseMovementSpeed;
    public float chaseRotationSpeed;
    public float chaseMovementAcceleration;

    [Header("Misc Movement Values")]
    public bool canMove;
    public float slowdownDistance;
    public bool slowdownWhenNear;
    private float moveSpeedModifier;
    private float rotationSpeedModifier;

    [Header("Vision Values")]
    public float peripheralRadius;
    public float visionRadius;
    public float visionAngle;

    // Calculated Variables
    private protected Vector2 targetDirection; // straight line to target
    private protected float distanceToTarget;
    private protected float angleToTarget;
    private protected Vector2 desiredDirection; // current direction of the path 
    private protected Vector2 facingDirection;
    private protected Vector2 targetVelocity;
        
    // Values relative to player
    private protected Vector2 playerDirection;
    private protected float distanceToPlayer;
    private protected float angleToPlayer;

    [Header("States")]
    private protected bool isPatroling;
    private protected bool isAlerted;
    private protected bool isSearching;
    private protected bool isProwling;
    private protected bool isChasing;
    
    [Header("Layers")] 
    public LayerMask obstaclesLayerMask;
    public LayerMask enemiesLayerMask;

    [Header("AI Delay Values")]
    private protected float aiUpdateDelay;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        aipath = gameObject.GetComponent<AIPath>();
        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        lastSeenLocation = GameObject.FindGameObjectWithTag("LastLocation").transform;
        player = GameObject.FindGameObjectWithTag("Player");
        path = gameObject.GetComponent<PatrolPath>();

        if (destinationSetter.target == null) {
            destinationSetter.target = lastSeenLocation;
        }

        Reset();
        ResetStates();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!canMove) { return;}
        // Reset modifiers
        moveSpeedModifier = 1.0f;
        rotationSpeedModifier = 1.0f;

        // Values relative to target
        targetDirection = (destinationSetter.target.transform.position - transform.position).normalized;
        distanceToTarget = (destinationSetter.target.transform.position - transform.position).magnitude;
        angleToTarget = Vector2.Angle(facingDirection, targetDirection);
        
        // Values relative to player 
        if (player != null) {
            playerDirection = (player.transform.position - transform.position).normalized;
            distanceToPlayer = (player.transform.position - transform.position).magnitude;
            angleToPlayer = Vector2.Angle(facingDirection, playerDirection);
        }

        if (isAlerted) {
            desiredDirection = targetDirection;
            //Debug.Log("ALERTED");
        }
        else {
            desiredDirection = (Vector2)(aipath.steeringTarget - transform.position).normalized; // Calculate the direction towards the path laid out by AIPath
            //Debug.Log("not alerted");
        }

        float angle = Vector2.Angle(facingDirection, desiredDirection); // Calculate the angle between the current vector and the target vector
        float signedAngle = Vector2.SignedAngle(facingDirection, desiredDirection); // Calculate the signed angle between current vector and target vector
        float sign = Mathf.Sign(signedAngle); // Gives 1 or -1 given the signedAngle

        // if not facing the same way as direction you turn faster, but move slower
        if (Mathf.Abs(signedAngle) > 25.0f)  {
            moveSpeedModifier = 0.3f;
            rotationSpeedModifier = 1.5f;
        }

        // Slow Down when near
        if (slowdownWhenNear && distanceToTarget < slowdownDistance) {
            moveSpeedModifier = 0.3f;
        }
        // Use Mathf.Lerp to interpolate between the current angle and the target angle
        float step = rotationSpeed * rotationSpeedModifier * Time.deltaTime;
        float newAngle = Mathf.LerpAngle(0, angle * sign, step);

        // Rotate the current vector
        facingDirection = Quaternion.Euler(0, 0, newAngle) * facingDirection;

        targetVelocity = facingDirection * movementSpeed * moveSpeedModifier;
        transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(facingDirection.x), 
                                            transform.localScale.y, 
                                            transform.localScale.z);
                                            
        if (playerInPeripheralVision() || playerInDirectVision()) {
            lastSeenLocation.position = player.transform.position;
        }
    }

    protected private void Reset() {
        
        facingDirection = Vector2.up;
        rigidbody.velocity = Vector2.zero;
    }

    protected private void ResetStates() {
             
        isPatroling = false;
        isAlerted = false;
        isSearching = false;
        isProwling = false;
        isChasing = false;
    }

    // Helper Functions
    protected private void changeSpeed(float newMovementSpeed, float newRotationSpeed, float newAcceleration) {
        
        movementSpeed = newMovementSpeed;
        rotationSpeed = newRotationSpeed;
        movementAcceleration = newAcceleration;
    } 

    protected private void playNewAnimation(string animationName) {
        
        if(!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName)) {
            animator.Play(animationName);
        }
    }

    public void UpdateDestination(Transform nextWaypoint){
        
        destinationSetter.target = nextWaypoint;
    }

    private Vector2 rotateVector(Vector2 v, float angle) {
        
        // Convert the rotation angle from degrees to radians
        float rotationAngleRadians = angle * Mathf.Deg2Rad;

        // Calculate the new rotated vector
        float newX = v.x * Mathf.Cos(rotationAngleRadians) - v.y * Mathf.Sin(rotationAngleRadians);
        float newY = v.x * Mathf.Sin(rotationAngleRadians) + v.y * Mathf.Cos(rotationAngleRadians);

        return new Vector2(newX, newY);
    }

    protected virtual bool playerInPeripheralVision() {
        
        if (player == null) return false;
        
        RaycastHit2D hit = 
            Physics2D.Raycast(transform.position, playerDirection, peripheralRadius, obstaclesLayerMask);

        //Debug.DrawRay(transform.position, playerDirection * peripheralRadius, Color.red);

        if (distanceToPlayer > peripheralRadius) return false;

        if (hit.collider != null) {
            return (((Vector2)transform.position - hit.point).magnitude >= ((Vector2)transform.position - (Vector2)player.transform.position).magnitude);
        }

        return true;
    }

    protected private bool playerInDirectVision() {
        
        if (player == null) return false;
        
        RaycastHit2D hit = 
            Physics2D.Raycast(transform.position, playerDirection, visionRadius , obstaclesLayerMask);

        if (distanceToPlayer > visionRadius) return false;

        if (angleToPlayer > visionAngle/2) {
            return false;
        }

        if (hit.collider != null) {
            return (((Vector2)transform.position - hit.point).magnitude >= ((Vector2)transform.position - (Vector2)player.transform.position).magnitude);
        }

        return true;
    }

    // Gizmos
    void OnDrawGizmos()
    {
        
        if (aipath != null)
        {
            // Draws a blue line from this transform to the target
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + targetDirection.normalized * 1.0f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + facingDirection.normalized * 1.0f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, visionRadius);

            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rotateVector(facingDirection, visionAngle/2) * visionRadius);
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rotateVector(facingDirection, -visionAngle/2) * visionRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, peripheralRadius);
        }
    }
}
