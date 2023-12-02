using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FINALPlayerScript : MonoBehaviour
{
    // REFERENCE
    private new Rigidbody2D rigidbody;
    private new CapsuleCollider2D collider;
    private Animator animator;

    // STATS
    public float moveSpeed;
    public float moveAcceleration; // The larger this is, the faster the player gets to the target velocity

    // VARIABLES
    Vector2 playerInput;
    Vector2 targetVelocity;
    Vector2 lastInput;

    bool isCrouched;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        collider = gameObject.GetComponent<CapsuleCollider2D>();
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Player Input
        playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            isCrouched = !isCrouched;
        }
    }

    void FixedUpdate() {

        targetVelocity = playerInput * moveSpeed;

        if (isCrouched) {
            if (playerInput.magnitude == 0) {
                playNewAnimation("Player_Crouch_Idle");
            }
            else {
                playNewAnimation("Player_Crouch_Walk");
            }
        }
        else {
            if (playerInput.magnitude == 0) {
                playNewAnimation("Player_Idle");
            }
            else {
                playNewAnimation("Player_Walk");
            }
        }

        // Look left when facing left
        if (playerInput.magnitude != 0) {
            lastInput = playerInput;
        }
        transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(lastInput.x), transform.localScale.y, transform.localScale.z);

        // Applying the calculated velocity 
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * moveAcceleration);
    }

    // HELPER FUNCTIONS 

    // This function plays a given animation, but only calls it if the desired animation is not yet playing
    void playNewAnimation(string animationName) {
        if(!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
        }
    }
}
