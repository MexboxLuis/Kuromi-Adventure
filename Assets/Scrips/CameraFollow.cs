using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Camera Settings")]
    // Offset: X=detrás, Y=altura, Z=centrado
    public Vector3 offset = new Vector3(-4f, 2.5f, 0f);

    // Tiempo de suavizado: valores más bajos hacen la cámara más rígida.
    public float smoothTime = 0.1f;

    // Variable interna para que SmoothDamp funcione. No tocar.
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calcula la posición a la que la cámara quiere ir
        Vector3 desiredPosition = target.position + offset;

        // 2. ¡LA MAGIA DE SMOOTHDAMP!
        // Mueve la cámara suavemente hacia el objetivo sin el efecto "elástico".
        // Es como un operador de cámara profesional.
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // 3. Mantenemos que la cámara siempre mire a Kuromi
        transform.LookAt(target);
    }
}