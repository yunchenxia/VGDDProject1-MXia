using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombLaserAttack : Ability
{
    public override void Use(Vector3 spawnPos)

    {
        Debug.Log(spawnPos);
        RaycastHit[] hits = Physics.SphereCastAll(spawnPos, 10f, transform.forward, m_Info.Range);
        Debug.Log(-transform.up);
        foreach(RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.GetComponent<EnemyController>().DecreaseHealth(m_Info.power);            
            }
        }
        var emitterShape = cc_PS.shape;
        emitterShape.length = m_Info.Range;
        cc_PS.Play();
    }
}
