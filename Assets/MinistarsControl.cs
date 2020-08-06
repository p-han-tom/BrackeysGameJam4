﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinistarsControl : MonoBehaviour
{
    public string level;
    private Sprite fullMinistar;
    public Sprite emptyMinistar;
    private int stars;
    void Start() {
        fullMinistar = transform.GetChild(0).GetComponent<Image>().sprite;   
    }
    public void UpdateMinistars() {
        stars = PlayerPrefs.GetInt("Level"+level+"Stars", 0);
        for (int i = 2; i >= 0; i--) {
            transform.GetChild(i).GetComponent<Image>().sprite = fullMinistar;
        }
        for (int i = 2; i > stars-1; i--) {
            transform.GetChild(i).GetComponent<Image>().sprite = emptyMinistar;
        }
    }
}
