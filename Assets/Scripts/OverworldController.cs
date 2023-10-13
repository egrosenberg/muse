using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// json for single door
[System.Serializable]
public class Door
{
    public int col;
    public int out_id;
    public int row;
}
// json for each door array
[System.Serializable]
public class Doors
{
    public Door[] east;
    public Door[] north;
    public Door[] west;
    public Door[] south;
}

// json for single room
[System.Serializable]
public class Room
{
    public int col;
    public Doors doors;
    public int east;
    public int height;
    public int id;
    public int north;
    public int row;
    public int shape;
    public int size;
    public int south;
    public int west;
    public int width;
}
// json for room array
[System.Serializable]
public class Rooms
{
    public Room[] rooms;
}


public class OverworldController : MonoBehaviour
{
    private const int MAP_UNIT_S = 10;
    private const int WALL_OFFSET = 2;

    public GameObject m_builderObj;
    private MapBuilder m_builderScript;


    private GameObject m_Player;
    private GameObject m_Grid;

    public TextAsset m_jsonFile; // json file to generate from
    private Rooms m_rooms;
    private int m_roomID; // current room ID

    void Start()
    {
        // Get map building script from game object
        m_builderScript = m_builderObj.GetComponent<MapBuilder>();

        // find player and grid objects
        m_Player = GameObject.FindGameObjectWithTag("Player");
        m_Grid = GameObject.FindGameObjectWithTag("OverworldGrid");

        m_rooms = JsonUtility.FromJson<Rooms>(m_jsonFile.text);

        GenRoom(0);
    }

    // Draw doors in current room
    void DrawDoors()
    {
        int baseCol = m_rooms.rooms[m_roomID].col - 1;
        int baseRow = m_rooms.rooms[m_roomID].row - 1;

        if (m_rooms.rooms[m_roomID].doors.east != null)
        {
            foreach (Door cdoor in m_rooms.rooms[m_roomID].doors.east)
            {
                if (cdoor.out_id == 0)
                {
                    continue;
                }
                // Account for offset of room
                int col = cdoor.col - baseCol;
                int row = cdoor.row - baseRow;

                Debug.Log(col + ", " + row + "; leads to: " + cdoor.out_id);
                m_builderScript.DrawDoor(col, row, MapBuilder.ROTATE90, cdoor.out_id);
            }
        }
        if (m_rooms.rooms[m_roomID].doors.north != null)
        {
            foreach (Door cdoor in m_rooms.rooms[m_roomID].doors.north)
            {
                if (cdoor.out_id == 0)
                {
                    continue;
                }
                // Account for offset of room
                int col = cdoor.col - baseCol;
                int row = cdoor.row - baseRow;

                Debug.Log(col + ", " + row + "; leads to: " + cdoor.out_id);
                m_builderScript.DrawDoor(col, row, MapBuilder.ROTATE180, cdoor.out_id);
            }
        }
        if (m_rooms.rooms[m_roomID].doors.west != null)
        {
            foreach (Door cdoor in m_rooms.rooms[m_roomID].doors.west)
            {
                if (cdoor.out_id == 0)
                {
                    continue;
                }
                // Account for offset of room
                int col = cdoor.col - baseCol;
                int row = cdoor.row - baseRow;

                Debug.Log(col + ", " + row + "; leads to: " + cdoor.out_id);
                m_builderScript.DrawDoor(col, row, MapBuilder.ROTATE270, cdoor.out_id);
            }
        }
        if (m_rooms.rooms[m_roomID].doors.south != null)
        {
            foreach (Door cdoor in m_rooms.rooms[m_roomID].doors.south)
            {
                if (cdoor.out_id == 0)
                {
                    continue;
                }
                // Account for offset of room
                int col = cdoor.col - baseCol;
                int row = cdoor.row - baseRow;

                Debug.Log(col + ", " + row + "; leads to: " + cdoor.out_id);
                m_builderScript.DrawDoor(col, row, MapBuilder.ROTATE0, cdoor.out_id);
            }
        }
    }

    /**
     * Generates the room with the specified ID
     * 
     * @param id: int containing index of room to build
     */
    public void GenRoom(int id)
    {
        // error checking
        if (id >= m_rooms.rooms.Length)
        {
            Debug.Log("ERROR: INVALID ROOM ID");
            return;
        }
        m_roomID = id;
        Debug.Log("Drawing room ID: " + m_roomID);

        int width = (int)(m_rooms.rooms[m_roomID].width / MAP_UNIT_S) + WALL_OFFSET;
        int height = (int)(m_rooms.rooms[m_roomID].height / MAP_UNIT_S) + WALL_OFFSET;
        BuildRoomBlank(width, height);
        DrawDoors();
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
