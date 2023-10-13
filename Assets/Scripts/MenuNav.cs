using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MenuNav : MonoBehaviour
{
    public GameObject m_MenuObject;
    private Button[] m_MenuButtons;

    private int m_index;

    // Start is called before the first frame update
    void Start()
    {
        m_index = 0;
        m_MenuButtons = m_MenuObject.GetComponentsInChildren<Button>();

        Debug.Log(m_MenuButtons.Length);
    }

    /**
     * sets button at the specified index to hover and updates our internal index
     * 
     * @param index: int containing index to hover
     */
    void Select(int index)
    {
        // Error / wraparound checking
        if (index >= m_MenuButtons.Length)
        {
            index = 0;
        }
        if(index < 0)
        {
            index = m_MenuButtons.Length - 1;
        }

        m_MenuButtons[index].Select();

        Debug.Log("Selected " + index);

        m_index = index;
    }

    // Select previous index
    void OnNavUp()
    {
        Select(m_index - 1);
    }

    // Select next index
    void OnNavDown()
    {
        Select(m_index + 1);
    }
}
