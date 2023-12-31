using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For AIPath
using Pathfinding;

public class Markupo : FINALEnemyScript
{
    public bool isAmbush;
    public float poisonCooldown;
    public float currPoisonCooldown; 
    public GameObject poisonPuddle;
    protected override void Start()
    {
        base.Start();
        base.changeSpeed(base.patrolMovementSpeed, 
                         base.patrolRotationSpeed, 
                         base.patrolMovementAcceleration);
        if (!isAmbush) {
            StartCoroutine(PatrolState());
        }
        currPoisonCooldown = poisonCooldown;
    }

    protected override void Update()
    {
        if (currPoisonCooldown > 0.0f && isPatroling) {
            currPoisonCooldown -= Time.deltaTime;
        }
        base.Update();
    }

    // States
    protected IEnumerator PatrolState() { 
        isPatroling = true;
        path.Enable();
        yield return new WaitForSeconds(aiUpdateDelay);

        slowdownWhenNear = true;

        changeSpeed(patrolMovementSpeed, 
                    patrolRotationSpeed, 
                    patrolMovementAcceleration);

        GetComponent<Rigidbody2D>().velocity = Vector2.Lerp(GetComponent<Rigidbody2D>().velocity, targetVelocity, Time.deltaTime * movementAcceleration);
        if (GetComponent<Rigidbody2D>().velocity.magnitude > 0.5f) {
            base.playNewAnimation("Fire_Worm_Walk");
        }
        else {
            base.playNewAnimation("Fire_Worm_Idle");
        }

        // Transition
        if (playerInDirectVision()) {
            isPatroling = false;
            StartCoroutine(ProwlState());
        }
        else if ((playerInPeripheralVision() && !player.GetComponent<FINALPlayerScript>().isCrouched) || playerInPeripheralVision() && distanceToPlayer < 2.0f) {
            isPatroling = false;
            StartCoroutine(AlertState());
        }
        else if (currPoisonCooldown <= 0.0f) {  
            isPatroling = false;
            StartCoroutine(Excrete());
        }
        else {
            StartCoroutine(PatrolState());
        }
    }

    protected IEnumerator Excrete() {
        path.Disable();

        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        playNewAnimation("Fire_Worm_Death");
        yield return new WaitForSeconds(2.5f);
        if (poisonPuddle != null) {
            Instantiate(poisonPuddle, transform.position, transform.rotation);
        }
        currPoisonCooldown = poisonCooldown;
        StartCoroutine(PatrolState());
    }

    protected IEnumerator AlertState() {
        isAlerted = true;
        path.Disable();

        playNewAnimation("Fire_Worm_Attack");
        UpdateDestination(base.lastSeenLocation);
        rotationSpeed = 2.0f;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        yield return new WaitForSeconds(2.0f);
        playNewAnimation("Fire_Worm_Death");
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
            playNewAnimation("Fire_Worm_Walk");
        }
        else {
            playNewAnimation("Fire_Worm_Idle");
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

        playNewAnimation("Fire_Worm_Attack");
        UpdateDestination(base.lastSeenLocation);
        rotationSpeed = 5.0f;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        playNewAnimation("Fire_Worm_Death");
        yield return new WaitForSeconds(0.5f);

        // Transition
        StartCoroutine(ChaseState());
        isProwling = false;
    }

    protected IEnumerator ChaseState() {
        isChasing = true;
        yield return new WaitForSeconds(aiUpdateDelay);

        slowdownWhenNear = false;

        playNewAnimation("Fire_Worm_Walk");
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
        else {
            StartCoroutine(ChaseState());
        }
    }

    protected IEnumerator Flinch() {

        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        playNewAnimation("Fire_Worm_Hit");
        yield return new WaitForSeconds(0.5f);
        playNewAnimation("Fire_Worm_Death");
        yield return new WaitForSeconds(1.0f);

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
            StartCoroutine(collision.transform.GetComponent<FINALPlayerScript>().Knockback(playerDirection, 5.0f, true, 1.0f, 1.0f));
            StartCoroutine(Flinch());
        }
    }
}
