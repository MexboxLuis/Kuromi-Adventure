using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // --- SINGLETON (El Inmortal) ---
    public static GameManager instance;

    [Header("UI Elements")]
    public TextMeshProUGUI mainMessageText;
    public TextMeshProUGUI attemptsText;
    public TextMeshProUGUI timeText;

    // --- DATOS PERSISTENTES ---
    private int attemptsCounter = 0;
    private float totalGameTime = 0f; // Ahora es el tiempo total del juego

    // --- ESTADO DEL JUEGO ---
    private enum GameState { Playing, LevelTransition, Paused, GameWon }
    private GameState currentState;
    private PlayerController player;
    private Vector3 playerStartPosition;

    void Awake()
    {
        // Si ya existe una instancia Y NO SOY YO MISMO, me destruyo.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return; // �Importante! Salimos para no ejecutar m�s c�digo.
        }

        // Si no existe, me convierto en la instancia y me hago inmortal.
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Se suscribe a los eventos de carga de escena
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Esta funci�n se ejecuta CADA VEZ que se carga una escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Encontrar los componentes de la nueva escena
        player = FindObjectOfType<PlayerController>();
        mainMessageText = GameObject.Find("MainMessageText")?.GetComponent<TextMeshProUGUI>();
        attemptsText = GameObject.Find("AttemptsText")?.GetComponent<TextMeshProUGUI>();
        timeText = GameObject.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
        // 2. Preparar el nivel
        PrepareLevel();
    }

    void PrepareLevel()
    {
        if (player == null) return;

        // --- �L�NEA CLAVE A�ADIDA! ---
        // Guardamos la posici�n inicial del jugador EN ESTE NIVEL
        playerStartPosition = player.transform.position;

        // El resto de la funci�n se queda igual
        UpdateAttemptsUI();
        UpdateTimeUI();
        currentState = GameState.LevelTransition;
        Time.timeScale = 0f;
        player.enabled = false;

        int currentLevel = SceneManager.GetActiveScene().buildIndex + 1;
        mainMessageText.text = "Nivel " + currentLevel + "\n<size=40>Presiona ESPACIO para comenzar</size>";
        mainMessageText.gameObject.SetActive(true);
    }

    // En GameManager.cs

    void Update()
    {
        // --- LÓGICA DEL TEMPORIZADOR A PRUEBA DE BUGS ---
        // Solo sumamos tiempo si estamos en el estado "Playing".
        if (currentState == GameState.Playing)
        {
            // Usamos Time.unscaledDeltaTime para que sea independiente de los bajones de FPS
            // y del modo ahorro de energía. Es el tiempo real.
            totalGameTime += Time.unscaledDeltaTime;
            UpdateTimeUI();
        }
        
        // --- LÓGICA DE INPUT (se queda igual) ---
        bool continueInput = Input.GetKeyDown(KeyCode.Space);
        if (!continueInput && Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                continueInput = true;
            }
        }
        
        if ((currentState == GameState.LevelTransition || currentState == GameState.Paused) && continueInput)
        {
            StartLevel();
        }
        else if (currentState == GameState.GameWon && continueInput)
        {
            int finalSceneIndex = SceneManager.sceneCountInBuildSettings - 1;
            Destroy(gameObject);
            SceneManager.LoadScene(finalSceneIndex);
        }
    }

// En GameManager.cs

    void StartLevel()
    {
        // "Descongelamos" el cuerpo físico del jugador
        player.GetComponent<Rigidbody>().isKinematic = false;
        
        // Le decimos al jugador que resetee su estado interno (carril y velocidad)
        player.GetComponent<Rigidbody>().position = playerStartPosition;
        player.ResetPlayerState();
        // Reanudamos el tiempo y activamos el control
        Time.timeScale = 1f;
        player.enabled = true;
        
        // Ocultamos el mensaje y actualizamos el estado del juego
        mainMessageText.gameObject.SetActive(false);
        currentState = GameState.Playing;
    }


    public void PlayerDied()
    {
        if (currentState != GameState.Playing) return;

        // --- ¡ACCIÓN INMEDIATA ANTI-BUG! ---
        // 1. Desactivamos el control del jugador INMEDIATAMENTE.
        player.enabled = false;
        // 2. Congelamos su cuerpo FÍSICAMENTE para que no se mueva ni un píxel más.
        player.GetComponent<Rigidbody>().isKinematic = true;
        // ------------------------------------

        // Ahora, el resto de la lógica
        currentState = GameState.Paused;
        Time.timeScale = 0f;

        attemptsCounter++;
        UpdateAttemptsUI();

        mainMessageText.text = "¡Chocaste!\n<size=40>Presiona ESPACIO para reintentar</size>";
        mainMessageText.gameObject.SetActive(true);
    }

    public void LevelCompleted()
    {
        if (currentState != GameState.Playing) return;

        player.enabled = false;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Si hay un siguiente nivel, lo cargamos
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Si no, hemos ganado el juego
            ShowWinScreen();
        }
    }

    void ShowWinScreen()
    {
        Time.timeScale = 0f;
        // --- CAMBIO CLAVE ---
        // En lugar de Paused, ahora el estado es GameWon
        currentState = GameState.GameWon;

        // El texto ahora debe indicar que se puede continuar
        mainMessageText.text = "�FELICIDADES!\n" +
                               "<size=40>Completaste el juego con " + attemptsCounter + " intentos.</size>\n\n" +
                               "<size=30>Presiona ESPACIO para ver el final</size>";

        mainMessageText.gameObject.SetActive(true);
    }

    void UpdateAttemptsUI()
    {
        if (attemptsText != null)
        {
            attemptsText.text = "Intentos: " + attemptsCounter;
        }
    }
    void UpdateTimeUI()
    {
        if (timeText != null)
        {
            // Formateamos el tiempo para que muestre solo dos decimales
            // --- CAMBIO DE VARIABLE ---
            timeText.text = "Tiempo: " + totalGameTime.ToString("F2");
        }
    }

    public void ResetGameStats()
    {
        attemptsCounter = 0;
        totalGameTime = 0f;
        Debug.Log("�Estad�sticas del juego reseteadas!");
    }
}