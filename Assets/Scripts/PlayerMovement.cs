using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables definition
    [SerializeField] private int SecondsFireOnHold = 3;
    [SerializeField] private LayerMask layers;
    [SerializeField] private GameObject _arm;
    public Vector3 WallColiderOffset;
    public float Speed = 1;
    public float JumpForce = 150;
    public bool EnableDebug = false;
    public static PlayerMovement Instance { get; private set; }
    public bool JumpKey => Input.GetKeyDown(KeyCode.Space);
    private Rigidbody2D Rigidbody2D;
    private Animator Animator;
    private float Horizontal;
    private bool _grounded;
    private bool affectsRecoil = false;

     public bool Grounded
    {
        get
        {
            return _grounded;
        }
        private set
        {
            _grounded = value;
        }
    }
    private float CurrentTime;
    private bool PlayerIsRunning => Horizontal != 0.0f;
    private float? MouseDownTime;
    private float? MouseUpTime;
    #endregion
    public PlayerMovement()
    {
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    void Start()
    {
        SetInstanceReferences();
    }
    void Update()
    {
        HandleFireCancelation();
        SetGlobalValues();
        HandleUserInputs();
        if (EnableDebug)
            SetupDebug();
    }
    private void FixedUpdate()
    {
        SetPlayerVelocity();        
    }
    private void LateUpdate()
    {
    }
    #region Custom Methods
    private void SetGlobalValues()
    {
        //Se toma el valor de entrada en el eje horizontal y se almacena en la variable global
        Horizontal = Input.GetAxisRaw("Horizontal");
        //Setea la bandera como verdadera cuando hay colisi�n en la parte inferior, tomando como referencia la posici�n del jugador
        Grounded = Physics2D.Raycast(transform.position, Vector3.down, 0.1f);
        //Setea la variable global que es utilizada para el calculo de poder de disparo
        CurrentTime = Time.fixedTime;
    }
    private void SetPlayerVelocity()
    {
        //Variable para que se utilizar� para detectar colisi�n a la izquierda del jugador, utiliza el objeto WallColiderOffset para dar entrada el margen desde el editor de unity
        var playerLeftPositionForWallColition = new Vector3(transform.position.x - WallColiderOffset.x, transform.position.y + WallColiderOffset.y, transform.position.z + WallColiderOffset.z);
        //Variable para que se utilizar� para detectar colisi�n a la deracha del jugador, utiliza el objeto WallColiderOffset para dar entrada el margen desde el editor de unity
        var playerRightPositionForWallColition = transform.position + WallColiderOffset;
        //Setea las banderas de colisi�n izquierda y derecha
        var leftColliding = Physics2D.Raycast(playerLeftPositionForWallColition, Vector3.left, 0.1f, layers);
        var rightColliding = Physics2D.Raycast(playerRightPositionForWallColition, Vector3.right, 0.1f, layers);
        //Se aplica velocidad al jugador si la entrada horizontal es diferente a 0 y el jugador no tiene colisi�n a la derecha e izquierda (eso para arreglar el problema de pegarse en la pared)
        if (Horizontal != 0 && ((!rightColliding && Horizontal > 0) || (!leftColliding && Horizontal < 0)))
            Rigidbody2D.velocity = new Vector2(Horizontal * Speed, Rigidbody2D.velocity.y);
    }
    private void HandleUserInputs()
    {
        //Orquesta los llamados a los metodos, las descripciones son claras de que hace cada m�todo.
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
            //Seteo de variable para almacenar el momento en el que el usuario presion� el click izquierdo, esto es utilizado para calcular el poder del disparo
            MouseDownTime = CurrentTime;

            if (!affectsRecoil && Grounded)
            {
                affectsRecoil = true;
                Debug.Log("Setting affects recoil as true");
            }

        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Verificamos que el tiempo de presion tenga valor, ya que el mismo se limpia automaticamente al dejar mucho tiempo presionado el click, esto con el fin
            //de dar la funcionalidad de sobrecarga en el arma
            if (MouseDownTime != null)
            {
                //Seteo de variable para almacenar el momento en el que el usuario solt� el click izquierdo, esto es utilizado para calcular el poder del disparo
                MouseUpTime = CurrentTime;
                HandleFireOnMouse();
            }
        }
    }
    private void HandleCharacterFacing()
    {
        //Metodo utilizado para flipear el objeto jugador, basado en su posici�n de la c�mara con relaci�n en la posici�n del mouse.
        var playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
        transform.localScale = new Vector3(Input.mousePosition.x < playerScreenPoint.x ? -1.0f : 1.0f, transform.localScale.y, transform.localScale.z);
    }
    private void HandleCharacterAnimations()
    {
        //Setea la variable running del animator del player como verdadera cuando se est� en movimiento
        Animator.SetBool("running", PlayerIsRunning);
    }
    private void Jump()
    {
        //Agrega una fuerza en direcci�n vertical ascendente
        Rigidbody2D.AddForce(Vector2.up * JumpForce);
    }
    private void SetupDebug()
    {
        //Dibuja un rayo hacia abajo con relaci�n a la posici�n del personaje
        Debug.DrawRay(transform.position, Vector3.down * 0.1f, Color.red);
    }
    private void Rotation()
    {
        //Rota el arma en relaci�n con la posici�n del mouse
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _arm.transform.rotation = Quaternion.LookRotation(Vector3.forward, mousePosition - _arm.transform.position);
    }
    private void HandleFireOnMouse()
    {
        //Valor de poder del disparo (se multiplica por 100 para que no sea porcentual al mostrar el mensaje en consola)
        float shotPower = (MouseUpTime - MouseDownTime) / SecondsFireOnHold * 100 ?? 0f;
        Debug.Log("Shot power: " + shotPower + "%");

        //Condicionamos a que el poder del disparo sea mayor a 15 para que tenga efecto de retroceso
        if (shotPower > 15 && affectsRecoil)
        {
            affectsRecoil = false;
            Debug.Log("Setting affects recoil as false");
            Rigidbody2D.velocity = Vector3.zero;
            //Obtenemos la posici�n del jugador con relaci�n a la c�mara
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            //Obtenemos la posici�n del ca�on con relaci�n a la c�mara
            Vector3 cannonPosition = Camera.main.WorldToScreenPoint(GameObject.FindGameObjectsWithTag("CannonFire").First().transform.position);
            //Restamos las posiciones para obtener la direcci�n del disparo
            Vector3 direction = (Vector3)(cannonPosition - screenPoint);
            //Mantiene la direccion del vector pero con longitudes de 1 o 0, ejemplo  Vector3(0, 0, 5)  pasar�a a  Vector3(0, 0, 1) 
            direction.Normalize();
            //Se agrega una fuerza en la direcci�n contraria (*-1) con el efecto de impulso
            Rigidbody2D.AddForce(-1 * direction * (5 * shotPower / 100), ForceMode2D.Impulse);
        }
        //Se limpia la variable de cuando el mouse fue presionado
        MouseDownTime = null;
    }
    private void SetInstanceReferences()
    {
        //Inicializa las variables obteniendo la referencia del componente.
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
    }
    private void HandleFireCancelation()
    {
        //Si el tiempo en el que el mouse fue presionado tiene valor, verificamos que el tiempo que lleva presionado sea menor o igual, de lo contrario, cancela el fuego.
        if (MouseDownTime != null && (CurrentTime - MouseDownTime) > SecondsFireOnHold)
        {
            Debug.Log("Fire cancelled.");
            MouseDownTime = null;
        }
    }
    #endregion
}
