using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance;

    public Slider healthBar;
    private float health = 100f;

    [Header("Health Settings")]
    public HealthSettings healthConfig;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ModifyHealth(JudgementType judgement)
    {
        switch (judgement)
        {
            case JudgementType.Perfect:
                ChangeHealth(healthConfig.perfectGain);
                break;
            case JudgementType.Great:
                ChangeHealth(healthConfig.greatGain);
                break;
            case JudgementType.Miss:
                ChangeHealth(-healthConfig.missPenalty);
                break;
        }
    }

    private void ChangeHealth(float amount)
    {
        health = Mathf.Clamp(health + amount, 0, 100);
        UpdateHealthUI();

        if (health <= 0)
        {
            GameOver();
        }
    }

    private void UpdateHealthUI()
    {
        healthBar.value = health / 100f;
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        // Implement Game Over UI
    }

    public void SetHealthConfig(HealthSettings newConfig)
    {
        healthConfig = newConfig;
    }
}

[System.Serializable]
public class HealthSettings
{
    public float perfectGain = 2f;
    public float greatGain = 1f;
    public float missPenalty = 10f;
}