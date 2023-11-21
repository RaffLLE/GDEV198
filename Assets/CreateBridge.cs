using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBridge : MonoBehaviour
{
    public Collider2D collider;
    public GameObject land;
    public GameObject water;

    private void OnTriggerStay2D(Collider2D collider) {
        Debug.Log(collider.name);
        if (collider.name == "Cast Circle") {
            land.SetActive(true);
            water.SetActive(false);
        }
    }
}
