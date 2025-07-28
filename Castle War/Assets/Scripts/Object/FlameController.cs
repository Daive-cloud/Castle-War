using System.Linq;
using UnityEngine;

public class FlameController : MonoBehaviour
{
    public float BurningRadius = 1.2f;
    public float BurningDamageWindown = .2f;
    private float lifeTime;
    private int burningDamage;
    private float lastTimeBurned;

    private void Start()
    {
        lifeTime = Random.Range(1f, 3f);

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (Time.time - lastTimeBurned > BurningDamageWindown)
        {
            lastTimeBurned = Time.time;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, BurningRadius);
            var vaildUnits = colliders.ToList().Where(unit => unit != null && unit.TryGetComponent(out Unit _));

            foreach (var unit in vaildUnits)
            {
                burningDamage = Random.Range(1, 5);
                unit.GetComponent<UnitStats>().TakeBurningDamage(burningDamage);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,BurningRadius);
    }
}
