using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For Error
using System;
// For Volume
using UnityEngine.Rendering;

public class FINALPlayerScript : MonoBehaviour
{
    [Header("Objects")]
    public ScreenDamageEffect screenFX;
    public new Smart2DCamera camera;
    private new Rigidbody2D rigidbody;
    private new CapsuleCollider2D collider;
    private Animator animator;
    public PlayerHP playerHealth;
    private FINALEnemyScript[] enemies;

    [Header("Stats")]
    public float moveSpeed;
    public float moveAcceleration; // The larger this is, the faster the player gets to the target velocity

    // VARIABLES
    Vector2 playerInput;
    Vector2 lastInput;
    Vector2 targetVelocity;
    int adrenaline;

    // CALCULATION VARIABLES
    float movementModifier;

    // STATES
    public bool isCrouched;
    public bool isDecaying;
    public bool isCursed;
    public float curseTimer;
    bool canMove;
    //bool canAction;

    [Header("Tumble Action")]
    public float tumbleCooldown;
    bool canTumble;

    [Header ("HP")]
    public float regenCooldown;
    float currRegenCooldown;
    
    [Header("Layers")] 
    public LayerMask vaultableLayer;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        collider = gameObject.GetComponent<CapsuleCollider2D>();
        animator = gameObject.GetComponent<Animator>();
        playerHealth = gameObject.GetComponent<PlayerHP>();

        try {
            camera.Reset();
        } 
        catch (NullReferenceException ex) { 
            Debug.Log(ex);
        }

        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToClosestEnemy = 100.0f;
        adrenaline = 0;

        // getting all enemy info
        try {
            enemies = GameObject.FindObjectsOfType(typeof(FINALEnemyScript)) as FINALEnemyScript[];
        }
        catch (NullReferenceException ex) {
            //Debug.Log(ex);
            enemies = null;
        }

        foreach(FINALEnemyScript enemy in enemies) {
            if (distanceToClosestEnemy > Vector2.Distance(this.transform.position, enemy.transform.position)) {
                distanceToClosestEnemy = Vector2.Distance(this.transform.position, enemy.transform.position);
                camera.lookAt = enemy.transform;
            }
            if (enemy.isChasing || enemy.isProwling) {
                adrenaline += 1;
            }
        }

        if (isDecaying) {
            currRegenCooldown = 1.0f;
            playerHealth.currHP -= Time.deltaTime;

            if (playerHealth.currHP <= 0) {
                isDecaying = false;
                rigidbody.velocity = Vector2.zero;
                DisableAll();
                StopAllCoroutines();
                playerHealth.currHP = 0;
                StartCoroutine(Death());
            }
        }

        if (isCursed) {
            curseTimer -= Time.deltaTime;
            if (curseTimer <= 0) {
                curseTimer = 0;
                isCursed = false;
            }
        }

        if (camera != null && playerHealth.currHP > 0.0f) {
            if (isCursed) {
                camera.ChangeCameraSize(camera.baseCameraSize - 0.5f);
                camera.CameraMoveLock(true);
                camera.CameraSizeLock(true);
            }
            else if (adrenaline > 0) {
                camera.Alert();
            }
            else if (isCrouched) {
                camera.ChangeCameraSize(camera.baseCameraSize - 0.5f);
                camera.CameraMoveLock(true);
                camera.CameraSizeLock(true);
            }
            else {
                camera.Reset();
            }
        }

        if (!canMove) return;

        // Player Input
        playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            isCrouched = !isCrouched;
        }
    }
    
    void FixedUpdate() {

        // if (Input.GetKeyDown(KeyCode.T)) {
        //     StartCoroutine(TakeDamage(1.0f, 2.0f));
        // }

        if (!canMove) return;

        // Look left when facing left
        if (playerInput.magnitude != 0) {
            lastInput = playerInput;
        }

        if (isCursed) {
             // Applying the calculated velocity 
            targetVelocity = lastInput * moveSpeed * movementModifier;
            rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * moveAcceleration);
            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(lastInput.x), transform.localScale.y, transform.localScale.z);
            Slowdown(0.4f);
            return;
        }

        if (currRegenCooldown <= 0) {
            playerHealth.heal(Time.timeScale * 0.0025f);
            if (playerHealth.currHP > playerHealth.maxHP) {
                playerHealth.containHP();
            }
        }
        else {
            currRegenCooldown -= Time.timeScale;
        }

        if (isCrouched) {
            movementModifier = 0.5f;
            if (playerInput.magnitude == 0) {
                playNewAnimation("Player_Crouch_Idle");
            }
            else {
                playNewAnimation("Player_Crouch_Walk");
            }
        }
        else {
            movementModifier = 1.0f;
            if (playerInput.magnitude == 0) {
                playNewAnimation("Player_Idle");
            }
            else {
                playNewAnimation("Player_Walk");
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (canTumble) {
                StartCoroutine(Tumble(playerInput));
            }
        }

        transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(lastInput.x), transform.localScale.y, transform.localScale.z);

        // Applying the calculated velocity 
        targetVelocity = playerInput * moveSpeed * movementModifier;
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, targetVelocity, Time.deltaTime * moveAcceleration);
    }

    // ACTION FUNCTIONS
    private IEnumerator Tumble(Vector2 direction) {
        DisableAll();
        isCrouched = false;
        camera.Reset();
        playNewAnimation("Player_Tumble");
        rigidbody.velocity = direction * 6.0f;
        yield return new WaitForSeconds(0.5f);
        rigidbody.velocity = Vector2.zero;
        playNewAnimation("Player_Getup");
        yield return new WaitForSeconds(0.7f);      
        StartCoroutine(TumbleCooldown(tumbleCooldown));
        EnableAll();
    }
    private IEnumerator TumbleCooldown(float cooldown) {
        canTumble = false;
        yield return new WaitForSeconds(cooldown);
        canTumble = true;
    }

    public IEnumerator TakeDamage(float damageTaken, float immunityDuration, Color color) {
        if (!playerHealth.isImmune && playerHealth.currHP > 0) {
            DisableAll();
            isCrouched = false;
            camera.Reset();
            screenFX.changeColor(color);

            StartRegenCooldown(regenCooldown);
            playerHealth.damage(damageTaken, immunityDuration);
            playNewAnimation("Player_Damaged");
            rigidbody.velocity = Vector2.zero;
            yield return new WaitForSeconds(0.5f);
            if (playerHealth.currHP <= 0) {
                playerHealth.currHP = 0.0f;
                GameOver();
            }
            else {
                EnableAll();
            }
        }
    }

    public IEnumerator Knockback(Vector2 impactDirection, float impactMagnitude, bool damaging, float damageValue, float immunityDuration) {
        if (canMove) {
            DisableAll();
            isCrouched = false;
            camera.Reset();

            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * -Mathf.Sign(impactDirection.x), transform.localScale.y, transform.localScale.z);

            playNewAnimation("Player_Knockedback");
            rigidbody.velocity = impactDirection * impactMagnitude;
            yield return new WaitForSeconds(0.3f);

            rigidbody.velocity = Vector2.zero;

            if (damaging) {
                playNewAnimation("Player_Getup");
                yield return new WaitForSeconds(0.3f);
                StartCoroutine(TakeDamage(damageValue, immunityDuration, Color.red));
                yield return new WaitForSeconds(0.3f);
            }
            else {
                playNewAnimation("Player_Getup");
                yield return new WaitForSeconds(0.4f);
                EnableAll();
            }
        }
    }

    void StartRegenCooldown(float cooldown) {
        currRegenCooldown = cooldown * 100;
    }

    private IEnumerator Death() {
        yield return new WaitForSeconds(0.3f);
        playNewAnimation("Player_Death");
        collider.enabled = false;
        camera.useTempSettings(this.transform, 2.0f, 0.025f);
    }

    public void GameOver() {
        DisableAll();
        StopAllCoroutines();
        StartCoroutine(Death());
    }

    // HELPER FUNCTIONS 

    // This function plays a given animation, but only calls it if the desired animation is not yet playing
    void playNewAnimation(string animationName) {
        if(!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName)) {
            animator.Play(animationName);
        }
    }

    void EnableAll() {
        //canAction = true;
        canMove = true;
    }

    void DisableAll() {
        //canAction = false;
        canMove = false;
    }

    void Reset() {
        EnableAll();
        adrenaline = 0;
        canTumble = true;
    }

    public void Curse(float duration) {
        isCursed = true;
        curseTimer = duration;
    }

    public void Slowdown(float value) {
        rigidbody.velocity = rigidbody.velocity.normalized * value;
    }
}
