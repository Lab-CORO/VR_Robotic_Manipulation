using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Change the text on a Slider's handle to match it's current value.
/// </summary>
public class SliderHandleText : MonoBehaviour
{
    [Header("Text of the Slider")]
    [SerializeField] TextMeshProUGUI _textMeshPro;

    private Slider _slider;
    
    // Start is called before the first frame update
    void Start()
    {
        _slider = GetComponent<Slider>();
    }
    
    /// <summary>
    /// Change the text display to math the slider's value
    /// </summary>
    /// <param name="characters">Number of characters to display</param>
    public void ChangeSliderText(int characters)
    {
        _textMeshPro.text = _slider.value.ToString("g" + characters.ToString());
    }
}
