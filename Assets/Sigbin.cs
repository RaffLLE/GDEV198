using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sigbin : FINALEnemyScript
{
    public float attackCooldown;
    float currAttackCooldown;
    public float attackDistance; 
    public bool isAttacking;
    private Vector2 tempVelocity;
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        changeSpeed(patrolMovementSpeed, 
                    patrolRotationSpeed, 
                    patrolMovementAcceleration);
        StartCoroutine(PatrolState());
    }

    // Update is called once per frame
    void Update()
    {
        if (currAttackCooldown > 0.0f) {
            currAttackCooldown -= Time.deltaTime;
        }

        base.Update();
        
        // for collision purposes
        tempVelocity = rigidbody.velocity;
    }

    protected IEnumerator PatrolState() { 
        isPatroling = true;
        path.Enable();
        yield return new WaitForSeconds(aiUpdateDelay);

        slowdownWhenNear = true;

        changeSpeed(patrolMovementSpeed, 
                    patrolRotationSpeed, 
                    patrolMovementAcceleration);

        GetComponent<Rigidbody2D>().velocity = Vector2.Lerp(GetComponent<Rigidbody2D>().velocity, targetVelocity, Time.deltaTime * movementAcceleration);
        if (GetComponent<Rigidbody2D>().velocity.magnitude > 0.1f) {
            base.playNewAnimation("Black_Dog_Run");
        }
        else {
            base.playNewAnimation("Black_Dog_Idle");
        }

        // Transition
        if (playerInDirectVision() || (playerInPeripheralVision() && distanceToPlayer < 1.5f)) {
            isPatroling = false;
            StartCoroutine(ProwlState());
        }
        else if ((playerInPeripheralVision() && !player.GetComponent<FINALPlayerScript>().isCrouched)) {
            isPatroling = false;
            StartCoroutine(AlertState());
        }
        else {
            StartCoroutine(PatrolState());
        }
    }

    protected IEnumerator AlertState() {
        isAlerted = true;
        path.Disable();

        playNewAnimation("Black_Dog_Hit");
        UpdateDestination(base.lastSeenLocation);
        rotationSpeed = 2.0f;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        yield return new WaitForSeconds(2.0f);
        playNewAnimation("Black_Dog_Attack");
        yield return new WaitForSeconds(1.0f);

        // Transition
        if (playerInPeripheralVision() || playerInDirectVision()) {
            isAlerted = false;
            StartCoroutine(ProwlState());
        }
        else {
            isAlerted = false;
            StartCoroutine(SearchState());
        }
    }

    protected IEnumerator SearchState() {
        isSearching = true;

        yield return new WaitForSeconds(aiUpdateDelay);
        slowdownWhenNear = true;

        changeSpeed(patrolMovementSpeed * 0.7f, 
                    patrolRotationSpeed * 1.5f, 
                    patrolMovementAcceleration * 10.0f);

        GetComponent<Rigidbody2D>().velocity = Vector2.Lerp(GetComponent<Rigidbody2D>().velocity, targetVelocity, Time.deltaTime * movementAcceleration);
        if (GetComponent<Rigidbody2D>().velocity.magnitude > 0.5f) {
            playNewAnimation("Black_Dog_Run");
        }
        else {
            playNewAnimation("Black_Dog_Idle");
        }

        // Transition
        if (playerInPeripheralVision() || playerInDirectVision()) {
            isSearching = false;
            StartCoroutine(ProwlState());
        }
        else if (base.distanceToTarget < 1.0f) {
            isSearching = false;
            StartCoroutine(PatrolState());
            path.Enable();
        }
        else {
            StartCoroutine(SearchState());
        }
    }

    protected IEnumerator ProwlState() {
        isProwling = true;
        path.Disable();
        yield return new WaitForSeconds(aiUpdateDelay);

        playNewAnimation("Black_Dog_Hit");
        UpdateDestination(base.lastSeenLocation);
        rotationSpeed = 5.0f;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        playNewAnimation("Black_Dog_Attack");
        yield return new WaitForSeconds(0.5f);

        // Transition
        StartCoroutine(ChaseState());
        isProwling = false;
    }

    protected IEnumerator ChaseState() {
        isChasing = true;
        path.Disable();
        yield return new WaitForSeconds(aiUpdateDelay);
        UpdateDestination(base.lastSeenLocation);

        slowdownWhenNear = false;

        playNewAnimation("Black_Dog_Run");
        changeSpeed(chaseMovementSpeed, 
                    chaseRotationSpeed, 
                    chaseMovementAcceleration);
        
        GetComponent<Rigidbody2D>().velocity = Vector2.Lerp(GetComponent<Rigidbody2D>().velocity, targetVelocity, Time.deltaTime * movementAcceleration);

        // Transition
        if (interestFalloff <= 0.0f) {
            StartCoroutine(PatrolState());
            path.Enable();
            isChasing = false;
        }
        else if (currAttackCooldown <= 0.0f && playerInPeripheralVision() && distanceToPlayer <= attackDistance) {
            StartCoroutine(Attack());
        }
        else {
            StartCoroutine(ChaseState());
        }
    }
    private IEnumerator Attack() {

        isAttacking = true;

        // wind up
        Debug.Log("Wind Up");
        animator.Play("Black_Dog_Hit");
        rigidbody.velocity = Vector2.zero;
        changeSpeed(movementSpeed, 
                    0.5f,
                    5.0f);

        yield return new WaitForSeconds(1.0f);

        // set target
        Vector2 attackDirection = facingDirection.normalized; 
        changeSpeed(movementSpeed, 
                    0.1f,
                    5.0f);
        animator.Play("Black_Dog_Attack");
        yield return new WaitForSeconds(0.2f);

        // attack
        Debug.Log("Attack");
        rigidbody.velocity = attackDirection * 8.0f;
        yield return new WaitForSeconds(0.6f);
        currAttackCooldown = attackCooldown;

        // stop
        rigidbody.velocity = Vector2.zero;
        changeSpeed(chaseMovementSpeed, 
                    chaseRotationSpeed,
                    chaseMovementAcceleration);

        animator.Play("Black_Dog_Attack");
        yield return new WaitForSeconds(0.6f);
        // go back to chase
        StartCoroutine(ChaseState());
        isAttacking = false;
    }

    protected IEnumerator Flinch() {

        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        playNewAnimation("Black_Dog_Hit");
        yield return new WaitForSeconds(2.0f);
        playNewAnimation("Black_Dog_Attack");
        yield return new WaitForSeconds(1.0f);

        // Transition
        if (isChasing) {
            StartCoroutine(ChaseState());
        }
        else {
            StartCoroutine(PatrolState());
        }
    }

    protected IEnumerator Knockdown() {

        GetComponent<Rigidbody2D>().velocity = tempVelocity * -1.0f;
        playNewAnimation("Black_Dog_Hit");
        yield return new WaitForSeconds(0.1f);
        
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        playNewAnimation("Black_Dog_Death");
        yield return new WaitForSeconds(2.0f);

        // Transition
        if (isChasing) {
            StartCoroutine(ChaseState());
        }
        else {
            StartCoroutine(PatrolState());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {

        if (collision.transform.tag == "Player") {
            StopAllCoroutines();
            StartCoroutine(collision.transform.GetComponent<FINALPlayerScript>().TakeDamage(0.0f, 10.0f, Color.black));
            player.GetComponent<FINALPlayerScript>().isDecaying = true;
            StartCoroutine(Flinch());
        }
        else if (tempVelocity.magnitude >= 3.0f && isAttacking) {
            Debug.Log("OW");
            StopAllCoroutines();
            StartCoroutine(Knockdown());
        }
    }
}
