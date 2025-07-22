using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public PlayerInput _Input;

    public static GameManager _I;

    private void Awake()
    {
        if (_I == null)
        {
            _I = this;
        }
    }

    public static bool OnPressF1()
    {
        return _I._Input.actions["ConsoleToggle"].WasPressedThisFrame();
    }

    public static bool OnSubmit()
    {
        return _I._Input.actions["Submit"].WasPressedThisFrame();
    }
}
