using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public Rigidbody2D rigidbody;

    EnemyBehavior[] enemies;

    public Light2D globalLight;

    public Vector2 movement;
    public float speed;

    public float baseSpeed = 2.0f;
    public float crouchSpeed = 1.0f;

    public float dashSpeed;
    public float dashCooldown;
    public float dashCooldownCounter;
    public float dashDuration;
    public float dashDurationCounter;

    public bool inHazard;

    // Start is called before the first frame update
    void Start() {

        // set dash stats
        dashSpeed = 4.5f;
        dashCooldown = 2.0f;
        dashCooldownCounter = 0.0f;
        dashDuration = 0.55f;
        dashDurationCounter = 0.0f;

        // getting all enemy info
        enemies = Object.FindObjectsOfType(typeof(EnemyBehavior)) as EnemyBehavior[];
    }

    // Update is called once per frame
    void Update() {

            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            movement = movement.normalized;

            if (!inHazard) {
                if (Input.GetKey(KeyCode.LeftShift)) {
                    speed = crouchSpeed;
                }

                else { 
                    speed =  baseSpeed;
                    if (movement.sqrMagnitude > 0)
                    {
                        foreach(EnemyBehavior enemy in enemies) {
                            if (Vector3.Distance(enemy.transform.position, this.transform.position) < enemy.detectRadius) {
                                enemy.rageGaugeIncrease(enemy.maxRage/3 * Time.deltaTime);
                            }    
                        }
                    }
                }

                // dash code
                if (Input.GetKey(KeyCode.Space)) {
                    if (dashCooldownCounter <= 0 && dashDurationCounter <= 0) {
                        dashDurationCounter = dashDuration;

                        foreach(EnemyBehavior enemy in enemies) {
                            if (Vector3.Distance(enemy.transform.position, this.transform.position) < enemy.detectRadius) {
                                enemy.rageGaugeIncrease(enemy.maxRage/2);
                            }    
                        }
                    }
                }

                // start dashing
                if (dashDurationCounter > 0) {
                    dashDurationCounter -= Time.deltaTime;
                    speed = dashSpeed;

                    if (dashDurationCounter <= 0) {
                        dashCooldownCounter = dashCooldown;
                    }
                }

                // start dash cooldown slowing the player
                if (dashCooldownCounter > 0) {
                    dashCooldownCounter -= Time.deltaTime;
                    speed = crouchSpeed;

                    if (dashCooldownCounter <= 0) {
                        speed = baseSpeed;
                    }
                }
            }
            
            animator.SetFloat("velocity", movement.sqrMagnitude);

            // to remember last direction faced
            // LEFT
            if (movement.x < 0) {
                animator.SetFloat("horizontal", -1);
                animator.SetFloat("vertical", 0);
            }
            // RIGHT
            if (movement.x > 0) {
                animator.SetFloat("horizontal", 1);
                animator.SetFloat("vertical", 0);
            }
            // DOWN
            if (movement.y < 0) {
                animator.SetFloat("horizontal", 0);
                animator.SetFloat("vertical", -1);
            }
            // UP
            if (movement.y > 0) {
                animator.SetFloat("horizontal", 0);
                animator.SetFloat("vertical", 1);
            }
    }

    void FixedUpdate() {

        rigidbody.MovePosition(rigidbody.position + movement * speed * Time.fixedDeltaTime);
    }

    // when in contact with a trigger
    private void OnTriggerStay2D(Collider2D collider) {
        Debug.Log(collider.name);
        inHazard = true;
        if (collider.name == "Hazards") {
            speed = crouchSpeed/2;
        }
        else if (collider.name == "Hazards 2") {
            globalLight.color = Color.red;
            globalLight.intensity = 0.8f;
            speed = crouchSpeed;
        }
    }

    private void OnTriggerExit2D(Collider2D collider) {
        speed = baseSpeed;
        globalLight.color = Color.white;
        globalLight.intensity = 0.0f;
        inHazard = false;
    }
}
