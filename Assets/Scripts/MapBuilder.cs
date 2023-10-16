using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;


public class MapBuilder : MonoBehaviour
{
    public static Matrix4x4 ROTATE0 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);
    public static Matrix4x4 ROTATE90 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90f), Vector3.one);
    public static Matrix4x4 ROTATE180 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 180f), Vector3.one);
    public static Matrix4x4 ROTATE270 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 270f), Vector3.one);

    // tiles for drawing map
    public TileBase m_TileCorner; // Base bottom-right
    public TileBase m_TileWall; // Base bottom
    public TileBase m_TileDoor; // Base bottom
    public TileBase m_TileFloor;

    public Sprite m_DoorOpenSprite; // sprite for open door
    public GameObject m_DoorPrefab; // Placeable doors

    // Wall / Doors are S, E, N, W
    private Tile[] m_WallTilesArray;
    private Tile[] m_DoorTilesArray;
    // SE, NW, NE, SW
    private Tile[] m_ConerTilesArray;

    private GameObject m_WallsObject;
    private Tilemap m_WallsMap;

    private GameObject m_FloorObject;
    private Tilemap m_FloorMap;

    private GameObject m_Player;
    private GameObject m_Grid;

    private bool m_OddX;
    private bool m_OddY;


    // some dummy variables for testing tilemap gen
    public int m_height = 8;
    public int m_width = 8;



    // Start is called before the first frame update
    void Awake()
    {
        // Grab walls tilemap from scene
        m_WallsObject = GameObject.FindGameObjectWithTag("Walls");
        m_WallsMap = m_WallsObject.GetComponent<Tilemap>();

        // Grab floor tilemap from scene
        m_FloorObject = GameObject.FindGameObjectWithTag("Floor");
        m_FloorMap = m_FloorObject.GetComponent<Tilemap>();
        m_Grid = GameObject.FindGameObjectWithTag("OverworldGrid");

        m_OddX = (m_width % 2) != 0;
        m_OddY = (m_height % 2) != 0;

        DrawRoom();
    }

    // Erase all old tiles, prep for new room.
    void ClearRoom()
    {
        if (m_WallsMap == null || m_FloorMap == null)
        {
            return;
        }
        // erase old room
        m_WallsMap.ClearAllTiles();
        m_FloorMap.ClearAllTiles();

        // destroy all doors
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        foreach (GameObject door in doors)
        {
            Destroy(door);
        }
    }

    void RefreshTiles()
    {
        m_WallsMap.RefreshAllTiles();
        m_FloorMap.RefreshAllTiles();
    }

    /**
     * Re-draw current room based on member variables.
     * 
     * 1. Draw floor
     * 2. Draw walls
     * 3. Draw corners
     */
    public void DrawRoom()
    {
        ClearRoom();

        // construct a room
        int floorWidth = m_width - 2;
        int floorHeight = m_height - 2;
        int originX = (floorWidth / 2);
        int originY = (floorHeight / 2);

        int cursorX = originX;

        int boundX = -originX;
        boundX -= m_OddX ? 1 : 0; // shift if odd

        int cursorY = originY;

        int boundY = -originY;
        boundY -= m_OddY ? 1 : 0; // shift if odd

        // draw the floor
        for (int x = cursorX; x > boundX; --x)
        {
            for (int y = cursorY; y > boundY; --y)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                m_FloorMap.SetTile(pos, m_TileFloor);
            }
        }

        // DRAW WALLS
        // set the cursor and bound for N/S walls 
        cursorX = (floorWidth / 2);

        boundX = -(floorWidth / 2);
        boundX -= m_OddX ? 1 : 0; // shift if odd

        cursorY = m_height / 2;
        // draw the north wall
        for (int x = cursorX; x > boundX; --x)
        {
            Vector3Int pos = new Vector3Int(x, cursorY, 0);
            m_WallsMap.SetTile(pos, m_TileWall);
            m_WallsMap.SetTransformMatrix(pos, ROTATE180);
        }
        // draw the south wall
        // re-set cursor (offset by 1 because bounded side)
        cursorY = -(m_height / 2) + 1;
        cursorY -= m_OddY ? 1 : 0; // shift if odd
        for (int x = cursorX; x > boundX; --x)
        {
            Vector3Int pos = new Vector3Int(x, cursorY, 0);
            m_WallsMap.SetTile(pos, m_TileWall);
        }

        // set the cursor and bounds for E/W walls
        cursorY = (floorHeight / 2);

        boundY = -(floorHeight / 2);
        boundY -= m_OddY ? 1 : 0; // shift if odd

        cursorX = m_width / 2;

        // draw the east wall
        for (int y = cursorY; y > boundY; --y)
        {
            Vector3Int pos = new Vector3Int(cursorX, y, 0);
            m_WallsMap.SetTile(pos, m_TileWall);
            m_WallsMap.SetTransformMatrix(pos, ROTATE90);
        }

        // draw the west wall
        // re-set cursor (offset by 1 because bounded side)
        cursorX = -(m_width / 2) + 1;
        cursorX -= m_OddX ? 1 : 0; // shift if odd

        for (int y = cursorY; y > boundY; --y)
        {
            Vector3Int pos = new Vector3Int(cursorX, y, 0);
            m_WallsMap.SetTile(pos, m_TileWall);
            m_WallsMap.SetTransformMatrix(pos, ROTATE270);
        }

        // draw corners
        int cornerX = m_width / 2;
        int cornerBoundX = -(m_width / 2) + 1;
        cornerBoundX -= m_OddX ? 1 : 0; // shift if od
        int cornerY = m_height / 2;
        int cornerBoundY = -(m_height / 2) + 1;
        cornerBoundY -= m_OddY ? 1 : 0; // shift if odd
        // SW
        Vector3Int cornerPos = new Vector3Int(cornerBoundX, cornerBoundY, 0);
        m_WallsMap.SetTile(cornerPos, m_TileCorner);
        // SE
        cornerPos = new Vector3Int(cornerX, cornerBoundY, 0);
        m_WallsMap.SetTile(cornerPos, m_TileCorner);
        m_WallsMap.SetTransformMatrix(cornerPos, ROTATE90);
        // NE
        cornerPos = new Vector3Int(cornerX, cornerY, 0);
        m_WallsMap.SetTile(cornerPos, m_TileCorner);
        m_WallsMap.SetTransformMatrix(cornerPos, ROTATE180);
        // NW
        cornerPos = new Vector3Int(cornerBoundX, cornerY, 0);
        m_WallsMap.SetTile(cornerPos, m_TileCorner);
        m_WallsMap.SetTransformMatrix(cornerPos, ROTATE270);

        RefreshTiles();
    }

    /*
     * Re-draw current room based on dimmensions passed in
     * 
     * @param width: width to set room to
     * @param height: height to set room to
     */
    public void DrawRoom(int width, int height)
    {
        // update member variables
        m_width = width;
        m_height = height;

        // re-track odd/even
        m_OddX = (m_width % 2) != 0;
        m_OddY = (m_height % 2) != 0;

        // draw room
        DrawRoom();
    }

    /**
     * Place a door on wall layer at specified position / rotation
     * 
     * @param x: int x coordinate respective to room NW
     * @param y: int y coordinate respective to room NW
     * @param rotation: Matrix4x4 instructing how to rotate door
     */
    public void DrawDoor(int x, int y, Matrix4x4 rotation, int out_id, Door doorObj)
    {
        // calculate NW corner
        int west = -(m_width / 2) + 1;
        west -= m_OddX ? 1 : 0; // shift if odd
        int north = m_height / 2;

        // calculate door position
        Vector3Int position = new Vector3Int(west + x, north - y, 0);

        // place door
        GameObject door = Instantiate(m_DoorPrefab, position, rotation.rotation, m_Grid.transform);
        MapDoor doorScript = door.GetComponent<MapDoor>();
        doorScript.out_id = out_id;
        doorScript.door = doorObj;

        if (doorObj.is_open)
        {
            door.GetComponentInChildren<SpriteRenderer>().sprite = m_DoorOpenSprite;
        }

        //m_WallsMap.SetTransformMatrix(position, rotation);
        m_WallsMap.SetTile(position, null);

        RefreshTiles();
    }
}
    