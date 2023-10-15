using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Monster : Character
{
    private const bool ADD_STAT_TO_DAMAGE = true;
    private const bool NO_HEAL = false;
    public int[] BASE_MONSTER_STATS = { 16, 14, 15, 8, 12, 8 };

    public Stats m_AttackAttribute;        // stat the monster uses to attack
    public Character m_Player;             // player the monster targets
    public Dice[] m_DamageDice;            // dice array to use for damage formula

    private DamageFormula m_AttackDamage;  // damage formula for calculating attack damage
    private ResourceBar m_HPBar;           // hp bar to display monster's health

    private bool m_IsTakingTurn = false;

    void Start()
    {
        FindObjects();

        GameObject dieObject = GameObject.FindGameObjectWithTag("MonsterDie");
        m_DieRoller = dieObject.GetComponent<DieRoller>();
        m_DieRoller.SetVisibility(false);

        GameObject hpObject = GameObject.FindGameObjectWithTag("MosterHealth");
        m_HPBar = hpObject.GetComponent<ResourceBar>();

        GameObject.FindGameObjectWithTag("MonsterName").GetComponent<TextMeshProUGUI>().text = this.name;

        this.AddXP(34000);
        this.SetStatArray(BASE_MONSTER_STATS);

        this.UpdateResources();
        m_HPBar.SetValue(this.m_HP);
        m_HPBar.SetMax(this.m_MaxHP);

        m_AttackDamage = new DamageFormula(m_DamageDice, ADD_STAT_TO_DAMAGE, m_AttackAttribute, NO_HEAL);

    }

    // Function to call async coroutine for turn
    public void StartTurn()
    {
        if (!m_IsTakingTurn)
        {
            m_IsTakingTurn = true;
            StartCoroutine(ProgressTurn());
        }
    }

    public IEnumerator ProgressTurn()
    {
        m_DialogueText.text = this.name + " attacks " + m_Player.name + "!";

        yield return new WaitForSecondsRealtime(ACTION_DELAY);


        int roll = m_DieRoller.Roll(m_WeaponAttack) + m_WeaponAttack;

        bool hits = m_Player.DoesHit(roll);

        if (hits)
        {
            int damage = m_AttackDamage.Roll(this);
            m_Player.Damage(damage);

            m_DialogueText.text = this.name + " hit " + m_Player.name + " for " + damage + " damage!";
        }
        else
        {
            m_DialogueText.text = this.name + " missed!";
        }


        this.EndTurn();

        yield return m_IsTakingTurn = false;

    }

    public bool RollAttack()
    {
        bool success = false;

        int bonus = this.GetWeaponAttack();
        int dieRoll = this.m_DieRoller.Roll(bonus);

        return success;
    }

    public override int Damage(int ammount)
    {
        int toReturn = base.Damage(ammount);

        m_HPBar.SetValue(m_HP);

        return toReturn;
    }
}
