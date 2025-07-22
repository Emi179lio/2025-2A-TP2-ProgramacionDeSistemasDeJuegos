using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private Character prefab;
    [SerializeField] private PlayerControllerModelSO playerControllerModel;
    [SerializeField] private SpawnButtonConfig[] setupModels;
    [SerializeField] private RuntimeAnimatorController animatorController;

    private static Dictionary<Type, List<Type>> interfaceCache = new Dictionary<Type, List<Type>>();

    public static CharacterSpawner _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            ConsoleManager._Instance?.SendLog("Error: solo puede haber un Spawner activo.");
            Destroy(gameObject);
            return;
        }

        _instance = this;

        SpawnButton[] _P = FindObjectsByType<SpawnButton>(FindObjectsSortMode.None);

        for (int i = 0; i < setupModels.Length && i < _P.Length; i++)
        {
            _P[i].config = setupModels[i];
        }
    }

    public void Spawn(ScriptableObject[] additionalModels, RuntimeAnimatorController animatorController)
    {
        var models = new List<ScriptableObject>(additionalModels);

        if (playerControllerModel != null)
        {
            models.Add(playerControllerModel);
            ConsoleManager._Instance.SendLog("PlayerControllerModel added to setup.");
        }
        else
        {
            ConsoleManager._Instance.SendLog("Warning: PlayerControllerModel is not assigned.");
        }

        StartCoroutine(SpawnCoroutine(models.ToArray(), animatorController));
    }


    private IEnumerator SpawnCoroutine(ScriptableObject[] setupModels, RuntimeAnimatorController animatorController)
    {
        ConsoleManager._Instance.SendLog("Spawning new character...");

        var result = Instantiate(prefab, transform.position, transform.rotation);

        yield return null;

        // Get components and ensure PlayerController is present
        var setupComponentsList = new List<MonoBehaviour>(result.GetComponentsInChildren<MonoBehaviour>(true));

        bool hasPlayerControllerModel = false;
        foreach (var model in setupModels)
        {
            if (model is IPlayerControllerModel)
            {
                hasPlayerControllerModel = true;
                break;
            }
        }

        var playerController = result.GetComponent<PlayerController>();
        if (hasPlayerControllerModel && playerController == null)
        {
            playerController = result.gameObject.AddComponent<PlayerController>();
            setupComponentsList.Add(playerController);
            ConsoleManager._Instance.SendLog("PlayerController was missing and has been added.");
        }

        const int componentsPerFrame = 5;
        int processedComponents = 0;

        foreach (var component in setupComponentsList)
        {
            if (component == null) continue;

            var componentType = component.GetType();
            if (!interfaceCache.ContainsKey(componentType))
            {
                var interfaces = new List<Type>();
                foreach (var iface in componentType.GetInterfaces())
                {
                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ISetup<>))
                    {
                        interfaces.Add(iface);
                    }
                }
                interfaceCache[componentType] = interfaces;
            }

            foreach (var iface in interfaceCache[componentType])
            {
                var modelType = iface.GetGenericArguments()[0];

                foreach (var model in setupModels)
                {
                    if (model == null)
                    {
                        ConsoleManager._Instance.SendLog("One of the setupModels is null. Skipping.");
                        continue;
                    }

                    if (modelType.IsAssignableFrom(model.GetType()))
                    {
                        var method = iface.GetMethod("Setup");
                        if (method != null)
                        {
                            try
                            {
                                method.Invoke(component, new object[] { model });
                                ConsoleManager._Instance.SendLog($"{component.GetType().Name} configured with {model.name}");
                            }
                            catch (Exception e)
                            {
                                ConsoleManager._Instance.SendLog($"Error invoking Setup on {component.GetType().Name}: {e.Message}");
                            }
                        }
                        else
                        {
                            ConsoleManager._Instance.SendLog($"Setup method not found on {component.GetType().Name}");
                        }
                        break;
                    }
                }
            }

            processedComponents++;
            if (processedComponents >= componentsPerFrame)
            {
                processedComponents = 0;
                yield return null;
            }
        }

        var animator = result.GetComponentInChildren<Animator>();
        if (animator == null)
            animator = result.gameObject.AddComponent<Animator>();

        if (animatorController != null)
        {
            animator.runtimeAnimatorController = animatorController;
            ConsoleManager._Instance.SendLog("AnimatorController assigned.");
        }
        else
        {
            ConsoleManager._Instance.SendLog("No AnimatorController was provided.");
        }
    }
}
