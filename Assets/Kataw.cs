using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kataw : MonoBehaviour
{
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = gameObject.GetComponent<Animator>();
        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        aipath = gameObject.GetComponent<AIPath>();
    }
}
