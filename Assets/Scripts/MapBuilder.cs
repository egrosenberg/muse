using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;


public class MapBuilder : MonoBehaviour
{
    private const int N_ANGLES = 4;

    private Matrix4x4 ROTATE90 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90f), Vector3.one);
    private Matrix4x4 ROTATE180 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 180f), Vector3.one);
    private Matrix4x4 ROTATE270 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 270f), Vector3.one);

    // tiles for drawing map
    public TileBase m_TileCorner; // Base bottom-right
    public TileBase m_TileWall; // Base bottom
    public TileBase m_TileDoor; // Base bottom
    public TileBase m_TileFloor;

    // Wall / Doors are S, E, N, W
    private Tile[] m_WallTilesArray;
    private Tile[] m_DoorTilesArray;
    // SE, NW, NE, SW
    private Tile[] m_ConerTilesArray;

    private GameObject m_WallsObject;
    private Tilemap m_WallsMap;

    private GameObject m_FloorObject;
    private Tilemap m_FloorMap;

    private bool m_OddX;
    private bool m_OddY;


    // some dummy variables for testing tilemap gen
    public int m_height = 8;



    // Start is called before the first frame update
    void Start()
    {
        // Grab walls tilemap from scene
        m_WallsObject = GameObject.FindGameObjectWithTag("Walls");
        m_WallsMap = m_WallsObject.GetComponent<Tilemap>();
        
        // Grab floor tilemap from scene
        m_FloorObject = GameObject.FindGameObjectWithTag("Floor");
        m_FloorMap = m_FloorObject.GetComponent<Tilemap>();

        m_OddX = (m_height % 2) != 0;
        m_OddY = (m_height % 2) != 0;

        DrawRoom();

    }

    void DrawRoom()
    {
        // erase old room
        m_WallsMap.ClearAllTiles();
        m_FloorMap.ClearAllTiles();


        // construct a room
        int floorsize = m_height - 2;
        int origin = (floorsize / 2);

        int cursorX = origin;

        int boundX = -origin;
        boundX -= m_OddY ? 1 : 0; // shift if odd

        int cursorY = origin;

        int boundY = -origin;
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
        cursorX = (floorsize / 2);

        boundX = -(floorsize / 2);
        boundX -= m_OddY ? 1 : 0; // shift if odd

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
        cursorY = (floorsize / 2);

        boundY = -(floorsize / 2);
        boundY -= m_OddY ? 1 : 0; // shift if odd

        cursorX = m_height / 2;

        // draw the east wall
        for (int y = cursorY; y > boundY; --y)
        {
            Vector3Int pos = new Vector3Int(cursorX, y, 0);
            m_WallsMap.SetTile(pos, m_TileWall);
            m_WallsMap.SetTransformMatrix(pos, ROTATE90);
        }

        // draw the west wall
        // re-set cursor (offset by 1 because bounded side)
        cursorX = -(m_height / 2) + 1;
        cursorX -= m_OddX ? 1 : 0; // shift if odd

        for (int y = cursorY; y > boundY; --y)
        {
            Vector3Int pos = new Vector3Int(cursorX, y, 0);
            m_WallsMap.SetTile(pos, m_TileWall);
            m_WallsMap.SetTransformMatrix(pos, ROTATE270);
        }
        // draw corners
        int corner = m_height / 2;
        int cornerBound = -(m_height / 2) + 1;
        cornerBound -= m_OddX ? 1 : 0; // shift if odd
        // SW
        Vector3Int cornerPos = new Vector3Int(cornerBound, cornerBound, 0);
        m_WallsMap.SetTile(cornerPos, m_TileCorner);
        // SE
        cornerPos = new Vector3Int(corner, cornerBound, 0);
        m_WallsMap.SetTile(cornerPos, m_TileCorner);
        m_WallsMap.SetTransformMatrix(cornerPos, ROTATE90);
        // NE
        cornerPos = new Vector3Int(corner, corner, 0);
        m_WallsMap.SetTile(cornerPos, m_TileCorner);
        m_WallsMap.SetTransformMatrix(cornerPos, ROTATE180);
        // NW
        cornerPos = new Vector3Int(cornerBound, corner, 0);
        m_WallsMap.SetTile(cornerPos, m_TileCorner);
        m_WallsMap.SetTransformMatrix(cornerPos, ROTATE270);
    }
}
    