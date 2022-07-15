using System;
using System.Linq;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Vector3 WallColiderOffset;
    public float Speed = 1;
    public float JumpForce = 150;
    public bool EnableDebug = false;
    private Rigidbody2D Rigidbody2D;
    private Animator Animator;
    private float Horizontal;
    private bool Grounded;
    private float CurrentTime;
    private bool PlayerIsRunning => Horizontal != 0.0f;
    [SerializeField] private int SecondsFireOnHold = 3;
    [SerializeField] private LayerMask layers;


    public static PlayerMovement Instance { get; private set; }
    public PlayerMovement()
    {

    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public bool JumpKey => Input.GetKeyDown(KeyCode.Space);
    [SerializeField] private GameObject _arm;
    private float? MouseDownTime;
    private float? MouseUpTime;

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
        HandleFireCancelation();
        SetGlobalValues();
        HandleUserInputs();
        if (EnableDebug)
            SetupDebug();
    }

    private void HandleFireCancelation()
    {
        if (MouseDownTime != null && (CurrentTime - MouseDownTime) > SecondsFireOnHold)
        {
            Debug.Log("Fire cancelled.");
            MouseDownTime = null;
        }
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
        CurrentTime = Time.fixedTime;
    }
    private void SetPlayerVelocity()
    {

        var playerLeftPositionForWallColition = new Vector3(transform.position.x - WallColiderOffset.x, transform.position.y + WallColiderOffset.y, transform.position.z + WallColiderOffset.z);
        var playerRightPositionForWallColition = transform.position + WallColiderOffset;
        var leftColliding = Physics2D.Raycast(playerLeftPositionForWallColition, Vector3.left, 0.1f, layers);
        var rightColliding = Physics2D.Raycast(playerRightPositionForWallColition, Vector3.right, 0.1f, layers);        
        if (Horizontal != 0 && ((!rightColliding && Horizontal > 0) || (!leftColliding && Horizontal < 0)))
            Rigidbody2D.velocity = new Vector2(Horizontal * Speed, Rigidbody2D.velocity.y);
    }
    private void HandleUserInputs()
    {
        HandleCharacterFacing();
        HandleCharacterAnimations();
        Rotation();
        HandleMouseLeftClickDown();
        if (JumpKey && Grounded)
        {
            Jump();
        }
    }

    private void HandleMouseLeftClickDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseDownTime = CurrentTime;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (MouseDownTime != null)
            {
                MouseUpTime = CurrentTime;
                HandleFireOnMouse();
            }
        }
    }

    private void HandleCharacterFacing()
    {
        var playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
        transform.localScale = new Vector3(Input.mousePosition.x < playerScreenPoint.x ? -1.0f : 1.0f, transform.localScale.y, transform.localScale.z);
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
    private void Rotation()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _arm.transform.rotation = Quaternion.LookRotation(Vector3.forward, mousePosition - _arm.transform.position);
    }

    private void HandleFireOnMouse()
    {
        float shotPower = (MouseUpTime - MouseDownTime) / SecondsFireOnHold * 100 ?? 0f;
        Debug.Log("Shot power: " + shotPower + "%");

        Rigidbody2D.velocity = Vector3.zero;
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 direction = (Vector3)(Camera.main.WorldToScreenPoint(GameObject.FindGameObjectsWithTag("CannonFire").First().transform.position) - screenPoint);
        direction.Normalize();
        Rigidbody2D.AddForce(-1 * direction * (5 * shotPower / 100), ForceMode2D.Impulse);


        MouseDownTime = null;
    }
}
