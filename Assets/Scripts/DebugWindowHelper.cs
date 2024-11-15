using UnityEngine;

public class DebugWindowHelper : MonoBehaviour
{
    [SerializeField] private GameObject debugWindowGameObject;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote)) //same key as the tilde key
        {
            debugWindowGameObject.SetActive(!debugWindowGameObject.activeInHierarchy);
        }
    }
}
