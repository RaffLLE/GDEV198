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
            base.isPatroling = StartCoroutine(PatrolState());
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    protected IEnumerator PatrolState() { 
        yield return new WaitForSeconds(aiUpdateDelay);

        GetComponent<Rigidbody2D>().velocity = Vector2.Lerp(GetComponent<Rigidbody2D>().velocity, targetVelocity, Time.deltaTime * movementAcceleration);
        if (GetComponent<Rigidbody2D>().velocity.magnitude > 0.5f) {
            base.playNewAnimation("Fire_Worm_Walk");
        }
        else {
            base.playNewAnimation("Fire_Worm_Idle");
        }

        // Transition
        if (base.playerInPeripheralVision()) {
            base.isAlerted = StartCoroutine(AlertState());
            base.path.Disable();
        }
        else {
            base.isPatroling = StartCoroutine(PatrolState());
        }
    }

    protected IEnumerator AlertState() {
        yield return new WaitForSeconds(aiUpdateDelay);

        base.playNewAnimation("Fire_Worm_Attack");
        UpdateDestination(base.lastSeenLocation);
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        yield return new WaitForSeconds(2.0f);
        base.playNewAnimation("Fire_Worm_Death");
        yield return new WaitForSeconds(1.0f);
        base.changeSpeed(base.patrolMovementSpeed, 
                         base.patrolRotationSpeed, 
                         base.patrolMovementAcceleration);
        base.isPatroling = StartCoroutine(PatrolState());
        base.path.Enable();
    }

    protected IEnumerator SearchState() {
        yield return new WaitForSeconds(aiUpdateDelay);
    }

    protected IEnumerator ProwlState() {
        yield return new WaitForSeconds(aiUpdateDelay);
    }

    protected IEnumerator ChaseState() {
        yield return new WaitForSeconds(aiUpdateDelay);
    }
}
