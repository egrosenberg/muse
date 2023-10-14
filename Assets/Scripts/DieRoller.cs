using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DieRoller : MonoBehaviour
{
    private const int N_SIDES = 20;
    private const int MIN_SPINS = 10;
    private const int MAX_SPINS = 15;
    private const float SPIN_DELAY = 0.1f;
    private const int TOTAL_DEGREES = 360;
    private const int MIN_ROTATION_ANGLE = 120;
    private const int MAX_ROTATION_ANGLE = 60;

    public GameObject m_DieImageObj;
    public GameObject m_BonusObj;
    public GameObject m_TotalObj;

    public GameObject m_DieTextGameObject;
    private TextMeshProUGUI m_TextComponent;
    private TextMeshProUGUI m_BonusText;
    private TextMeshProUGUI m_TotalText;
    private RectTransform m_DieTransform;

    private bool m_IsRolling = false;
    private float m_NextSpinT = 0f;
    private int m_SpinsRemaining = 0;
    private int m_Result = 20;
    private int m_DieAngle = 0;
    private int m_SpinN = 0;
    private int m_Bonus = 0;


    void Start()
    {
        m_TextComponent = m_DieTextGameObject.GetComponent<TextMeshProUGUI>();
        m_BonusText = m_BonusObj.GetComponent<TextMeshProUGUI>();
        m_TotalText = m_TotalObj.GetComponent<TextMeshProUGUI>();

        m_DieTransform = m_DieImageObj.GetComponent<RectTransform>();

        Roll(4);
    }

    void Roll(int bonus)
    {
        m_Bonus = bonus;
        m_SpinsRemaining = Random.Range(MIN_SPINS, MAX_SPINS);
        m_SpinN = 0;
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

            if (m_SpinsRemaining >= 3)
            {
                // rotate die sprite
                int angleToRotate = Random.Range(MIN_ROTATION_ANGLE, MAX_ROTATION_ANGLE);
                m_DieAngle += angleToRotate;
                m_DieAngle = m_DieAngle % TOTAL_DEGREES;

                Quaternion targetAngle = Quaternion.Euler(0f, 0f, m_DieAngle);

                m_DieTransform.rotation = targetAngle;
            }
            // update next time to roll
            m_NextSpinT = Time.time + SPIN_DELAY * m_SpinN;
            m_SpinN++;
            // update spins remaing and check if we are done rolling
            m_SpinsRemaining -= 1;
            if (m_SpinsRemaining < 3)
            {
                m_TextComponent.text = m_Result.ToString("#,0");
            }
            if (m_SpinsRemaining == 1)
            {
                m_BonusObj.SetActive(true);
                m_BonusText.text = "+" + m_Bonus.ToString();
            }
            if (m_SpinsRemaining == 0)
            {
                m_TotalObj.SetActive(true);
                m_TotalText.text = (m_Result + m_Bonus).ToString();
                // if we are done, set rolling to false and set result to die face
                m_IsRolling = false;
            }
        }
    }
}
