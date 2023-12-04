using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For Error
using System;

public class FINALPlayerScript : MonoBehaviour
{
    [Header("Objects")]
    public new Smart2DCamera camera;
    private new Rigidbody2D rigidbody;
    private new CapsuleCollider2D collider;
    private Animator animator;
    private PlayerHP playerHealth;

    [Header("Stats")]
    public float moveSpeed;
    public float moveAcceleration; // The larger this is, the faster the player gets to the target velocity

    // VARIABLES
    Vector2 playerInput;
    Vector2 lastInput;
    Vector2 targetVelocity;
    int adrenaline;

    // CALCULATION VARIABLES
    float movementCrouchModifier;

    // STATES
    public bool isCrouched;
    bool canMove;
    //bool canAction;

    [Header("Tumble Action")]
    public float tumbleCooldown;
    bool canTumble;

    [Header ("HP")]
    public float regenCooldown;
    bool canRegen;
    
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
        if (adrenaline > 0) {
            camera.Alert();
        }

        if (!canMove) return;

        // Player Input
        playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            isCrouched = !isCrouched;
            if (camera != null && adrenaline < 1) {
                if (isCrouched) {
                    camera.ChangeCameraSize(2.5f);
                    camera.CameraMoveLock(true);
                    camera.CameraSizeLock(true);
                }
                else {
                    camera.Reset();
                }
            }
        }
    }
    
    void FixedUpdate() {

        if (Input.GetKeyDown(KeyCode.T)) {
            StartCoroutine(TakeDamage(1.0f, 2.0f));
        }

        if (canRegen) {
            playerHealth.heal(Time.timeScale * 0.0025f);
            if (playerHealth.currHP > playerHealth.maxHP) {
                playerHealth.containHP();
            }
        }

        if (!canMove) return;

        if (isCrouched) {
            movementCrouchModifier = 0.5f;
            if (playerInput.magnitude == 0) {
                playNewAnimation("Player_Crouch_Idle");
            }
            else {
                playNewAnimation("Player_Crouch_Walk");
            }
        }
        else {
            movementCrouchModifier = 1.0f;
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

        // Look left when facing left
        if (playerInput.magnitude != 0) {
            lastInput = playerInput;
        }
        transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(lastInput.x), transform.localScale.y, transform.localScale.z);

        // Applying the calculated velocity 
        targetVelocity = playerInput * moveSpeed * movementCrouchModifier;
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

    public IEnumerator TakeDamage(float damageTaken, float immunityDuration) {
        if (!playerHealth.isImmune && playerHealth.currHP > 0) {
            DisableAll();
            StartCoroutine(RegenCooldown(regenCooldown));
            playerHealth.damage(damageTaken, immunityDuration);
            playNewAnimation("Player_Damaged");
            rigidbody.velocity = Vector2.zero;
            if (playerHealth.currHP <= 0) {
                GameOver();
            }
            yield return new WaitForSeconds(0.5f);
            EnableAll();
        }
    }

    public IEnumerator Knockback(Vector2 impactDirection, float impactMagnitude, bool damaging, float damageValue, float immunityDuration) {
        if (canMove) {
            DisableAll();
            isCrouched = false;
            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * -Mathf.Sign(impactDirection.x), transform.localScale.y, transform.localScale.z);

            playNewAnimation("Player_Knockedback");
            rigidbody.velocity = impactDirection * impactMagnitude;
            yield return new WaitForSeconds(0.3f);

            rigidbody.velocity = Vector2.zero;

            if (damaging) {
                playNewAnimation("Player_Getup");
                yield return new WaitForSeconds(0.3f);
                StartCoroutine(TakeDamage(damageValue, immunityDuration));
                yield return new WaitForSeconds(0.3f);
            }
            else {
                playNewAnimation("Player_Getup");
                yield return new WaitForSeconds(0.4f);
                EnableAll();
            }
        }
    }

    private IEnumerator RegenCooldown(float cooldown) {
        canRegen = false;
        yield return new WaitForSeconds(regenCooldown);
        canRegen = true;
    }

    private IEnumerator Death() {
        yield return new WaitForSeconds(0.1f);
        playNewAnimation("Player_Death");
        collider.enabled = false;
        camera.useTempSettings(this.transform, 2.0f, 0.025f);
    }

    public void GameOver() {
        DisableAll();
        StopAllCoroutines();

        StartCoroutine(Death());
        // playNewAnimation("Player_Death");
        // collider.enabled = false;
        // camera.useTempSettings(this.transform, 1.5f, 0.025f);
    }

    // HELPER FUNCTIONS 

    // This function plays a given animation, but only calls it if the desired animation is not yet playing
    void playNewAnimation(string animationName) {
        if(!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName)) {
            animator.Play(animationName);
        }
    }

    public void increaseAdrenaline() {
        adrenaline += 1;
    }

    public void decreaseAdrenaline() {
        adrenaline -= 1;
        if (adrenaline < 1) {
            try {
                camera.Reset();
            }
            catch (NullReferenceException ex) { 
                Debug.Log(ex);
            }
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
}
