using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Ship : MonoBehaviour
{

    public static Ship instance;

    Gun[] guns;

    [Header("Input System")]
    public InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction attackAction;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Vector2 moveInput;

    GameObject shield;
    int powerUpGunLevel = 0;

    void Start()
    {

        shield = transform.Find("Shield").gameObject;
        DeactivateShield();

        guns = transform.GetComponentsInChildren<Gun>();
        foreach (Gun gun in guns)
        {
            gun.isActive = true;
            if (gun.powerUpLevelRequirement != 0)
            {
                gun.gameObject.SetActive(false);
            }
        }
    }
    private void Awake()
    {
        // Make the Ship persistent across scenes and prevent duplicates
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // private void OnEnable()
    // {
    //     Debug.Log($"Ship OnEnable called for '{gameObject.name}'");
    // }

    private void OnEnable()
    {
        Debug.Log($"Ship OnEnable called for '{gameObject.name}'");
        // (Re)configure input bindings when enabled
        SetupInputActions();
    }

    private void OnDisable()
    {
        UnbindInputActions();
        Debug.Log($"Ship OnDisable called for '{gameObject.name}'");
    }

    // Unbind and disable existing input action callbacks
    void UnbindInputActions()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
            moveAction.Disable();
            moveAction = null;
        }

        if (attackAction != null)
        {
            attackAction.performed -= OnAttack;
            attackAction.Disable();
            attackAction = null;
        }
    }

    // Configure and enable input actions (safe to call multiple times)
    public void SetupInputActions()
    {
        // Ensure we unbind first to avoid duplicate handlers
        UnbindInputActions();

        if (inputActions == null)
        {
            Debug.LogError("❌ No se asignó el InputActionAsset en el inspector.");
            return;
        }

        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogError("❌ No se encontró el ActionMap 'Player' en el InputActionAsset.");
            return;
        }

        moveAction = playerMap.FindAction("Move");
        if (moveAction == null)
        {
            Debug.LogError("❌ No se encontró la acción 'Move' dentro del ActionMap 'Player'.");
            return;
        }

        moveAction.Enable();
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;

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

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        ControladorSonidos.Instancia.ReproducirSonido("NaveMovimiento");
        // Diagnostic log to confirm input is received
        // (prints every frame while moving; remove or reduce spam later)
        // Use LogFormat to make it easy to filter
        Debug.LogFormat("[Ship] OnMove: {0} (context.phase={1})", moveInput, context.phase);
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Debug.Log("[Ship] OnAttack performed");

        if (guns == null || guns.Length == 0)
        {
            Debug.LogWarning("⚠️ No hay Guns asignadas como hijos del Ship.");
            return;
        }

        foreach (Gun gun in guns)
        {
            if (gun.gameObject.activeSelf)
            {
                gun.Shoot();
            }
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


    void ActivateShield()
    {
        shield.SetActive(true);
    }

    void DeactivateShield()
    {
        shield.SetActive(false);
    }

    bool HasShield()
    {
        return shield.activeSelf;
    }

    void AddGuns()
    {
        powerUpGunLevel++;
        foreach (Gun gun in guns)
        {
            if (gun.powerUpLevelRequirement == powerUpGunLevel)
            {
                gun.gameObject.SetActive(true);
            }
        }
    }

    void IncreaseSpeed()
    {
        moveSpeed *= 2f;
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
            if (HasShield())
            {
                DeactivateShield();
            }
            else
            {
                Destroy(gameObject);
            }
            Destroy(destructable.gameObject);
        }

        PowerUp powerUp = collision.GetComponent<PowerUp>();
        if (powerUp)
        {
            if (powerUp.activateShield)
            {
                ActivateShield();
            }
            if (powerUp.addGuns)
            {
                AddGuns();
            }
            if (powerUp.increaseSpeed)
            {
                IncreaseSpeed();
            }
            Level.instance.AddScore(powerUp.pointValue);
            Destroy(powerUp.gameObject);
        }
    }

}
