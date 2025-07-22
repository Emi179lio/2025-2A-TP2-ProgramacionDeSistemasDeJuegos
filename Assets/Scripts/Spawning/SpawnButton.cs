using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonLabel;
    [HideInInspector] public SpawnButtonConfig config;

    CharacterSpawner _Spawner;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonLabel = GetComponentInChildren<TextMeshProUGUI>();
        _Spawner = FindFirstObjectByType<CharacterSpawner>();

        if (config != null && buttonLabel != null)
            buttonLabel.text = config.buttonText;
    }

    private void OnEnable()
    {
        if (!button)
        {
            Debug.LogError($"{name} ({GetType().Name}): Button reference is missing.");
            enabled = false;
            return;
        }

        button.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        button?.onClick?.RemoveListener(HandleClick);
    }

    private void HandleClick()
    {
        if (config == null || _Spawner == null)
        {
            Debug.LogError("SpawnButton config or spawner is missing.");
            return;
        }

        _Spawner.Spawn(config.setupModels, config.animatorController);
    }
}
