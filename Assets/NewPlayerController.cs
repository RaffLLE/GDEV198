using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For 2D Light
using UnityEngine.Rendering.Universal;
// For Couroutine
using UnityEngine.Events;
// For Error
using System;

public class NewPlayerController : MonoBehaviour
{
    [Header("Object Reference")]
    public new Rigidbody2D rigidbody;
    public Light2D playerLight;
    public Light2D globalLight;
    public ParticleSystem callEffect;
    public Animator animator;
    public new Camera camera;
    public new CapsuleCollider2D collider;
    NewEnemyBehavior[] enemies;
    public HelperBehavior helper;
    
    [Header("Physics Reference")]
    private float moveSpeed;
    private Vector2 moveForce;
    private Vector2 playerInput;
    private Vector2 forceToApply;
    public float forceDamping;

    [Header("Base Player Stats")]
    public float baseSpeed;
    public float movementAcceleration;
    public float detectionRadius;

    [Header("Player State")]
    private bool moveDisabled;
    private bool actionDisabled;
    private bool isCrouched;
    public float MaxHP;
    public float CurrHP;
    private bool isImmune;
    private bool inHazard;
    private bool isChased; // If enemies are chasing

    [Header("Player Cooldowns")]
    public float callCooldown;
    public float tumbleCooldown;
    public float immunityDuration;

    [Header("Actions")]
    private bool canCall;
    private bool canTumble;

    [Header("Calculated Values")]
    public float moveSpeedModifier;
    public Vector2 targetVelocity;
    public Vector2 lastInput;
    private Vector2 directionToClosestEnemy;

    [Header("Camera Values")]
    public Vector2 offset;
    public Vector3 desiredCameraPosition;
    public float cameraSmoothSpeed;
    
    [Header("HP Regen Values")]
    public float hpRegenInterval;
    public bool canRegen;

    // Start is called before the first frame update
    void Start()
    {
        // helps consistent FPS
        Application.targetFrameRate = 60;

        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        collider = gameObject.GetComponent<CapsuleCollider2D>();
        
        // getting all enemy info
        try {
            enemies = GameObject.FindObjectsOfType(typeof(NewEnemyBehavior)) as NewEnemyBehavior[];
        }
        catch (NullReferenceException ex) {
            //Debug.Log(ex);
            enemies = null;
        }

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
            if (isCrouched) {
                collider.size = new Vector2(0.2f, 0.3f);
                playerLight.pointLightOuterRadius = 0.5f;
                playerLight.pointLightInnerRadius = 0.4f;
            }
            else {
                collider.size = new Vector2(0.3f, 0.6f);
                playerLight.pointLightOuterRadius = 1.0f;
                playerLight.pointLightInnerRadius = 0.5f;
            }
        }

        if (!isChased && !isImmune && canRegen && CurrHP < MaxHP) {
            StartCoroutine(hpRegen());
        }

        // Action Input
        if (!actionDisabled || Time.timeScale == 0) {
            if (Input.GetKeyDown(KeyCode.E) && canCall) {
                callEffect.Play();
                if (helper != null) {
                    StartCoroutine(helper.AssistResponse());
                }
                    StartCoroutine(CallCooldown());
            }
            if (Input.GetKeyDown(KeyCode.Space) && canTumble && playerInput.magnitude > 0) {
                StartCoroutine(Tumble(playerInput));
                StartCoroutine(TumbleCooldown());
            }
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                isCrouched = !isCrouched;
            }
            if (Input.GetKeyDown(KeyCode.T)) {
                inHazard = !inHazard;
            }
        }

        Vector2 dirToEnemy;
        directionToClosestEnemy = Vector2.up * 100.0f;
        isChased = false;

        foreach(NewEnemyBehavior enemy in enemies) {
            dirToEnemy = enemy.transform.position - this.transform.position;
            if (directionToClosestEnemy.magnitude > dirToEnemy.magnitude) {
                directionToClosestEnemy = dirToEnemy;
            }
            if (enemy.isAlert) {
                isChased = true;
            }
        }

        if (directionToClosestEnemy.magnitude <= detectionRadius) {
            directionToClosestEnemy = directionToClosestEnemy.normalized;
        }
        else {
            directionToClosestEnemy = Vector2.zero;
        }

        // Helper NPC Info
        Vector2 directionToHelper;
        float distanceToHelper;

        try {
            directionToHelper = helper.transform.position - transform.position;
            distanceToHelper = directionToHelper.magnitude;
        } 
        catch (NullReferenceException ex) {
            
            //Debug.Log(ex);
            directionToHelper = Vector2.down;
            distanceToHelper = 0.0f;
        }

        // CAMERA
        desiredCameraPosition = new Vector3(transform.position.x + offset.x + directionToClosestEnemy.x * 2.0f, 
                                            transform.position.y + offset.y + directionToClosestEnemy.y * 2.0f, 
                                            -10.0f);

        float desiredCameraSize = 0;
        
        if (!isChased) {
            desiredCameraSize = 3.0f;
            if (distanceToHelper > 2.0f) {
                desiredCameraPosition = new Vector3(transform.position.x + offset.x + directionToHelper.normalized.x * 2.0f, 
                                            transform.position.y + offset.y + directionToHelper.normalized.y * 2.0f, 
                                            -10.0f);
            }
            else if (isCrouched) {
                desiredCameraSize = 2.0f;
            }
        }
        else {
            desiredCameraSize = 4.0f;
        }
        
        if (CurrHP <= 0) {
            desiredCameraPosition = new Vector3(transform.position.x + offset.x, 
                                                transform.position.y + offset.y, 
                                                -10.0f);
            desiredCameraSize = 1.5f;
        }

        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, desiredCameraSize, cameraSmoothSpeed);

        camera.transform.position = Vector3.Lerp(camera.transform.position, desiredCameraPosition, cameraSmoothSpeed);
        // END OF CAMERA

        globalLight.color = Color.Lerp(Color.red, Color.white, CurrHP/MaxHP);

        // Look left when facing left
        transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(lastInput.x), transform.localScale.y, transform.localScale.z);//Mathf.Sign(facingDirection.x);

        if (isCrouched) {
            moveSpeedModifier = 0.5f;
        }
        else {
            moveSpeedModifier = 1.0f;
        }

        targetVelocity = playerInput * moveSpeed * moveSpeedModifier;

        if (inHazard) {
            HazardDebuff(0.1f, 0.5f);
        }
    }

    void FixedUpdate() {
        if (!moveDisabled) {
            rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * movementAcceleration);
        }
        enemies = GameObject.FindObjectsOfType(typeof(NewEnemyBehavior)) as NewEnemyBehavior[];
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

    public IEnumerator TakeDamage(float damageTaken) {
        if (!isImmune) {
            DisableAll();
            Debug.Log("OOF");
            playAnimationOnce("Player_Damaged");
            CurrHP -= damageTaken;
            rigidbody.velocity = Vector2.zero;
            StartCoroutine(immunityFrame());
            yield return new WaitForSeconds(0.5f);
            if (CurrHP <= 0) {
                GameOver();
            }
            else {
                EnableAll();
            }
        }
    }

    public void GameOver() {
        StopAllCoroutines();
        StartCoroutine(Death());
    }

    private IEnumerator Death() {
        DisableAll();
        rigidbody.velocity = Vector2.zero;
        playAnimationOnce("Player_Death");
        yield return new WaitForSeconds(0.1f);
        collider.enabled = false;
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

    private IEnumerator immunityFrame() {
        isImmune = true;
        yield return new WaitForSeconds(immunityDuration);
        isImmune = false;
    }

    private IEnumerator hpRegen() {
        canRegen = false;
        CurrHP += 0.1f;
        yield return new WaitForSeconds(hpRegenInterval);
        canRegen = true;
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
        CurrHP = MaxHP;
    }

    void HazardDebuff(float slowedMovementSpeed, float damage) {
        rigidbody.velocity = rigidbody.velocity.normalized * slowedMovementSpeed;
        if (damage > 0) {
            StartCoroutine(TakeDamage(damage));
        }
    }

    private void OnTriggerStay2D(Collider2D collider) {
        //Debug.Log(collider.name);
        if (collider.name.Contains("Poison Hazard")) {
            HazardDebuff(0.3f, 0.5f);
            //Destroy(collider.gameObject);
        }
        if (collider.name.Contains("Water Hazard")) {
            HazardDebuff(0.1f, 0.0f);
        }
        else {
            Debug.Log("Unknown Hazard");
        }
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
