using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{

    public Bullet bullet;

    public bool autoShoot = false;
    public float shootIntervalSeconds = 0.5f;
    public float shootDelaySeconds = 2.0f;
    float shootTimer = 0f;
    float delayTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (autoShoot)
        {
            if(delayTimer >= shootDelaySeconds)
            {
                if (shootTimer >= shootIntervalSeconds)
                {
                    Shoot();
                    shootTimer = 0f;
                }
                else
                {
                    shootTimer += Time.deltaTime;
                }
            }
            else
            {
                delayTimer += Time.deltaTime;
            }
        }
    }

    // Instancia un Bullet a partir de la referencia al prefab (tipo Bullet).
    // También aseguramos la posición/z/escala básica y asignamos dirección.
    public void Shoot()
    {

        ControladorSonidos.Instancia.ReproducirSonido("NaveLaser");

        if (bullet == null)
        {
            Debug.LogWarning("⚠️ Bullet prefab no asignado en Gun.");
            return;
        }

        // Preparar posición de spawn y forzar Z en 0 para que quede en la vista de la cámara 2D.
        Vector3 spawnPos = transform.position;
        spawnPos.z = 0f;

        // Intentar usar el pool si existe
        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.SpawnBullet(spawnPos, (Vector2)transform.right);
            return;
        }

        // Fallback: instanciación directa si no hay pool (compatibilidad)
        Bullet b = Instantiate(bullet, spawnPos, Quaternion.identity);
        b.gameObject.SetActive(true);
        b.transform.localScale = Vector3.one;
        b.direction = (Vector2)transform.right;

        var sr = b.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.enabled = true;
        else
            Debug.LogWarning("⚠️ No se encontró un SpriteRenderer en el prefab Bullet. Asegúrate de que el prefab tiene un renderer para ser visible.");
    }
}
