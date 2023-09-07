using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveforce;
    
    [SerializeField]
    private float movementSpeed;

    // Start is called before the first frame update
    void Start()
    {
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
}
