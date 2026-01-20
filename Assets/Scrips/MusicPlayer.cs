using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;
    private AudioSource audioSource;
    private bool isMuted = false;

    void Awake()
    {
        // Evita duplicados
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Obtener el AudioSource
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
   
        if (Input.GetKeyDown(KeyCode.M))
        {
            isMuted = !isMuted;
            if (audioSource != null)
            {
                audioSource.mute = isMuted;
            }
        }

  
        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            Destroy(gameObject);
        }
    }
}
