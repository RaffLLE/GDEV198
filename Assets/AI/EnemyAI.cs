using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    private List<Detector> detectors;

    [SerializeField]
    private AIData aiData;

    [SerializeField]
    private float detectionDelay = 0.05f;

    private void Start() {
        InvokeRepeating("PerformDetection", 0, detectionDelay);
    }

    private void PerformDetection(){
        foreach (Detector detector in detectors){
            detector.Detect(aiData);
        }
    }
}
