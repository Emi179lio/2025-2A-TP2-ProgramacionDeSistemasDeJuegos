using UnityEngine;

[CreateAssetMenu(menuName = "Spawner/Spawn Button Config")]
public class SpawnButtonConfig : ScriptableObject
{
    public string buttonText;
    public ScriptableObject[] setupModels;
    public RuntimeAnimatorController animatorController;
}