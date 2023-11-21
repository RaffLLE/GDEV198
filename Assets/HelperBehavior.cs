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
    public Rigidbody2D rigidbody;
    Vector2 playerDirection;
    public AIPath aipath;
    float distanceToPlayer;
    Vector2 facingDirection;
    Animator animator;
    AIDestinationSetter destinationSetter;
    public bool assistReady;
    public bool isAssisting;
    public float assistCooldown;
    public float responseDelay;
    public float castTime;
    public ParticleSystem responseEffect;
    public Vector2 dirLastMoved;
    public Transform castCircle;
    public float castCircleSizeIncrease;
    public float castCircleMaxSize;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = gameObject.GetComponent<Animator>();
        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        aipath = gameObject.GetComponent<AIPath>();
        rigidbody = gameObject.GetComponent<Rigidbody2D>();

        assistReady = true;
        isAssisting = false;

        castCircle.localScale = Vector3.zero;
        castCircleSizeIncrease = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Values relative to player
        playerDirection = (player.transform.position - transform.position).normalized;
        distanceToPlayer = (player.transform.position - transform.position).magnitude;

        facingDirection = playerDirection;

        if (!AnimatorIsPlaying() && aipath.canMove) {
            if (distanceToPlayer > 2.5f) {
                playAnimationOnce("Merfolk_Swim");
            }
            else {
                playAnimationOnce("Merfolk_Idle");
            }
        }

        if (rigidbody.velocity.magnitude != 0) {
            dirLastMoved = rigidbody.velocity;
        }

        if (distanceToPlayer > 1.5f) {
            aipath.maxSpeed = 4;
        }
        else {
            aipath.maxSpeed = 2;
        }

        castCircle.localScale += new Vector3(castCircleSizeIncrease, 0, 0);

        if (castCircle.localScale.x > castCircleMaxSize) {
            castCircleSizeIncrease = 0;
            castCircle.localScale = Vector3.zero;
        }

        if (aipath.canMove) {
            // Look left when facing left
            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(facingDirection.x), transform.localScale.y, transform.localScale.z);
        }
        else {
            transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x) * Mathf.Sign(dirLastMoved.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public IEnumerator AssistResponse() {
        aipath.canMove = false;
        playAnimationOnce("Merfolk_Swim");
        rigidbody.velocity = new Vector2(player.GetComponent<NewPlayerController>().lastInput.normalized.x, 0) * 2.0f;
        yield return new WaitForSeconds(responseDelay);
        rigidbody.velocity = Vector2.zero;
        playAnimationOnce("Merfolk_Cast");
        castCircleSizeIncrease = 0.2f;
        responseEffect.Play();
        yield return new WaitForSeconds(castTime);
        aipath.canMove = true;
    }

    private IEnumerator AssistCooldown() {
        assistReady = false;
        yield return new WaitForSeconds(assistCooldown);
        assistReady = true;
    }

    void playAnimationOnce(string animationName) {
        if(!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
        }
    }

    bool AnimatorIsPlaying(){
        return animator.GetCurrentAnimatorStateInfo(0).length >
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
}
