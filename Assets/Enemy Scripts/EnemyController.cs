using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveforce;
    
    [SerializeField]
    private float baseSpeed;

    float movementSpeed;

    // Start is called before the first frame update
    void Start()
    {
        Reset();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = moveforce * movementSpeed;
    }

    public void MovementInput(Vector2 movement) {
        moveforce = movement;
    }

    public void ChangeSpeed(float newSpeed) {
        movementSpeed = newSpeed;
    }

    public void Reset(){
        movementSpeed = baseSpeed;
    }
}
