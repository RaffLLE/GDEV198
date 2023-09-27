using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterRemovalMagic : MonoBehaviour
{
    public Collider2D collider;

    // Start is called before the first frame update
    void Start()
    {
        collider = gameObject.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay2D(Collider2D collider) {
        if (collider.name.Contains("Water Hazard") && transform.localScale != Vector3.zero) {
            Destroy(collider.gameObject);
        }
    }
}
