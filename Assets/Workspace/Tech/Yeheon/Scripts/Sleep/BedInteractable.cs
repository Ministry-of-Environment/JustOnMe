using UnityEngine;
using UnityEngine.InputSystem;

public class BedInteractable : MonoBehaviour
{
    [SerializeField] private SleepManager sleepManager;

    private bool playerInRange;

    void Update()
    {
        if (!playerInRange)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            sleepManager.StartSleep();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}