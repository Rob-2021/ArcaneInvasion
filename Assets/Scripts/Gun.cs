using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{

    public Bullet bullet;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Instancia un Bullet a partir de la referencia al prefab (tipo Bullet).
    // También aseguramos la posición/z/escala básica y asignamos dirección.
    public void Shoot()
    {
        if (bullet == null)
        {
            Debug.LogWarning("⚠️ Bullet prefab no asignado en Gun.");
            return;
        }

        // Preparar posición de spawn y forzar Z en 0 para que quede en la vista de la cámara 2D.
        Vector3 spawnPos = transform.position;
        spawnPos.z = 0f;

        // Instanciar como componente Bullet (preserva la referencia al script)
        Bullet b = Instantiate(bullet, spawnPos, Quaternion.identity);

        // Asegurar que el GameObject está activo y con escala correcta
        b.gameObject.SetActive(true);
        b.transform.localScale = Vector3.one;

        // Establecer dirección por defecto tomando el 'right' local de la gun
        // Convertimos explícitamente a Vector2 para evitar ambigüedades.
        b.direction = (Vector2)transform.right;

        // Asegurar que el SpriteRenderer (si existe) está activo y en una orden visible.
        var sr = b.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            // Opcional: ajustar sortingOrder si las balas quedan detrás de otros sprites.
            // sr.sortingOrder = 10;
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró un SpriteRenderer en el prefab Bullet. Asegúrate de que el prefab tiene un renderer para ser visible.");
        }
    }
}
