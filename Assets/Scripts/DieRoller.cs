using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DieRoller : MonoBehaviour
{
    private const int N_SIDES = 20;
    private const int MIN_SPINS = 10;
    private const int MAX_SPINS = 11;
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
    private bool m_IsVisible = true;


    void Start()
    {
        RefreshComponents();
    }
    
    // Refreshes text and transform components from gameobjects
    public void RefreshComponents()
    {
        m_TextComponent = m_DieTextGameObject.GetComponent<TextMeshProUGUI>();
        m_BonusText = m_BonusObj.GetComponent<TextMeshProUGUI>();
        m_TotalText = m_TotalObj.GetComponent<TextMeshProUGUI>();

        m_DieTransform = m_DieImageObj.GetComponent<RectTransform>();
    }


    /**
     * Rolls the d20 with the specified bonus
     * bonus is only used for display purposes
     * 
     * @param bonus: d20 roll bonus to display for clarity purposes
     * 
     * @return d20 result (does not account for bonus)
     */
    public int Roll(int bonus)
    {
        // hide bonus and total
        m_BonusObj.SetActive(false);
        m_TotalObj.SetActive(false);

        m_Bonus = bonus;
        m_SpinsRemaining = Random.Range(MIN_SPINS, MAX_SPINS);
        m_SpinN = 0;
        m_Result = Random.Range(1, N_SIDES + 1);

        m_IsRolling = true;

        return m_Result;
    }


    // TODO: maybe make this a coroutine so it runs async

    /**
     * Check if we are rolling, if we are currently rolling:
     *   Check if we are scheduled to change sides:
     *      Rotate die sprite and set to a random face on the die
     *      Schedule the next time to change sides
     *      Gradually slow down rolling speed
     *      
     * On the last three rotations we will stay on the result
     * On the second to last, we display the bonus
     * On the final, we display the result
     * 
     * If we are not planning on displaying the die, we skip the update function altogether
     */
    void Update()
    {
        // This is kind of sloppy, but it means i get to use the current die roller
        // without making a new base class and having this extend it
        if (!m_IsVisible)
        {
            return;
        }

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

    // Set visibility member variable
    public void SetVisibility(bool visible)
    {
        m_IsVisible = visible;
    }
}
