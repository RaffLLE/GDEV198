using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For AIPath
using Pathfinding;
// For Couroutine
using UnityEngine.Events;

public class HelperBehavior : MonoBehaviour
{
    public GameObject player;
    Vector2 playerDirection;
    float distanceToPlayer;
    Vector2 facingDirection;
    Animator animator;
    AIDestinationSetter destinationSetter;
    public bool assistReady;
    public float assistCooldown;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = gameObject.GetComponent<Animator>();
        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();

        assistReady = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Values relative to player
        playerDirection = (player.transform.position - transform.position).normalized;
        distanceToPlayer = (player.transform.position - transform.position).magnitude;

        facingDirection = playerDirection;

        if (assistReady) {
            animator.Play("Merfolk_Idle");
            if (Input.GetKeyDown(KeyCode.X)) {
                animator.Play("Merfolk_Cast");
                StartCoroutine(AssistCooldown());
            }
        }
        // Look left when facing left
        transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(facingDirection.x), transform.localScale.y, transform.localScale.z);//Mathf.Sign(facingDirection.x);
    }

    private IEnumerator AssistCooldown() {
        assistReady = false;
        yield return new WaitForSeconds(assistCooldown);
        assistReady = true;
    }
}
