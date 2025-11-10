using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [Header("Pool Settings")]
    public Bullet bulletPrefab;
    public int initialSize = 20;

    private readonly List<Bullet> pool = new List<Bullet>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Preallocate
        for (int i = 0; i < initialSize; i++)
        {
            var b = CreateNewBullet();
            b.gameObject.SetActive(false);
            pool.Add(b);
        }
    }

    private Bullet CreateNewBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("BulletPool: no se asignó bulletPrefab en el inspector.");
            return null;
        }

        Bullet b = Instantiate(bulletPrefab, transform);
        b.gameObject.SetActive(false);
        return b;
    }

    public Bullet SpawnBullet(Vector3 position, Vector2 direction, float speed = -1f, float lifeTime = -1f)
    {
        Bullet b = null;

        // Buscar un inactivo
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] == null) continue;
            if (!pool[i].gameObject.activeInHierarchy)
            {
                b = pool[i];
                break;
            }
        }

        if (b == null)
        {
            b = CreateNewBullet();
            pool.Add(b);
        }

        if (b == null)
            return null;

        b.transform.position = position;
        b.transform.rotation = Quaternion.identity;
        b.transform.localScale = Vector3.one;
        b.direction = direction.normalized;
    if (speed > 0f) b.speed = speed;
    if (lifeTime > 0f) b.SetLifeTime(lifeTime);

        b.gameObject.SetActive(true);
        return b;
    }

    public void Despawn(Bullet b)
    {
        if (b == null) return;
        // Dejarlo inactivo para reutilizar
        b.gameObject.SetActive(false);
        // opcional: resetear transform/velocidad
        b.velocity = Vector2.zero;
    }
}
