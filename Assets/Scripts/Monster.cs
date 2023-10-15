using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Monster : Character
{
    public const float ACTION_DELAY = 1.5f;

    public int[] BASE_MONSTER_STATS = { 16, 14, 15, 8, 12, 8 };
    public Stats m_AttackAttribute;
    public DamageFormula m_AttackDamage;
    public Character m_Player;

    private ResourceBar m_HPBar;

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

        m_AttackDamage = new DamageFormula(new Character.Dice[]{ Dice.D4, Dice.D6 }, true, m_AttackAttribute, false);

        StartCoroutine(ProgressTurn());
    }

    public IEnumerator ProgressTurn()
    {
        m_DialogueText.text = this.name + " attacks " + m_Player.name + "!";

        yield return new WaitForSeconds(ACTION_DELAY);

        int roll = m_DieRoller.Roll(m_WeaponAttack) + m_WeaponAttack;

        bool hits = m_Player.DoesHit(roll);

        if(hits)
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
