using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// For AIPath
using Pathfinding;

public class Markupo : FINALEnemyScript
{
    // Patrol Speed
    public float patrolMovementSpeed;
    public float patrolRotationSpeed;
    // Chase Speed
    public float chaseMovementSpeed;
    public float chaseRotationSpeed;
    // Misc Movement Values
    public float slowdownDistance;
    public float movementAcceleration;

    void Start()
    {
        base.Start();
        base.changeSpeed(patrolMovementSpeed, patrolRotationSpeed);
        base.movementAcceleration = this.movementAcceleration;
    }

    void Update()
    {
        base.Update();
    }
}
