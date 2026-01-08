using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasFunc : MonoBehaviour
{
    public Image HealthBar;
    public Image HealthBarBackground;
    [SerializeField, Tooltip("主血条到目标值的快速过渡时间（秒）")]
    private float mainLerpDuration = 0.08f;

    [SerializeField, Tooltip("背景血条到目标值的慢速过渡时间（秒）")]
    private float backgroundLerpDuration = 0.6f;

    private Coroutine _healthAnimCoroutine;
    private void Awake()
    {
        // Use null-conditional in case the event manager isn't initialized yet
        MyEventManager.Instance?.AddEventListener<float>(EventName.PlayerHealthChange, UpdateHealthBar);
    }

    private void UpdateHealthBar(float percent)
    {
        // Guard clauses
        if (HealthBar == null || HealthBarBackground == null)
        {
            Debug.LogWarning("CanvasFunc: HealthBar or HealthBarBackground is not assigned.");
            return;
        }

        percent = Mathf.Clamp01(percent);
        Debug.Log("Updating health bar to: " + percent);

        // Stop any running animation and start a new one towards the target percent.
        if (_healthAnimCoroutine != null)
            StopCoroutine(_healthAnimCoroutine);
        _healthAnimCoroutine = StartCoroutine(AnimateHealthChange(percent));
    }
    private IEnumerator AnimateHealthChange(float targetPercent)
    {
        // Ensure durations are sane
        float mainDur = Mathf.Max(0.0001f, mainLerpDuration);
        float bgDur = Mathf.Max(0.0001f, backgroundLerpDuration);

        float startMain = HealthBar.fillAmount;
        float startBg = HealthBarBackground.fillAmount;

        // Taking damage: main bar drops quickly, background follows more slowly.
        if (targetPercent < startMain)
        {
            // Fast transition for main bar
            float t = 0f;
            while (t < mainDur)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(startMain, targetPercent, t / mainDur);
                HealthBar.fillAmount = v;
                yield return null;
            }
            HealthBar.fillAmount = targetPercent;

            // Slow transition for background bar
            t = 0f;
            while (t < bgDur)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(startBg, targetPercent, t / bgDur);
                HealthBarBackground.fillAmount = v;
                yield return null;
            }
            HealthBarBackground.fillAmount = targetPercent;
        }
        else
        {
            // Healing or increase: make both move together quickly so UI reflects the heal immediately
            float t = 0f;
            while (t < mainDur)
            {
                t += Time.deltaTime;
                float vMain = Mathf.Lerp(startMain, targetPercent, t / mainDur);
                float vBg = Mathf.Lerp(startBg, targetPercent, t / mainDur);
                HealthBar.fillAmount = vMain;
                HealthBarBackground.fillAmount = vBg;
                yield return null;
            }
            HealthBar.fillAmount = targetPercent;
            HealthBarBackground.fillAmount = targetPercent;
        }

        _healthAnimCoroutine = null;
    }
}