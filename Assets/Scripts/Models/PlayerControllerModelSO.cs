using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Characters/PlayerControllerModel")]
public class PlayerControllerModelSO : ScriptableObject, IPlayerControllerModel
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private InputActionReference jumpInput;

    [Header("Gameplay Settings")]
    [SerializeField] private float airborneSpeedMultiplier = 0.5f;

    InputActionReference IPlayerControllerModel.MoveInput => moveInput;
    InputActionReference IPlayerControllerModel.JumpInput => jumpInput;
    float IPlayerControllerModel.AirborneSpeedMultiplier => airborneSpeedMultiplier;
}
