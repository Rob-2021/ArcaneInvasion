using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    public GameObject explosion;

    bool canBeDestroyed = false;
    public int scoreValue = 100;

    // Start is called before the first frame update
    void Start()
    {
        Level.instance.AddDestructable();
    }

    // Update is called once per frame
    void Update()
    {

        if(transform.position.x < -10)
        {
            //Level.instance.RemoveDestructable();
            DestroyDestructable();
        }

        if (transform.position.x < 8.0f && !canBeDestroyed)
        {
            canBeDestroyed = true;
            Gun[] guns = transform.GetComponentsInChildren<Gun>();
            foreach (Gun gun in guns)
            {
                gun.isActive = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canBeDestroyed)
        {
            return;
        }
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            if (!bullet.isEnemy)
            {
                Level.instance.AddScore(scoreValue);
                DestroyDestructable();
                Destroy(bullet.gameObject);
            }
        }
    }

    void DestroyDestructable()
    {
        // Try to spawn the explosion at the visual center of the sprite (if present)
        Vector3 spawnPos = transform.position;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            spawnPos = sr.bounds.center;
        }

        Instantiate(explosion, spawnPos, Quaternion.identity);

        Level.instance.RemoveDestructable();
        Destroy(gameObject);
    }

}
