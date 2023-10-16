using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCharacter : Character
{
    public int[] BASE_PLAYER_STATS = new int[N_STATS];

    private Stats m_WeaponAttackStat;
    private Spell[] m_Spells;
    private Character m_Target;
    private ResourceBar m_HPBar;
    private ResourceBar m_MPBar;
    private TextMeshProUGUI m_NameTag;

    private bool m_Busy = false;

    void Start()
    {
        SetStatArray(BASE_PLAYER_STATS);

        m_DieRoller = m_DieRollerObject.GetComponent<DieRoller>();
        FindObjects();

        // give ourselves some levels for testing
        LevelUp();
        LevelUp();
        LevelUp();
        LevelUp();
        LevelUp();

        UpdateResources();

        // Get Resource bars and nametag
        m_HPBar = GameObject.FindGameObjectWithTag("PlayerHP").GetComponent<ResourceBar>();
        m_MPBar = GameObject.FindGameObjectWithTag("PlayerMP").GetComponent<ResourceBar>();
        m_NameTag = GameObject.FindGameObjectWithTag("PlayerName").GetComponent<TextMeshProUGUI>();

        SetStatArray(BASE_PLAYER_STATS);

        m_Target = GameObject.FindGameObjectWithTag("Targeted").GetComponent<Character>();

        // init resource bars
        m_NameTag.text = this.name;

        m_HPBar.SetValue(this.m_HP);
        m_HPBar.SetMax(this.m_MaxHP);

        m_MPBar.SetValue(this.m_MP);
        m_MPBar.SetMax(this.m_MaxMP);

        // some dummy code to test spellcasting
        Spell EBlast = new Spell();
        EBlast.name = "Eldritch Blast";
        EBlast.mpCost = 1;
        EBlast.damageFormula = new DamageFormula(new Dice[] { Dice.D10 }, true, Stats.CHA, false);
        EBlast.attackType = Spell.SpellAttackTypes.ATTACK;
        EBlast.effectType = Spell.SpellEffectTypes.DAMAGE;
        EBlast.spellEffect = SpellEffects.NONE;
        EBlast.effectDuration = 0;
        EBlast.castStat = Stats.CHA;
        EBlast.saveStat = Stats.STR;

        Spell Charm = new Spell();
        Charm.name = "Charm";
        Charm.mpCost = 2;
        Charm.damageFormula =  null;
        Charm.attackType = Spell.SpellAttackTypes.SAVE;
        Charm.effectType = Spell.SpellEffectTypes.UTILITY;
        Charm.spellEffect = SpellEffects.CHARM;
        Charm.effectDuration = 2;
        Charm.castStat = Stats.CHA;
        Charm.saveStat = Stats.WIS;

        m_Spells = new Spell[2];
        m_Spells[0] = EBlast;
        m_Spells[1] = Charm;

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
    /**
     * Helper function to reduce mp on both character sheet and resource bar
     * 
     * @param ammount: ammount of mp to reduce by
     */
    public void SpendMP(int ammount)
    {
        m_MP -= ammount;

        if (m_MP < 0)
        {
            m_MP = 0;
        }

        if (m_MP > m_MaxMP)
        {
            m_MP = m_MaxMP;
        }

        m_MPBar.SetValue(m_MP);
    }

    /**
     * Checks if spell is castable and calls coroutine
     * 
     * @parma name: name of spell to cast
     */
    public void Cast(string name)
    {
        // check if character is busy
        if (!m_Busy)
        {
            // find spell
            Spell spell = null;
            foreach (Spell s in m_Spells)
            {
                if (s.name.Equals(name))
                {
                    spell = s;
                }
            }
            // if spell doesnt exist, return
            if (spell == null)
            {
                Debug.LogError("ERROR: SPELL DOES NOT EXIST (" + name + ") IN " + this.name);
                return;
            }
            // check if player has enough MP
            if (m_MP < spell.mpCost)
            {
                // output cant cast and return
                m_DialogueText.text = "Not enough MP to cast " + name + "!";
                return;
            }

             m_Busy = true;
             StartCoroutine(CastSpell(spell));
        }
    }
    private IEnumerator CastSpell(Spell spell)
    {
        GameObject actionsMenu = GameObject.FindGameObjectWithTag("ActionsMenu");
        actionsMenu.SetActive(false);
        SpendMP(spell.mpCost);
        yield return spell.Cast(this, m_Target, m_DialogueText);
        actionsMenu.SetActive(true);
        yield return null;
    }
}
