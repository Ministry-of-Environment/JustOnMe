using UnityEngine;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
public class PeriodLight2DController : MonoBehaviour
{
    [Header("Target Light")]
    [SerializeField] private Light2D targetLight;

    [Header("Beginning")]
    [SerializeField] private Color beginningColor = Color.white;
    [SerializeField, Range(0f, 2f)] private float beginningIntensity = 1f;

    [Header("Progress")]
    [SerializeField] private Color progressColor = new Color(1f, 0.88f, 0.65f);
    [SerializeField, Range(0f, 2f)] private float progressIntensity = 0.85f;

    [Header("Peak")]
    [SerializeField] private Color peakColor = new Color(1f, 0.55f, 0.35f);
    [SerializeField, Range(0f, 2f)] private float peakIntensity = 0.65f;

    [Header("Ending / Night")]
    [SerializeField] private Color endingColor = new Color(0.35f, 0.45f, 0.85f);
    [SerializeField, Range(0f, 2f)] private float endingIntensity = 0.35f;

    [Header("Result Wait")]
    [SerializeField] private Color resultWaitColor = new Color(0.25f, 0.3f, 0.55f);
    [SerializeField, Range(0f, 2f)] private float resultWaitIntensity = 0.25f;

    [Header("Transition")]
    [SerializeField, Min(0f)] private float transitionDuration = 2f;

    private Color targetColor;
    private float targetIntensity;

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light2D>();
        }
    }

    private void OnEnable()
    {
        if (GamePeriodCycle.Instance != null)
        {
            GamePeriodCycle.Instance.OnPhaseChanged.AddListener(HandlePhaseChanged);
            ApplyPhaseImmediate(GamePeriodCycle.Instance.CurrentPhase);
        }
    }

    private void Start()
    {
        // OnEnable 시점에 GamePeriodCycle이 아직 없던 경우 대비
        if (GamePeriodCycle.Instance != null)
        {
            GamePeriodCycle.Instance.OnPhaseChanged.RemoveListener(HandlePhaseChanged);
            GamePeriodCycle.Instance.OnPhaseChanged.AddListener(HandlePhaseChanged);
            ApplyPhaseImmediate(GamePeriodCycle.Instance.CurrentPhase);
        }
    }

    private void OnDisable()
    {
        if (GamePeriodCycle.Instance != null)
        {
            GamePeriodCycle.Instance.OnPhaseChanged.RemoveListener(HandlePhaseChanged);
        }
    }

    private void Update()
    {
        if (targetLight == null)
        {
            return;
        }

        if (transitionDuration <= 0f)
        {
            targetLight.color = targetColor;
            targetLight.intensity = targetIntensity;
            return;
        }

        float t = Time.deltaTime / transitionDuration;

        targetLight.color = Color.Lerp(targetLight.color, targetColor, t);
        targetLight.intensity = Mathf.Lerp(targetLight.intensity, targetIntensity, t);
    }

    private void HandlePhaseChanged(PeriodPhase phase)
    {
        SetTargetByPhase(phase);
    }

    private void ApplyPhaseImmediate(PeriodPhase phase)
    {
        SetTargetByPhase(phase);

        if (targetLight == null)
        {
            return;
        }

        targetLight.color = targetColor;
        targetLight.intensity = targetIntensity;
    }

    private void SetTargetByPhase(PeriodPhase phase)
    {
        switch (phase)
        {
            case PeriodPhase.Beginning:
                targetColor = beginningColor;
                targetIntensity = beginningIntensity;
                break;

            case PeriodPhase.Progress:
                targetColor = progressColor;
                targetIntensity = progressIntensity;
                break;

            case PeriodPhase.Peak:
                targetColor = peakColor;
                targetIntensity = peakIntensity;
                break;

            case PeriodPhase.Ending:
                targetColor = endingColor;
                targetIntensity = endingIntensity;
                break;

            case PeriodPhase.ResultWait:
                targetColor = resultWaitColor;
                targetIntensity = resultWaitIntensity;
                break;

            default:
                targetColor = Color.white;
                targetIntensity = 1f;
                break;
        }
    }
}