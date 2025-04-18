using UnityEngine;
using TMPro;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance;

    public TMP_Text comboText;  // Reference to your TMP text element
    private int combo = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void IncreaseCombo()
    {
        combo++;
        UpdateComboUI();
    }

    public void ResetCombo()
    {
        combo = 0;
        UpdateComboUI();
    }

    private void UpdateComboUI()
    {
        // Only display combo when greater than 0
        comboText.text = combo > 0 ? combo.ToString() : "";
    }
}
