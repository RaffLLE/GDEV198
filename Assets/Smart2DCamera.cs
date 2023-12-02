using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smart2DCamera : MonoBehaviour
{
    private new Camera camera;

    public Transform follow;
    public Transform lookAt;
    public float baseCameraSize;
    public float maxCameraSizeIncrease;


    public Vector2 offset;
    public float cameraSmoothSpeed;
    public float cameraSizeSmoothSpeed;
    public float skewToLookAt;
    public float maxDistance;

    public bool staticCameraSize;
    public bool staticCameraFocus;

    // Variables
    private Vector3 desiredCameraPosition;
    private float desiredCameraSize;
    private float distanceToLookAt;
    private Vector2 directionToLookAt;

    // Start is called before the first frame update
    void Start()
    {
        camera = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (follow != null && lookAt != null) {
            distanceToLookAt = Vector2.Distance(follow.position, lookAt.position);
            directionToLookAt = lookAt.position - follow.position;
        }
        else {
            distanceToLookAt = 0;
            directionToLookAt = Vector2.up;
        }

        if (staticCameraFocus || distanceToLookAt > maxDistance) {
            desiredCameraPosition = new Vector3(follow.position.x + offset.x, 
                                                follow.position.y + offset.y, 
                                                -10.0f);
        }
        else {

            desiredCameraPosition = new Vector3(follow.position.x + offset.x + directionToLookAt.x * skewToLookAt, 
                                                follow.position.y + offset.y + directionToLookAt.y * skewToLookAt, 
                                                -10.0f);
        }


        camera.transform.position = Vector3.Lerp(transform.position, desiredCameraPosition, cameraSmoothSpeed);

        if (staticCameraSize || distanceToLookAt > maxDistance) {
            desiredCameraSize = baseCameraSize;
        }
        else {
            desiredCameraSize = Mathf.Clamp(distanceToLookAt / 2, baseCameraSize, baseCameraSize + maxCameraSizeIncrease);
        }

        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, desiredCameraSize, cameraSizeSmoothSpeed);
    }

    void ChangeFollow(Transform newFollow) {
        follow = newFollow;    
    }

    void ChangeLookAt(Transform newLookAt) {
        lookAt = newLookAt;    
    }

    void ChangeCameraSize(float size) {
        baseCameraSize = size;    
    }

    void ToggleCameraSizeLock() {
        staticCameraSize = !staticCameraSize;
    }

    void ToggleCameraLock() {
        staticCameraFocus = !staticCameraFocus;
    }

    void RemoveLookAt() {
        lookAt = null;
    }
}
