using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageFontUI : MonoBehaviour
{
     private TextMeshPro DamageFont => GetComponent<TextMeshPro>();

    [SerializeField] private AnimationCurve m_ScaleCurve;
    [SerializeField] private float m_FontExistTime;

    private float Timer;
    private float FontSize;

    private void Start()
    {
        FontSize = DamageFont.fontSize;
    }

    private void Update()
    {
        Timer += Time.deltaTime;

        DamageFont.fontSize = FontSize * m_ScaleCurve.Evaluate(Timer);
        DamageFont.transform.position += Vector3.up * 2 * Time.deltaTime;

        if(Timer >= m_FontExistTime)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamageValue(int _value) => DamageFont.text = _value.ToString();

}
