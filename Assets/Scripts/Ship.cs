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
    private InputAction attackAction;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    private Vector2 moveInput;

    void Start()
    {
        guns = transform.GetComponentsInChildren<Gun>();
        foreach (Gun gun in guns)
        {
            gun.isActive = true;
        }
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

        // Registrar la acción "Attack" (disparo)
        attackAction = playerMap.FindAction("Attack");
        if (attackAction == null)
        {
            Debug.LogWarning("⚠️ No se encontró la acción 'Attack' en el ActionMap 'Player'. Usa el inspector para asignar un InputActionAsset con esa acción.");
        }
        else
        {
            attackAction.Enable();
            attackAction.performed += OnAttack;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
        }

        if (attackAction != null)
        {
            attackAction.Disable();
            attackAction.performed -= OnAttack;
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        ControladorSonidos.Instancia.ReproducirSonido("NaveMovimiento");
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (guns == null || guns.Length == 0)
        {
            Debug.LogWarning("⚠️ No hay Guns asignadas como hijos del Ship.");
            return;
        }

        foreach (Gun gun in guns)
        {
            gun.Shoot();
        }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            if (bullet.isEnemy)
            {
                Destroy(gameObject);
                Destroy(bullet.gameObject);
            }
        }

        Destructable destructable = collision.GetComponent<Destructable>();
        if (destructable != null)
        {
            Destroy(gameObject);
            Destroy(destructable.gameObject);
        }
    }


}
