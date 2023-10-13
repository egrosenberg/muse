using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Forced delay between moves
    private float m_moveDelay = 0.1f;
    // The next schedulable move time
    private float m_nextMove = 0.0f;

    private GameObject m_OverworldControllerObject;
    private OverworldController m_OverworldController;

    // Start is called before the first frame update
    void Start()
    {
        m_OverworldControllerObject = GameObject.FindGameObjectWithTag("WorldBuilder");
        m_OverworldController = m_OverworldControllerObject.GetComponent<OverworldController>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
    }

    /**
     * Move player based on input axis
     * This function is called whenever a player presses a button for the "Move" action
     * 
     * @param value, InputValue as a 2d vector to use for movement
     */
    void OnMove(InputValue value)
    {
        // Dont move if we are on cooldown
        if (Time.time < m_nextMove)
        {
            return;
        }
        Vector2 moveAxis = value.Get<Vector2>();

        // Set the next schedulable movement time to current time + delay
        m_nextMove = Time.time + m_moveDelay;

        // Grab current position
        Vector3 newPos = this.transform.position;

        // find new position from input axis
        newPos.x += moveAxis.x;
        newPos.y += moveAxis.y;

        // Check to see if new position will cause collision
        Collider2D[] collisions = Physics2D.OverlapCircleAll(newPos, 0.3f);

        // Log each position (for debuging purposes)
        foreach (Collider2D collision in collisions)
        {
            Debug.Log(collision.gameObject.name);
        }

        if (collisions.Length < 1)
        {
            // move player if we are not going to collide
            this.transform.position = newPos;
        }

        foreach (Collider2D collision in collisions)
        {
            if (collision.gameObject.tag == "Door")
            {
                int out_id = collision.gameObject.GetComponent<MapDoor>().out_id;
                m_OverworldController.GenRoom(out_id-1);
            }
        }
    }
}
