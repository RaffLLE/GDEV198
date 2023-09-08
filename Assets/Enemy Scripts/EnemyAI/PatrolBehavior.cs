using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolBehavior : SteeringBehavior
{
    public List<Transform> patrolPoints;

    [SerializeField]
    private bool showGizmo = true;

    int pointCounter = 0;

    float maxDistanceToPoint = 1.0f;

    [SerializeField]
    float patrolStrength = 0.5f;

    //gizmo parameters
    private Vector2 pointPosition;
    private float[] interestsTemp;

    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, AIData aiData)
    {
        pointPosition = patrolPoints[pointCounter].position;
        // is seeking current target
        if (aiData.currentTarget != null) {
            return (danger, interest);
        }
        // if not patrol
        //If we havent yet reached the target do the main logic of finding the interest directions
        Vector2 directionToTarget = (pointPosition - (Vector2)transform.position);
        if (directionToTarget.magnitude < maxDistanceToPoint){
            pointCounter = WrapAround(pointCounter + 1, patrolPoints.Count);
        }
        for (int i = 0; i < interest.Length; i++)
        {
            float result = Vector2.Dot(directionToTarget.normalized, Directions.eightDirections[i]);

            //accept only directions at the less than 90 degrees to the target direction
            if (result > 0)
            {
                float valueToPutIn = result;
                if (valueToPutIn > interest[i])
                {
                    interest[i] = valueToPutIn * patrolStrength;
                }

            }
        }
        interestsTemp = interest;
        return (danger, interest);
    }

    int WrapAround(int count, int max){
        if (count >= max){
            return 0;
        }
        else return count;
    }

    private void OnDrawGizmos()
    {
        if (showGizmo == false)
            return;
        Gizmos.DrawSphere(pointPosition, 0.2f);

        if (Application.isPlaying && interestsTemp != null)
        {
            if (interestsTemp != null)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < interestsTemp.Length; i++)
                {
                    Gizmos.DrawRay(transform.position, Directions.eightDirections[i] * interestsTemp[i]*2);
                }
            }
        }
    }
}
