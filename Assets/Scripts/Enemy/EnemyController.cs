using UnityEngine;

public class EnemyController : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    private int m_MaxHealth;

    [SerializeField]
    private float m_Speed;

    [SerializeField]
    private float m_Damage;

    [SerializeField]
    private ParticleSystem m_DeathExplosion;

    [SerializeField]
    private float m_healthPillDropRate;

    [SerializeField]
    private GameObject m_HealthPill;

    [SerializeField]
    private int m_Score;

    [SerializeField]
    private bool m_teleporter;

    [SerializeField]
    private float m_teleportTimer;
    #endregion

    #region Private Variables
    private float p_curHealth;

    private Vector3[] p_possible;
    #endregion

    #region Cached Components
    private Rigidbody cc_Rb;
    #endregion

    #region Cached References
    private Transform cr_Player;
    #endregion



    #region Initialization
    private void Awake()
    {
        p_curHealth = m_MaxHealth;

        cc_Rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        cr_Player = FindObjectOfType<PlayerController>().transform;
        m_teleportTimer = 3;
    }
    #endregion

    #region Main Updates
    private void Update()
    {
        if (m_teleporter && m_teleportTimer <= 0)
        {
            m_teleportTimer = 3;
            float x = transform.position.x;
            float y = transform.position.y;
            float z = transform.position.z;
            p_possible = new Vector3[8];
            p_possible[0] = new Vector3(x + 4, y, z);
            p_possible[1] = new Vector3(x, y, z + 4);
            p_possible[2] = new Vector3(x - 4, y, z);
            p_possible[3] = new Vector3(x, y, z - 4);
            p_possible[4] = new Vector3(x + 4, y, z + 4);
            p_possible[5] = new Vector3(x + 4, y, z - 4);
            p_possible[6] = new Vector3(x - 4, y, z + 4);
            p_possible[7] = new Vector3(x - 4, y, z - 4);
            int index = Random.Range(0, 8);
            transform.position = p_possible[index];
        }
        m_teleportTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
     
        Vector3 dir = cr_Player.position - transform.position;
        dir.Normalize();
        cc_Rb.MovePosition(cc_Rb.position + dir * m_Speed * Time.fixedDeltaTime);
    }
    #endregion

    #region Collision Methods
    private void OnCollisionStay(Collision collision)
    {
        GameObject other = collision.collider.gameObject;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().DecreaseHealth(m_Damage);
        }
    }
    #endregion

    #region Health Methods
    public void DecreaseHealth (float amount)
    {
        p_curHealth -= amount;
        if (p_curHealth <= 0)
        {
            ScoreManager.singleton.IncreaseScore(m_Score);
            if (Random.value < m_healthPillDropRate)
            {
                Instantiate(m_HealthPill, transform.position, Quaternion.identity);
            }
            Instantiate(m_DeathExplosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
    #endregion



}
