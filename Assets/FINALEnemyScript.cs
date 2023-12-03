using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For AIPath
using Pathfinding;

public class FINALEnemyScript : MonoBehaviour
{
    // Automatic Variables
    new Rigidbody2D rigidbody;
    Animator animator;    
    AIPath aipath;
    AIDestinationSetter destinationSetter;
    Transform lastSeenLocation;

    // Movement Variables
    private protected float movementSpeed;
    private protected float rotationSpeed;
    private protected float movementAcceleration;

    // Calculated Variables
    Vector2 targetDirection; 
    Vector2 facingDirection;
    Vector2 targetVelocity;

    [Header("AI Delay Values")]
    private protected float aiUpdateDelay;

    [Header("States")]
    private protected Coroutine patrolState;

    // Start is called before the first frame update
    private protected void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        aipath = gameObject.GetComponent<AIPath>();
        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        lastSeenLocation = GameObject.FindGameObjectWithTag("LastLocation").transform;

        patrolState = StartCoroutine(PatrolState());

        Reset();
    }

    // Update is called once per frame
    private protected void Update()
    {
        if (destinationSetter.target == null) {
            destinationSetter.target = lastSeenLocation;
        }

        targetDirection = (Vector2)(aipath.steeringTarget - transform.position).normalized; // Calculate the direction towards the path laid out by AIPath

        float angle = Vector2.Angle(facingDirection, targetDirection); // Calculate the angle between the current vector and the target vector
        float signedAngle = Vector2.SignedAngle(facingDirection, targetDirection); // Calculate the signed angle between current vector and target vector
        float sign = Mathf.Sign(signedAngle); // Gives 1 or -1 given the signedAngle

        // Use Mathf.Lerp to interpolate between the current angle and the target angle
        float step = rotationSpeed *  Time.deltaTime;
        float newAngle = Mathf.LerpAngle(0, angle * sign, step);

        // Rotate the current vector
        facingDirection = Quaternion.Euler(0, 0, newAngle) * facingDirection;

        // Look left when facing left
        transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(facingDirection.x), transform.localScale.y, transform.localScale.z);//Mathf.Sign(facingDirection.x);

        targetVelocity = targetDirection * movementSpeed;
    }

    protected private IEnumerator PatrolState() {
        yield return new WaitForSeconds(aiUpdateDelay);
        // Move towards facingDirection with acceleration
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * movementAcceleration);
        StartCoroutine(PatrolState());
    }

    protected private void Reset() {
        facingDirection = Vector2.up;
        rigidbody.velocity = Vector2.zero;
    }

    public void changeSpeed(float newMovementSpeed, float newRotationSpeed) {
        movementSpeed = newMovementSpeed;
        rotationSpeed = newRotationSpeed;
    } 
}
