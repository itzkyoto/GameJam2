using UnityEngine;


[RequireComponent(typeof(Player))]
public class PlayerInputManager : MonoBehaviour
{
    private Player player;
    private InputManager input;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        input = InputManager.Instance;

        if (input == null)
        {
            Debug.LogError("PlayerInputManager: no hay un InputManager en la escena.", this);
            enabled = false;
            return;
        }

        input.JumpPressed += player.OnJumpPressed;
        input.JumpReleased += player.OnJumpReleased;
        input.AttackPressed += OnAttackPressed;
    }

    private void OnDisable()
    {
        if (input == null) return;

        input.JumpPressed -= player.OnJumpPressed;
        input.JumpReleased -= player.OnJumpReleased;
        input.AttackPressed -= OnAttackPressed;
    }

    private void Update()
    {
        player.SetMoveInput(input.Movement);
    }

    private void OnAttackPressed()
    {
       
        player.SetMoveInput(input.Movement);
        player.OnShootPressed();
    }
}
