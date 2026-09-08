using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// =========================================================================================
/// PROYECTO: Hito 1 - Prototipo Funcional 2D y Game Feel ("El Laboratorio de Movimiento")
/// SCRIPT: PlayerController.cs
/// DESCRIPCIÓN: Controlador integral del personaje jugador en 2D.
/// Gestiona:
///   1. Movimiento horizontal responsivo sin inercia artificial (GetAxisRaw).
///   2. Sistema de salto condicional a contacto con suelo (prevención de salto infinito).
///   3. Detección física de suelo mediante OverlapCircle y capas (LayerMask).
///   4. Sincronización de animaciones con estados físicos y velocidades.
///   5. Sistema de colisiones e interacciones (Monedas, Pinchos, Bombas/Obstáculos con knockback).
/// =========================================================================================
public class PlayerController : MonoBehaviour
{
    // =========================================================================================
    // BLOQUE 1: VARIABLES SERIALIZADAS CONFIGURABLES EN EL INSPECTOR DE UNITY
    // =========================================================================================

    [Header("--- PARÁMETROS DE MOVIMIENTO Y GAME FEEL ---")]
    [Tooltip("Velocidad de desplazamiento horizontal en unidades de Unity por segundo")]
    [SerializeField] private float speed = 2f;

    [Tooltip("Fuerza del impulso vertical que se aplica al Rigidbody2D al saltar")]
    [SerializeField] private float jumpForce = 4f;

    [Header("--- DETECCIÓN FÍSICA DE SUELO ---")]
    [Tooltip("Objeto Transform vacío ubicado en la base/pies del personaje")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Radio del círculo imaginario de detección para comprobar si pisa el suelo")]
    [SerializeField] private float groundRadius = 0.1f;

    [Tooltip("Máscara de capas que define qué objetos del escenario se consideran suelo caminable")]
    [SerializeField] private LayerMask groundLayer;

    [Header("--- INTERFAZ DE USUARIO (UI) ---")]
    [Tooltip("Elemento de texto en pantalla (TextMeshPro) para mostrar la cantidad de monedas recolectadas")]
    [SerializeField] private TMP_Text TextCoins;


    // =========================================================================================
    // BLOQUE 2: REFERENCIAS A COMPONENTES INTERNOS Y VARIABLES DE CONTROL PRIVADAS
    // =========================================================================================

    private Rigidbody2D rb2D;       // Componente del motor de física 2D que controla la velocidad y gravedad
    private Animator animator;      // Componente que controla la máquina de estados de animaciones (Idle, Run, Jump, etc.)

    private float move;             // Almacena el valor de la tecla presionada (-1: izquierda, 0: quieto, 1: derecha)
    private bool isGrounded;        // Bandera booleana (true/false) que indica si el jugador está tocando el suelo
    private int coins = 0;          // Contador interno de monedas recolectadas durante la partida


    // =========================================================================================
    // BLOQUE 3: MÉTODO START - INICIALIZACIÓN DE COMPONENTES
    // Se ejecuta una sola vez al inicio cuando el objeto se activa en la escena.
    // =========================================================================================
    void Start()
    {
        // Obtenemos y enlazamos automáticamente los componentes adjuntos al GameObject
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    // =========================================================================================
    // BLOQUE 4: MÉTODO UPDATE - ENTRADA DE USUARIO Y LÓGICA POR FRAME
    // Se ejecuta en cada fotograma del juego. Ideal para capturar teclas de forma instantánea.
    // =========================================================================================
    void Update()
    {
        // -------------------------------------------------------------------------------------
        // PASO 4.1: LECTURA DEL TECLADO / INPUT HORIZONTAL
        // Usamos GetAxisRaw para obtener valores exactos (-1, 0, 1) sin suavizado ni retardo.
        // -------------------------------------------------------------------------------------
        move = Input.GetAxisRaw("Horizontal");

        // -------------------------------------------------------------------------------------
        // PASO 4.2: APLICACIÓN DE VELOCIDAD HORIZONTAL
        // Modificamos la velocidad horizontal (X) multiplicando por 'speed' y conservamos la vertical (Y).
        // -------------------------------------------------------------------------------------
        rb2D.linearVelocity = new Vector2(move * speed, rb2D.linearVelocity.y);

        // -------------------------------------------------------------------------------------
        // PASO 4.3: GIRO DE ORIENTACIÓN DEL SPRITE (FLIP HORIZONTAL)
        // Si el personaje se mueve, multiplicamos la escala X por su signo (+1 derecha, -1 izquierda).
        // -------------------------------------------------------------------------------------
        if (move != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);
        }

        // -------------------------------------------------------------------------------------
        // PASO 4.4: LÓGICA DE SALTO
        // El salto SOLO se permite si se pulsa la tecla ("Jump" / Barra espaciadora) Y 'isGrounded' es true.
        // Esto evita saltos dobles o saltos infinitos en el aire.
        // -------------------------------------------------------------------------------------
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
        }

        // -------------------------------------------------------------------------------------
        // PASO 4.5: ACTUALIZACIÓN DE PARÁMETROS DEL ANIMATOR
        // Enviamos los datos físicos al Animator para cambiar entre animaciones Idle, Run, Jump y Fall.
        // -------------------------------------------------------------------------------------
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(move));                       // Velocidad horizontal absoluta
            animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);      // Velocidad vertical (subiendo o cayendo)
            animator.SetBool("IsGrounded", isGrounded);                        // ¿Está en el suelo?
        }
    }


    // =========================================================================================
    // BLOQUE 5: MÉTODO FIXEDUPDATE - FÍSICAS Y DETECCIÓN DE SUELO
    // Se ejecuta a intervalos fijos de tiempo sincronizados con el motor de físicas de Unity.
    // =========================================================================================
    void FixedUpdate()
    {
        // -------------------------------------------------------------------------------------
        // DETECCIÓN DE CONTACTO CON EL SUELO
        // Creamos un círculo de colisión en la posición de 'groundCheck' con radio 'groundRadius'.
        // Si dicho círculo choca con cualquier collider que pertenezca a 'groundLayer', retorna true.
        // -------------------------------------------------------------------------------------
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        }
    }


    // =========================================================================================
    // BLOQUE 6: MÉTODO ONTRIGGERENTER2D - INTERACCIONES Y COLISIONES
    // Se activa automáticamente cuando un collider Trigger entra en contacto con el jugador.
    // =========================================================================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // -------------------------------------------------------------------------------------
        // CASO 6.1: RECOLECCIÓN DE MONEDA (Tag: "Coin")
        // Destruye la moneda recogida, incrementa el contador y actualiza el texto de la UI.
        // -------------------------------------------------------------------------------------
        if (collision.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            coins++;
            if (TextCoins != null)
            {
                TextCoins.text = coins.ToString();
            }
        }

        // -------------------------------------------------------------------------------------
        // CASO 6.2: TRAMPA MORTAL DE PINCHOS (Tag: "Spikes")
        // Si el jugador pisa los pinchos, se reinicia la escena activa inmediatamente.
        // -------------------------------------------------------------------------------------
        if (collision.CompareTag("Spikes"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // -------------------------------------------------------------------------------------
        // CASO 6.3: OBSTÁCULO DESTRUIBLE (Tag: "Barrel" o "Bomb")
        // Aplica retroceso (knockback) al jugador, desactiva colliders del obstáculo,
        // activa su animación de explosión y destruye el objeto tras 0.5 segundos.
        // -------------------------------------------------------------------------------------
        if (collision.CompareTag("Barrel") || collision.CompareTag("Bomb"))
        {
            // 1. Calculamos la dirección del empuje alejando al jugador del obstáculo
            Vector2 knockbackDir = (rb2D.position - (Vector2)collision.transform.position).normalized;
            rb2D.linearVelocity = Vector2.zero;
            rb2D.AddForce(knockbackDir * 3f, ForceMode2D.Impulse);

            // 2. Desactivamos las colisiones del objeto para no chocar dos veces
            BoxCollider2D[] colliders = collision.gameObject.GetComponents<BoxCollider2D>();
            foreach (BoxCollider2D col in colliders)
            {
                col.enabled = false;
            }

            // 3. Activamos el Animator del obstáculo para reproducir la animación de explosión
            if (collision.TryGetComponent(out Animator obstacleAnim))
            {
                obstacleAnim.enabled = true;
            }

            // 4. Destruimos el GameObject del obstáculo tras medio segundo
            Destroy(collision.gameObject, 0.5f);
        }
    }


    // =========================================================================================
    // BLOQUE 7: MÉTODO ONDRAWGIZMOSSELECTED - DEPURACIÓN VISUAL EN EL EDITOR DE UNITY
    // Dibuja guías visuales en la ventana de 'Scene' para facilitar el ajuste del radio de suelo.
    // =========================================================================================
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;                                         // Color verde para la guía
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);           // Dibuja la esfera de detección
        }
    }
}
