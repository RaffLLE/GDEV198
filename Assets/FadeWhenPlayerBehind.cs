using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeWhenPlayerBehind : MonoBehaviour
{
    public GameObject player;
    public SpriteRenderer image;
    public float vanishSpeed;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        image = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Values relative to player
        Vector2 playerDirection = (player.transform.position - transform.position).normalized;
        float distanceToPlayer = (player.transform.position - transform.position).magnitude;

        Color desiredColor;

        if (distanceToPlayer <= 2.2f && transform.position.y - 1.0f < player.transform.position.y)
        {
            desiredColor = new Color(image.color.r, image.color.g, image.color.b, 0.5f);
        }
        else {
            desiredColor = new Color(image.color.r, image.color.g, image.color.b, 1.0f);
        }

        image.color = Color.Lerp(image.color, desiredColor, vanishSpeed);
    }
}
