using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 15f;
    public int damage = 3;
    public float lifeTime = 10f;

    private Transform target;

    public void SetTarget(Transform newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        direction = direction.normalized;

        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (other.CompareTag("Player"))
        {
            return;
        }
        Debug.Log("碰到：" + other.name);

        if (enemyHealth == null)
        {
            return;
        }

        //Debug.Log("成功造成傷害");
        enemyHealth.TakeDamage(damage);

        Destroy(gameObject);
    }
}
