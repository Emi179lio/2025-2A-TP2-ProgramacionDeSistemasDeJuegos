using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Character))]
public class PlayerController : MonoBehaviour, ISetup<IPlayerControllerModel>
{
    public IPlayerControllerModel Model { get; set; }

    private bool _isJumping;
    private bool _isDoubleJumping;
    private Character _character;
    private Coroutine _jumpCoroutine;

    private void Awake()
        => _character = GetComponent<Character>();

    public void Setup(IPlayerControllerModel model)
    {
        Model = model;
        if (Model.MoveInput?.action != null)
        {
            Model.MoveInput.action.started += HandleMoveInput;
            Model.MoveInput.action.performed += HandleMoveInput;
            Model.MoveInput.action.canceled += HandleMoveInput;
        }
        if (Model.JumpInput?.action != null)
            Model.JumpInput.action.performed += HandleJumpInput;

        ConsoleManager._Instance.SendLog("Character configured");
    }
    private void OnDisable()
    {
        if (Model.MoveInput?.action != null)
        {
            Model.MoveInput.action.performed -= HandleMoveInput;
            Model.MoveInput.action.canceled -= HandleMoveInput;
        }
        if (Model.JumpInput?.action != null)
            Model.JumpInput.action.performed -= HandleJumpInput;
    }

    private void HandleMoveInput(InputAction.CallbackContext ctx)
    {
        var direction = ctx.ReadValue<Vector2>().x;
        if (_isJumping || _isDoubleJumping)
            direction *= Model.AirborneSpeedMultiplier;
        _character?.SetDirection(direction);
    }

    private void HandleJumpInput(InputAction.CallbackContext ctx)
    {
        if (_isJumping)
        {
            if (_isDoubleJumping)
                return;
            RunJumpCoroutine();
            _isDoubleJumping = true;
            return;
        }
        RunJumpCoroutine();
        _isJumping = true;
    }

    private void RunJumpCoroutine()
    {
        if (_jumpCoroutine != null)
            StopCoroutine(_jumpCoroutine);
        _jumpCoroutine = StartCoroutine(_character.Jump());
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        foreach (var contact in other.contacts)
        {
            if (Vector3.Angle(contact.normal, Vector3.up) < 5)
            {
                _isJumping = false;
                _isDoubleJumping = false;
            }
        }
    }
}