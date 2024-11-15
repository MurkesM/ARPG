using UnityEngine;

public class AttributeComponent : MonoBehaviour
{
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int maxHealth = 100;

    public void ApplyHealthChange(int delta)
    {
        int healthBeforeChange = currentHealth;

        currentHealth += delta;

        print($"HealthChange- Before: {healthBeforeChange} : After: {currentHealth} : Delta: {delta}");
    }
}
