﻿using UnityEngine;
using System.Collections;

public class ToggleButtonController : MonoBehaviour 
{
    public GameObject contain;
    public bool isDefaultOn;
    private Animator animator;
    private bool isButtonOn;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (isDefaultOn)
        {
            OnClick();
            isDefaultOn = false;
        }
    }

    public void OnClick()
    {        
        if (!isButtonOn)
        {
            if (!isDefaultOn)
                SoundManager.Instance.playButtonSound();
            animator.Play("Pressed", -1, 0);
            isButtonOn = true;
            contain.SetActive(true);
        }
    }

    public void ResetNormal()
    {
        animator.Play("Normal", -1, 0);
        isButtonOn = false;
        contain.SetActive(false);
    }
}
