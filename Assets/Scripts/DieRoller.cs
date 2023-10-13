using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DieRoller : MonoBehaviour
{
    private const int N_SIDES = 20;
    private const int MIN_SPINS = 10;
    private const int MAX_SPINS = 30;
    private const float SPIN_DELAY = 0.5f;

    private TextMeshProUGUI m_TextComponent;

    private bool m_IsRolling = false;
    private float m_NextSpinT = 0f;
    private int m_SpinsRemaining = 0;
    private int m_Result = 20;


    void Start()
    {
        m_TextComponent = this.GetComponentInChildren<TextMeshProUGUI>();

        Roll();
    }

    void Roll()
    {
        int m_SpinsRemaining = Random.Range(MIN_SPINS, MAX_SPINS + 1);

        int m_Result = Random.Range(1, N_SIDES + 1);

        m_IsRolling = true;
    }

    private void Update()
    {
        // if it is time to display next number AND we are spinning
        if (m_IsRolling && Time.time >= m_NextSpinT)
        {
            // show a random number
            int displayN = Random.Range(1, N_SIDES + 1);
            m_TextComponent.text = displayN.ToString();
            // update next time to roll
            m_NextSpinT = Time.time + SPIN_DELAY;
            // update spins remaing and check if we are done rolling
            if (--m_SpinsRemaining <= 0)
            {
                // if we are done, set rolling to false and set result to die face
                m_IsRolling = false;
                //m_TextComponent.text = m_Result.ToString("#,0");
            }
        }
    }
}
