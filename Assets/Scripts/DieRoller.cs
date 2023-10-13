using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DieRoller : MonoBehaviour
{
    private const int N_SIDES = 20;
    private const int MIN_SPINS = 15;
    private const int MAX_SPINS = 30;
    private const float SPIN_DELAY = 0.1f;
    private const int TOTAL_DEGREES = 360;
    private const int MIN_ROTATION_ANGLE = 120;
    private const int MAX_ROTATION_ANGLE = 60;

    public GameObject m_DieImageObj;

    private TextMeshProUGUI m_TextComponent;
    private RectTransform m_DieTransform;

    private bool m_IsRolling = false;
    private float m_NextSpinT = 0f;
    private int m_SpinsRemaining = 0;
    private int m_Result = 20;
    private int m_DieAngle = 0;


    void Start()
    {
        m_TextComponent = this.GetComponentInChildren<TextMeshProUGUI>();

        m_DieTransform = m_DieImageObj.GetComponent<RectTransform>();

        Roll();
    }

    void Roll()
    {
        m_SpinsRemaining = Random.Range(MIN_SPINS, MAX_SPINS);

        m_Result = Random.Range(1, N_SIDES + 1);

        m_IsRolling = true;
    }

    void Update()
    {
        // if it is time to display next number AND we are spinning
        if (m_IsRolling && Time.time >= m_NextSpinT)
        {
            // show a random number
            int displayN = Random.Range(1, N_SIDES + 1);
            m_TextComponent.text = displayN.ToString();

            // rotate die sprite
            int angleToRotate = Random.Range(MIN_ROTATION_ANGLE, MAX_ROTATION_ANGLE);
            m_DieAngle += angleToRotate;
            m_DieAngle = m_DieAngle % TOTAL_DEGREES;

            Quaternion targetAngle = Quaternion.Euler(0f, 0f, m_DieAngle);

            m_DieTransform.rotation = targetAngle;

            Debug.Log(targetAngle.ToString());
            // update next time to roll
            m_NextSpinT = Time.time + SPIN_DELAY;
            // update spins remaing and check if we are done rolling
            m_SpinsRemaining -= 1;
            if (m_SpinsRemaining < 0)
            {
                // if we are done, set rolling to false and set result to die face
                m_IsRolling = false;
                m_TextComponent.text = m_Result.ToString("#,0");
            }
        }
    }
}
