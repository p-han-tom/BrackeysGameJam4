﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAbility : MonoBehaviour
{
    public void onEnterAbility() {
        GetComponent<Player>().isAttacking = true;
    } 

    public void onExitAbility() {
        GetComponent<Player>().isAttacking = false;
    }

    public bool cancelAbility = false;
    public void attackCheckpoint() {
        if (cancelAbility) {
            cancelAbility = false;
            GetComponent<Player>().isAttacking = false;
            GetComponent<Animator>().Rebind();
        }
    }
}
