using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// =========================================================================================
// ESTE SCRIPT HACE ESTO:
// Controlador principal del jugador en 2D ("PlayerController").
// Se encarga del movimiento horizontal, salto, detección de suelo, animaciones,
// recolección de monedas, colisión con trampas/obstáculos y efectos de sonido.
// 
// ★★★ RESUMEN: LOS 5 BLOQUES MÁS IMPORTANTES DE ESTE ARCHIVO ★★★
// 1. [IMPORTANTE] Detección física de suelo con OverlapCircle en FixedUpdate (evita salto infinito).
// 2. [IMPORTANTE] Sistema de salto condicionado a isGrounded en Update (salto controlado).
// 3. [IMPORTANTE] Movimiento horizontal responsivo con GetAxisRaw en Update (control preciso).
// 4. [IMPORTANTE] Sistema de colisiones en OnTriggerEnter2D (monedas, muerte por pinchos y knockback).
// 5. [IMPORTANTE] Giro del sprite con Mathf.Sign en Update (voltea izquierda/derecha).
// =========================================================================================
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    // =========================================================================================
    // ESTE BLOQUE HACE ESTO: Variables configurables desde el Inspector de Unity
    // Aquí defines valores como la velocidad, fuerza de salto, capas y sonidos sin tocar el código.
    // =========================================================================================

    [Header("--- PARÁMETROS DE MOVIMIENTO Y SALTO ---")]
    // [IMPORTANTE] Este valor define la velocidad horizontal a la que camina el personaje
    [Tooltip("Velocidad de desplazamiento horizontal en unidades por segundo")]
    [SerializeField] private float speed = 2f;

    // [IMPORTANTE] Este valor define con cuánta fuerza salta el personaje hacia arriba
    [Tooltip("Fuerza del impulso vertical que se aplica al saltar")]
    [SerializeField] private float jumpForce = 2f;


    [Header("--- DETECCIÓN FÍSICA DE SUELO ---")]
    // [IMPORTANTE] Este objeto vacío se coloca en los pies del personaje para saber desde dónde medir el suelo
    [Tooltip("Objeto Transform vacío ubicado en los pies del personaje")]
    [SerializeField] private Transform groundCheck;

    // [IMPORTANTE] Radio del círculo invisible para comprobar si tocamos el suelo
    [Tooltip("Radio del círculo de detección para comprobar si pisa el suelo")]
    [SerializeField] private float groundRadius = 0.1f;

    // [IMPORTANTE] Esta máscara le indica al juego qué capas (Layers) se consideran suelo caminable
    [Tooltip("Máscara de capas que define qué objetos del escenario son suelo")]
    [SerializeField] private LayerMask groundLayer;


    [Header("--- INTERFAZ DE USUARIO (UI) ---")]
    // Este elemento de texto muestra en pantalla la cantidad de monedas recolectadas
    [Tooltip("Elemento de texto (TextMeshPro) para mostrar las monedas en pantalla")]
    [SerializeField] private TMP_Text TextCoins;


    [Header("--- EFECTOS DE SONIDO (AUDIO) ---")]
    // Este clip de audio se reproduce cuando el jugador recolecta una moneda
    [Tooltip("Sonido que se reproduce al recoger una moneda")]
    [SerializeField] private AudioClip coinClip;

    // Este clip de audio se reproduce cuando el jugador choca con un barril o bomba
    [Tooltip("Sonido que se reproduce al chocar con un barril o bomba")]
    [SerializeField] private AudioClip barrelClip;


    // =========================================================================================
    // ESTE BLOQUE HACE ESTO: Variables privadas y componentes internos del personaje
    // No aparecen en el Inspector. Guardan el estado interno y las referencias del script.
    // =========================================================================================

    // Componente de físicas 2D (controla la gravedad, fuerzas y velocidad)
    private Rigidbody2D rb2D;

    // Componente de animaciones (controla cuándo corre, salta o queda quieto)
    private Animator animator;

    // Componente emisor de sonido del personaje
    private AudioSource audioSource;

    // Guarda el valor de las teclas de dirección (-1: izquierda, 0: quieto, 1: derecha)
    private float move;

    // [IMPORTANTE] Guarda si el personaje está tocando el suelo (true) o en el aire (false)
    private bool isGrounded;

    // Contador interno de monedas recolectadas
    private int coins = 0;


    // =========================================================================================
    // ESTE BLOQUE HACE ESTO: Método Start()
    // Se ejecuta una sola vez al iniciar la escena.
    // Sirve para obtener y conectar automáticamente los componentes del personaje.
    // =========================================================================================
    void Start()
    {
        // Esta línea obtiene y guarda el componente Rigidbody2D del personaje
        rb2D = GetComponent<Rigidbody2D>();

        // Esta línea obtiene y guarda el componente Animator para manejar animaciones
        animator = GetComponent<Animator>();

        // Esta línea obtiene y guarda el componente AudioSource para reproducir los sonidos
        audioSource = GetComponent<AudioSource>();
    }


    // =========================================================================================
    // ESTE BLOQUE HACE ESTO: Método Update()
    // Se ejecuta en cada fotograma (frame) del juego.
    // Sirve para detectar teclas del jugador y reaccionar de inmediato.
    // =========================================================================================
    void Update()
    {
        // -------------------------------------------------------------------------------------
        // ★★★ [IMPORTANTE: MOVIMIENTO HORIZONTAL RESPONSIVO] ★★★
        // Este bloque hace esto: Lee el teclado horizontal (flechas o teclas A/D)
        // y le aplica velocidad inmediata al Rigidbody2D sin inercia resbaladiza.
        // -------------------------------------------------------------------------------------
        move = Input.GetAxisRaw("Horizontal");
        rb2D.linearVelocity = new Vector2(move * speed, rb2D.linearVelocity.y);

        // -------------------------------------------------------------------------------------
        // ★★★ [IMPORTANTE: GIRO DE ORIENTACIÓN DEL PERSONAJE / FLIP] ★★★
        // Este bloque hace esto: Gira visualmente el sprite según la dirección a la que camina.
        // Cambia la escala en X: 1 mira a la derecha, -1 mira a la izquierda.
        // -------------------------------------------------------------------------------------
        if (move != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);
        }

        // -------------------------------------------------------------------------------------
        // ★★★ [IMPORTANTE: LÓGICA DE SALTO CONDICIONADO A SUELO] ★★★
        // Este bloque hace esto: Solo permite saltar si presionas espacio ("Jump") Y además 'isGrounded' es true.
        // Esto evita saltar en el aire y saltos dobles no permitidos.
        // -------------------------------------------------------------------------------------
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Aplica la fuerza de salto hacia arriba en el eje Y
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
        }

        // -------------------------------------------------------------------------------------
        // Este bloque hace esto: Actualiza las variables del Animator para reproducir animaciones
        // -------------------------------------------------------------------------------------
        if (animator != null)
        {
            // Envía la velocidad horizontal absoluta (0 = quieto/Idle, >0 = correr/Run)
            animator.SetFloat("Speed", Mathf.Abs(move));

            // Envía la velocidad vertical (positivo = subiendo, negativo = cayendo)
            animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);

            // Indica si el personaje está en el suelo o en el aire
            animator.SetBool("IsGrounded", isGrounded);
        }
    }


    // =========================================================================================
    // ESTE BLOQUE HACE ESTO: Método FixedUpdate()
    // Se ejecuta a intervalos de tiempo fijos sincronizados con el motor de físicas de Unity.
    // Es el lugar ideal para comprobaciones físicas como tocar el suelo.
    // =========================================================================================
    void FixedUpdate()
    {
        // -------------------------------------------------------------------------------------
        // ★★★ [IMPORTANTE - EL MÁS CRÍTICO: DETECCIÓN FÍSICA DE SUELO] ★★★
        // Este bloque hace esto: Evita el salto infinito. Comprueba si los pies del personaje tocan
        // el suelo creando un círculo invisible en 'groundCheck' que colisiona con 'groundLayer'.
        // -------------------------------------------------------------------------------------
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        }
    }


    // =========================================================================================
    // ★★★ [IMPORTANTE: SISTEMA DE COLISIONES E INTERACCIONES (OnTriggerEnter2D)] ★★★
    // ESTE BLOQUE HACE ESTO: Se activa cuando el personaje choca con un objeto "Trigger".
    // Gestiona monedas, trampa mortal de pinchos y choque con retroceso (knockback).
    // =========================================================================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // -------------------------------------------------------------------------------------
        // Caso A: Recolectar una moneda (objeto con etiqueta "Coin")
        // -------------------------------------------------------------------------------------
        if (collision.CompareTag("Coin"))
        {
            // Reproduce el sonido de recolectar moneda
            audioSource.PlayOneShot(coinClip);

            // Destruye y elimina la moneda del escenario
            Destroy(collision.gameObject);

            // Aumenta en 1 el contador de monedas
            coins++;

            // Actualiza el texto en la interfaz (UI) con la nueva cantidad de monedas
            if (TextCoins != null)
            {
                TextCoins.text = coins.ToString();
            }
        }

        // -------------------------------------------------------------------------------------
        // Caso B: Trampa mortal de pinchos (objeto con etiqueta "Spikes")
        // -------------------------------------------------------------------------------------
        if (collision.CompareTag("Spikes"))
        {
            // Reinicia la escena activa inmediatamente al morir
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // -------------------------------------------------------------------------------------
        // Caso C: Impacto con obstáculo destruible (etiqueta "Barrel" o "Bomb")
        // -------------------------------------------------------------------------------------
        if (collision.CompareTag("Barrel") || collision.CompareTag("Bomb"))
        {
            // Reproduce el sonido de rotura o explosión
            audioSource.PlayOneShot(barrelClip);

            // 1. Calcula la dirección contraria para empujar al jugador lejos del obstáculo
            Vector2 knockbackDir = (rb2D.position - (Vector2)collision.transform.position).normalized;

            // Frena la velocidad previa del jugador para que el empuje sea limpio
            rb2D.linearVelocity = Vector2.zero;

            // Aplica la fuerza de empuje (knockback) tipo impulso
            rb2D.AddForce(knockbackDir * 4f, ForceMode2D.Impulse);

            // 2. Desactiva los colliders del obstáculo para no golpearlo dos veces
            BoxCollider2D[] colliders = collision.gameObject.GetComponents<BoxCollider2D>();
            foreach (BoxCollider2D col in colliders)
            {
                col.enabled = false;
            }

            // 3. Activa la animación de explosión en el objeto obstáculo (si tiene Animator)
            if (collision.TryGetComponent(out Animator obstacleAnim))
            {
                obstacleAnim.enabled = true;
            }

            // 4. Destruye el objeto obstáculo después de 0.5 segundos (para que se vea la animación)
            Destroy(collision.gameObject, 0.5f);
        }
    }


    // =========================================================================================
    // ESTE BLOQUE HACE ESTO: Método OnDrawGizmosSelected()
    // Dibuja guías visuales en la ventana Scene de Unity para facilitar el ajuste del radio de suelo.
    // Solo es visible para el desarrollador en el editor, no en el juego final.
    // =========================================================================================
    private void OnDrawGizmosSelected()
    {
        // Dibuja un círculo verde en los pies del personaje si 'groundCheck' está asignado
        if (groundCheck != null)
        {
            // Asigna el color verde a la guía
            Gizmos.color = Color.green;

            // Dibuja la circunferencia con el radio de detección configurado
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
