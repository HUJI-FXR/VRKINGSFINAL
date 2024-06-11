using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class RaycastFromHand : MonoBehaviour
{
    void FireRay()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitData;

        Physics.Raycast(ray, out hitData);
    }

    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitData;
        Physics.Raycast(ray, out hitData);
        
        if (hitData.collider)
        {
            Image curImage = hitData.collider.gameObject.GetComponent<Image>();
            if (curImage == null) {
                return;

            }

            Sprite img = curImage.sprite;

            var croppedTexture = new Texture2D((int)img.rect.width, (int)img.rect.height);
            var pixels = img.texture.GetPixels((int)img.textureRect.x,
                                                    (int)img.textureRect.y,
                                                    (int)img.textureRect.width,
                                                    (int)img.textureRect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            if (img != null)
            {
                Sprite curSprite = Sprite.Create(croppedTexture, new Rect(0.0f, 0.0f, croppedTexture.width, croppedTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                Image curFinImage = GetComponent<Image>();
                curFinImage.sprite = curSprite;
                Debug.Log("aaaa");
            }
            //Debug.Log("Hit " + hitData.collider);
        }
        else
        {
            //Debug.Log("Hit nothing!");
        }
    }
}
