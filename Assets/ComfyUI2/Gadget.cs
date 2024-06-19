using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;


public enum GadgetSelection
{
    unSelected,
    preSelected,
    selected
}

// TODO change name of class to GadgetScript or something
public class Gadget : MonoBehaviour
{
    /*[NonSerialized]
    public DiffusionRequest diffusionRequest;*/

    public TextMeshProUGUI MechanismText;
    public TextMeshProUGUI ButtonText;

    public ScreenRecorder screenRecorder = null;
    public Camera gadgetCamera;
    public Camera xrCamera;    

    public PlayGadgetSounds playGadgetSounds;

    public GameObject gadgetImagePanel;


    // Strategy Design Pattern
    private List<GadgetMechanism> GadgetMechanisms = new List<GadgetMechanism>();
    private int gadgetMechanismIndex = 0;

    // TODO make this Queue<List<Texture2D>>
    public Queue<Texture2D> textureQueue = new Queue<Texture2D>();

    private Gadget gadget;
    private void Start()
    {
        if (gadgetCamera == null || xrCamera == null || playGadgetSounds == null || gadgetImagePanel == null)
        {
            Debug.LogError("Add all requirements of Gadget");
            return;
        }
        gadget = GetComponent<Gadget>();
        GadgetMechanism cameraGadgetMechanism = new CameraGadgetMechanism(gadget, screenRecorder, gadgetCamera, xrCamera);
        GadgetMechanism combineImagesGadgetMechanism = new CombineImagesGadgetMechanism(gadget);
        GadgetMechanism throwingGadgetMechanism = new ThrowingGadgetMechanism(gadget);
        GadgetMechanisms.Add(cameraGadgetMechanism);
        GadgetMechanisms.Add(combineImagesGadgetMechanism);
        GadgetMechanisms.Add(throwingGadgetMechanism);

        ChangeToMechanic(0);
    }

    // Passing along the various 
    public void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {        
        playGadgetSounds.PlaySound("HoverOverElements");
        GadgetMechanisms[gadgetMechanismIndex].OnGameObjectHoverEntered(args);
    }
    public void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        GadgetMechanisms[gadgetMechanismIndex].OnGameObjectHoverExited(args);
    }
    public void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        playGadgetSounds.PlaySound("SelectElement");
        GadgetMechanisms[gadgetMechanismIndex].onGameObjectSelectEntered(args);
    }
    public void onGameObjectSelectExited(SelectExitEventArgs args)
    {
        GadgetMechanisms[gadgetMechanismIndex].onGameObjectSelectExited(args);
    }
    public void OnUIHoverEntered(UIHoverEventArgs args)
    {
        playGadgetSounds.PlaySound("HoverOverElements");
        GadgetMechanisms[gadgetMechanismIndex].OnUIHoverEntered(args);
    }
    public void OnUIHoverExited(UIHoverEventArgs args)
    {
        GadgetMechanisms[gadgetMechanismIndex].OnUIHoverExited(args);
    }
    public void OnClick()
    {
        GadgetMechanisms[gadgetMechanismIndex].OnClick();
    }


    // todo THESE 3 move this out of gadget, bad design need it in camera mechanism with correct input system of game.
    public void TakePicture()
    {
        if (GadgetMechanisms.Count <= 0)
        {
            return;
        }
        if (GadgetMechanisms[gadgetMechanismIndex].GetType() != typeof(CameraGadgetMechanism))
        { 
            return;
        }
        if ((GadgetMechanisms[gadgetMechanismIndex] as CameraGadgetMechanism).takingPicture)
        {
            GadgetMechanisms[gadgetMechanismIndex].OnGameObjectHoverExited(null);
        }        
    }

    public void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0)
        {
            return;
        }
        if (GadgetMechanisms[gadgetMechanismIndex].GetType() != typeof(ThrowingGadgetMechanism))
        {
            return;
        }
        (GadgetMechanisms[gadgetMechanismIndex] as ThrowingGadgetMechanism).DiffusableGrabbed(args);
    }
    public void DiffusableUnGrabbed(SelectExitEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0)
        {
            return;
        }
        if (GadgetMechanisms[gadgetMechanismIndex].GetType() != typeof(ThrowingGadgetMechanism))
        {
            return;
        }
        (GadgetMechanisms[gadgetMechanismIndex] as ThrowingGadgetMechanism).DiffusableUnGrabbed(args);
    }

    private void DeleteOutline(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }
        if (obj.TryGetComponent<Outline>(out Outline curOutline))
        {
            Destroy(curOutline);
        }
    }
    public void ChangeOutline(GameObject obj, GadgetSelection gadgetSelection)
    {
        if (obj == null)
        {
            return;
        }

        // Only GameObjects with valid textures
        if (obj.GetComponent<Renderer>() == null )
        {
            return;
        }
        if (obj.GetComponent<Renderer>().material.mainTexture == null)
        {
            return;
        }

        Color curColor = Color.black;
        float outlineWidth = 0;
        switch (gadgetSelection)
        {
            case GadgetSelection.unSelected:
                DeleteOutline(obj);
                return;
            case GadgetSelection.preSelected:
                curColor = new Color(0, 0, 255); ;
                outlineWidth = 20;
                break;
            case GadgetSelection.selected:
                curColor = new Color(0, 255, 0); ;
                outlineWidth = 20;
                break;
        }

        if (obj.TryGetComponent<Outline>(out Outline curOutline))
        {
            curOutline.OutlineColor = curColor;
            curOutline.OutlineWidth = outlineWidth;
            return;
        }
        Outline elseOutline = obj.AddComponent<Outline>();
        elseOutline.OutlineColor = curColor;
        elseOutline.OutlineWidth = outlineWidth;
    }

    // For managing the current Diffusion Mechanism
    public void ChangeToNextMechanic()
    {
        gadgetMechanismIndex++;
        gadgetMechanismIndex %= GadgetMechanisms.Count;
        ChangeToMechanic(gadgetMechanismIndex);
    }
    public void ChangeToPreviousMechanic()
    {
        gadgetMechanismIndex--;
        gadgetMechanismIndex %= GadgetMechanisms.Count;
        ChangeToMechanic(gadgetMechanismIndex);
    }
    public void ChangeToMechanic(int index)
    {
        textureQueue.Clear();
        gadgetMechanismIndex = index;
        MechanismText.text = GadgetMechanisms[index].mechanismText;
        ButtonText.text = GadgetMechanisms[index].buttonText;
    }

    public Texture2D getGeneratedTexture()
    {        
        if (textureQueue.Count == 0)
        {
            GeneralGameScript.instance.uiDiffusionTexture.CreateImagesInside(new List<Texture2D>(), gadgetImagePanel, true);
            return null;
        }

        Texture2D current = textureQueue.Dequeue();
        GeneralGameScript.instance.uiDiffusionTexture.CreateImagesInside(textureQueue.ToList<Texture2D>(), gadgetImagePanel, true);
        return current;
    }

    public bool AddTexturesToQueue(List<Texture2D> textures)
    {
        GeneralGameScript.instance.uiDiffusionTexture.CreateImagesInside(textures, gadgetImagePanel, true);

        foreach (Texture2D texture in textures)
        {
            textureQueue.Enqueue(texture);
        }

        return true;
    }
}
