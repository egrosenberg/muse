using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCharacter : Character
{
    public int[] BASE_PLAYER_STATS = new int[N_STATS];
    public Dice[] RAPIER_DICE = new Dice[0];
    public static Stats RAPIER_STAT = Stats.DEX;
    public int PARRY_MP_COST = 3;
    public AudioClip SFX_RAPIER;
    public AudioClip SFX_PARRY;
    public AudioClip SFX_HEAL;
    public AudioClip SFX_CHARM;

    private Spell[] m_Spells;
    private Monster m_Target;
    private bool m_TargetSelf;
    private ResourceBar m_HPBar;
    private ResourceBar m_MPBar;
    private TextMeshProUGUI m_NameTag;
    private DamageFormula m_WeaponDamge;
    private GameObject m_ActionsMenu;

    private bool m_Busy = false;

    void Start()
    {
        SetStatArray(BASE_PLAYER_STATS);
        SetWeaponStat(RAPIER_STAT);
        
        m_DieRoller = m_DieRollerObject.GetComponent<DieRoller>();
        FindObjects();

        // give ourselves some levels for testing
        SetLevel(3);


        // Get Resource bars and nametag
        m_HPBar = GameObject.FindGameObjectWithTag("PlayerHP").GetComponent<ResourceBar>();
        m_MPBar = GameObject.FindGameObjectWithTag("PlayerMP").GetComponent<ResourceBar>();
        m_NameTag = GameObject.FindGameObjectWithTag("PlayerName").GetComponent<TextMeshProUGUI>();

        // get actions ui
        m_ActionsMenu = GameObject.FindGameObjectWithTag("ActionsMenu");

        SetStatArray(BASE_PLAYER_STATS);

        m_Target = GameObject.FindGameObjectWithTag("Targeted").GetComponent<Monster>();

        // init resource bars
        m_NameTag.text = this.name;

        DrawResources();

        // some dummy code to test spellcasting
        Spell EBlast = new Spell();
        EBlast.name = "Eldritch Blast";
        EBlast.mpCost = 1;
        EBlast.damageFormula = new DamageFormula(new Dice[] { Dice.D10 }, true, Stats.CHA, false);
        EBlast.attackType = Spell.SpellAttackTypes.ATTACK;
        EBlast.effectType = Spell.SpellEffectTypes.DAMAGE;
        EBlast.spellEffect = Effects.NONE;
        EBlast.effectDuration = 0;
        EBlast.castStat = Stats.CHA;
        EBlast.saveStat = Stats.STR;

        // Charm
        Spell Charm = new Spell();
        Charm.name = "Charm";
        Charm.mpCost = 3;
        Charm.damageFormula =  null;
        Charm.attackType = Spell.SpellAttackTypes.SAVE;
        Charm.effectType = Spell.SpellEffectTypes.UTILITY;
        Charm.spellEffect = Effects.CHARM;
        Charm.effectDuration = 2;
        Charm.castStat = Stats.CHA;
        Charm.saveStat = Stats.WIS;
        Charm.sfx = SFX_CHARM;

        // Heal
        Spell Heal = new Spell();
        Heal.name = "Heal";
        Heal.mpCost = 5;
        Heal.damageFormula = new DamageFormula(new Dice[] { Dice.D8, Dice.D8 }, true, Stats.CHA, true);
        Heal.attackType = Spell.SpellAttackTypes.NONE;
        Heal.effectType = Spell.SpellEffectTypes.DAMAGE;
        Heal.spellEffect = Effects.NONE;
        Heal.effectDuration = 0;
        Heal.castStat = Stats.CHA;
        Heal.saveStat = Stats.CHA;
        Heal.sfx = SFX_HEAL;

        m_Spells = new Spell[3];
        m_Spells[0] = EBlast;
        m_Spells[1] = Charm;
        m_Spells[2] = Heal;

        // set up rapier for attacks
        m_WeaponDamge = new DamageFormula(RAPIER_DICE, true, m_WeaponAbility, false);

        UpdateResources();
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

        DrawResources();

        return toReturn;
    }
    // override draw resources, draw hp and mp
    protected override void DrawResources()
    {
        m_HPBar.SetValue(this.m_HP);
        m_HPBar.SetMax(this.m_MaxHP);
        m_MPBar.SetValue(this.m_MP);
        m_MPBar.SetMax(this.m_MaxMP);
    }

    // set whether our spells should target self
    public void SetTargetSelf(bool targetSelf)
    {
        m_TargetSelf = targetSelf;
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

        DrawResources();
    }

    /**
     * Checks if spell is castable and calls coroutine
     * 
     * @param name: name of spell to cast
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

            // check who to target
            Character target = m_TargetSelf ? this : m_Target;

             m_Busy = true;
             StartCoroutine(CastSpell(spell, target));
        }
    }
    // calls private coroutine to execute parry
    public void Parry()
    {
        if (!m_Busy)
        {
            // check if we have enough MP to parry
            if (m_MP < PARRY_MP_COST)
            {
                // output cant parry and return
                m_DialogueText.text = "Not enough MP to parry!";
                return;
            }
            m_Busy = true;
            StartCoroutine(CallParry());
        }
    }
    // calls private coroutine to execute attack
    public void Attack()
    {
        if (!m_Busy)
        {
            m_Busy = true;
            StartCoroutine(CallAttack());
        }
    }

    // starts the round after player input, disables input
    private IEnumerator StartRound()
    {
        // Mark round as started
        OverworldController.ROUND_IN_PROGRESS = true;
        // disable actions menu
        m_ActionsMenu.SetActive(false);
        yield return null;
    }
    // ends the round and re-enales input
    private IEnumerator EndRound()
    {
        base.EndTurn();
        // call monster turn and then re-enable actions menu
        yield return StartCoroutine(m_Target.ProgressTurn());
        // enable actions menu
        m_ActionsMenu.SetActive(true);
        m_Busy = false;
        // Mark round as complete
        OverworldController.ROUND_IN_PROGRESS = false;
        yield return null;
    }

    /**
     * Casts a spell at a specified target by calling spell function
     * 
     * @param spell: spell to cast
     * @param target: character to target
     */
    private IEnumerator CastSpell(Spell spell, Character target)
    {
        yield return StartRound();

        SpendMP(spell.mpCost);

        // cast spell
        yield return spell.Cast(this, target, m_DialogueText);

        yield return EndRound();
    }
    // Applies parry to self and sends dialogue message
    private IEnumerator CallParry()
    {
        yield return StartRound();

        // spend MP
        SpendMP(PARRY_MP_COST);

        // update dialogue message
        m_DialogueText.text = this.name + " Parries!";
        // add parry for 1 round
        ApplyEffect(Effects.PARRY, 2);
        // wait
        yield return OverworldController.WaitForPlayer();

        yield return EndRound();
    }
    // Attempts to attack targeted creature
    private IEnumerator CallAttack()
    {
        yield return StartRound();

        m_DialogueText.text = this.name + " attacks the " + m_Target.name + "!";

        // roll attack
        int dieRoll = m_DieRoller.Roll(m_WeaponAttack);

        float delay = m_DieRoller.GetFinish() - Time.time + ACTION_DELAY;

        yield return new WaitForSecondsRealtime(delay);

        // check if attack hits
        int toHit = dieRoll + m_WeaponAttack;
        bool success = m_Target.DoesHit(toHit);

        // check for critical success or failure
        bool crit = dieRoll == CRITICAL_HIT || dieRoll == CRITICAL_MISS;
        success = dieRoll == CRITICAL_HIT ? true : success;
        success = dieRoll == CRITICAL_MISS ? false : success;

        // oputput text
        string critical = crit ? " critically" : "";
        string hit = success ? " hits" : " misses";
        m_DialogueText.text = this.name + critical + hit + "!";
        yield return OverworldController.WaitForPlayer();

        // deal damage
        if (success && m_WeaponDamge != null)
        {
            int damage = m_WeaponDamge.Roll(this);

            // account for critical hit. roll damage dice again but dont add modifier
            damage += crit ? m_WeaponDamge.Roll(this) - GetMod(m_WeaponDamge.ability) : 0;

            m_Target.Damage(damage);

            m_DialogueText.text = m_Target.name + " takes " + damage + " damage!";
            yield return OverworldController.WaitForPlayer();
        }

        m_DieRoller.Deactivate();

        yield return EndRound();
    }
}
