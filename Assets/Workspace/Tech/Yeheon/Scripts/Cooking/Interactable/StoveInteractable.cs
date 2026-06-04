using UnityEngine;
using UnityEngine.InputSystem;

public class StoveInteractable : MonoBehaviour
{
    [SerializeField] private CookingGameManager cookingGameManager;

    private bool playerInRange = false;

    void Update()
    {
        if (!playerInRange)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            cookingGameManager.StartCooking();
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