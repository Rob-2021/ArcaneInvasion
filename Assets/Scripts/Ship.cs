using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Ship : MonoBehaviour
{
    Vector2 initialPosition;
    public static Ship instance;

    int hits = 3;
    bool invincible = false;
    float invincibleTimer = 0f;
    float invincibleTime = 2f;

    Gun[] guns;

    [Header("Input System")]
    public InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction attackAction;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    float speedMultiplier = 1f;

    private Vector2 moveInput;

    SpriteRenderer spriteRenderer;

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

        initialPosition = transform.position;

        spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
    }

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
        float moveAmount = moveSpeed * speedMultiplier * Time.fixedDeltaTime;

        Vector2 move = moveInput.normalized * moveAmount;
        pos += move;

        // Limitar posición
        pos.x = Mathf.Clamp(pos.x, 1.5f, 16f);
        pos.y = Mathf.Clamp(pos.y, 1f, 9f);

        transform.position = pos;
    }

    private void Update()
    {
        // Handle invincibility timing and sprite blinking every frame
        if (invincible)
        {
            if (invincibleTimer >= invincibleTime)
            {
                invincibleTimer = 0f;
                invincible = false;
                if (spriteRenderer != null)
                    spriteRenderer.enabled = true;
            }
            else
            {
                invincibleTimer += Time.deltaTime;
                if (spriteRenderer != null)
                {
                    // Blink: toggle based on fractional part of timer
                    spriteRenderer.enabled = (Mathf.FloorToInt(invincibleTimer * 10) % 2) == 0;
                }
            }
        }
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
            if (gun.powerUpLevelRequirement <= powerUpGunLevel)
            {
                gun.gameObject.SetActive(true);
            }
            else{
                gun.gameObject.SetActive(false);
            }
        }
    }

    void SetSpeedMultiplier(float mult)
    {
        speedMultiplier = mult;
    }

    void ResetShip()
    {
        transform.position = initialPosition;
        DeactivateShield();
        powerUpGunLevel = -1;
        AddGuns();
        SetSpeedMultiplier(1f);
        hits = 3;
        Level.instance.ResetLevel();
    }

    void Hit(GameObject gameObjectHit)
    {
        if (HasShield())
        {
            DeactivateShield();
        }
        else
        {
            if (!invincible)
            {
                hits--;
                if(hits == 0)
                {
                    ResetShip();
                }
                else
                {
                    invincible = true;
                    invincibleTimer = 0f;
                    if (spriteRenderer != null)
                        spriteRenderer.enabled = false; // start blink sequence
                }
                Destroy(gameObjectHit);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            if (bullet.isEnemy)
            {
                Hit(bullet.gameObject);
            }
        }

        Destructable destructable = collision.GetComponent<Destructable>();
        if (destructable != null)
        {
            Hit(destructable.gameObject);
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
                SetSpeedMultiplier(speedMultiplier + 1);
            }
            Level.instance.AddScore(powerUp.pointValue);
            Destroy(powerUp.gameObject);
        }
    }

}
