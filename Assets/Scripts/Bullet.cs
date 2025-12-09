using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public Vector2 direction = new Vector2(1, 0);
    public float speed = 20f;

    public Vector2 velocity;
    public float lifeTime = 3f;

    public bool isEnemy = false;

    // Start is called before the first frame update
    void Start()
    {
        // Si la bala se usa por instanciación directa (sin pool), la destruimos tras lifeTime
        if (BulletPool.Instance == null)
            Destroy(gameObject, lifeTime);

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Si usamos pool, programar el retorno tras lifeTime
        if (BulletPool.Instance != null)
        {
            CancelInvoke(nameof(ReturnToPool));
            Invoke(nameof(ReturnToPool), lifeTime);
        }
    }

    private void OnDisable()
    {
        // Cancelar cualquier invoke pendiente al desactivar
        CancelInvoke(nameof(ReturnToPool));
    }

    private void ReturnToPool()
    {
        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.Despawn(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Mantener compatibilidad: actualizar velocity en Update por si la dirección cambia.
        velocity = direction.normalized * speed;
    }
    
    private void FixedUpdate()
    {
        Vector2 pos = transform.position;

        pos += velocity * Time.fixedDeltaTime;

        transform.position = pos;
    }

    private void OnValidate()
    {
        // Permite ver movimiento en editor si se cambia la dirección/speed
        velocity = direction.normalized * speed;
    }

    /// <summary>
    /// Ajusta el tiempo de vida de la bala (útil para pooling).
    /// </summary>
    public void SetLifeTime(float t)
    {
        lifeTime = t;
    }
}
