using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



public class Character : MonoBehaviour
{
    public enum Stats : int
    {
        STR = 0,
        DEX = 1,
        CON = 2,
        INT = 3,
        WIS = 4,
        CHA = 5
    }

    public enum Dice : int
    {
        D4 = 4,
        D6 = 6,
        D8 = 8,
        D10 = 10,
        D12 = 12
    }

    public enum SpellEffects : int
    {
        NONE = 0,
        CHARM = 1
    }

    public class DamageFormula
    {
        public Dice[] dice;       // array of dice to roll
        public bool addModifier;  // true if we add  ability modifier to damage
        public Stats ability;     // The ability used for the attack 
        public bool isHealing;    // inverts damage to negative if healing

        public DamageFormula(Dice[] dice, bool addMod, Stats stat, bool doesHeal)
        {
            this.dice = new Dice[dice.Length];
            dice.CopyTo(this.dice, 0);

            this.addModifier = addMod;

            this.ability = stat;

            this.isHealing = doesHeal;
        }

        /**
         * gets a random result of the damage formula
         * 
         * @param character: character to get ability mod from
         * @return total damage/healing
         */
        public int Roll(Character character)
        {
            int result = 0;

            // roll each die in the array
            foreach (Dice d in dice)
            {
                result += UnityEngine.Random.Range(1, (int)d + 1);
            }
            // conditionally add modifier
            if (addModifier)
            {
                result += character.GetMod(ability);
            }
            // invert if healing
            if (isHealing)
            {
                result *= -1;
            }

            return result;
        }
    }

    public class Spell
    {
        public enum SpellAttackTypes : int
        {
            ATTACK = 0,
            SAVE = 1
        }

        public enum SpellEffectTypes : int
        {
            DAMAGE = 0,
            UTILITY = 1
        }

        public int mpCost;                  // magic points cost of spell
        public DamageFormula damageFormula; // damage formula for spell
        public SpellAttackTypes attackType; // spell attack type (attack / save)
        public SpellEffectTypes effectType; // effect type (damage / utility)
        public SpellEffects spellEffect;    // effect applies by the spell
        public int effectDuration;          // duration of the effect applied by the spell
        public Stats castStat;              // what stat does the caster use?
        public Stats saveStat;              // what stat does the target use to save

        /**
         * Attempts to apply the effects of the spell,
         *   automatically rolls attacks and saves
         *   on success, rolls damage and applies effects
         *   
         * @param caster: character who is casting the spell
         * @param target: character to target with the spell
         * 
         * @return true on success, false otherwise
         */
        public bool Cast(Character caster, Character target)
        {
            bool success = false;

            // if this is a spell attack roll to hit
            if (attackType == SpellAttackTypes.ATTACK)
            {
                int bonus = caster.GetWeaponAttack();
                int dieRoll = caster.GetRoller().Roll(bonus);
                int toHit = dieRoll + bonus;
                success = target.DoesHit(toHit);
            }
            // if this is is a saving throw spell
            if (attackType == SpellAttackTypes.SAVE)
            {
                int bonus = target.GetSave(saveStat);
                int dieRoll = target.GetRoller().Roll(bonus);
                int toSave = dieRoll + bonus;

                success = caster.DoesSave(toSave);
            }

            // if the spell succeeds, do effects
            if (success)
            {
                // check if we need to apply effects
                if (spellEffect != SpellEffects.NONE)
                {
                    target.ApplyEffect(spellEffect, effectDuration);
                }
                int damage = damageFormula.Roll(caster);
                target.Damage(damage);
            }

            return success;
        }
    }

    public const int N_STATS = 6;
    public const int BASE_STAT = 8;
    public const int MAX_STAT = 20;
    public const int MED_STAT = 10;
    public const int HP_PER_LVL = 5;
    public const int MP_PER_LVL = 4;
    public const int MAX_LEVEL = 10;
    public const int BASE_ARMOR = 11;
    public const int DC_BASE = 8;
    public const Stats DEFAULT_SPELL_ABILITY = Stats.CHA;
    public static readonly int[] PB_AT_LVL = {2, 2, 2, 2, 3, 3, 3, 3, 4, 4};
    public static readonly int[] XP_AT_LVL = {300, 600, 1800, 3800, 7500, 9000, 11000, 14000, 16000, 21000};

    protected int[] m_StatArray;               // array containing ability scores
    protected bool[] m_SaveProfs;              // array of saving throw proficiencies
    protected bool[] m_CheckProfs;             // array of ability check proficiencies
    protected int m_ArmorBase;                 // AC bonus from armor
    protected Stats m_SpellAbility;            // ability used for spellcasting
    protected int m_Level;                     // current level
    protected int m_XP;                        // current xp progress in level
    protected int m_PB;                        // current proficiency bonus
    protected int m_MaxHP;                     // max hit points
    protected int m_MaxMP;                     // max magic points
    protected int m_HP;                        // current hit points
    protected int m_MP;                        // current magic points
    protected int m_AC;                        // total armor class
    protected int m_SpellDC;                   // difficulty class for spell saving throws
    protected int m_WeaponAttack;              // weapon attack bonus
    protected int m_SpellAttack;               // spell attack bonus
    protected int[] m_EffectTimers;            // stores all current effect timers and how many turns remaining
    protected TextMeshProUGUI m_DialogueText;  // where to post status updates ect.

    public GameObject m_DieRollerObject;
    protected DieRoller m_DieRoller;
    
    void Awake()
    {
        // initialize stats, checks, and saves
        m_StatArray = new int[N_STATS];
        m_SaveProfs = new bool[N_STATS];
        m_CheckProfs = new bool[N_STATS];
        for (int i = 0; i < N_STATS; ++i)
        {
            m_StatArray[i] = BASE_STAT;
            m_SaveProfs[i] = false;
            m_CheckProfs[i] = false;
        }

        m_SpellAbility = DEFAULT_SPELL_ABILITY;

        // init level
        m_Level = 1;
        m_XP = 0;

        // init armor
        m_ArmorBase = BASE_ARMOR;


        UpdateResources();

        m_EffectTimers = new int[Enum.GetNames(typeof(SpellEffects)).Length];
    }

    // Start is called before the first frame update
    void Start()
    {
        m_DieRoller = m_DieRollerObject.GetComponent<DieRoller>();
        FindObjects();
    }

    protected void FindObjects()
    {
        // grab our text box
        GameObject textObject = GameObject.FindGameObjectWithTag("DialogueText");
        m_DialogueText = textObject.GetComponent<TextMeshProUGUI>();
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

        // update armor class
        m_AC = m_ArmorBase + GetMod(Stats.DEX);

        // update spell save dc + attacks
        m_SpellDC = DC_BASE + m_PB + GetMod(m_SpellAbility);
        m_WeaponAttack = m_PB + GetMod(Stats.DEX);
        m_SpellAttack = m_PB + GetMod(m_SpellAbility);
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
        bool levelUp = false;
        // increment xp
        m_XP += ammount;
        // check for level up
        while (m_XP >= XP_AT_LVL[m_Level])
        {
            int remainderXP = m_XP - XP_AT_LVL[m_Level];
            LevelUp();
            m_XP = remainderXP;
            levelUp = true;
        }

        return levelUp;
    }
    
    /**
     * checks if an attack roll hits
     * 
     * @param attack: value of attack roll
     * @return true if attack hits, false if it misses
     */
    public bool DoesHit(int attack)
    {
        return attack >= m_AC;
    }
    /**
     * checks if an saving throw succeeds
     * 
     * @param save: value of the saving throw
     * @return true if the saving throw succeeds, false otherwise
     */
    public bool DoesSave(int save)
    {
        return save >= m_SpellDC;
    }

    /**
     * Gets saving throw bonus for given ability
     * 
     * @param stat: ability to get saving throw for
     * 
     * @return: total bonus to saving throw
     */
    public int GetSave(Stats stat)
    {
        int total = 0;

        total += GetMod(stat);
        total += m_SaveProfs[(int)stat] ? m_PB : 0;

        return total;
    }
    // Getter for spell attack bonus
    public int GetSpellAttack()
    {
        return m_SpellAttack;
    }
    // Getter for weapon attack bonus
    public int GetWeaponAttack()
    {
        return m_WeaponAttack;
    }
    // Getter for armor class
    public int GetAC()
    {
        return m_AC;
    }
    // Getter for spell save DC
    public int GetDC()
    {
        return m_SpellDC;
    }
    // Getter for die roller
    public DieRoller GetRoller()
    {
        return m_DieRoller;
    }

    /**
     * assigns damage to character (use negative value to heal)
     * 
     * @param ammount: ammount of damage to deal
     * @return remaining hp
     */
    virtual public int Damage(int ammount)
    {
        m_HP -= ammount;

        if (m_HP < 0)
        {
            m_HP = 0;
        }

        if (m_HP > m_MaxHP)
        {
            m_HP = m_MaxHP;
        }

        return m_HP;
    }
    /**
     * Applies spell effect to character
     * If the effect is already present only override duration if it is longer
     * 
     * @param effect: effect to appply to character
     * @param duration: duration of effect
     */
    public void ApplyEffect(SpellEffects effect, int duration)
    {
        int currentTimer = m_EffectTimers[(int)effect];
        m_EffectTimers[(int)effect] = duration > currentTimer ? duration : currentTimer;
    }

    // Reduces all effect timers
    public void EndTurn()
    {
        for (int i = 0; i < m_EffectTimers.Length; ++i)
        {
            if (m_EffectTimers[i] > 0)
            {
                m_EffectTimers[i]--;
            }
        }
    }
}
