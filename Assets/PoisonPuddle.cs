using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonPuddle : MonoBehaviour
{
    private GameObject puddle;
    public float damage;
    [Range(0.0f, 1.0f)]
    public float slowAmount;
    public float duration;

    void Start()
    {
        puddle = this.gameObject;
        Destroy(puddle, duration);
    }

    void OnDestroy() {
        //Debug.Log("pain");
    }

    private void OnTriggerStay2D(Collider2D collider) {

        if (collider.transform.tag == "Player") {
            StartCoroutine(collider.transform.GetComponent<FINALPlayerScript>().TakeDamage(0.5f, 1.5f));
            collider.transform.GetComponent<FINALPlayerScript>().Slowdown(slowAmount);
        }
    }
}
