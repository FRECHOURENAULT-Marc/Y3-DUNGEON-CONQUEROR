using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFeedback : MonoBehaviour
{
    [SerializeField] PlayerReferences _PReferences;

    [SerializeField] Image _damageImage;

    [SerializeField] float _effectDuration;

    float _maxAlpha;
    Coroutine _damageEffectCoroutine;

    void Start()
    {
        _maxAlpha = 235f / 255f;
        SetDamageImageAlpha(0f);

        _PReferences.HealthController.GetDamage += HandleGetDamage;
    }

    void OnDestroy()
    {
        _PReferences.HealthController.GetDamage -= HandleGetDamage;
    }

    void HandleGetDamage()
    {
        if (_damageEffectCoroutine != null)
            StopCoroutine(_damageEffectCoroutine);

        SetDamageImageAlpha(0f);
        _damageEffectCoroutine = StartCoroutine(PlayDamageEffect());
    }

    IEnumerator PlayDamageEffect()
    {
        yield return FadeDamageImage(0f, _maxAlpha);
        yield return new WaitForSeconds(0.1f);
        yield return FadeDamageImage(_maxAlpha, 0f);

        _damageEffectCoroutine = null;
    }

    IEnumerator FadeDamageImage(float startAlpha, float endAlpha)
    {
        if (_effectDuration <= 0f)
        {
            SetDamageImageAlpha(endAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < _effectDuration)
        {
            elapsed += Time.deltaTime;
            SetDamageImageAlpha(Mathf.Lerp(startAlpha, endAlpha, elapsed / _effectDuration));
            yield return null;
        }

        SetDamageImageAlpha(endAlpha);
    }

    void SetDamageImageAlpha(float alpha)
    {
        Color color = _damageImage.color;
        color.a = alpha;
        _damageImage.color = color;
    }
}
