using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpOnEnable : MonoBehaviour
{

    [SerializeField] private UIDiffusionTexture diffusion;
    [SerializeField] private List<Texture2D> textures;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        Debug.Log("Pop up!");
        diffusion.CreatePopup(textures);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
