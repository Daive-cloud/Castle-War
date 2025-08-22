using UnityEngine;

public class Pointer : MonoBehaviour
{
    [SerializeField] private float ExistWindow;
    [SerializeField] private AnimationCurve ScaleCurve;

    private float Timer;
    private Vector3 m_Scale;

    private void Start()
    {
        m_Scale = transform.localScale;
    }

    private void Update()
    {
        Timer += Time.deltaTime;

        float scaleMultiplier = ScaleCurve.Evaluate(Timer);
        transform.localScale = m_Scale *scaleMultiplier;

        if(Timer > ExistWindow)
        {
            Destroy(gameObject);
        }
    }
}
