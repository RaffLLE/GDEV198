using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For 2D Light
using UnityEngine.Rendering.Universal;
// For Couroutine
using UnityEngine.Events;

public class NewPlayerController : MonoBehaviour
{
    [Header("Object Reference")]
    public Rigidbody2D rb;
    public Light2D light;
    public ParticleSystem callEffect;
    
    [Header("Physics Reference")]
    private float moveSpeed;
    private Vector2 moveForce;
    private Vector2 playerInput;
    private Vector2 forceToApply;
    public float forceDamping;

    [Header("Base Player Stats")]
    public float baseSpeed;

    [Header("Player State")]
    private bool moveDisabled;
    private bool canCall;
    [Header("Player Cooldowns")]
    public float callCooldown;

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

        if (Input.GetKeyDown(KeyCode.E) && canCall) {
            callEffect.Play();
            StartCoroutine(CallCooldown());
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

    private IEnumerator CallCooldown() {
        canCall = false;
        yield return new WaitForSeconds(callCooldown);
        canCall = true;
    }

    void Reset() {
        moveSpeed = baseSpeed;
        moveDisabled = false;
        canCall = true;
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
