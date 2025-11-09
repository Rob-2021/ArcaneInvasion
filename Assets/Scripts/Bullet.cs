using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public Vector2 direction = new Vector2(1, 0);
    public float speed = 20f;

    public Vector2 velocity;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 3);
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
}
