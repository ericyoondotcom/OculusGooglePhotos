using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    public Image bar;
    public TextMeshProUGUI text;

    public void SetProgress(float amount)
    {
        bar.fillAmount = amount;
        text.text = Mathf.RoundToInt(amount * 100).ToString() + "%";
    }
}
