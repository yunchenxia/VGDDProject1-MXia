﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    protected AbilityInfo m_Info;
    #endregion

    #region Cached Components
    protected ParticleSystem cc_PS;
    #endregion

    #region Intialization
    private void Awake()
    {
        cc_PS = GetComponent<ParticleSystem>();
    }
    #endregion

    #region Use Methods
    public abstract void Use(Vector3 spawnPos);
    #endregion



}
