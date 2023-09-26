using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For AIPath
using Pathfinding;
// For Couroutine
using UnityEngine.Events;
// For 2D Light
using UnityEngine.Rendering.Universal;

public class NewEnemyBehavior : MonoBehaviour
{
    [Header("Objects")]
    public GameObject player;
    public AIPath aipath;
    public Rigidbody2D rigidbody;
    public AIDestinationSetter destinationSetter;
    Animator animator;    
    public Light2D visionCone;
    public Light2D peripheralVision;
    public LayerMask obstaclesLayerMask;
    public LayerMask enemiesLayerMask;

    [Header("Calculated Values")]
    public Vector2 targetDirection; 
    public Vector2 facingDirection;
    public Vector2 targetVelocity;
    public float movementSpeed;
    public float rotationSpeed;
    public float peripheralRadius;
    public float visionRadius;
    public float moveSpeedModifier;
    public float rotationSpeedModifier;
    public Transform lastSeenLocation;
    float directionSign = 1.0f;
    Vector2 tempVelocity;
    Vector2 playerDirection;
    float distanceToPlayer;
    float angleToPlayer;

    [Header("AI Delay Values")]
    public float aiUpdateDelay;
    public float aiStartDelay;

    [Header("Movement Values")]
    public float slowdownDistance;
    public float movementAcceleration;
    public float patrolMovementSpeed;
    public float patrolRotationSpeed;
    public float chaseMovementSpeed;
    public float chaseRotationSpeed;
    
    [Header("Vision Values")]
    public float normalVisionRadius;
    public float alertVisionRadius;
    public float visionAngle;
    public float normalPeripheralRadius;
    public float alertPeripheralRadius;

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

    [Header("Alerted Values")]
    public float alertTimeDelay;

    [Header("Attack Values")]
    public float attackCooldown;
    public float attackDistance;
    public float attackWindup;
    public float attackDuration;
    public float attackIntensity;
    public bool canAttack = true;

    [Header("Stunned Values")]
    public float stunDuration;

    [Header("Others")]
    public float currVisionFalloffTimer;
    public float maxVisionFalloffTime;

    // Start is called before the first frame update
    void Start()
    {
        aipath = gameObject.GetComponent<AIPath>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        player = GameObject.FindGameObjectWithTag("Player");
        animator = gameObject.GetComponent<Animator>();

        facingDirection = Vector2.up;
        rigidbody.velocity = Vector2.zero;
        StartCoroutine(StartState());

        changeSpeed(patrolMovementSpeed,
                    patrolRotationSpeed);
        
        animator.Play("Flying_Enemy_Idle");
    }

    void Update()
    {
        // If player is in sights
        if (playerSeen()) {
            // Update last seen location to where player was seen and make the target facing direction the player
            lastSeenLocation.position = player.transform.position;
            targetDirection = (Vector2)(player.transform.position - transform.position).normalized;
        }
        else {
            // If not, face the direction where they are pathing
            targetDirection = (Vector2)(aipath.steeringTarget - transform.position).normalized; // Calculate the direction towards the path laid out by AIPath
        }

        float angle = Vector2.Angle(facingDirection, targetDirection); // Calculate the angle between the current vector and the target vector
        float signedAngle = Vector2.SignedAngle(facingDirection, targetDirection); // Calculate the signed angle between current vector and target vector
        float sign = Mathf.Sign(signedAngle); // Gives 1 or -1 given the signedAngle

        // Values relative to player
        playerDirection = (player.transform.position - transform.position).normalized;
        distanceToPlayer = (player.transform.position - transform.position).magnitude;
        angleToPlayer = Vector2.Angle(facingDirection, playerDirection);

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

        // Rotate the current vector
        facingDirection = Quaternion.Euler(0, 0, newAngle) * facingDirection;

        // Adjust vision cone to face facing direction
        visionCone.transform.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, facingDirection));

        visionCone.pointLightOuterRadius = visionRadius + 0.5f;
        peripheralVision.pointLightOuterRadius = Mathf.Min(peripheralRadius + 0.5f, visionRadius);

        // Look left when facing left
        transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(facingDirection.x), transform.localScale.y, transform.localScale.z);//Mathf.Sign(facingDirection.x);

        targetVelocity = targetDirection * movementSpeed * moveSpeedModifier;// * Time.deltaTime;

        // for collision purposes
        tempVelocity = rigidbody.velocity;

        //Debug.Log(rigidbody.velocity.magnitude);
    }

    // ---------------------------------------------------
    // States
    // ---------------------------------------------------

    private IEnumerator StartState(){

        yield return new WaitForSeconds(aiStartDelay);
        targetClosestPatrolPoint();
        changeSpeed(patrolMovementSpeed,
                    patrolRotationSpeed);
        peripheralRadius = normalPeripheralRadius;
        visionRadius = normalVisionRadius;
        StartCoroutine(PatrolState());
        rigidbody.drag = 1.0f;
    }

    private IEnumerator PatrolState() {
        yield return new WaitForSeconds(aiUpdateDelay);

        changeSpeed(patrolMovementSpeed, 
                    patrolRotationSpeed);
        peripheralRadius = normalPeripheralRadius;
        visionRadius = normalVisionRadius;

        // Move towards facingDirection with acceleration
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * movementAcceleration);

        if (Vector2.Distance(transform.position, destinationSetter.target.position) < 0.2){

            yield return new WaitForSeconds(waitTimeBetweenPoints);
            IterateWaypointIndex();
            UpdateDestination(patrolPoints[pointIndex]);
        }
        if (playerSeen()){

            StartCoroutine(Alerted());
        }
        else {

            StartCoroutine(PatrolState());
        }
    }

    private IEnumerator Alerted() {

        peripheralRadius = alertPeripheralRadius;
        visionRadius = alertVisionRadius;
        animator.Play("Flying_Enemy_Alerted");
        yield return new WaitForSeconds(alertTimeDelay);
        animator.Play("Flying_Enemy_Idle");

        Debug.Log("Alerted");
        UpdateDestination(lastSeenLocation);
        rigidbody.velocity = Vector2.zero;
        changeSpeed(patrolMovementSpeed * 0.8f,
                    patrolRotationSpeed * 1.2f);
        StartCoroutine(Searching());
    }

    private IEnumerator Searching() {
        yield return new WaitForSeconds(aiUpdateDelay);

        Debug.Log("Searching");
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * movementAcceleration);
        if (playerSeen()) {
            currChaseTimer = maxChaseTimer;
            StartCoroutine(ChaseState());
        }
        else {
            if (Vector2.Distance(lastSeenLocation.position, transform.position) < 0.5f){
                UpdateDestination(patrolPoints[pointIndex]);
                StartCoroutine(PatrolState());
            }
            else {
                StartCoroutine(Searching());
            }
        }
    }

    private IEnumerator ChaseState() {

        yield return new WaitForSeconds(aiUpdateDelay);
        peripheralRadius = alertPeripheralRadius;
        visionRadius = alertVisionRadius;

        RaycastHit2D hit = 
            Physics2D.Raycast((Vector2)transform.position + playerDirection * 0.5f, playerDirection, distanceToPlayer, enemiesLayerMask);

        changeSpeed(chaseMovementSpeed, 
                    chaseRotationSpeed);
        if (distanceToPlayer < attackDistance 
                && (playerSeen())) {
            if (distanceToPlayer < attackDistance * 0.95f) {
                rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, 
                                                -playerDirection * 1.2f, 
                                                Time.deltaTime * movementAcceleration);
            }
            else {
                
                rigidbody.velocity = rotateVector(targetVelocity * Mathf.Clamp((distanceToPlayer/attackDistance), 0, 1) * 0.3f, 90.0f * directionSign);
            }
        }
        else {

            rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * movementAcceleration);
        }

        if (playerSeen()) {

            currChaseTimer = maxChaseTimer;
        }
        else {
            currChaseTimer -= Time.deltaTime;
        }

        if (currChaseTimer <= 0 && Vector2.Distance(lastSeenLocation.position, transform.position) < 0.4f) {
            UpdateDestination(patrolPoints[pointIndex]);
            StartCoroutine(PatrolState());
        }
        else {
            if (Vector2.Distance(player.transform.position, transform.position) < attackDistance 
                && canAttack
                && (angleToPlayer < visionAngle/2)
                && hit.collider == null) {
                StartCoroutine(Attack());
            }
            else {
                currChaseTimer -= 2.0f * Time.deltaTime;
                StartCoroutine(ChaseState());

            }
        }

        //Debug.Log(currChaseTimer);
    }

    private IEnumerator Attack() {

        float lockOnDelay = attackWindup * 0.2f;

        // wind up
        Debug.Log("Wind Up");
        animator.Play("Flying_Enemy_Alerted");
        rigidbody.velocity = Vector2.zero;
        changeSpeed(movementSpeed, 
                    0.5f);

        yield return new WaitForSeconds(attackWindup - lockOnDelay);

        // set target
        Vector2 attackDirection = facingDirection.normalized; 
        changeSpeed(movementSpeed, 
                    0.1f);
        animator.Play("Flying_Enemy_Attack");
        yield return new WaitForSeconds(lockOnDelay);

        // attack
        Debug.Log("Attack");
        rigidbody.velocity = attackDirection * attackIntensity;
        yield return new WaitForSeconds(attackDuration);
        StartCoroutine(AttackCooldown());

        // stop
        rigidbody.velocity = Vector2.zero;
        changeSpeed(movementSpeed, 
                    chaseRotationSpeed);

        // go back to chase
        animator.Play("Flying_Enemy_Idle");
        StartCoroutine(ChaseState());
    }

    private IEnumerator Stunned() {
        Debug.Log("Stunned");
        animator.Play("Flying_Enemy_Knockdown");
        rigidbody.velocity = Vector2.zero;
        changeSpeed(0, 0);
        peripheralRadius = alertVisionRadius;
        visionRadius = 0.6f;
        rigidbody.drag = 20.0f;
        yield return new WaitForSeconds(stunDuration);
        rigidbody.drag = 1.0f;
        peripheralRadius = alertPeripheralRadius;
        animator.Play("Flying_Enemy_Idle");
        changeSpeed(chaseMovementSpeed, 
                    chaseRotationSpeed);
        canAttack = false;
        if (playerSeen()) {
            peripheralRadius = alertPeripheralRadius;
            visionRadius = alertVisionRadius;
            StartCoroutine(ChaseState());
        }
        else {
            peripheralRadius = alertPeripheralRadius;
            visionRadius = alertVisionRadius;
            StartCoroutine(Searching());
        }
        yield return new WaitForSeconds(3.0f);
        canAttack = true;
    }

    private IEnumerator Knockback(Vector2 collisionPoint, bool collidedWithPlayer) {
        //rigidbody.velocity = (collisionPoint - (Vector2)transform.position).normalized * 5.0f;
        rigidbody.velocity = -tempVelocity * 0.8f;
        yield return new WaitForSeconds(0.2f);
        if (collidedWithPlayer) { 
            animator.Play("Flying_Enemy_Idle");
            StartCoroutine(AttackCooldown());
            StartCoroutine(ChaseState());
        }
        else {
            StartCoroutine(Stunned());
        }
    }

    // ---------------------------------------------------
    // End of States
    // ---------------------------------------------------

    // ---------------------------------------------------
    // Helper Functions
    // ---------------------------------------------------

    private IEnumerator AttackCooldown() {
        canAttack = false;
        yield return new WaitForSeconds(Random.Range(attackCooldown, attackCooldown + 1.0f));
        canAttack = true;
    }

    public void changeSpeed(float newMovementSpeed, float newRotationSpeed) {
        movementSpeed = newMovementSpeed;
        rotationSpeed = newRotationSpeed;
    } 

    public void targetClosestPatrolPoint(){
        pointIndex = GetClosestPointIndex(patrolPoints);
        if (patrolPoints != null){
            destinationSetter.target = patrolPoints[pointIndex];
        }
        else {
            destinationSetter.target = transform;
        }
    }

    private bool playerSeen() {
        return (playerInDirectVision() || playerInPeripheralVision());
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

    // ---------------------------------------------------
    // End of Helper Functions
    // ---------------------------------------------------

    // ---------------------------------------------------
    // Built In Functions
    // ---------------------------------------------------

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

    private void OnCollisionEnter2D(Collision2D collision) {

        if (!canAttack) {
            directionSign = -directionSign;
        }
        if (tempVelocity.magnitude >= 3.0f) {
            Debug.Log("OW");
            StopAllCoroutines();
            StartCoroutine(Knockback(collision.gameObject.transform.position, collision.collider.CompareTag("Player")));
        }
        // You can access collision information and handle the collision here
    }

    // ---------------------------------------------------
    // End of Built In Functions
    // ---------------------------------------------------
}
