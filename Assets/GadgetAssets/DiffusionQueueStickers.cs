using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiffusionQueueStickers : MonoBehaviour
{
    public Material ReadyStickerMaterial;
    public Material WaitingStickerMaterial;

    private List<GameObject> _stickers = new List<GameObject>();
    private Renderer renderer;

    private int currentDiffusionImageCount = 0;
    private int MAX_DIFFUSION_IMAGES;

    // Start is called before the first frame update
    private void Start()
    {
        foreach (Transform child in transform)
        {
            _stickers.Add(child.gameObject);
        }

        // TODO download Linq for sorting: https://discussions.unity.com/t/how-to-sort-a-list-of-gameobjects-by-their-name/58831

        //_stickers.Sort();
        _stickers.Reverse();

        MAX_DIFFUSION_IMAGES = _stickers.Count;

        UpdateStickers(3);
    }

    public void AddOne()
    {
        if (currentDiffusionImageCount < MAX_DIFFUSION_IMAGES)
        {
            Debug.LogError("Tried to increase Diffusion image Sticker progress bar while it's already full");
            return;
        }
        currentDiffusionImageCount++;
        UpdateStickers(currentDiffusionImageCount);
    }

    public void RemoveOne()
    {
        if (currentDiffusionImageCount <= 0)
        {
            Debug.LogError("Tried to decrease Diffusion image Sticker progress bar while it's already empty");
            return;
        }
        currentDiffusionImageCount--;
        UpdateStickers(currentDiffusionImageCount);
    }

    private void UpdateStickers(int num)
    {
        if (ReadyStickerMaterial == null || WaitingStickerMaterial == null)
        {
            Debug.LogError("Please the Ready and Waiting sticker materials for the Diffusion image progress bar");
            return;
        }
        if (_stickers.Count < num)
        {
            Debug.LogError("Too many Diffusion images ready, the progress bar is not big enough");
            return;
        }

        for (int i  = 0; i < num; i++)
        {
            renderer = _stickers[i].GetComponent<Renderer>();
            renderer.material = ReadyStickerMaterial;
        }

        for (int i = num; i < _stickers.Count; i++)
        {
            renderer = _stickers[i].GetComponent<Renderer>();
            renderer.material = WaitingStickerMaterial;
        }
    }
}
