using UnityEngine;
using UnityEngine.InputSystem;

public class Movimento : MonoBehaviour
{
    private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed * Time.deltaTime;
    }

    public void OnMove(InputValue value)
    {
    moveInput = value.Get<Vector2>();
    }
    
}
