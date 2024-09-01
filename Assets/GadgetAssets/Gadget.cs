using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
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
    /// <summary>
    /// TextMeshPro object that shows the text that represents the current Mechanism.
    /// </summary>
    public GameObject displayTexturesGadget;
    
    /// <summary>
    /// TextMeshPro object that shows the text that represents the current Mechanism.
    /// </summary>
    public TextMeshProUGUI MechanismText;
    public GameObject gadgetImagePanel;

    /// <summary>
    /// Plays the sounds associated with the Gadget's actions.
    /// </summary>
    public PlayGadgetSounds playGadgetSounds;

    /// <summary>
    /// Manages the AI Gadget Assistant.
    /// </summary>
    public AIGadgetAssistant aiGadgetAssistant;

    /// <summary>
    /// Used for shooting images with virtual cameras.
    /// </summary>
    //public ScreenRecorder screenRecorder = null;

    /// <summary>
    /// Left hand Gadget Camera for taking pictures.
    /// </summary>
    //public Camera gadgetCamera;

    /// <summary>
    /// XR Origin Main Camera.
    /// </summary>
    public Camera xrCamera;

    // Strategy Design Pattern
    /// <summary>
    /// Gadget Mechanisms list to be cycled through.
    /// </summary>
    [NonSerialized]
    public List<GadgetMechanism> GadgetMechanisms;
    // Represents the current used Gadget Mechanism.
    private int gadgetMechanismIndex = 0;

    // TODO make this Queue<List<Texture2D>>, talk to NADAV on gadget representation of special textures
    [NonSerialized]
    public Queue<Texture2D> textureQueue;

    // Controllers, for symmetric input
    public GameObject LeftHandController;
    public GameObject RightHandController;
    private string LeftHandBuildControllerInputName = "OculusTouchControllerLeft";
    private string RightHandBuildControllerInputName = "OculusTouchControllerRight";
    private string LeftHandLinkControllerInputName = "OculusTouchControllerOpenXR";
    private string RightHandLinkControllerInputName = "OculusTouchControllerOpenXR1";
    private string LeftHandSimulatedControllerInputName = "XRSimulatedController";
    private string RightHandSimulatedControllerInputName = "XRSimulatedController1";

    private void Awake()
    {
        textureQueue = new Queue<Texture2D>();
    }

    private void Start()
    {
        //gadgetCamera == null || 
        if (xrCamera == null || playGadgetSounds == null || gadgetImagePanel == null || aiGadgetAssistant == null ||
            LeftHandController == null || RightHandController == null)
        {
            Debug.LogError("Add all requirements of Gadget");
            return;
        }
    }

    // Passing along the various Controller interactions onto the Mechanisms.
    public void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;

        playGadgetSounds.PlaySound("HoverOverElements");
        GadgetMechanisms[gadgetMechanismIndex].OnGameObjectHoverEntered(args);
    }
    public void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[gadgetMechanismIndex].OnGameObjectHoverExited(args);
    }

    public void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;

        playGadgetSounds.PlaySound("SelectElement");
        GadgetMechanisms[gadgetMechanismIndex].onGameObjectSelectEntered(args);
    }
    public void onGameObjectSelectExited(SelectExitEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[gadgetMechanismIndex].onGameObjectSelectExited(args);
    }

    // TODO do I need these 2?
    /*public void OnUIHoverEntered(UIHoverEventArgs args)
    {
        playGadgetSounds.PlaySound("HoverOverElements");
        //GadgetMechanisms[gadgetMechanismIndex].OnUIHoverEntered(args);
    }*/
    /*public void OnClick()
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[gadgetMechanismIndex].OnClick();
    }*/


    // todo THESE 2 move this out of gadget, bad design need it in camera mechanism with correct input system of game.
    public void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if (GadgetMechanisms.Count <= 0) return;
        if (GadgetMechanisms[gadgetMechanismIndex].GetType() != typeof(ThrowingGadgetMechanism)) return;

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

    /// <summary>
    /// Helper function for ChangeOutline that destroys an existing outline on an outlined GameObject
    /// </summary>
    /// <param name="obj"></param>
    private void DeleteOutline(GameObject obj)
    {
        if (obj == null) return;
        if (obj.TryGetComponent<Outline>(out Outline curOutline)) Destroy(curOutline);
    }

    /// <summary>
    /// Changes the outlining on a given GameObject
    /// </summary>
    /// <param name="obj">GameObject to change outlining on</param>
    /// <param name="gadgetSelection">Outlining to change onto</param>
    public void ChangeOutline(GameObject obj, GadgetSelection gadgetSelection)
    {
        if (obj == null)    return;

        // Only GameObjects with valid textures
        if (obj.GetComponent<Renderer>() == null ) return;
        if (obj.GetComponent<Renderer>().material.mainTexture == null && obj.GetComponent<DiffusableObject>() == null)  return;

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
        if (GadgetMechanisms.Count <= 0) return;

        gadgetMechanismIndex++;
        gadgetMechanismIndex %= GadgetMechanisms.Count;
        ChangeToMechanic(gadgetMechanismIndex);
    }
    public void ChangeToPreviousMechanic()
    {
        if (GadgetMechanisms.Count <= 0) return;

        gadgetMechanismIndex--;
        gadgetMechanismIndex %= GadgetMechanisms.Count;
        ChangeToMechanic(gadgetMechanismIndex);
    }
    public void ChangeToMechanic(int index)
    {
        if (GadgetMechanisms.Count <= 0) return;

        // COMMENTED OUT, WHY IS THIS NECESSARY?
        // textureQueue.Clear();
        
        gadgetMechanismIndex = index;
        MechanismText.text = GadgetMechanisms[index].mechanismText;
    }

    public Texture2D getGeneratedTexture()
    {        
        if (textureQueue.Count == 0) return null;

        Texture2D current = textureQueue.Dequeue();
        GameManager.getInstance().uiDiffusionTexture.CreateImagesInside(new List<Texture2D>(textureQueue), displayTexturesGadget, true);
        
        // TODO the gadgetImagePanel is simply the panel onwhich we see the popups, we should only use it through the
        // TODO if the above it correct, gadgetImagePanel is unneeded in this script, DISCUSS this
        // CreatePopup function, else it breaks the panel.
        /*if (textureQueue.Count == 0)
        {
            GameManager.getInstance().uiDiffusionTexture.CreateImagesInside(new List<Texture2D>(), gadgetImagePanel, true);
        }
        else
        {
            GameManager.getInstance().uiDiffusionTexture.CreateImagesInside(textureQueue.ToList<Texture2D>(), gadgetImagePanel, true);
        }*/
            
        return current;
    }

    public bool AddTexturesToQueue(List<Texture2D> textures)
    {
        // TODO WHY IS THIS HERE, UNCOMMENT MAYBE?
        //GameManager.getInstance().uiDiffusionTexture.CreateImagesInside(textures, gadgetImagePanel, true);
        
        if (textures == null) return false;

        foreach (Texture2D texture in textures)
        {
            textureQueue.Enqueue(texture);
        }
        
        GameManager.getInstance().uiDiffusionTexture.CreateImagesInside(new List<Texture2D>(textureQueue), displayTexturesGadget, true);

        return true;
    }



    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //

    /// <summary>
    /// Function that finds the controller that performed an action given the action's context
    /// </summary>
    /// <returns>GameObject representing the Controller</returns>
    public GameObject GetActionController(InputAction.CallbackContext context)
    {
        var device = context.control.device;

        if (device.name == LeftHandBuildControllerInputName || device.name == LeftHandLinkControllerInputName ||  
            device.name == LeftHandSimulatedControllerInputName)
        {
            return LeftHandController;
        }
        else if (device.name == RightHandBuildControllerInputName || device.name == RightHandLinkControllerInputName || 
            device.name == RightHandSimulatedControllerInputName)
        {
            return RightHandController;
        }

        return null;
    }


    // TODO should all these input functions below not send context onward? is context necessary for the specific mechanisms? should they deal with it, or Gadget?

    public void ChangeMechanicInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ChangeToNextMechanic();
        }
    }
    public void PlaceTextureInput(InputAction.CallbackContext context)
    {
        if (GadgetMechanisms.Count <= 0) return;
        
        if (context.performed)
        {
            GameObject curController = GetActionController(context);

            GadgetMechanisms[gadgetMechanismIndex].PlaceTextureInput(curController);
            Debug.Log("Placing Texture");
        }
    }
    public void ActivateGeneration(InputAction.CallbackContext context)
    {        
        if (GadgetMechanisms.Count <= 0) return;

        if (context.performed)
        {            
            GadgetMechanisms[gadgetMechanismIndex].ActivateGeneration(null);
            Debug.Log("Generating Texture");
        }
    }

    // TODO change name of this function look at GadgetMechanism TODO note
    public void TakeScreenshot(Texture2D screenshot, Camera camera)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[gadgetMechanismIndex].TakeScreenshot(screenshot, camera);
        Debug.Log("Taking Screenshot");
    }
    public void GeneralActivation(DiffusionTextureChanger dtc)
    {
        if (GadgetMechanisms.Count <= 0) return;

        GadgetMechanisms[gadgetMechanismIndex].GeneralActivation(dtc);
        Debug.Log("Activating General Generation");
        return;
    }
}
