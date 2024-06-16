using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
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
    public DiffusionRequest diffusionRequest;

    public TextMeshProUGUI MechanismText;
    public TextMeshProUGUI ButtonText;

    public ScreenRecorder screenRecorder = null;
    public Camera gadgetCamera;
    public Camera xrCamera;

    // Strategy Design Pattern
    public List<GadgetMechanism> GadgetMechanisms = new List<GadgetMechanism>();
    public int gadgetMechanismIndex = 0;

    // TODO make this Queue<List<Texture2D>>
    public Queue<Texture2D> textureQueue = new Queue<Texture2D>();

    private Gadget gadget;
    private void Start()
    {
        if (gadgetCamera == null || xrCamera == null)
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
    public virtual void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        GadgetMechanisms[gadgetMechanismIndex].OnGameObjectHoverEntered(args);
    }
    public virtual void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        GadgetMechanisms[gadgetMechanismIndex].OnGameObjectHoverExited(args);
    }
    public virtual void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        GadgetMechanisms[gadgetMechanismIndex].onGameObjectSelectEntered(args);
    }
    public virtual void onGameObjectSelectExited(SelectExitEventArgs args)
    {
        GadgetMechanisms[gadgetMechanismIndex].onGameObjectSelectExited(args);
    }
    public virtual void OnUIHoverEntered(UIHoverEventArgs args)
    {
        GadgetMechanisms[gadgetMechanismIndex].OnUIHoverEntered(args);
    }
    public virtual void OnUIHoverExited(UIHoverEventArgs args)
    {
        GadgetMechanisms[gadgetMechanismIndex].OnUIHoverExited(args);
    }
    public virtual void OnClick()
    {
        GadgetMechanisms[gadgetMechanismIndex].OnClick();
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
        gadgetMechanismIndex = index;
        MechanismText.text = GadgetMechanisms[index].mechanismText;
        ButtonText.text = GadgetMechanisms[index].buttonText;
    }

    public Texture2D getGeneratedTexture()
    {
        if (textureQueue.Count == 0)
        {
            return null;
        }
        return textureQueue.Dequeue();
    }
}
