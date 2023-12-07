using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Santelmo : FINALEnemyScript
{
    public float followDuration;
    bool isFollowing;
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
        base.Update();

        if (followDuration > 0.0f) {
            followDuration -= Time.deltaTime;
        }
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
        base.playNewAnimation("Santelmo_Idle");
        if (((playerInPeripheralVision()) || distanceToPlayer <= 1.5f) && !player.GetComponent<FINALPlayerScript>().isCursed) {
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

        rotationSpeed = 2.0f;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        yield return new WaitForSeconds(1.0f);

        // Transition
        followDuration = 5.0f;
        UpdateDestination(player.transform);
        StartCoroutine(FollowState());
        isAlerted = false;
    }

    protected IEnumerator FollowState() {
        yield return new WaitForSeconds(aiUpdateDelay);
        slowdownWhenNear = false;
        path.Disable();
        isFollowing = true;
        changeSpeed(chaseMovementSpeed, 
                    chaseRotationSpeed, 
                    chaseMovementAcceleration);
        GetComponent<Rigidbody2D>().velocity = Vector2.Lerp(GetComponent<Rigidbody2D>().velocity, targetVelocity, Time.deltaTime * movementAcceleration);
        if (followDuration <= 0.0f || player.GetComponent<FINALPlayerScript>().isCursed) {
            followDuration = 0.0f;
            path.Enable();
            isFollowing = false;
            path.UpdateDestination(path.waypoints[path.waypointIndex]);
            StartCoroutine(PatrolState());
        }
        else {
            StartCoroutine(FollowState());
        }
    }

    private void OnTriggerStay2D(Collider2D collider) {

        if (collider.transform.tag == "Player") {
            collider.transform.GetComponent<FINALPlayerScript>().Curse(15.0f);
        }
    }
}
