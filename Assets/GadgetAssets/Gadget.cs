using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    //public TextMeshProUGUI ButtonText;

    public ScreenRecorder screenRecorder = null;
    public Camera gadgetCamera;
    public Camera xrCamera;    

    public PlayGadgetSounds playGadgetSounds;

    public GameObject gadgetImagePanel;


    // Strategy Design Pattern
    public List<GadgetMechanism> GadgetMechanisms;
    private int gadgetMechanismIndex = 0;

    // TODO make this Queue<List<Texture2D>>
    [NonSerialized]
    public Queue<Texture2D> textureQueue;

    private Gadget gadget;

    public AIGadgetAssistant aiGadgetAssistant;
    private void Awake()
    {
        //GadgetMechanisms = new List<GadgetMechanism>();
        textureQueue = new Queue<Texture2D>();
    }

    private void Start()
    {
        if (gadgetCamera == null || xrCamera == null || playGadgetSounds == null || gadgetImagePanel == null || aiGadgetAssistant == null)
        {
            Debug.LogError("Add all requirements of Gadget");
            return;
        }
        /*gadget = GetComponent<Gadget>();
        GadgetMechanism cameraGadgetMechanism = new CameraGadgetMechanism(gadget, screenRecorder, gadgetCamera, xrCamera);
        GadgetMechanism combineImagesGadgetMechanism = new CombineImagesGadgetMechanism(gadget);
        GadgetMechanism throwingGadgetMechanism = new ThrowingGadgetMechanism(gadget);
        GadgetMechanisms.Add(cameraGadgetMechanism);
        GadgetMechanisms.Add(combineImagesGadgetMechanism);
        GadgetMechanisms.Add(throwingGadgetMechanism);*/

        //ChangeToMechanic(0);
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
        //GadgetMechanisms[gadgetMechanismIndex].OnUIHoverEntered(args);
    }
    /*public void OnUIHoverExited(UIHoverEventArgs args)
    {
        GadgetMechanisms[gadgetMechanismIndex].OnUIHoverExited(args);
    }*/
    public void OnClick()
    {
        GadgetMechanisms[gadgetMechanismIndex].OnClick();
    }


    // todo THESE 2 move this out of gadget, bad design need it in camera mechanism with correct input system of game.
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
        //ButtonText.text = GadgetMechanisms[index].buttonText;
    }

    public Texture2D getGeneratedTexture()
    {        
        if (textureQueue.Count == 0)
        {
            return null;
        }

        Texture2D current = textureQueue.Dequeue();
        if (textureQueue.Count == 0)
        {
            GameManager.getInstance().uiDiffusionTexture.CreateImagesInside(new List<Texture2D>(), gadgetImagePanel, true);
        }
        else
        {
            GameManager.getInstance().uiDiffusionTexture.CreateImagesInside(textureQueue.ToList<Texture2D>(), gadgetImagePanel, true);
        }
            
        return current;
    }

    public bool AddTexturesToQueue(List<Texture2D> textures)
    {
        GameManager.getInstance().uiDiffusionTexture.CreateImagesInside(textures, gadgetImagePanel, true);

        foreach (Texture2D texture in textures)
        {
            textureQueue.Enqueue(texture);
        }

        return true;
    }



    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //
    public void ChangeMechanicInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ChangeToNextMechanic();
        }
    }
    public void TakeTextureInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GadgetMechanisms[gadgetMechanismIndex].TakeTextureInput(context);
            Debug.Log("Taking Texture");
        }
    }
    public void PlaceTextureInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GadgetMechanisms[gadgetMechanismIndex].PlaceTextureInput(context);
            Debug.Log("Placing Texture");
        }
    }
    public void ActivateGeneration(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GadgetMechanisms[gadgetMechanismIndex].ActivateGeneration(context);
            Debug.Log("Generating Texture");
        }
    }
    public void TakeScreenshot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GadgetMechanisms[gadgetMechanismIndex].TakeScreenshot(context);
            Debug.Log("Taking Screenshot");
        }
    }
    public void GeneralActivation(DiffusionTextureChanger dtc)
    {
        GadgetMechanisms[gadgetMechanismIndex].GeneralActivation(dtc);
        Debug.Log("Activating General Generation");
        return;
    }
}
