using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI uiText;
    [SerializeField] private float followSpeed = 1f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);

    private CharacterBehavior target;
    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        if (target != null)
            uiText.transform.position = camera.WorldToScreenPoint(target.transform.position + offset);
    }

    public void SetTarget(CharacterBehavior characterBehavior)
    {
        target = characterBehavior;
        SetText(characterBehavior.AttributeComponent.CurrentHealth.ToString());
        target.AttributeComponent.OnHealthChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int currentHealth)
    {
        SetText(currentHealth.ToString());
    }

    private void SetText(string text)
    {
        uiText.SetText(text);
    }

    private void OnDestroy()
    {
        if (target)
            target.AttributeComponent.OnHealthChanged -= OnHealthChanged;
    }
}