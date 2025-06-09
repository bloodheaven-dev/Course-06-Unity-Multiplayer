using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<Vector2> MoveEvent;
    public event Action<bool> PrimaryFireEvent;

    public Vector2 AimPosition { get; private set; }

    private Controls controls;

    private void OnEnable()
    {
        InitializeControls();
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.SetCallbacks(null);
            controls.Disable();
            controls.Dispose();
            controls = null;
        }
    }

    private void InitializeControls()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
        }
        controls.Player.Enable();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnPrimaryFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PrimaryFireEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            PrimaryFireEvent?.Invoke(false);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        AimPosition = context.ReadValue<Vector2>();
    }

    #if UNITY_EDITOR
    //Reload Domain MAGIC
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ReinitializeAllInputReaders()
    {
        InputReader[] inputReaders = Resources.FindObjectsOfTypeAll<InputReader>();
        foreach (InputReader inputReader in inputReaders)
        {
            inputReader.OnDisable();
            inputReader.InitializeControls();
        }
    }

    #endif
}
