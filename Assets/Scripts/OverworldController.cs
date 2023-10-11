using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldController : MonoBehaviour
{
    public GameObject m_builderObj;
    private MapBuilder m_builderScript;

    private GameObject m_Player;
    private GameObject m_Grid;

    // Start is called before the first frame update
    void Start()
    {
        // Get map building script from game object
        m_builderScript = m_builderObj.GetComponent<MapBuilder>();

        // find player and grid objects
        m_Player = GameObject.FindGameObjectWithTag("Player");
        m_Grid = GameObject.FindGameObjectWithTag("OverworldGrid");
    }

    void OnGenRandom()
    {
        int width = (int)Random.Range(4f, 10f);
        int height = (int)Random.Range(4f, 10f);

        BuildRoomBlank(width, height);
    }

    /**
     * Builds a room based on specified width and height
     * automatically shift grid and player based on even/odd
     * 
     * @param width: int containing width of room
     * @param height: int containing height of room
     */
    void BuildRoomBlank(int width, int height)
    {
        // shift player and grid
        Vector3 playerPos = Vector3.zero;
        Vector3 gridPos = Vector3.zero;

        playerPos.x = (width % 2 == 0) ? -0.5f : 0f;
        playerPos.y = (height % 2 == 0) ? -0.5f : 0f;

        gridPos.x = (width % 2 == 0) ? -1f : -0.5f;
        gridPos.y = (height % 2 == 0) ? -1f : -0.5f;

        m_Player.transform.position = playerPos;
        m_Grid.transform.position = gridPos;

        // build room
        m_builderScript.DrawRoom(width, height);
    }
}
