using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine;

// json for single door
[System.Serializable]
public class Door
{
    public int col;
    public int out_id;
    public int row;
    public bool is_open = false;
    public Door linked;
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
    public bool cleared = false;
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
    private const float PERCENT_TO_RESTORE = 0.1f;

    public string[] VOCAB_ARTICLES;
    public string[] VOCAB_NAMES;
    public string[] VOCAB_ADJECTIVES;
    public string[] VOCAB_ENTRANCES;
    public string[] VOCAB_PUNCTUATION;

    public AudioClip ENTER_ROOM;

    public GameObject m_CombatUI;
    public GameObject m_ActionsMenu;
    public GameObject m_GameOverMenu;

    public GameObject m_builderObj;
    private MapBuilder m_builderScript;


    private GameObject m_Player;
    private GameObject m_Grid;

    public TextAsset m_jsonFile;                // json file to generate from
    private Rooms m_rooms;                      // rooms object to store the current dungeon in
    private int m_roomID;                       // current room ID
    private TextMeshProUGUI m_DialogueText;     // Dialogue box to put announcements in
    private PlayerInput m_PlayerInput;          // player's input component
    private bool m_InCombat;                    // track whether in combat or not
    private GameObject m_MonsterObject;         // current monster's game object
    private Monster m_Monster;                  // current monster's script
    private GameObject m_PlayerCharacterObject; // player character sheet's game object
    private PlayerCharacter m_PlayerCharacter;  // player character for game

    private bool m_IsFirstCombat;

    void Start()
    {
        // Get map building script from game object
        m_builderScript = m_builderObj.GetComponent<MapBuilder>();

        // find game objects
        m_Player = GameObject.FindGameObjectWithTag("Player");
        m_Grid = GameObject.FindGameObjectWithTag("OverworldGrid");
        m_MonsterObject = GameObject.FindGameObjectWithTag("Targeted");
        m_PlayerCharacterObject = GameObject.FindGameObjectWithTag("PlayerSheet");

        // find components
        m_PlayerInput = m_Player.GetComponent<PlayerInput>();
        m_Monster = m_MonsterObject.GetComponent<Monster>();
        m_PlayerCharacter = m_PlayerCharacterObject.GetComponent<PlayerCharacter>();

        m_rooms = JsonUtility.FromJson<Rooms>(m_jsonFile.text);

        // grab our text box
        GameObject textObject = GameObject.FindGameObjectWithTag("DialogueText");
        m_DialogueText = textObject.GetComponent<TextMeshProUGUI>();

        LinkDoors();

        m_InCombat = false;
        m_IsFirstCombat = true;

        m_roomID = 0;
        // set first room to cleared and generate it
        m_rooms.rooms[m_roomID].cleared = true;
        GenRoom(m_roomID);
    }

    private void Update()
    {
        // check if combat has ended
        if (m_InCombat && m_Monster.GetHP() <= 0)
        {
            m_InCombat = false;
            StartCoroutine(EndCombat());
        }
        // check if player is dead
        if (m_PlayerCharacter.GetHP() <= 0)
        {
            StartCoroutine(GameOver());
        }
    }

    // goes through every door in the dungeon and attempts to link them to one another
    private void LinkDoors()
    {
        foreach (Room room in m_rooms.rooms)
        {
            // link north doors
            if (room.doors.north != null)
            {
                foreach (Door door in room.doors.north)
                {
                    if (door.out_id == 0)
                    {
                        continue;
                    }
                    LinkDoor(door, 'N', room.id);
                }
            }
            // link east doors
            if (room.doors.east != null)
            {
                foreach (Door door in room.doors.east)
                {
                    if (door.out_id == 0)
                    {
                        continue;
                    }
                    LinkDoor(door, 'E', room.id);
                }
            }
            // link south doors
            if (room.doors.south != null)
            {
                foreach (Door door in room.doors.south)
                {
                    if (door.out_id == 0)
                    {
                        continue;
                    }
                    LinkDoor(door, 'S', room.id);
                }
            }
            // link west doors
            if (room.doors.west != null)
            {
                foreach (Door door in room.doors.west)
                {
                    if (door.out_id == 0)
                    {
                        continue;
                    }
                    LinkDoor(door, 'W', room.id);
                }
            }
        }
    }
    // attempts to link an individual door to its corresponding door
    private void LinkDoor(Door door, char direction, int doorRoom)
    {
        int roomId = door.out_id - 1;

        // only check wall that door should be able to link to
        Door[] wall;
        switch (direction)
        {
            case 'N':
                wall = m_rooms.rooms[roomId].doors.south;
                break;
            case 'E':
                wall = m_rooms.rooms[roomId].doors.west;
                break;
            case 'S':
                wall = m_rooms.rooms[roomId].doors.north;
                break;
            case 'W':
                wall = m_rooms.rooms[roomId].doors.east;
                break;
            default:
                wall = m_rooms.rooms[roomId].doors.north;
                break;
        }

        if (wall == null)
        {
            Debug.LogError("ERROR: UNABLE TO LINK ROOM " + doorRoom + " to " + door.out_id);
            return;
        }

        // check doors on wall
        foreach (Door match in wall)
        {
            if (match.out_id == doorRoom)
            {
                door.linked = match;
            }
        }
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

                m_builderScript.DrawDoor(col, row, MapBuilder.ROTATE90, cdoor.out_id, cdoor);
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

                m_builderScript.DrawDoor(col, row, MapBuilder.ROTATE180, cdoor.out_id, cdoor);
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

                m_builderScript.DrawDoor(col, row, MapBuilder.ROTATE270, cdoor.out_id, cdoor);
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

                m_builderScript.DrawDoor(col, row, MapBuilder.ROTATE0, cdoor.out_id, cdoor);
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
            Debug.LogError("ERROR: INVALID ROOM ID");
            return;
        }
        // play enter room sound effect
        SoundController sfxSource = GameObject.FindGameObjectWithTag("SoundController").GetComponent<SoundController>();
        sfxSource.PlaySFX(ENTER_ROOM);

        m_roomID = id;
        Debug.Log("Drawing room ID: " + m_roomID);

        int width = (int)(m_rooms.rooms[m_roomID].width / MAP_UNIT_S) + WALL_OFFSET;
        int height = (int)(m_rooms.rooms[m_roomID].height / MAP_UNIT_S) + WALL_OFFSET;
        BuildRoomBlank(width, height);
        DrawDoors();

        // dont start combat if the room is already cleared
        if (m_rooms.rooms[m_roomID].cleared == false)
        {
            StartCombat();
        }
        else
        {
            // disable combat ui
            m_CombatUI.SetActive(false);
        }
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


    // Tells player they have died and loads game over screen
    public IEnumerator GameOver()
    {
        m_ActionsMenu.SetActive(false);

        yield return new WaitForSeconds(Character.ACTION_DELAY);

        m_DialogueText.text = "You have died :(";

        yield return new WaitForSeconds(Character.ACTION_DELAY);

        m_GameOverMenu.SetActive(true);
    }

    // Reloads scene
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /**
     * Generate a string for a monster's enterance
     * 
     * @param monsterName: name of monster to generate entrance for
     * 
     * @return: string containing a complete entrance text
     */
    private string GenerateRandomEntrance(string monsterName)
    {
        int articleN = UnityEngine.Random.Range(0, VOCAB_ARTICLES.Length);
        int adjectiveN = UnityEngine.Random.Range(0, VOCAB_ADJECTIVES.Length);
        int entranceN = UnityEngine.Random.Range(0, VOCAB_ENTRANCES.Length);
        int punctuationN = UnityEngine.Random.Range(0, VOCAB_PUNCTUATION.Length);

        return VOCAB_ARTICLES[articleN] + " " + VOCAB_ADJECTIVES[adjectiveN] + " " + monsterName + " " + VOCAB_ENTRANCES[entranceN] + VOCAB_PUNCTUATION[punctuationN];
    }

    /**
     * Starts combat:
     *   Enables combat ui
     *   spawns in an enemy
     *   sets dialogue box content
     */
    private void StartCombat()
    {
        if (m_IsFirstCombat)
        {
            m_IsFirstCombat = false;
        }
        else
        {
            // set monster to random level and refresh all resources and bars
            m_Monster.SetLevel(UnityEngine.Random.Range(Monster.MIN_LEVELUP+1, Monster.MAX_LEVELUP));
        }


        m_InCombat = true;
        // disable player controls
        m_PlayerInput.enabled = false;

        // enable combat ui
        m_CombatUI.SetActive(true);

        m_MonsterObject.name = VOCAB_NAMES[UnityEngine.Random.Range(0, VOCAB_NAMES.Length)];
        m_Monster.RefreshAll();
        m_DialogueText.text = GenerateRandomEntrance(m_MonsterObject.name);
    }

    /**
     * ends combat:
     *   Announces combat has ended
     *   close combat ui
     *   re-enable character control
     */
    private IEnumerator EndCombat()
    {
        yield return new WaitForSeconds(Character.ACTION_DELAY);

        m_ActionsMenu.SetActive(false);
        
        // announce victory
        m_DialogueText.text = "The " + m_MonsterObject.name + " has been defeated!";

        yield return new WaitForSeconds(Character.ACTION_DELAY);

        // give xp and restore resources
        yield return GiveXP();
        RestorePlayer();

        // disable combat ui
        m_CombatUI.SetActive(false);

        // set room as cleared
        m_rooms.rooms[m_roomID].cleared = true;

        // re enable player controls
        m_PlayerInput.enabled = true;
    }
    // Awards the player xp based on the challenge of the encounter
    private IEnumerator GiveXP()
    {
        // calculate xp reward and give xp
        int xpReward = Monster.XP_PER_CR[m_Monster.GetLevel()];
        m_PlayerCharacter.AddXP(xpReward);
        // calculate remaining xp required ect.
        int playerXP = m_PlayerCharacter.GetXP();
        int nextLevel = PlayerCharacter.XP_AT_LVL[m_PlayerCharacter.GetLevel()];

        // post dialogue message
        m_DialogueText.text = "You gained " + xpReward + " xp!\n";
        m_DialogueText.text += "Current XP: " + playerXP + "\nNext level (" + (m_PlayerCharacter.GetLevel() + 1) + "): " + nextLevel + "XP (" + (nextLevel - playerXP) + "XP away)";
        yield return new WaitForSeconds(Character.ACTION_DELAY*4);
    }
    // partially refreshes player hp and mp
    private void RestorePlayer()
    {
        int toHP = (int)(PERCENT_TO_RESTORE * m_PlayerCharacter.GetMaxHP());
        int toMP = (int)(PERCENT_TO_RESTORE * m_PlayerCharacter.GetMaxMP());

        m_PlayerCharacter.Damage(-toHP);
        m_PlayerCharacter.SpendMP(-toMP);
    }
}
