using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ship : MonoBehaviour
{

    Gun[] guns;

    [Header("Input System")]
    public InputActionAsset inputActions;
    private InputAction moveAction;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    private Vector2 moveInput;

    bool shoot;

    void Start()
    {
        guns = transform.GetComponentsInChildren<Gun>();
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            Debug.LogError("❌ No se asignó el InputActionAsset en el inspector.");
            return;
        }

        // Busca el ActionMap llamado "Player"
        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogError("❌ No se encontró el ActionMap 'Player' en el InputActionAsset.");
            return;
        }

        // Busca la acción "Move"
        moveAction = playerMap.FindAction("Move");
        if (moveAction == null)
        {
            Debug.LogError("❌ No se encontró la acción 'Move' dentro del ActionMap 'Player'.");
            return;
        }

        moveAction.Enable();
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector2 pos = transform.position;
        float moveAmount = moveSpeed * Time.fixedDeltaTime;

        Vector2 move = moveInput.normalized * moveAmount;
        pos += move;

        // Limitar posición
        pos.x = Mathf.Clamp(pos.x, 1.5f, 16f);
        pos.y = Mathf.Clamp(pos.y, 1f, 9f);

        transform.position = pos;
    }
}
