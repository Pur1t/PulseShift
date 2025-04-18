using UnityEngine;
using UnityEngine.UI;

public class HealthSettingsMenu : MonoBehaviour
{
    public Slider perfectGainSlider;
    public Slider greatGainSlider;
    public Slider missPenaltySlider;

    void Start()
    {
        LoadSettings();
    }

    public void ApplySettings()
    {
        HealthSettings newSettings = new HealthSettings
        {
            perfectGain = perfectGainSlider.value,
            greatGain = greatGainSlider.value,
            missPenalty = missPenaltySlider.value
        };

        HealthSystem.Instance.SetHealthConfig(newSettings);
    }

    void LoadSettings()
    {
        perfectGainSlider.value = HealthSystem.Instance.healthConfig.perfectGain;
        greatGainSlider.value = HealthSystem.Instance.healthConfig.greatGain;
        missPenaltySlider.value = HealthSystem.Instance.healthConfig.missPenalty;
    }
}