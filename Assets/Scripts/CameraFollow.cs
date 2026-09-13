using UnityEngine;

// =========================================================================================
// ESTE SCRIPT HACE ESTO:
// Controlador de seguimiento de la cámara ("CameraFollow").
// Hace que la cámara principal siga al jugador en los ejes X e Y de manera continua.
// =========================================================================================
public class CameraFollow : MonoBehaviour
{
    // =========================================================================================
    // ESTE BLOQUE HACE ESTO: Variable pública para definir a qué objetivo debe seguir la cámara
    // =========================================================================================
    [Tooltip("Transform del objeto a seguir (normalmente el GameObject del Player)")]
    public Transform target;


    // =========================================================================================
    // ESTE BLOQUE HACE ESTO: Método LateUpdate()
    // Se ejecuta al final de cada frame, después de que el Player ya completó su movimiento en Update.
    // Esto garantiza que la cámara no tiemble ni tenga tirones visuales.
    // =========================================================================================
    private void LateUpdate()
    {
        // Verifica que se haya asignado un objetivo antes de intentar seguirlo
        if (target != null)
        {
            // Actualiza la posición de la cámara: copia X e Y del jugador y conserva su propia Z
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
    }
}
