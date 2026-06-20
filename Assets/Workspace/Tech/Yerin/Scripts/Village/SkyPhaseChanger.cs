using System.Collections;
using UnityEngine;

public class SkyPhaseChanger : MonoBehaviour
{
    [Header("Base Sky Renderers")]
    [SerializeField] private SpriteRenderer[] baseSkyRenderers;

    [Header("Fade Sky Renderers")]
    [SerializeField] private SpriteRenderer[] fadeSkyRenderers;

    [Header("Night Filter")]
    [SerializeField] private SpriteRenderer nightFilterRenderer;
    [SerializeField, Range(0f, 1f)] private float nightFilterAlpha = 0.45f;

    [Header("Phase Sky Sprites")]
    [SerializeField] private Sprite dawnSky;
    [SerializeField] private Sprite daySky;
    [SerializeField] private Sprite sunsetSky;
    [SerializeField] private Sprite nightSky;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    private Coroutine fadeCoroutine;
    private Sprite currentSky;
    private float currentFilterAlpha;

    private void Start()
    {
        SetFadeSkyAlpha(0f);
        SetNightFilterAlpha(0f);

        if (GamePeriodCycle.Instance == null)
        {
            Debug.LogWarning("GamePeriodCycle Instance가 없습니다.");
            return;
        }

        GamePeriodCycle.Instance.OnPhaseChanged.AddListener(ChangeSkyByPhase);

        // 마을 씬에 들어왔을 때 현재 시간대에 즉시 동기화
        ChangeSkyInstant(GamePeriodCycle.Instance.CurrentPhase);
    }

    private void OnDestroy()
    {
        if (GamePeriodCycle.Instance == null)
        {
            return;
        }

        GamePeriodCycle.Instance.OnPhaseChanged.RemoveListener(ChangeSkyByPhase);
    }

    private void ChangeSkyByPhase(PeriodPhase phase)
    {
        Sprite targetSky = GetSkySprite(phase);
        float targetFilterAlpha = GetNightFilterAlpha(phase);

        if (targetSky == null)
        {
            return;
        }

        if (currentSky == targetSky && Mathf.Approximately(currentFilterAlpha, targetFilterAlpha))
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeToSky(targetSky, targetFilterAlpha));
    }

    private IEnumerator FadeToSky(Sprite targetSky, float targetFilterAlpha)
    {
        foreach (SpriteRenderer renderer in fadeSkyRenderers)
        {
            renderer.sprite = targetSky;
        }

        float startFilterAlpha = currentFilterAlpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            SetFadeSkyAlpha(t);

            float filterAlpha = Mathf.Lerp(startFilterAlpha, targetFilterAlpha, t);
            SetNightFilterAlpha(filterAlpha);

            yield return null;
        }

        foreach (SpriteRenderer renderer in baseSkyRenderers)
        {
            renderer.sprite = targetSky;
        }

        currentSky = targetSky;
        currentFilterAlpha = targetFilterAlpha;

        SetFadeSkyAlpha(0f);
        SetNightFilterAlpha(targetFilterAlpha);

        fadeCoroutine = null;

        Debug.Log("하늘 배경 페이드 전환 완료");
    }

    private void ChangeSkyInstant(PeriodPhase phase)
    {
        Sprite targetSky = GetSkySprite(phase);
        float targetFilterAlpha = GetNightFilterAlpha(phase);

        if (targetSky == null)
        {
            return;
        }

        foreach (SpriteRenderer renderer in baseSkyRenderers)
        {
            renderer.sprite = targetSky;
        }

        foreach (SpriteRenderer renderer in fadeSkyRenderers)
        {
            renderer.sprite = targetSky;
        }

        currentSky = targetSky;
        currentFilterAlpha = targetFilterAlpha;

        SetFadeSkyAlpha(0f);
        SetNightFilterAlpha(targetFilterAlpha);

        Debug.Log($"하늘 배경 즉시 동기화: {phase}");
    }

    private void SetFadeSkyAlpha(float alpha)
    {
        foreach (SpriteRenderer renderer in fadeSkyRenderers)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    private void SetNightFilterAlpha(float alpha)
    {
        if (nightFilterRenderer == null)
        {
            return;
        }

        Color color = nightFilterRenderer.color;
        color.a = alpha;
        nightFilterRenderer.color = color;
    }

    private Sprite GetSkySprite(PeriodPhase phase)
    {
        switch (phase)
        {
            case PeriodPhase.Beginning:
                return dawnSky;

            case PeriodPhase.Progress:
                return daySky;

            case PeriodPhase.Peak:
                return sunsetSky;

            case PeriodPhase.Ending:
                return nightSky;

            case PeriodPhase.ResultWait:
                return nightSky;

            default:
                return dawnSky;
        }
    }

    private float GetNightFilterAlpha(PeriodPhase phase)
    {
        switch (phase)
        {
            case PeriodPhase.Ending:
            case PeriodPhase.ResultWait:
                return nightFilterAlpha;

            default:
                return 0f;
        }
    }
}