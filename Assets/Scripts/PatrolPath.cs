using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PatrolPath : MonoBehaviour
{
    public Transform[] waypoints;
    int waypointIndex;
    public float waitTimeBetweenPoints; 
    bool isActive;

    AIDestinationSetter destinationSetter;

    // Start is called before the first frame update
    void Start()
    {
        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        UpdateDestination(waypoints[waypointIndex]); 
        isActive = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //When enemy reaches patrol point go to next one
        if (Vector2.Distance(this.transform.position, destinationSetter.target.position) < 0.2 && isActive)
        {
            IterateWaypointIndex();
            StartCoroutine(WaitToNewPoint());
        }
    }

    private IEnumerator WaitToNewPoint() { 
        yield return new WaitForSeconds(waitTimeBetweenPoints);
        UpdateDestination(waypoints[waypointIndex]);
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

    public void Disable() {
        isActive = false;
    }

    public void Enable() {
        isActive = true;
        UpdateDestination(waypoints[waypointIndex]);
    }
}
