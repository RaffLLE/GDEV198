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
    public Rigidbody2D rigidbody;
    public Light2D light;
    public ParticleSystem callEffect;
    public Animator animator;
    public Camera camera;
    
    [Header("Physics Reference")]
    private float moveSpeed;
    private Vector2 moveForce;
    private Vector2 playerInput;
    private Vector2 forceToApply;
    public float forceDamping;

    [Header("Base Player Stats")]
    public float baseSpeed;
    public float movementAcceleration;

    [Header("Player State")]
    private bool moveDisabled;
    private bool actionDisabled;
    private bool isCrouched;

    [Header("Acttions")]
    private bool canCall;
    private bool canTumble;

    [Header("Player Cooldowns")]
    public float callCooldown;
    public float tumbleCooldown;

    [Header("Calculated Values")]
    public float moveSpeedModifier;
    public Vector2 targetVelocity;
    private Vector2 lastInput;

    [Header("Camera Values")]
    public Vector2 offset;
    public Vector3 desiredCameraPosition;
    public float cameraSmoothSpeed;

    // Start is called before the first frame update
    void Start()
    {
        // helps consistent FPS
        Application.targetFrameRate = 60;

        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();

        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        // Movement
        if (!moveDisabled) {
            playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (playerInput.magnitude == 0.0f) {
                if (isCrouched) {
                    playAnimationOnce("Player_Crouch_Idle");
                }
                else {
                    playAnimationOnce("Player_Idle");
                }
            }
            else {
                lastInput = playerInput;
                if (isCrouched) {
                    playAnimationOnce("Player_Crouch_Walk");
                }
                else {
                    playAnimationOnce("Player_Walk");
                }
            }
        }

        // Action Input
        if (!actionDisabled) {
            if (Input.GetKeyDown(KeyCode.E) && canCall) {
                callEffect.Play();
                StartCoroutine(CallCooldown());
            }
            if (Input.GetKeyDown(KeyCode.Space) && canTumble && playerInput.magnitude > 0) {
                StartCoroutine(Tumble(playerInput));
                StartCoroutine(TumbleCooldown());
            }
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                isCrouched = !isCrouched;
            }
        }

        desiredCameraPosition = new Vector3(transform.position.x + offset.x, 
                                            transform.position.y + offset.y, 
                                            -10.0f);

        camera.transform.position = Vector3.Lerp(camera.transform.position, desiredCameraPosition, cameraSmoothSpeed);

        // Look left when facing left
        transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(lastInput.x), transform.localScale.y, transform.localScale.z);//Mathf.Sign(facingDirection.x);

        if (isCrouched) {
            moveSpeedModifier = 0.5f;
        }
        else {
            moveSpeedModifier = 1.0f;
        }

        targetVelocity = playerInput * moveSpeed * moveSpeedModifier;
    }

    void FixedUpdate() {
        if (!moveDisabled) {
            rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * movementAcceleration);
        }
    }

    private IEnumerator Tumble(Vector2 direction) {
        DisableAll();
        Debug.Log("Tumble");
        playAnimationOnce("Player_Tumble");
        rigidbody.velocity = direction * 6.0f;
        yield return new WaitForSeconds(0.5f);
        rigidbody.velocity = Vector2.zero;
        playAnimationOnce("Player_Getup");
        yield return new WaitForSeconds(0.3f);
        Debug.Log("Done");
        EnableAll();
    }

    private IEnumerator CallCooldown() {
        canCall = false;
        yield return new WaitForSeconds(callCooldown);
        canCall = true;
    }

    private IEnumerator TumbleCooldown() {
        canTumble = false;
        yield return new WaitForSeconds(tumbleCooldown);
        canTumble = true;
    }

    void playAnimationOnce(string animationName) {
        if(!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
        }
    }

    private void DisableAll() {
        moveDisabled = true;
        actionDisabled = true;
    }

    private void EnableAll() {
        moveDisabled = false;
        actionDisabled = false;
    }

    void Reset() {
        moveSpeed = baseSpeed;
        moveDisabled = false;
        actionDisabled = false;
        canCall = true;
        canTumble = true;
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
