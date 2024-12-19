using System.Collections.Generic;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    [SerializeField] private FloatingText floatingTextPrefab;

    private Dictionary<CharacterBehavior, FloatingText> floatingTextWithCharacters = new Dictionary<CharacterBehavior, FloatingText>();

    private void Awake()
    {
        CharacterBehavior.OnNetworkSpawned += CreateFloatingText;
        CharacterBehavior.OnDestroyed += DestroyFloatingText;
    }

    private void CreateFloatingText(CharacterBehavior characterBehavior)
    {
        if (floatingTextWithCharacters.TryGetValue(characterBehavior, out FloatingText _))
            return;

        FloatingText floatingText = Instantiate(floatingTextPrefab, transform);
        floatingText.SetTarget(characterBehavior);

        floatingTextWithCharacters.Add(characterBehavior, floatingText);
    }

    private void DestroyFloatingText(CharacterBehavior characterBehavior)
    {
        if (!floatingTextWithCharacters.TryGetValue(characterBehavior, out FloatingText floatingText))
            return;

        floatingTextWithCharacters.Remove(characterBehavior);
        Destroy(floatingText.gameObject);
    }

    private void OnDestroy()
    {
        CharacterBehavior.OnNetworkSpawned -= CreateFloatingText;
        CharacterBehavior.OnDestroyed -= DestroyFloatingText;
    }
}