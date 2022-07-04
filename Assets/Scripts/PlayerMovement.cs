using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float Speed = 1;
    public float JumpForce = 150;
    public bool EnableDebug = false;
    private Rigidbody2D Rigidbody2D;
    private Animator Animator;
    private float Horizontal;
    private bool Grounded;
    private bool PlayerIsRunning => Horizontal != 0.0f;

    public bool JumpKey => Input.GetKeyDown(KeyCode.Space);

    void Start()
    {
        SetInstanceReferences();
    }
    private void SetInstanceReferences()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
    }
    void Update()
    {
        SetGlobalValues();
        HandleUserInputs();
        if (EnableDebug)
            SetupDebug();
    }
    private void FixedUpdate()
    {
        SetPlayerVelocity();
    }
    #region Custom Methods

    private void SetGlobalValues()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");
        Grounded = Physics2D.Raycast(transform.position, Vector3.down, 0.1f);
    }
    private void SetPlayerVelocity()
    {
        Rigidbody2D.velocity = new Vector2(Horizontal * Speed, Rigidbody2D.velocity.y);
    }
    private void HandleUserInputs()
    {
        HandleCharacterFacing();
        HandleCharacterAnimations();
        if (JumpKey && Grounded)
        {
            Jump();
        }
    }
    private void HandleCharacterFacing()
    {
        if (PlayerIsRunning) transform.localScale = new Vector3(Horizontal < 0 ? -1.0f : 1.0f, transform.localScale.y, transform.localScale.z);
    }
    private void HandleCharacterAnimations()
    {
        Animator.SetBool("running", PlayerIsRunning);
    }
    private void Jump()
    {
        Rigidbody2D.AddForce(Vector2.up * JumpForce);
    }
    private void SetupDebug()
    {
        Debug.DrawRay(transform.position, Vector3.down * 0.1f, Color.red);
    }
    #endregion
}