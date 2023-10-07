using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Forced delay between moves
    private float moveDelay = 0.1f;
    // The next schedulable move time
    private float nextMove = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
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
        if (Time.time < nextMove)
        {
            return;
        }
        Vector2 moveAxis = value.Get<Vector2>();

        // Set the next schedulable movement time to current time + delay
        nextMove = Time.time + moveDelay;

        // Grab current position
        Vector3 curPos = this.transform.position;

        // find new position from input axis
        curPos.x += moveAxis.x;
        curPos.y += moveAxis.y;
        // move player
        this.transform.position = curPos;
    }
}
