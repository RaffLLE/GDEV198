using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerController : MonoBehaviour
{
    [Header("Physics Reference")]
    public Rigidbody2D rb;
    private float moveSpeed;
    private Vector2 moveForce;
    private Vector2 playerInput;
    private Vector2 forceToApply;
    public float forceDamping;

    [Header("Base Player Stats")]
    [SerializeField]
    private float baseSpeed;

    [Header("Player Info")]
    private bool moveDisabled;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (!moveDisabled) {
            playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            moveForce = playerInput * moveSpeed;
        }
    }

    void FixedUpdate() {
        forceToApply /= forceDamping;
        if (forceToApply.magnitude < 0.01f)
        {
            forceToApply = Vector2.zero;
        }
        rb.velocity = moveForce + forceToApply;
    }
    void Reset() {
        moveSpeed = baseSpeed;
    }

    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     // Enemy Knockback
    //     if (collision.collider.CompareTag("Enemy"))
    //     {
    //         Vector2 enemyPos = collision.gameObject.transform.position;
    //         Debug.Log(rb.position - enemyPos);
    //         forceToApply += (rb.position - enemyPos) * 50;
    //         //Destroy(collision.gameObject);
    //     }
    // }
}
