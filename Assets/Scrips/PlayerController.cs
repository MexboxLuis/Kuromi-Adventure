using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 5f;
    public float laneChangeSpeed = 15f;
    public float jumpForce = 9f;

    [Header("Lane Settings")]
    public float laneDistance = 2.5f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.3f;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private bool isGrounded;
    private int currentLane = 0;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    public float minSwipeDistance = 50f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        // En Update solo nos preocupamos de LEER el input y el estado
        CheckIfGrounded();
        HandleInput();
    }

    void FixedUpdate()
    {
        // En FixedUpdate APLICAMOS todo el movimiento f�sico

        // --- L�GICA UNIFICADA ---
        // 1. Calculamos la velocidad lateral (cambio de carril)
        float targetZPosition = currentLane * laneDistance;
        float zVelocity = (targetZPosition - transform.position.z) * laneChangeSpeed;

        // 2. Creamos el vector de velocidad final
        // X = Velocidad constante hacia adelante
        // Y = Mantenemos la velocidad vertical actual (para saltos y gravedad)
        // Z = Le damos la velocidad de cambio de carril que calculamos
        Vector3 targetVelocity = new Vector3(forwardSpeed, rb.velocity.y, zVelocity);

        // 3. Aplicamos esta velocidad al Rigidbody
        rb.velocity = targetVelocity;
    }

    // En PlayerController.cs, reemplaza la función HandleInput

    private void HandleInput()
    {
        // --- MANTENEMOS LOS CONTROLES DE TECLADO PARA PROBAR EN EL EDITOR ---
#if UNITY_EDITOR
        // Izquierda
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) { ChangeLane(1); }
        // Derecha
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) { ChangeLane(-1); }
        // Saltar
        if (Input.GetButtonDown("Jump") && isGrounded) { rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); }
#endif

        // --- LÓGICA DE SWIPE PARA MÓVILES ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Cuando el dedo toca la pantalla
            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }
            // Cuando el dedo se levanta de la pantalla
            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                ProcessSwipe();
            }
        }
    }

    private void ChangeLane(int direction)
    {
        currentLane = Mathf.Clamp(currentLane + direction, -1, 1);
    }

    // �YA NO NECESITAMOS HandleLaneChanging()! La hemos movido a FixedUpdate.
    // Puedes borrar esa funci�n.

    private void CheckIfGrounded()
    {
        Vector3 rayStartPoint = transform.position;
        float rayDistance = (capsuleCollider.height / 2) + groundCheckDistance;
        isGrounded = Physics.Raycast(rayStartPoint, Vector3.down, rayDistance, groundLayer);
        Debug.DrawRay(rayStartPoint, Vector3.down * rayDistance, isGrounded ? Color.green : Color.red);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si chocamos con un objeto que tenga la etiqueta "Obstacle"...
        if (collision.gameObject.CompareTag("Platform"))
        {
            // ...buscamos el GameManager en la escena y le decimos que hemos muerto.
            FindObjectOfType<GameManager>().PlayerDied();

            // Desactivamos el script para no poder movernos m�s
            // this.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si entramos en un trigger con la etiqueta "Goal"...
        if (other.CompareTag("Goal"))
        {
            // ...le avisamos al GameManager que hemos completado el nivel.
            FindObjectOfType<GameManager>().LevelCompleted();

            // Desactivamos el script para no poder movernos m�s mientras se cambia de nivel.
            // this.enabled = false;
        }
    }

    public void ResetPlayerState()
    {
        // Reseteamos el carril al central (0)
        currentLane = 0;

        // Tambi�n es una buena pr�ctica resetear la velocidad del Rigidbody aqu�
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero; // Detiene cualquier rotaci�n
        // transform.position = new Vector3(transform.position.x, transform.position.y, 0);

    }
    
    private void ProcessSwipe()
{
    // Calculamos la distancia del swipe
    float swipeDistance = Vector2.Distance(startTouchPosition, endTouchPosition);

    // Si la distancia es mayor que nuestro mínimo...
    if (swipeDistance > minSwipeDistance)
    {
        // Calculamos el vector del swipe
        Vector2 swipeDirection = endTouchPosition - startTouchPosition;
        
        // Comprobamos si el swipe fue más horizontal que vertical
        if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
        {
            // Swipe Horizontal
            if (swipeDirection.x > 0)
            {
                // Swipe a la Derecha
                ChangeLane(-1);
            }
            else
            {
                // Swipe a la Izquierda
                ChangeLane(1);
            }
        }
        else
        {
            // Swipe Vertical
            if (swipeDirection.y > 0 && isGrounded)
            {
                // Swipe hacia Arriba (Salto)
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            // (Aquí podrías añadir lógica para un swipe hacia abajo si quisieras)
        }
    }
}

}