using UnityEngine;

public class CookingGameManager : MonoBehaviour
{
    [Header("Mini Games")]
    [SerializeField] private FireMiniGame fireMiniGame;
    [SerializeField] private FlipMiniGame flipMiniGame;
    [SerializeField] private SaltMiniGame saltMiniGame;
    [SerializeField] private PepperMiniGame pepperMiniGame;

    [Header("UI")]
    [SerializeField] private GameObject cookingCanvas;

    [Header("Result UI")]
    [SerializeField] private GameObject successPanel;
    [SerializeField] private GameObject failPanel;

    [Header("Timer")]
    [SerializeField] private CookingTimer cookingTimer;

    [Header("Timer UI")]
    [SerializeField] private GameObject timerBarRoot;

    [Header("PlayerMove")]
    [SerializeField] private PlayerMove playerMove;

    [Header("Hunger")]
    [SerializeField] private HungerSystem hungerSystem;
    [SerializeField, Min(0)] private int cookingSuccessHungerRecovery = 20;

    private bool isCooking = false;

    private void Start()
    {
        BindHungerSystem();
        BindEvents();

        DisableAllMiniGames();

        timerBarRoot.SetActive(false);

        successPanel.SetActive(false);
        failPanel.SetActive(false);
    }

    private void BindEvents()
    {
        if (fireMiniGame == null || flipMiniGame == null || saltMiniGame == null || pepperMiniGame == null || cookingTimer == null)
        {
            Debug.LogWarning("CookingGameManager의 미니게임 또는 타이머 참조가 비어 있습니다.");
            return;
        }

        fireMiniGame.OnMiniGameSuccess += StartFlipMiniGame;
        fireMiniGame.OnMiniGameFail += FailCooking;

        flipMiniGame.OnMiniGameSuccess += StartSaltMiniGame;
        flipMiniGame.OnMiniGameFail += FailCooking;

        saltMiniGame.OnMiniGameSuccess += StartPepperMiniGame;

        pepperMiniGame.OnMiniGameSuccess += SuccessCooking;

        cookingTimer.OnTimerEnd += FailCooking;
    }

    private void BindHungerSystem()
    {
        if (hungerSystem != null)
        {
            return;
        }

        hungerSystem = FindFirstObjectByType<HungerSystem>();
    }

    public void StartCooking()
    {
        if (isCooking)
            return;

        isCooking = true;

        playerMove.SetMovementEnabled(false);

        cookingCanvas.SetActive(true);

        timerBarRoot.SetActive(true);

        successPanel.SetActive(false);
        failPanel.SetActive(false);

        DisableAllMiniGames();

        cookingTimer.StartTimer();

        fireMiniGame.gameObject.SetActive(true);

        fireMiniGame.StartGame();
    }

    private void StartFlipMiniGame()
    {
        fireMiniGame.gameObject.SetActive(false);

        flipMiniGame.gameObject.SetActive(true);

        flipMiniGame.StartGame();
    }

    private void StartSaltMiniGame()
    {
        flipMiniGame.gameObject.SetActive(false);

        saltMiniGame.gameObject.SetActive(true);

        saltMiniGame.StartGame();
    }

    private void StartPepperMiniGame()
    {
        saltMiniGame.gameObject.SetActive(false);

        pepperMiniGame.gameObject.SetActive(true);

        pepperMiniGame.StartGame();
    }

    private void SuccessCooking()
    {
        pepperMiniGame.gameObject.SetActive(false);

        cookingTimer.StopTimer();

        timerBarRoot.SetActive(false);

        successPanel.SetActive(true);

        if (hungerSystem != null && cookingSuccessHungerRecovery > 0)
        {
            hungerSystem.ConsumeFood(cookingSuccessHungerRecovery);
        }

        Invoke(nameof(EndCooking), 2f);
    }

    private void FailCooking()
    {
        DisableAllMiniGames();

        cookingTimer.StopTimer();

        timerBarRoot.SetActive(false);

        failPanel.SetActive(true);

        Invoke(nameof(EndCooking), 2f);
    }

    private void EndCooking()
    {
        isCooking = false;

        CancelInvoke(nameof(EndCooking));

        successPanel.SetActive(false);
        failPanel.SetActive(false);

        cookingCanvas.SetActive(false);

        playerMove.SetMovementEnabled(true);
    }

    private void DisableAllMiniGames()
    {
        if (fireMiniGame != null) fireMiniGame.gameObject.SetActive(false);
        if (flipMiniGame != null) flipMiniGame.gameObject.SetActive(false);
        if (saltMiniGame != null) saltMiniGame.gameObject.SetActive(false);
        if (pepperMiniGame != null) pepperMiniGame.gameObject.SetActive(false);
    }
}
