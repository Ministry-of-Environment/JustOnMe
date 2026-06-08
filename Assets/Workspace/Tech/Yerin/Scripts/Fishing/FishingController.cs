using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingController : MonoBehaviour
{
    [Header("Fish")]
    [SerializeField] private FishTable fishTable;
    [SerializeField] private PollutionLevel currentPollutionLevel = PollutionLevel.Level1;

    [Header("Mini Game")]
    [SerializeField] private FishingMiniGameUI miniGameUI;
    [SerializeField] private List<FishRarityDifficulty> difficulties = new();

    [Header("Guide UI")]
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private string readyGuideMessage = "F : Try fishing";
    [SerializeField] private string miniGameGuideMessage = "Space : Timing";

    private bool isFishing;

    private void Start()
    {
        SetGuideText(readyGuideMessage);
    }

    private void OnFishing(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        TryStartFishing();
    }

    private void OnFishingConfirm(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (!isFishing)
        {
            return;
        }

        if (miniGameUI == null)
        {
            return;
        }

        miniGameUI.TryConfirm();
    }

    private void TryStartFishing()
    {
        if (isFishing)
        {
            return;
        }

        if (fishTable == null)
        {
            Debug.LogError("FishTable이 없습니다.");
            return;
        }

        if (miniGameUI == null)
        {
            Debug.LogError("FishingMiniGameUI가 없습니다.");
            return;
        }

        FishData selectedFish = fishTable.GetRandomFish(currentPollutionLevel);

        if (selectedFish == null)
        {
            Debug.LogWarning("현재 오염도에서 잡을 수 있는 물고기가 없습니다.");
            return;
        }

        FishRarityDifficulty difficulty = GetDifficulty(selectedFish.Rarity);

        if (difficulty == null)
        {
            Debug.LogError($"{selectedFish.Rarity} 희귀도에 해당하는 난이도 데이터가 없습니다.");
            return;
        }

        isFishing = true;
        SetGuideText(miniGameGuideMessage);

        Debug.Log($"물고기가 걸렸습니다: {selectedFish.FishName} / 희귀도: {selectedFish.Rarity}");

        miniGameUI.StartMiniGame(
            selectedFish,
            difficulty,
            OnFishingSuccess,
            OnFishingFail
        );
    }

    private FishRarityDifficulty GetDifficulty(FishRarity rarity)
    {
        foreach (FishRarityDifficulty difficulty in difficulties)
        {
            if (difficulty.Rarity == rarity)
            {
                return difficulty;
            }
        }

        return null;
    }

    private void OnFishingSuccess(FishData fish)
    {
        isFishing = false;
        SetGuideText(readyGuideMessage);

        Debug.Log($"물고기 잡기 성공: {fish.FishName}");

        // TODO: 인벤토리에 물고기 추가
    }

    private void OnFishingFail(FishData fish)
    {
        isFishing = false;
        SetGuideText(readyGuideMessage);

        Debug.Log($"물고기를 놓쳤습니다: {fish.FishName}");
    }

    private void SetGuideText(string message)
    {
        if (guideText == null)
        {
            return;
        }

        guideText.text = message;
    }
}