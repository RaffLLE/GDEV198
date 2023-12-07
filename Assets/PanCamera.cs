using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanCamera : MonoBehaviour
{
    public new Smart2DCamera camera;
    public Transform view;
    public float size;
    public float panSpeed;

    private void OnTriggerStay2D(Collider2D collider) {

        if (collider.transform.tag == "Player") {
            camera.useTempSettings(view, size, panSpeed);
        }
    }
    private void OnTriggerExit2D(Collider2D collider) {

        if (collider.transform.tag == "Player") {
            camera.useNormalSettings();
        }
    }
}
