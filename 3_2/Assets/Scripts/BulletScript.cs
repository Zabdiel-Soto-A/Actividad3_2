using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Rigidbody2D Rigidbody2D;
    public float Speed;
    private Vector3 Direction;
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = Direction * Speed;
    }
    public void SetDirection(Vector3 direction)
    {
        Direction = direction;
    }
    public void DestroyBullet()
    {
        Destroy(gameObject);
    }
    void Update()
    {
        Rigidbody2D.linearVelocity = Vector2.right * Speed;
    }
}
