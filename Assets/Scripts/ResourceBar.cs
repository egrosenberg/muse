using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceBar : MonoBehaviour
{
    public GameObject m_ValueBarObject = null;
    public GameObject m_ValueTextObject = null;
    public GameObject m_BackgroundBar = null;
    public string m_ResourceName = "";
    public float m_Value = 20f;
    public float m_Max = 20f;

    private float m_MaxWidth = 0f;
    private float m_BarHeight = 0f;
    private RectTransform m_ValueBarRect;
    private TextMeshProUGUI m_ValueText = null;
    

    // Start is called before the first frame update
    void Start()
    {
        // Grab rect transform for value bar
        m_ValueBarRect = m_ValueBarObject.GetComponent<RectTransform>();

        // Initialize max bar width and bar height
        m_MaxWidth = m_BackgroundBar.GetComponent<RectTransform>().sizeDelta.x;
        m_BarHeight = m_BackgroundBar.GetComponent<RectTransform>().sizeDelta.y;

        // Grab TMP value
        m_ValueText = m_ValueTextObject.GetComponent<TextMeshProUGUI>();

        // Draw initial resource bar
        Draw();
    }

    // Re-Draws hp bar with current values
    void Draw()
    {
        // Calculate percentage of bar to fill
        float current_percent = m_Value / m_Max;
        // Don't over-fill even if values are weird
        if (current_percent > 100f)
        {
            current_percent = 100f;
        }
        // Set hp bar scale
        m_ValueBarRect.sizeDelta = new Vector2(m_MaxWidth * current_percent, m_BarHeight);

        if (m_ValueText != null)
        {
            m_ValueText.text = m_ResourceName + ": " + m_Value.ToString("#,0") + "/" + m_Max.ToString("#,0");
        }
    }

    /**
     * Sets the current resource value and re-draw resource bar with updated value
     * 
     * @param value: float containing new value to set to
     */
    void SetValue(float value)
    {
        m_Value = value;
        Draw();
    }

}
