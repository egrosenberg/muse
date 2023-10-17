using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ActionsMenu : MonoBehaviour
{
    // get attack action and select it
    private void OnEnable()
    {
        Button attackButton = GameObject.FindGameObjectWithTag("MenuButton").GetComponent<Button>();
        attackButton.Select();
    }
}
