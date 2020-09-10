using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    public int force = 5;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal") * Time.fixedDeltaTime;
        float y = Input.GetAxis("Vertical") * Time.fixedDeltaTime;

        Vector2 movement = new Vector2(x*force, y*force);

        // Appelle la fonction AddForce() du Rigidbody2D rb2d avec le mouvement multiplié par la vitesse.
        rb.transform.Translate(movement);
    }
}
