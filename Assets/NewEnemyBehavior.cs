using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class NewEnemyBehavior : MonoBehaviour
{
    public AIPath aipath;
    public Rigidbody2D rigidbody;

    public Vector2 targetDirection; // The target vector you want to match
    public float rotationSpeed; // Speed of rotation
    public Vector2 facingDirection; // What we will rotate

    public float movementSpeed;
    public float angleModifier;

    // Start is called before the first frame update
    void Start()
    {
        aipath = gameObject.GetComponent<AIPath>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();

        facingDirection = Vector2.up;
    }

    // Update is called once per frame
    void Update()
    {
        targetDirection = (Vector2)(aipath.steeringTarget - this.transform.position).normalized; 

        float angle = Vector2.Angle(facingDirection, targetDirection); // Calculate the angle between the current vector and the target vector
        float signedAngle = Vector2.SignedAngle(facingDirection, targetDirection); // Calculate the signed angle between current vector and target vector

        angleModifier = 1 - Mathf.Abs(signedAngle)/180.0f; // Calculate the intensity of the speed based on angle magnitude (bigger turn means slower movement)

        float sign = Mathf.Sign(signedAngle);

        // Use Mathf.Lerp to interpolate between the current angle and the target angle
        float step = rotationSpeed *  Time.deltaTime;
        float newAngle = Mathf.LerpAngle(0, angle * sign, step);

        // Rotate the current vector
        facingDirection = Quaternion.Euler(0, 0, newAngle) * facingDirection;

        // Move towards facingDirection
        rigidbody.velocity = targetDirection * movementSpeed * angleModifier * Time.deltaTime;

        //Debug.Log(Mathf.Abs(signedAngle));

    }

    void FixedUpdate()
    {

    }

    void OnDrawGizmos()
    {
        if (aipath != null)
        {
            // Draws a blue line from this transform to the target
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position, (Vector2)this.transform.position + targetDirection.normalized * 1.0f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(this.transform.position, (Vector2)this.transform.position + facingDirection.normalized * 1.0f);
        }
    }
}
