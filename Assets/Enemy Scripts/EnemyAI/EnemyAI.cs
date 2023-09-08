using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    private EnemyController controller;

    [Header("Alert")]
    [SerializeField]
    private GameObject alertWarning;
    [SerializeField]
    private AudioClip alertSound;

    [SerializeField]
    private List<SteeringBehavior> steeringBehaviors;

    [SerializeField]
    private List<Detector> detectors;

    [SerializeField]
    private AIData aiData;

    [Header("Detection Update")]
    [SerializeField]
    private float detectionDelay = 0.05f, aiUpdateDelay = 0.06f;

    [Header("Attack")]
    [SerializeField]
    private float attackDistance = 0.5f, attackDelay = 1f;

    [Header("Alert")]
    [SerializeField]
    private float alertDelay = 2.0f, jerkDuration = 1.0f, jerkIntensity = 10.0f;

    [Header("Patrol")]
    [SerializeField]
    private float patrolSpeed = 2.0f;

    [SerializeField]
    private Vector2 movementInput;

    [SerializeField]
    private ContextSolver movementDirectionSolver;

    //bool following = false;

    private void Start()
    {
        //Detecting Player and Obstacles around
        InvokeRepeating("PerformDetection", 0, detectionDelay);
        controller = GetComponent<EnemyController>();
        StartCoroutine(Patrol());
    }

    private void PerformDetection()
    {
        foreach (Detector detector in detectors)
        {
            detector.Detect(aiData);
        }
    }

    private void Update()
    {
        if (aiData.GetTargetsCount() > 0)
        {
            //Target acquisition logic
            aiData.currentTarget = aiData.targets[0];
        }
        //Moving the Agent
        controller.MovementInput(movementInput);
    }

    private IEnumerator Alerted(){

        //following = true;
        // set target to last position player seen
        movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviors, aiData);

        // show warning sign
        alertWarning.SetActive(true);
        SoundManager.Instance.PlaySound(alertSound, 0.3f);

        // jerk
        controller.ChangeSpeed(jerkIntensity);
        yield return new WaitForSeconds(jerkDuration);

        // stop
        controller.ChangeSpeed(0.0f);
        yield return new WaitForSeconds(alertDelay);

        // follow
        alertWarning.SetActive(false);
        controller.Reset();
        StartCoroutine(ChaseAndAttack());
    }

    private IEnumerator ChaseAndAttack()
    {
        if (aiData.currentTarget == null)
        {
            //Stopping Logic
            Debug.Log("Stopping");
            movementInput = Vector2.zero;
            //following = false;
            StartCoroutine(Patrol());
        }
        else
        {
            float distance = Vector2.Distance(aiData.currentTarget.position, transform.position);
            if (distance < attackDistance)
            {
                //Attack logic
                movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviors, aiData) * 0;
                Debug.Log("Attack");
                yield return new WaitForSeconds(attackDelay);
                StartCoroutine(ChaseAndAttack());
            }
            else
            {
                //Chase logic
                movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviors, aiData);
                yield return new WaitForSeconds(aiUpdateDelay);
                StartCoroutine(ChaseAndAttack());
            }

        }

    }

    private IEnumerator Patrol()
    {
        if (aiData.currentTarget != null)
        {
            StartCoroutine(Alerted());
        }
        else {
            movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviors, aiData);
            controller.ChangeSpeed(patrolSpeed);
            yield return new WaitForSeconds(aiUpdateDelay);
            StartCoroutine(Patrol());
        }
        yield break;
    }
}