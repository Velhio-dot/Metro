using UnityEngine;

public class LocationSwitch : MonoBehaviour
{
    [Header("🎯 Room Settings")]
    [SerializeField] private GameObject roomWalls;

    [Header("🌍 Location Settings")]
    [SerializeField] private bool isMainLocationTrigger = false;
    [SerializeField] private GameObject entireLocation;

    [Header("⚙️ Behavior Settings")]
    [SerializeField] private bool hideWallsOnEnter = true;
    [SerializeField] private bool showWallsOnExit = true;
    [SerializeField] private bool disableLocationOnExit = true;

    private bool playerInRoom = false;
    private bool playerInLocation = false;

    void Start()
    {
        if (roomWalls != null)
        {
            roomWalls.SetActive(true);
        }

        if (isMainLocationTrigger && entireLocation != null)
        {
            entireLocation.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && IsPlayerAlive(other.gameObject))
        {
            if (roomWalls != null && !isMainLocationTrigger)
            {
                playerInRoom = true;
                if (hideWallsOnEnter) roomWalls.SetActive(false);
            }

            if (isMainLocationTrigger)
            {
                playerInLocation = true;
                if (entireLocation != null) entireLocation.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && IsPlayerAlive(other.gameObject))
        {
            if (roomWalls != null && !isMainLocationTrigger)
            {
                playerInRoom = false;
                if (showWallsOnExit) roomWalls.SetActive(true);
            }

            if (isMainLocationTrigger)
            {
                playerInLocation = false;
                if (disableLocationOnExit && entireLocation != null)
                {
                    entireLocation.SetActive(false);
                }
            }
        }
    }

    private bool IsPlayerAlive(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        return health != null && !health.IsDead;
    }

    public static void DisableLocation(GameObject location)
    {
        if (location != null) location.SetActive(false);
    }

    public static void EnableLocation(GameObject location)
    {
        if (location != null) location.SetActive(true);
    }
}