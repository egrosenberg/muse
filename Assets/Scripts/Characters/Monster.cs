using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Monster : Character
{
    private const bool ADD_STAT_TO_DAMAGE = true;
    private const bool NO_HEAL = false;
    public int[] BASE_MONSTER_STATS = { 16, 14, 15, 8, 12, 8 };
    public static int MIN_LEVELUP = 1;
    public static int MAX_LEVELUP = 5;

    public Stats m_AttackAttribute;        // stat the monster uses to attack
    private PlayerCharacter m_Player;      // player the monster targets
    public Dice[] m_DamageDice;            // dice array to use for damage formula

    private DamageFormula m_AttackDamage;  // damage formula for calculating attack damage
    private ResourceBar m_HPBar;           // hp bar to display monster's health
    private TextMeshProUGUI m_NameTag;     // name tag for above hp

    private bool m_IsTakingTurn = false;

    void Start()
    {
        FindObjects();

        m_Player = GameObject.FindGameObjectWithTag("PlayerSheet").GetComponent<PlayerCharacter>();
        m_NameTag = GameObject.FindGameObjectWithTag("MonsterName").GetComponent<TextMeshProUGUI>();

        if (m_Player != null)
        {
            Debug.Log("Player found");
        }

        GameObject dieObject = GameObject.FindGameObjectWithTag("MonsterDie");
        m_DieRoller = dieObject.GetComponent<DieRoller>();
        m_DieRoller.SetVisibility(false);

        GameObject hpObject = GameObject.FindGameObjectWithTag("MosterHealth");
        m_HPBar = hpObject.GetComponent<ResourceBar>();

        // level up a random ammount of times
        int level = Random.Range(MIN_LEVELUP, MAX_LEVELUP);
        for (int i = 0; i < level; ++i)
        {
            LevelUp();
        }

        this.SetStatArray(BASE_MONSTER_STATS);

        this.RefreshAll();

        m_AttackDamage = new DamageFormula(m_DamageDice, ADD_STAT_TO_DAMAGE, m_AttackAttribute, NO_HEAL);

    }

    // Function to call async coroutine for turn
    public void StartTurn()
    {
        StartCoroutine(ProgressTurn());
    }

    protected override void DrawResources()
    {
        m_NameTag.text = this.name;
        m_HPBar.SetValue(this.m_HP);
        m_HPBar.SetMax(this.m_MaxHP);
    }

    /**
     * Run AI for one turn
     */
    public IEnumerator ProgressTurn()
    {
        if (m_IsTakingTurn)
        {
            yield return null;
        }
        m_IsTakingTurn = true;

        // check if monster is dead?
        if (m_HP <= 0)
        {
            m_DialogueText.text = this.name + " is dead and cannot act!";
            yield return new WaitForSecondsRealtime(ACTION_DELAY);
        }
        // monster is not dead, do turn
        else
        {
            // check if monster can attack
            bool canAttack = true;

            // Handle being charmed
            if (m_EffectTimers[(int)Effects.CHARM] > 0)
            {
                // we cannot attack this turn
                canAttack = false;

                m_DialogueText.text = this.name + " is Charmed and cannot act!";

                yield return new WaitForSecondsRealtime(ACTION_DELAY);

            }
            if (m_EffectTimers[(int)Effects.REELING] > 0)
            {

                // we cannot attack this turn
                canAttack = false;

                m_DialogueText.text = this.name + " is Reeling and cannot act!";

                yield return new WaitForSecondsRealtime(ACTION_DELAY);
            }

            // if we are attacking
            if (canAttack)
            {
                m_DialogueText.text = this.name + " attacks " + m_Player.name + "!";

                yield return new WaitForSecondsRealtime(ACTION_DELAY);


                int roll = m_DieRoller.Roll(m_WeaponAttack) + m_WeaponAttack;

                bool hits = m_Player.DoesHit(roll, this);

                // check if we hit
                if (hits)
                {
                    // apply damage
                    int damage = m_AttackDamage.Roll(this);
                    m_Player.Damage(damage);

                    // update dialogue box
                    m_DialogueText.text = this.name + " hit " + m_Player.name + " for " + damage + " damage!";
                }
                else
                {
                    // check if we are reeling and update dialogue box
                    if (m_EffectTimers[(int)Effects.REELING] > 0)
                    {
                        m_DialogueText.text = this.name + " missed and was sent Reeling!";
                    }
                    else
                    {
                        m_DialogueText.text = this.name + " missed!";
                    }
                }
                yield return new WaitForSecondsRealtime(ACTION_DELAY);
            }
        }

        // call parent end of turn
        this.EndTurn();

        yield return m_IsTakingTurn = false; // mark that turn is finished

    }

    public bool RollAttack()
    {
        bool success = false;

        int bonus = this.GetWeaponAttack();
        int dieRoll = this.m_DieRoller.Roll(bonus);

        return success;
    }

    /**
     * Override base class damage by adding in hp bar support
     * 
     * @param ammount: damage to deal
     * 
     * @return: remaining hp
     */
    public override int Damage(int ammount)
    {
        int toReturn = base.Damage(ammount);

        m_HPBar.SetValue(m_HP);

        return toReturn;
    }
}
