using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For AIPath
using Pathfinding;

public class Markupo : FINALEnemyScript
{
    public bool isAmbush;
    protected override void Start()
    {
        base.Start();
        base.changeSpeed(base.patrolMovementSpeed, 
                         base.patrolRotationSpeed, 
                         base.patrolMovementAcceleration);
        if (!isAmbush) {
            StartCoroutine(PatrolState());
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override bool playerInPeripheralVision() {

        // Can't see player in peripheral if crouched        
        if (player.GetComponent<FINALPlayerScript>().isCrouched) {return false; }

        return base.playerInPeripheralVision();
    }

    // States
    protected IEnumerator PatrolState() { 
        isPatroling = true;
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
        if (base.playerInPeripheralVision()) {
            base.path.Disable();
            StartCoroutine(AlertState());
        }
        else {
            StartCoroutine(PatrolState());
        }

        isPatroling = false;
    }

    protected IEnumerator AlertState() {
        isAlerted = true;

        base.playNewAnimation("Fire_Worm_Attack");
        UpdateDestination(base.lastSeenLocation);
        base.rotationSpeed = 2.0f;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        yield return new WaitForSeconds(2.0f);
        base.playNewAnimation("Fire_Worm_Death");
        yield return new WaitForSeconds(1.0f);

        // Transition
        if (playerInPeripheralVision()) {
            StartCoroutine(ProwlState());
        }
        else {
            StartCoroutine(SearchState());
        }

        isAlerted = false;
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
        if (playerInPeripheralVision()) {
            StartCoroutine(ProwlState());
        }
        else if (base.distanceToTarget < 1.0f) {
            StartCoroutine(PatrolState());
            path.Enable();
        }
        else {
            StartCoroutine(SearchState());
        }
        isSearching = false;
    }

    protected IEnumerator ProwlState() {
        isProwling = true;
        yield return new WaitForSeconds(aiUpdateDelay);

        player.GetComponent<FINALPlayerScript>().increaseAdrenaline();

        playNewAnimation("Fire_Worm_Attack");
        UpdateDestination(base.lastSeenLocation);
        rotationSpeed = 5.0f;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);
        playNewAnimation("Fire_Worm_Death");
        yield return new WaitForSeconds(0.5f);

        // Transition
        isProwling = false;
        StartCoroutine(ChaseState());

        isProwling = false;
    }

    protected IEnumerator ChaseState() {
        isChasing = true;
        yield return new WaitForSeconds(aiUpdateDelay);

        slowdownWhenNear = false;

        changeSpeed(chaseMovementSpeed, 
                    chaseRotationSpeed, 
                    chaseMovementAcceleration);
        
        GetComponent<Rigidbody2D>().velocity = Vector2.Lerp(GetComponent<Rigidbody2D>().velocity, targetVelocity, Time.deltaTime * movementAcceleration);

        // Transition
        if (distanceToTarget < 1.0f) {
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
