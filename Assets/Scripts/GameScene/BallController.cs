// BallController.cs
using UnityEngine;

public class BallController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Camera cam;
    private Vector2 screenBounds;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    private void Start()
    {
        // Шарик активен при запуске
    }

    // Метод для активации шарика
    public void ActivateBall(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        gameObject.SetActive(true);
        rb.velocity = Vector2.zero; // Сброс скорости

        // Увеличиваем диапазон случайности по оси X до [-3, 3], сохраняя силу по оси Y = 5
        float randomX = UnityEngine.Random.Range(-3f, 3f);
        rb.AddForce(new Vector2(randomX, 5f), ForceMode2D.Impulse);

        Debug.Log($"BallController: Ball activated at {spawnPosition} with force ({randomX}, 5)");
    }

    // Метод для деактивации шарика и возвращения его в пул
    public void DeactivateBall()
    {
        gameObject.SetActive(false);
        // Возвращаем шарик обратно в пул
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Debug.LogError($"{gameObject.name}: ObjectPooler instance not found.");
        }
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        // Получаем границы экрана
        screenBounds = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.transform.position.z));

        Vector2 clampedPosition = transform.position;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x + 0.5f, screenBounds.x - 0.5f);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y + 0.5f, screenBounds.y - 0.5f);

        transform.position = clampedPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Добавьте логику столкновений, если необходимо
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Если шарик входит в триггер (например, ящик), обработайте это
        // Добавление очков уже осуществляется в BinController
        if (other.CompareTag("Bin"))
        {
            // Деактивируем шарик и возвращаем его в пул
            DeactivateBall();
        }
    }
}
