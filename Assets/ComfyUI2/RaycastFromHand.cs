using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class RaycastFromHand : MonoBehaviour
{
    public GameObject Diffusables;

    void FireRay()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitData;

        Physics.Raycast(ray, out hitData);
    }

    public void OnUIHoverEntered(UIHoverEventArgs args)
    {
        if (args == null || args.uiObject == null) {
            return;
        }

        if (args.uiObject.TryGetComponent<UnityEngine.UI.Image>(out UnityEngine.UI.Image innerImg))
        {
            GetComponent<Image>().sprite = innerImg.sprite;          
        }
    }

    public void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log("kkk");
        if (Diffusables == null)
        {
            return;
        }

        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != Diffusables.transform)
        {
            Debug.Log("IS not parent");
            return;
        }
        Debug.Log("IS parent");
        Outline curOutline = args.interactableObject.transform.gameObject.AddComponent<Outline>();
        curOutline.OutlineColor = new Color(100, 100, 100);
        curOutline.OutlineWidth = 50;
    }

    public void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (Diffusables == null)
        {
            return;
        }

        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != Diffusables.transform)
        {
            
            return;
        }

        if (args.interactableObject.transform.gameObject.TryGetComponent<Outline>(out Outline curOutline))
        {
            Destroy(curOutline);
            return;
        }
    }

    /*private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitData;
        Physics.Raycast(ray, out hitData);
        
        if (hitData.collider == null)
        {
            return;
        }
        if (hitData.collider.gameObject != null)
        {
            Debug.Log("rrrr");
            if (!hitData.collider.gameObject.TryGetComponent<UnityEngine.UI.Image>(out UnityEngine.UI.Image innerImg))
            {
                Debug.Log("zzzz");
                return;
            }
            Debug.Log("ggggggg");

            Sprite img = innerImg.sprite;

           *//* var croppedTexture = new Texture2D((int)img.rect.width, (int)img.rect.height);
            var pixels = img.texture.GetPixels((int)img.textureRect.x,
                                                    (int)img.textureRect.y,
                                                    (int)img.textureRect.width,
                                                    (int)img.textureRect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();*//*

            if (img != null)
            {
                *//*Sprite curSprite = Sprite.Create(croppedTexture, new Rect(0.0f, 0.0f, croppedTexture.width, croppedTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                Image curFinImage = GetComponent<Image>();*//*
                GetComponent<Image>().sprite = img;
                Debug.Log("aaaa");
            }
            //Debug.Log("Hit " + hitData.collider);
        }
        else
        {
            //Debug.Log("Hit nothing!");
        }
    }*/
}
