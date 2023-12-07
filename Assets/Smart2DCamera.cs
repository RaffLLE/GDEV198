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
    public Transform test;


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
    private float activeCameraSize;

    // Temporary Variables
    private Transform tempTarget;
    private bool useTemp;
    private float tempCameraSize;
    private float tempMoveSmooth; 
    private float tempSizeSmooth;

    // Start is called before the first frame update
    void Start()
    {
        camera = gameObject.GetComponent<Camera>();
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) {
            Alert();
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            Reset();
        }
        if (Input.GetKeyDown(KeyCode.F)) {
            if (useTemp) {
                useNormalSettings();
            }
            else {
                useTempSettings(test, 4.0f, 0.025f);
            }
        }

        if (follow != null && lookAt != null) {
            distanceToLookAt = Vector2.Distance(follow.position, lookAt.position);
            directionToLookAt = lookAt.position - follow.position;
        }
        else {
            distanceToLookAt = 0;
            directionToLookAt = Vector2.up;
        }

        // Camera Focus
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

        // Camera Size
        if (staticCameraSize || distanceToLookAt > maxDistance) {
            desiredCameraSize = activeCameraSize;
        }
        else {
            desiredCameraSize = Mathf.Clamp(distanceToLookAt * 0.8f, activeCameraSize, activeCameraSize + maxCameraSizeIncrease);
        }

        // Adjusting Camera
        if (!useTemp) {
            camera.transform.position = Vector3.Lerp(transform.position, desiredCameraPosition, cameraSmoothSpeed);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, desiredCameraSize, cameraSizeSmoothSpeed);
        }
        else {
            camera.transform.position = Vector3.Lerp(transform.position, new Vector3(tempTarget.position.x, tempTarget.position.y, -10.0f), tempMoveSmooth);
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, tempCameraSize, tempSizeSmooth);
        }

    }

    // Helper Functions

    // Changing Transforms
    public void ChangeFollow(Transform newFollow) {
        follow = newFollow;    
    }
    public void ChangeLookAt(Transform newLookAt) {
        lookAt = newLookAt;    
    }

    // Adjust Size
    public void ChangeCameraSize(float size) {
        activeCameraSize = size;    
    }
    public void ChangeSizeSmooth(float speed) {
        cameraSizeSmoothSpeed = speed;
    }
    public void ChangeMoveSmooth(float speed) {
        cameraSmoothSpeed = speed;
    }

    // Boolean
    public void CameraSizeLock(bool input) {
        staticCameraSize = input;
    }
    public void CameraMoveLock(bool input) {
        staticCameraFocus = input;
    }

    // Change State
    public void Alert() {
        CameraSizeLock(true);
        CameraMoveLock(true);
        ChangeCameraSize(baseCameraSize * 1.6f);
        ChangeMoveSmooth(0.05f);
        ChangeSizeSmooth(0.05f);
    }

    public void Reset() {
        CameraSizeLock(false);
        CameraMoveLock(false);
        ChangeCameraSize(baseCameraSize);
        ChangeMoveSmooth(0.025f);
        ChangeSizeSmooth(0.04f);
    }

    public void useTempSettings(Transform target, float size, float panSpeed) {
        useTemp = true;
        
        tempTarget = target;
        tempCameraSize = size;
        tempMoveSmooth = panSpeed;
        tempSizeSmooth = panSpeed;
    }

    public void useNormalSettings() {
        useTemp = false;
    }
}
