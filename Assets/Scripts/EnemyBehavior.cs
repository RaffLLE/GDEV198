using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Rendering.Universal;

public class EnemyBehavior : MonoBehaviour
{
    // Creates a drop down
    // https://www.youtube.com/watch?v=xZIINWmmlxQ
    enum EnemyType {
        Engkanto, 
        Sigbin, 
        Santelmo,
        Markupo
    };

    [SerializeField] EnemyType enemyType;

    public AIPath enemyMovement;
    public AIDestinationSetter destinationSetter;
    public PatrolPath patrolPath;

    public Animator animator;

    public Light2D visionCone;
    public Light2D peripheralVision;

    Vector3 moveDirection;
    float moveAngle;

    public GameObject player;

    // for relative player location
    float playerDistance;
    float playerLocationAngle;
    Vector3 playerDirection;

    // enemy vision
    float visionInnerAngle;
    float visionOuterAngle;
    float visionRadius;
    float visionRadiusOffset;

    float peripheralVisionRadius;

    float minLightIntensity;
    float maxLightIntensity;

    // how far til you can see the enemy
    float maxEnemyVisibilityDistance;

    // rage gauge
    float rageGauge;
    float minRage;
    public float maxRage;
    float rageIncreaseValue;
    float rageDecreaseValue;

    float rageCountdown;
    float rageMaxCount;

    // other enemy states
    float enemyVisibility;
    public bool playerInSight;
    public bool inRage;
    public bool inHazard;

    // enemy stats
    float baseSpeed;
    float chasingSpeed;
    public float detectRadius;

    // Start is called before the first frame update
    void Start()
    {
        enemyMovement = gameObject.GetComponent<AIPath>();
        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        patrolPath = gameObject.GetComponent<PatrolPath>();

        player = GameObject.FindGameObjectWithTag("Player");

        // !!!----Engkanto Stats----!!!
        if (enemyType == EnemyType.Engkanto) {

            visionInnerAngle = 20.0f;
            visionOuterAngle = 60.0f;
            visionRadius = 3.5f;
            visionRadiusOffset = 0.2f;

            peripheralVisionRadius = 2.0f;

            minLightIntensity = 0.1f; 
            maxLightIntensity = 4.0f; 

            maxEnemyVisibilityDistance = 5.0f;

            rageGauge = 0.0f;
            minRage = 0.0f;
            maxRage = 20.0f;
            rageIncreaseValue = 25.0f;
            rageDecreaseValue = 3.0f;

            rageCountdown = 0.0f;
            rageMaxCount = 15.0f;

            baseSpeed = 1.0f;
            chasingSpeed = 1.5f;
            detectRadius = 5.0f;
        }

        // !!!----Markupo Stats----!!!
        else if (enemyType == EnemyType.Markupo) {
            visionInnerAngle = 20.0f;
            visionOuterAngle = 80.0f;
            visionRadius = 2.5f;
            visionRadiusOffset = 0.2f;

            peripheralVisionRadius = 2.0f;

            minLightIntensity = 0.1f; 
            maxLightIntensity = 5.0f; 

            maxEnemyVisibilityDistance = 5.0f;

            rageGauge = 0.0f;
            minRage = 0.0f;
            maxRage = 15.0f;
            rageIncreaseValue = 30.0f;
            rageDecreaseValue = 2.0f;

            rageCountdown = 0.0f;
            rageMaxCount = 30.0f;

            baseSpeed = 2.0f;
            chasingSpeed = 3.5f;
            detectRadius = 6.0f;
        }



        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        // gets direction vector from aipath
        moveDirection = (enemyMovement.steeringTarget - this.transform.position).normalized;

        playerDistance = Vector3.Distance(player.transform.position, this.transform.position);
        playerDirection = (player.transform.position - this.transform.position).normalized;
        playerLocationAngle = -Vector3.SignedAngle(playerDirection, new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        // gets the angle between the y-axis and the direction
        // moveAngle is negative because its ... reverse if not for some reason ...
        moveAngle = -Vector3.SignedAngle(moveDirection, new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        playerInSight = CheckPlayerInSight();

        //Method to draw the ray in scene for debug purpose
	    Debug.DrawRay(this.transform.position , playerDirection * playerDistance, Color.red);

        //Debug.Log(playerInSight);
    }

    private bool CheckPlayerInSight() {
        //Check if obstacle is between player and enemy
        RaycastHit2D checkObstacle = Physics2D.Raycast(this.transform.position, playerDirection, playerDistance, LayerMask.GetMask("Obstacle"));
        
        // checking if player is within sight
        if ((moveAngle - visionOuterAngle/2 + 5.0f <= playerLocationAngle 
                    && moveAngle + visionOuterAngle/2 - 5.0f >= playerLocationAngle
                    && playerDistance <= visionRadius)
                    || playerDistance <= peripheralVisionRadius) {
            if (checkObstacle.collider == null) { return true;}
            else if (Vector3.Distance(this.transform.position, checkObstacle.collider.transform.position) < 
                        Vector3.Distance(this.transform.position, player.transform.position)) { return false;}
            else { return true;}
        }
        else { return false;}
    }

    void FixedUpdate() {
        // rotate vision cone
        visionCone.transform.eulerAngles = new Vector3(0, 0, moveAngle);

        if (playerDistance <= maxEnemyVisibilityDistance) {
            enemyVisibility = maxLightIntensity - playerDistance/maxEnemyVisibilityDistance * maxLightIntensity;
        }
        else {enemyVisibility = minLightIntensity;}
        visionCone.intensity = peripheralVision.intensity = enemyVisibility;

        if (playerInSight) {
            rageGaugeIncrease(rageIncreaseValue * Time.deltaTime);
            if (!inHazard) { enemyMovement.maxSpeed = baseSpeed/2;}
        }
        else {
            rageGaugeDecrease(rageDecreaseValue * Time.deltaTime);
            if (!inHazard) {  enemyMovement.maxSpeed = baseSpeed;}
        }
        rageGauge = Mathf.Clamp(rageGauge, minRage, maxRage);

        //Debug.Log(playerInSight);
        //Debug.Log(rageGauge);

        visionCone.color = Color.Lerp(Color.white, Color.red, rageGauge/maxRage);
        peripheralVision.color = Color.Lerp(Color.white, Color.red, rageGauge/maxRage);

        if (rageGauge >= maxRage && !inRage) {
            inRage = true;
            rageCountdown = rageMaxCount;

            destinationSetter.target = player.transform;
            enemyMovement.maxSpeed = chasingSpeed;

            peripheralVision.pointLightOuterRadius = peripheralVisionRadius + 0.5f;
            peripheralVision.intensity = 2.5f;
        }

        if (inRage) {

            if (playerInSight) {
                rageCountdown = rageMaxCount;
            }
            else {
                rageCountdown -= 5.0f * Time.deltaTime;
            }

            if (rageCountdown <= 0) {
                patrolPath.UpdateDestination(GetClosestWaypoint(patrolPath.waypoints));
                Reset();
            }
            Debug.Log(rageCountdown);
        }
    }

    Transform GetClosestWaypoint(Transform[] waypoints)
    {
        Transform closestPoint = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Transform w in waypoints)
        {
            float dist = Vector3.Distance(w.position, currentPos);
            if (dist < minDist)
            {
                closestPoint = w;
                minDist = dist;
            }
        }
        return closestPoint;
    }

    public void rageGaugeIncrease(float inc) {
        rageGauge += inc;
    }

    public void rageGaugeDecrease(float inc) {
        rageGauge -= inc;
    }

    // returning to presets
    void Reset() {
        // front vision cone presets
        visionCone.pointLightInnerAngle = visionInnerAngle;
        visionCone.pointLightOuterAngle = visionOuterAngle;
        visionCone.pointLightInnerRadius = 0;
        visionCone.pointLightOuterRadius = visionRadius + visionRadiusOffset;
        visionCone.color = Color.white;

        // peripheral vision presets
        peripheralVision.pointLightOuterAngle = 360;
        peripheralVision.pointLightOuterRadius = peripheralVisionRadius;
        peripheralVision.color = Color.white;

        // overall light
        enemyVisibility = visionCone.intensity = peripheralVision.intensity = minLightIntensity;

        // stats
        enemyMovement.maxSpeed = baseSpeed;

        // states
        rageGauge = 0.0f;
        inRage = false;
    }

    // when in contact with a trigger
    private void OnTriggerStay2D(Collider2D collider) {
        if (collider.name == "Hazards") {
            enemyMovement.maxSpeed = baseSpeed/5;
            Debug.Log("pain");
        }
        else if (collider.name == "Hazards 2" && enemyType != EnemyType.Markupo) {
            enemyMovement.maxSpeed = baseSpeed/8;
            Debug.Log("pain 2");
        }
    }

    private void OnTriggerExit2D(Collider2D collider) {
        enemyMovement.maxSpeed = baseSpeed;
    }
}