using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    public const int N_STATS = 6;
    public const int BASE_STAT = 8;
    public const int MAX_STAT = 20;
    public const int MED_STAT = 10;
    public const int HP_PER_LVL = 5;
    public const int MP_PER_LVL = 4;
    public const int MAX_LEVEL = 10;
    public static readonly int[] PB_AT_LVL = {2, 2, 2, 2, 3, 3, 3, 3, 4, 4};
    public static readonly int[] XP_AT_LVL = {300, 900, 2700, 6500, 1400, 2300, 34000, 48000, 64000, 85000};

    public enum Stats : int
    {
        STR,
        DEX,
        CON,
        INT,
        WIS,
        CHA
    }

    private int[] m_StatArray;
    private bool[] m_SaveProfs;
    private bool[] m_CheckProfs;
    private int m_Level;
    private int m_XP;
    private int m_PB;
    private int m_MaxHP;
    private int m_MaxMP;
    private int m_HP;
    private int m_MP;

    void Awake()
    {
        // initialize stats, checks, and saves
        m_StatArray = new int[N_STATS];
        for (int i = 0; i < N_STATS; ++i)
        {
            m_StatArray[i] = BASE_STAT;
            m_SaveProfs[i] = false;
            m_CheckProfs[i] = false;
        }
        // init level
        m_Level = 1;
        m_XP = 0;
        m_PB = PB_AT_LVL[m_Level];
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    /**
     * Get modifier for given stat
     * 
     * @param stat: stat to get modifier of
     */
    public int GetMod(Stats stat)
    {
        int statN = (int)stat;
        return (m_StatArray[statN] - MED_STAT) / 2;
    }

    // Update all resources and values (HP, MP, PB, ect.) based on current stats/level
    public void UpdateResources()
    {
        // record current missing resource valuess
        int missingHP = m_MaxHP - m_HP;
        int missingMP = m_MaxMP - m_MP;

        // update max hp/mp
        m_MaxHP = m_Level * (HP_PER_LVL + GetMod(Stats.CON));
        m_MaxMP = m_Level * (MP_PER_LVL + GetMod(Stats.INT));

        // update current hp/mp
        m_HP = m_MaxHP - missingHP;
        m_MP = m_MaxMP - missingMP;

        // update PB
        m_PB = PB_AT_LVL[m_Level];
    }

    /**
     * Set specified stat to given value
     * 
     * @param stat: stats enum containing which stat to change
     * @param value: int containing value to change stat to
     */
    public void SetStat(Stats stat, int value)
    {
        int statN = (int)stat;
        // set stat
        m_StatArray[statN] = value;
    }
    /**
     * Get bonus to ability check with specified stat
     * 
     * @param stat: stat to check
     * @return total bonus to stat ability check
     */
    public int CheckBonus(Stats stat)
    {
        return m_CheckProfs[(int)stat] ? GetMod(stat) + m_PB : GetMod(stat);
    }
    /**
     * Get bonus to saving throw with specified stat
     * 
     * @param stat: stat to check
     * @return total bonus to saving throw
     */
    public int SaveBonus(Stats stat)
    {
        return m_SaveProfs[(int)stat] ? GetMod(stat) + m_PB : GetMod(stat);
    }

    /**
     * Set all stats to those of a new stat array
     * 
     * @param array: int array containing the new stats to use
     */
    public void SetStatArray(int[] array)
    {
        // error checking
        if (array.Length != N_STATS)
        {
            Debug.LogError("ERROR: INVALID ARRAY LENGTH IN SET_STAT_ARRAY IN " + this.name);
            return;
        }
        // copy array content
        for (int i = 0; i < N_STATS; ++i)
        {
            m_StatArray[i] = array[i];
        }
    }

    // Advance to next level and update resources
    public void LevelUp()
    {
        m_Level += 1;
        // limit to MAX_LEVEL
        if (m_Level > MAX_LEVEL)
        {
            m_Level = MAX_LEVEL;
        }
        m_XP = 0;
        UpdateResources();
    }

    /**
     * Add xp to character, level up if at requisite xp
     * 
     * @param ammout: ammount of xp to add
     * @return true if level up, false if not.
     */
    public bool AddXP(int ammount)
    {
        // increment xp
        m_XP += ammount;
        // check for level up
        while (m_XP >= XP_AT_LVL[m_Level])
        {
            int remainderXP = m_XP - XP_AT_LVL[m_Level];
            LevelUp();
            m_XP = remainderXP;
        }

        return false;
    }
}
