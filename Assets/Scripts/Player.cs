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
    private GameObject m_ThisObject;

    // Start is called before the first frame update
    void Start()
    {
        m_OverworldControllerObject = GameObject.FindGameObjectWithTag("WorldBuilder");
        m_OverworldController = m_OverworldControllerObject.GetComponent<OverworldController>();
        m_ThisObject = GameObject.FindGameObjectWithTag("Player");
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

        // rotate sprite in chosen direction
        Quaternion targetAngle = this.transform.rotation;
        if (moveAxis.x > 0)
        {
            // look right
            targetAngle = Quaternion.Euler(0f, 0f, 90f);
        }
        else if (moveAxis.x < 0)
        {
            // look left
            targetAngle = Quaternion.Euler(0f, 0f, -90f);
        }
        else if (moveAxis.y > 0)
        {
            // look up
            targetAngle = Quaternion.Euler(0f, 0f, 180f);
        }
        else if (moveAxis.y < 0)
        {
            // look left
            targetAngle = Quaternion.Euler(0f, 0f, 0f);
        }
        m_ThisObject.transform.rotation = targetAngle;

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
                MapDoor mapDoor = collision.gameObject.GetComponent<MapDoor>();
                int out_id = mapDoor.out_id;
                mapDoor.door.is_open = true;
                mapDoor.door.linked.is_open = true;
                m_OverworldController.GenRoom(out_id-1);
            }
        }
    }
}
