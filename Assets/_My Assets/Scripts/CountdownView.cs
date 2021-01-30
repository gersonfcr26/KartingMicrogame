using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CountdownView : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_countdownText;

    public void SetCountdownView(float value)
    {
        m_countdownText.SetText(value.ToString("F0"));
    }
}
