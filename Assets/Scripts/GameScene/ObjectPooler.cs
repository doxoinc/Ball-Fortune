// ObjectPooler.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectPooler : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject objectPrefab;     // Префаб объекта для пула (Ball)
    public int poolSize = 20;          // Начальный размер пула

    private Queue<GameObject> objectPool = new Queue<GameObject>();

    #region Singleton
    public static ObjectPooler Instance { get; private set; }

    private void Awake()
    {
        // Реализация Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
            SceneManager.sceneLoaded += OnSceneLoaded; // Подписка на событие загрузки сцены
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // Отписка от события при уничтожении
            Instance = null;
        }
    }

    // Обработчик события загрузки сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"ObjectPooler: Загружена сцена {scene.name}");
        if (scene.name == "GameScene") // Замените "GameScene" на фактическое имя вашей игровой сцены
        {
            Debug.Log("ObjectPooler: Инициализируем пул объектов для GameScene.");
            ResetPool();
            InitializePool();
        }
    }

    // Инициализация пула объектов
    private void InitializePool()
    {
        Debug.Log("ObjectPooler: Инициализация пула объектов.");
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject();
        }
        Debug.Log($"ObjectPooler: Пул объектов инициализирован с {poolSize} объектами.");
    }

    // Создание нового объекта и добавление его в пул
    private void CreateNewObject()
    {
        GameObject obj = Instantiate(objectPrefab);
        obj.SetActive(false);
        objectPool.Enqueue(obj);
        Debug.Log($"ObjectPooler: Создан объект {obj.name} и добавлен в пул.");
    }

    // Сброс пула объектов
    private void ResetPool()
    {
        Debug.Log("ObjectPooler: Сброс пула объектов.");
        // Уничтожаем все объекты в пуле
        while (objectPool.Count > 0)
        {
            GameObject obj = objectPool.Dequeue();
            Destroy(obj);
        }
        objectPool.Clear();
        Debug.Log("ObjectPooler: Пул объектов сброшен.");
    }

    // Получение объекта из пула
    public GameObject GetPooledObject()
    {
        if (objectPool.Count > 0)
        {
            GameObject obj = objectPool.Dequeue();
            obj.SetActive(true);
            Debug.Log($"ObjectPooler: Получен объект {obj.name} из пула.");
            return obj;
        }
        else
        {
            // Если пул пуст, создаём новый объект
            Debug.Log("ObjectPooler: Пул пуст, создаём новый объект.");
            CreateNewObject();
            if (objectPool.Count > 0)
            {
                GameObject obj = objectPool.Dequeue();
                obj.SetActive(true);
                Debug.Log($"ObjectPooler: Создан и получен новый объект {obj.name}.");
                return obj;
            }
            else
            {
                Debug.LogError("ObjectPooler: Не удалось создать новый объект.");
                return null;
            }
        }
    }

    // Возврат объекта обратно в пул
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        objectPool.Enqueue(obj);
        Debug.Log($"ObjectPooler: Объект {obj.name} возвращён в пул.");
    }
}
