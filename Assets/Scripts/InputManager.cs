using System;
using UnityEngine;
using UnityEngine.InputSystem;


[DefaultExecutionOrder(-100)] 
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    
    private const string MovementAction = "Player/Movement";
    private const string JumpAction = "Player/Jump";
    private const string AttackAction = "Player/Attack";

    private InputAction movement;
    private InputAction jump;
    private InputAction attack;

    

    
    public Vector2 Movement => movement.ReadValue<Vector2>();

   
    public bool AttackHeld => attack.IsPressed();

 
    public event Action JumpPressed;
    public event Action JumpReleased;
    public event Action AttackPressed;
    public event Action AttackReleased;

    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        movement = InputSystem.actions.FindAction(MovementAction, true);
        jump = InputSystem.actions.FindAction(JumpAction, true);
        attack = InputSystem.actions.FindAction(AttackAction, true);
    }

    private void OnEnable()
    {
        if (Instance != this) return;

        InputSystem.actions.Enable();

        jump.performed += OnJumpPerformed;
        jump.canceled += OnJumpCanceled;
        attack.performed += OnAttackPerformed;
        attack.canceled += OnAttackCanceled;
    }

    private void OnDisable()
    {
        if (Instance != this) return;

        jump.performed -= OnJumpPerformed;
        jump.canceled -= OnJumpCanceled;
        attack.performed -= OnAttackPerformed;
        attack.canceled -= OnAttackCanceled;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

  

    private void OnJumpPerformed(InputAction.CallbackContext ctx) => JumpPressed?.Invoke();
    private void OnJumpCanceled(InputAction.CallbackContext ctx) => JumpReleased?.Invoke();
    private void OnAttackPerformed(InputAction.CallbackContext ctx) => AttackPressed?.Invoke();
    private void OnAttackCanceled(InputAction.CallbackContext ctx) => AttackReleased?.Invoke();
}
