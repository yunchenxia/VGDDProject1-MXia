using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PlayerController : MonoBehaviour
{

    #region Editor Vraibles
    [SerializeField]
    private float m_Speed;

    [SerializeField]
    private Transform m_CameraTransform;

    [SerializeField]
    private PlayerAttackInfo[] m_Attacks;

    [SerializeField]
    private int m_MaxHealth;

    [SerializeField]
    private HUDController m_HUD;
    #endregion

    #region Cached References
    private Animator cr_Anim;
    private Renderer cr_Renderer;

    
    #endregion


    #region Cached Components
    private Rigidbody cc_Rb;

    #endregion


    #region Private Variables

    private Vector2 p_Velocity;
    private float p_FrozenTimer;

    private Color p_DefaultColor;
    private float p_CurHealth;
    #endregion

    #region Initialization
    private void Awake()
    {
        p_Velocity = Vector2.zero;
        cc_Rb = GetComponent<Rigidbody>();
        cr_Anim = GetComponent<Animator>();
        cr_Renderer = GetComponentInChildren<Renderer>();
        p_DefaultColor = cr_Renderer.material.color;

        p_FrozenTimer = 0;
        p_CurHealth = m_MaxHealth;

        for (int i =0; i< m_Attacks.Length; i++)
        {
            PlayerAttackInfo attack = m_Attacks[i];
            attack.Cooldown = 0;

            if (attack.WindUpTime > attack.FrozenTime)
            {
                Debug.LogError(attack.AttackName + " has a wind up time that is larger than the amount of time that the player is frozen for");
            }
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion

    #region Main Updates
    private void Update()
    {
        if (p_FrozenTimer > 0)
        {
            p_Velocity = Vector2.zero;
            p_FrozenTimer -= Time.deltaTime;
            return;
        } else
        {
            p_FrozenTimer = 0;
        }

        for (int i = 0; i < m_Attacks.Length; i++)
        {
            PlayerAttackInfo attack = m_Attacks[i];

            if (attack.IsReady())
            {
                if (Input.GetButtonDown (attack.Button))
                {
                    p_FrozenTimer = attack.FrozenTime;
                    DecreaseHealth(attack.HealthCost);
                    StartCoroutine(UseAttack(attack));
                    break;
                }
            } else if (attack.Cooldown > 0)
            {
                attack.Cooldown -= Time.deltaTime;
            }
        }

        float forward = Input.GetAxis("Vertical");
        float right = Input.GetAxis("Horizontal");

        cr_Anim.SetFloat("Speed", Mathf.Clamp01(Mathf.Abs(forward) + Mathf.Abs(right)));

        float moveThreshhold = 0.3f;

        if (forward > 0 && forward < moveThreshhold)
        {
            forward = 0;
        } else if (forward < 0 && forward > -moveThreshhold)
        {
            forward = 0;
        }

        if(right > 0 && right < moveThreshhold)
        {
            right = 0;
        } else if (right < 0 && right > -moveThreshhold)
        {
            right = 0;
        }
        p_Velocity.Set(right, forward);
    }


    private void FixedUpdate()
    {
        cc_Rb.MovePosition(cc_Rb.position + m_Speed * Time.fixedDeltaTime *transform.forward * p_Velocity.magnitude);

        cc_Rb.angularVelocity = Vector3.zero;

        if (p_Velocity.sqrMagnitude > 0)
        {
            float angleToRotCam = Mathf.Deg2Rad * Vector2.SignedAngle(Vector2.up, p_Velocity);
            Vector3 camForward = m_CameraTransform.forward;
            Vector3 newRot = new Vector3(Mathf.Cos(angleToRotCam) * camForward.x - Mathf.Sin(angleToRotCam) * camForward.z, 0,
                Mathf.Cos(angleToRotCam) * camForward.z + Mathf.Sin(angleToRotCam) * camForward.x);
            float theta = Vector3.SignedAngle(transform.forward, newRot, Vector3.up);
            cc_Rb.rotation = Quaternion.Slerp(cc_Rb.rotation, cc_Rb.rotation * Quaternion.Euler(0, theta, 0), 0.2f);
        }
    }
    #endregion

    #region Health/DyingMethods
    public void DecreaseHealth(float amount)
    {
        p_CurHealth -= amount;
        m_HUD.UpdateHealth(1.0f * p_CurHealth / m_MaxHealth);
        if (p_CurHealth <= 0)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void IncreaseHealth(int amount)
    {
        p_CurHealth += amount;
        if (p_CurHealth > m_MaxHealth)
        {
            p_CurHealth = m_MaxHealth;
        }
        m_HUD.UpdateHealth(1.0f * p_CurHealth / m_MaxHealth);
    }

    #endregion

    #region Attack Methods
    private IEnumerator UseAttack(PlayerAttackInfo attack)
    {

        cc_Rb.rotation = Quaternion.Euler(0, m_CameraTransform.eulerAngles.y, 0);
        Quaternion new_Rotation = cc_Rb.rotation;
        cr_Anim.SetTrigger(attack.TriggerName);
        IEnumerator toColor = ChangeColor(attack.AbilityColor, 10);
        StartCoroutine(toColor);
        yield return new WaitForSeconds(attack.WindUpTime);

        Vector3 offset = transform.forward * attack.Offset.z + transform.right * attack.Offset.x + transform.up * attack.Offset.y;
        Vector3 spawnPos = transform.position + offset;
        if (attack.Bomb)
        {
            spawnPos = new Vector3(transform.position.x, 40, transform.position.z);
            Quaternion current_Rotation = attack.AbilityGO.transform.rotation;
            new_Rotation = new Quaternion(-current_Rotation.x, current_Rotation.y, current_Rotation.z, current_Rotation.w);
            
        }
        GameObject go = Instantiate(attack.AbilityGO, spawnPos, new_Rotation);
        go.GetComponent<Ability>().Use(spawnPos);

        StopCoroutine(toColor);
        StartCoroutine(ChangeColor(p_DefaultColor, 50));
        yield return new WaitForSeconds(attack.Cooldown);

        attack.ResetCooldown();
    }

    #endregion

    #region Misc Methods
    private IEnumerator ChangeColor(Color newColor, float speed)
    {
        Color curColor = cr_Renderer.material.color;
        while (curColor != newColor)
        {
            curColor = Color.Lerp(curColor, newColor, speed / 100);
            cr_Renderer.material.color = curColor;
            yield return null;
        }
    }

    #endregion

    #region Collision Methods
    private void OnTriggerEnter(Collider other)
    {
         if (other.CompareTag("HealthPill"))
        {
            IncreaseHealth(other.GetComponent<HealthPill>().HealthGain);
            Destroy(other.gameObject);
        }
    }

    #endregion


}
