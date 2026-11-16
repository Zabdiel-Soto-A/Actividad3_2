using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class CoinCollector : MonoBehaviour
{
    // Variables globales
    public static int totalCoins = 0;
    private static TMP_Text coinTextUI;

    // Referencias de Firebase (sin static para evitar conflictos)
    private DatabaseReference dbReference;
    private FirebaseAuth auth;
    private string userId = "";
    private static bool firebaseInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetStaticData()
    {
        totalCoins = 0;
        coinTextUI = null;
        firebaseInitialized = false;
    }

    private void Start()
    {
        // Búsqueda automática del texto en pantalla
        if (coinTextUI == null)
        {
            GameObject textObj = GameObject.Find("TextoMonedas");
            if (textObj != null)
            {
                coinTextUI = textObj.GetComponent<TMP_Text>();
            }
            else
            {
                coinTextUI = Object.FindFirstObjectByType<TMP_Text>();
            }
        }

        UpdateUI();

        // Inicializar Firebase solo una vez de forma global
        if (!firebaseInitialized)
        {
            firebaseInitialized = true;
            Debug.Log("1. Iniciando verificación de dependencias de Firebase...");
            
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                Debug.Log("2. Verificación de dependencias completada. Estado: " + task.Result);
                
                if (task.Result == DependencyStatus.Available)
                {
                    Debug.Log("3. Dependencias disponibles. Obteniendo instancias de Firebase...");
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    auth = FirebaseAuth.DefaultInstance;

                    // AQUÍ ESTÁ LA CLAVE: Añade la URL exacta de tu base de datos entre comillas
                    string databaseUrl = "https://videojuego-2d-dec96-default-rtdb.firebaseio.com/"; // <-- Reemplaza esto con tu URL real
                    dbReference = FirebaseDatabase.GetInstance(app, databaseUrl).RootReference;

                    Debug.Log("4. Instancias obtenidas con éxito. Llamando a SignInAnonymously()...");
                    SignInAnonymously();
                }
                else
                {
                    Debug.LogError("ERROR FATAL: No se pudieron resolver las dependencias de Firebase: " + task.Result);
                }
            });
        }
    }

    private void SignInAnonymously()
    {
        if (auth == null)
        {
            Debug.LogError("ERROR: El objeto 'auth' es nulo, no se puede iniciar sesión.");
            return;
        }

        Debug.Log("5. Ejecutando auth.SignInAnonymouslyAsync()...");

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("ERROR: El inicio de sesión anónimo fue cancelado.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("ERROR: Falló el inicio de sesión anónimo. Detalle: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            FirebaseUser newUser = result.User;
            userId = newUser.UserId;

            Debug.Log("6. ¡Éxito total! UID del jugador autenticado: " + userId);

            // Cargamos las monedas de la base de datos una vez autenticados
            LoadCoinsFromFirebase();
        });
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            totalCoins++;
            UpdateUI();
            
            // Guardar en la base de datos
            SaveCoinsToFirebase();

            Destroy(gameObject);
        }
    }

    private void UpdateUI()
    {
        if (coinTextUI != null)
        {
            coinTextUI.text = "Monedas: " + totalCoins;
        }
    }

    private void SaveCoinsToFirebase()
    {
        if (dbReference == null || string.IsNullOrEmpty(userId)) return;

        dbReference.Child("usuarios").Child(userId).Child("monedas").SetValueAsync(totalCoins)
            .ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error al guardar monedas en Firebase: " + task.Exception);
                }
            });
    }

    private void LoadCoinsFromFirebase()
    {
        if (dbReference == null || string.IsNullOrEmpty(userId)) return;

        dbReference.Child("usuarios").Child(userId).Child("monedas").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    totalCoins = int.Parse(snapshot.Value.ToString());
                    UpdateUI();
                    Debug.Log("Monedas cargadas desde Firebase: " + totalCoins);
                }
            }
            else
            {
                Debug.LogWarning("No se encontraron monedas previas para este usuario.");
            }
        });
    }
}