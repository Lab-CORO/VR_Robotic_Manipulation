using UnityEngine;
using TMPro;

public class PlusMinusButton : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro numberText;
    public GameObject plusButton;
    public GameObject minusButton;

    //Settings
    public int minValue = 0;
    public int maxValue = 100;
    public int currentValue = 50;


    private bool isHoldingPlus = false;
    private bool isHoldingMinus = false;
    private float holdTimer = 0f;
    public float repeatDelay = 0.1f;

    public event System.Action<PlusMinusButton, int> OnValueChanged;

    void Start()
    {
        UpdateDisplay();
    }

    public void Increment()
    {
        if (currentValue < maxValue)
        {
            currentValue++;
            UpdateDisplay();
        }
    }

    public void Decrement()
    {
        if (currentValue > minValue)
        {
            currentValue--;
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        numberText.text = currentValue.ToString();
        OnValueChanged?.Invoke(this, currentValue);
    }

    void Update()
    {
        if (isHoldingPlus)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= repeatDelay)
            {
                Increment();
                holdTimer = 0f;
            }
        }
        else if (isHoldingMinus)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= repeatDelay)
            {
                Decrement();
                holdTimer = 0f;
            }
        }
    }

    public void OnPlusDown() => isHoldingPlus = true;
    public void OnPlusUp() => isHoldingPlus = false;

    public void OnMinusDown() => isHoldingMinus = true;
    public void OnMinusUp() => isHoldingMinus = false;

}

