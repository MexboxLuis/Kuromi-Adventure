using UnityEngine;
using UnityEngine.Video; // Necesario para el Video Player
using TMPro; // Necesario para el texto
using UnityEngine.SceneManagement; // Necesario para reiniciar

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Referencia a nuestro Video Player
    public GameObject restartMessageObject; // Referencia al objeto de texto

    void Start()
    {
        // Ocultamos el mensaje de reinicio al empezar
        if (restartMessageObject != null)
        {
            restartMessageObject.SetActive(false);
        }

        // Nos suscribimos al evento "loopPointReached". Este evento se dispara
        // cuando el video llega al final (si no est� en bucle).
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Update()
    {
        // Si el mensaje de reinicio est� activo...
        if (restartMessageObject != null && restartMessageObject.activeInHierarchy)
        {
            // ...y el jugador presiona Escape...
        if (Input.GetKeyDown(KeyCode.Escape) || Input.touchCount > 0)
        {
                // --- L�GICA DE REINICIO CORREGIDA ---

                // 1. Buscamos si existe una instancia del GameManager inmortal.
                GameManager gm = FindObjectOfType<GameManager>();

                // 2. Si existe, le ordenamos que resetee sus contadores.
                if (gm != null)
                {
                    gm.ResetGameStats();
                }

                // 3. Finalmente, cargamos la primera escena para empezar de nuevo.
                SceneManager.LoadScene(0);
            }
        }
    }

    // Esta funci�n se llamar� autom�ticamente cuando el video termine.
    void OnVideoEnd(VideoPlayer vp)
    {
        // Mostramos el mensaje de reinicio
        if (restartMessageObject != null)
        {
            restartMessageObject.SetActive(true);
        }
    }
}