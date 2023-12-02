using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FINALPlayerScript : MonoBehaviour
{
    // REFERENCE
    private Rigidbody2D rigidbody;
    private CapsuleCollider2D collider;
    private Animator animator;

    // STATS
    public float moveSpeed;

    // VARIABLES
    Vector2 playerInput;
    Vector2 targetVelocity;

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
        playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        targetVelocity = playerInput * moveSpeed;
    }

    void FixedUpdate() {
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime);
    }
}
