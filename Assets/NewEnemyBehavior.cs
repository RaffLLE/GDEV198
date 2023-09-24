using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For AIPath
using Pathfinding;
// For Couroutine
using UnityEngine.Events;

public class NewEnemyBehavior : MonoBehaviour
{
    [Header("Objects")]
    public GameObject player;
    public AIPath aipath;
    public Rigidbody2D rigidbody;
    public AIDestinationSetter destinationSetter;
    public LayerMask obstaclesLayerMask;

    [Header("Calculated Values")]
    public Vector2 targetDirection; 
    public Vector2 facingDirection;
    public Vector2 targetVelocity;
    public float movementSpeed;
    public float rotationSpeed;
    public Transform lastSeenLocation;

    [Header("AI Delay Values")]
    public float aiUpdateDelay;
    public float aiStartDelay;

    [Header("Movement Values")]
    public float minMovementSpeed;
    public float slowdownMovementSpeed;
    public float slowdownDistance;
    public float maxMovementSpeed;
    public float movementAcceleration;
    public float maxRotationSpeed; 
    public float minRotationSpeed;
    
    [Header("Vision Values")]
    public float directVisionRadius;
    public float directVisionAngle;
    public float peripheralRadius;

    [Header("Patrol Values")]
    public List<Transform> patrolPoints = new List<Transform>();
    public int pointIndex = 0;
    public float waitTimeBetweenPoints;

    [Header("Rage Meter")]
    public float currentRage;
    public float maxRage;

    [Header("Chase Values")]
    public float maxChaseTimer;
    public float currChaseTimer;
    public float chaseSpeed;

    [Header("Alerted Values")]
    public float alertTimeDelay;

    [Header("Attack Values")]
    public float attackCooldown;
    public float attackDistance;
    public float attackWindup;
    public float attackDuration;
    public bool canAttack = true;

    // Start is called before the first frame update
    void Start()
    {
        aipath = gameObject.GetComponent<AIPath>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        player = GameObject.FindGameObjectWithTag("Player");

        facingDirection = Vector2.up;
        rigidbody.velocity = Vector2.zero;
        StartCoroutine(StartState());

        movementSpeed = minMovementSpeed;
        rotationSpeed = minRotationSpeed;
        
    }

    // Update is called once per frame
    void Update()
    {
        targetDirection = (Vector2)(aipath.steeringTarget - transform.position).normalized; // Calculate the direction towards the path laid out by AIPath

        float angle = Vector2.Angle(facingDirection, targetDirection); // Calculate the angle between the current vector and the target vector
        float signedAngle = Vector2.SignedAngle(facingDirection, targetDirection); // Calculate the signed angle between current vector and target vector
        float sign = Mathf.Sign(signedAngle); // Gives 1 or -1 given the signedAngle

        float moveSpeedModifier;
        float rotationSpeedModifier;

        // if not facing the same way as direction you turn faster, but move slower
        if (Mathf.Abs(signedAngle) > 25.0f)  
        {
            moveSpeedModifier = 0.3f;
            rotationSpeedModifier = 1.5f;
        }
        else {
            if (Vector2.Distance(transform.position, destinationSetter.target.position) < slowdownDistance) 
            {
                moveSpeedModifier = 0.3f;
                rotationSpeedModifier = 0.3f;
            }
            else 
            {
                moveSpeedModifier = 1.0f;
                rotationSpeedModifier = 1.0f;
            }
        }

        // Use Mathf.Lerp to interpolate between the current angle and the target angle
        float step = rotationSpeed * rotationSpeedModifier *  Time.deltaTime;
        float newAngle = Mathf.LerpAngle(0, angle * sign, step);

        if (playerInDirectVision() || playerInPeripheralVision()) {
            lastSeenLocation.position = player.transform.position;
        }

        // Rotate the current vector
        facingDirection = Quaternion.Euler(0, 0, newAngle) * facingDirection;

        targetVelocity = targetDirection * movementSpeed * moveSpeedModifier;// * Time.deltaTime;
    }

    private IEnumerator StartState(){

        yield return new WaitForSeconds(aiStartDelay);
        pointIndex = GetClosestPointIndex(patrolPoints);
        if (patrolPoints != null){
            destinationSetter.target = patrolPoints[pointIndex];
        }
        else {
            destinationSetter.target = transform;
        }
        movementSpeed = maxMovementSpeed;
        rotationSpeed = maxRotationSpeed;
        StartCoroutine(PatrolState());
    }

    private IEnumerator PatrolState() {

        // Move towards facingDirection
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * movementAcceleration);

        if (Vector2.Distance(transform.position, destinationSetter.target.position) < 0.2){

            yield return new WaitForSeconds(waitTimeBetweenPoints);
            IterateWaypointIndex();
            UpdateDestination(patrolPoints[pointIndex]);
        }

        yield return new WaitForSeconds(aiUpdateDelay);
        if (playerInDirectVision() || playerInPeripheralVision()){

            StartCoroutine(Alerted());
        }
        else {

            StartCoroutine(PatrolState());
        }
    }

    private IEnumerator Alerted() {
        Debug.Log("Alerted");
        destinationSetter.target = lastSeenLocation;
        rigidbody.velocity = Vector2.zero;
        yield return new WaitForSeconds(alertTimeDelay);
        movementSpeed = maxMovementSpeed;
        rotationSpeed = maxRotationSpeed;
        StartCoroutine(Searching());
    }

    private IEnumerator Searching() {
        yield return new WaitForSeconds(aiUpdateDelay);
        Debug.Log("Searching");
        rigidbody.velocity = targetVelocity;
        if (playerInDirectVision() || playerInPeripheralVision()) {

            currChaseTimer = maxChaseTimer;
            movementSpeed = maxMovementSpeed * 1.2f;
            rotationSpeed = maxRotationSpeed * 1.2f;
            StartCoroutine(ChaseState());
        }
        else {
            if (Vector2.Distance(lastSeenLocation.position, transform.position) < 0.5f){
                StartCoroutine(PatrolState());
            }
            else {
                StartCoroutine(Searching());
            }
        }
    }

    private IEnumerator ChaseState() {
        rigidbody.velocity = targetVelocity;
        yield return new WaitForSeconds(aiUpdateDelay);
        Debug.Log(currChaseTimer);

        if (playerInDirectVision() || playerInPeripheralVision()) {

            currChaseTimer = maxChaseTimer;
        }
        else {

            currChaseTimer -= Time.deltaTime;
        }

        if (currChaseTimer <= 0) {
            StartCoroutine(PatrolState());
            movementSpeed = maxMovementSpeed;
            rotationSpeed = maxRotationSpeed;
        }
        else {
            if (Vector2.Distance(player.transform.position, transform.position) < attackDistance && canAttack) {
                StartCoroutine(Attack());
            }
            else {
                StartCoroutine(ChaseState());

            }
        }
    }

    private IEnumerator Attack() {
        // wind up
        rigidbody.velocity = Vector2.zero;
        yield return new WaitForSeconds(attackWindup);
        Vector2 attackDirection = targetVelocity.normalized; 

        // attack
        rigidbody.velocity = attackDirection * 5.0f;
        yield return new WaitForSeconds(attackDuration);
        StartCoroutine(AttackCooldown());

        // stop
        rigidbody.velocity = Vector2.zero;

        // go back to chase
        StartCoroutine(ChaseState());
    }

    private IEnumerator AttackCooldown() {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private bool playerInPeripheralVision() {
        if (player == null) return false;

        Vector2 playerDirection = (player.transform.position - transform.position).normalized;
        float distanceToPlayer = (player.transform.position - transform.position).magnitude;
        RaycastHit2D hit = 
            Physics2D.Raycast(transform.position, playerDirection, peripheralRadius, obstaclesLayerMask);

        //Debug.DrawRay(transform.position, playerDirection * peripheralRadius, Color.red);

        if (distanceToPlayer > peripheralRadius) return false;

        if (hit.collider != null) {
            return (((Vector2)transform.position - hit.point).magnitude >= ((Vector2)transform.position - (Vector2)player.transform.position).magnitude);
        }

        return true;
    }

    private bool playerInDirectVision() {
        if (player == null) return false;

        Vector2 playerDirection = (player.transform.position - transform.position).normalized;
        float distanceToPlayer = (player.transform.position - transform.position).magnitude;
        RaycastHit2D hit = 
            Physics2D.Raycast(transform.position, playerDirection, directVisionRadius   , obstaclesLayerMask);


        float angleToPlayer = Vector2.Angle(facingDirection, playerDirection);

        if (distanceToPlayer > directVisionRadius) return false;

        if (angleToPlayer > directVisionAngle/2) {
            return false;
        }

        if (hit.collider != null) {
            return (((Vector2)transform.position - hit.point).magnitude >= ((Vector2)transform.position - (Vector2)player.transform.position).magnitude);
        }

        return true;
    }

    private Vector2 rotateVector(Vector2 v, float angle) {
        // Convert the rotation angle from degrees to radians
        float rotationAngleRadians = angle * Mathf.Deg2Rad;

        // Calculate the new rotated vector
        float newX = v.x * Mathf.Cos(rotationAngleRadians) - v.y * Mathf.Sin(rotationAngleRadians);
        float newY = v.x * Mathf.Sin(rotationAngleRadians) + v.y * Mathf.Cos(rotationAngleRadians);

        return new Vector2(newX, newY);
    }

    void IterateWaypointIndex(){
        pointIndex++;
        if (pointIndex == patrolPoints.Count){
            pointIndex = 0;
        }
        UpdateDestination(patrolPoints[pointIndex]);
    }

    public void UpdateDestination(Transform nextWaypoint)
    {
        destinationSetter.target = nextWaypoint;
    }

    int GetClosestPointIndex(List<Transform> points)
    {
        Transform closestPoint = null;
        int closestPointIndex = 0;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        for (int i = 0; i < points.Count; i++)
    {
            float dist = Vector3.Distance(points[i].position, currentPos);
            if (dist < minDist)
            {
                closestPoint = points[i];
                closestPointIndex = i;
                minDist = dist;
            }
        }
        return closestPointIndex;
    }

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
            Gizmos.DrawWireSphere(transform.position, directVisionRadius);

            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rotateVector(facingDirection, directVisionAngle/2) * directVisionRadius);
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rotateVector(facingDirection, -directVisionAngle/2) * directVisionRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, peripheralRadius);
        }
    }
}
