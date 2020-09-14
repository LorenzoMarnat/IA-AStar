using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    public int force = 5;

    private GameObject text;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        text = GameObject.FindGameObjectWithTag("gameOver");
        text.SetActive(false);
    }

    // Met le jeu en pause et affiche game over si l'ennemie touche le joueur
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "ennemy")
        {
            Time.timeScale = 0;
            text.SetActive(true);
        }
    }
    private void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal") * Time.fixedDeltaTime;
        float y = Input.GetAxis("Vertical") * Time.fixedDeltaTime;

        Vector2 movement = new Vector2(x*force, y*force);

        rb.transform.Translate(movement);
    }
}
