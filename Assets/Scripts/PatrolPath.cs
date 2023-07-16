using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PatrolPath : MonoBehaviour
{
    public Transform[] waypoints;
    int waypointIndex;

    AIDestinationSetter destinationSetter;
    EnemyBehavior enemyBehavior;

    // Start is called before the first frame update
    void Start()
    {
        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        enemyBehavior = gameObject.GetComponent<EnemyBehavior>();
        UpdateDestination(waypoints[waypointIndex]); 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //When enemy reaches patrol point go to next one
        if (Vector2.Distance(this.transform.position, destinationSetter.target.position) < 0.2)
        {
            IterateWaypointIndex();
            UpdateDestination(waypoints[waypointIndex]);
        }
    }

    //sets target of script to next waypoint
    public void UpdateDestination(Transform nextWaypoint)
    {
        destinationSetter.target = nextWaypoint;
    }

    //iterates between waypoints
    void IterateWaypointIndex(){
        waypointIndex++;
        if (waypointIndex == waypoints.Length){
            waypointIndex = 0;
        }
    }
}
