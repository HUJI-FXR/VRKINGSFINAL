using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;


// TODO need to remove button from several of these, choose exactly
public class GadgetMechanism : Object
{
    public static string MECHANISM_PRETEXT = "Mechanism:\n";
    
    public string mechanismText;
    public string buttonText;
    public Gadget gadget;
    public GadgetMechanism(Gadget gadget)
    {
        this.mechanismText = "";
        this.buttonText = "";
        this.gadget = gadget;
    }

    public virtual void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        return;
    }
    public virtual void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        return;
    }
    public virtual void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        return;
    }
    public virtual void onGameObjectSelectExited(SelectExitEventArgs args)
    {
        return;
    }
    public virtual void OnUIHoverEntered(UIHoverEventArgs args)
    {
        return;
    }
    public virtual void OnUIHoverExited(UIHoverEventArgs args)
    {
        return;
    }
    public virtual void OnClick()
    {
        return;
    }

    // TODO do I even need these two?
    public virtual void OnActivate(ActivateEventArgs args)
    {
        return;
    }
    public virtual void OnDeActivate(DeactivateEventArgs args)
    {
        return;
    }
}

public class CombineImagesGadgetMechanism : GadgetMechanism
{
    // TODO notice this CAN be generalized to larger number of objects, but requires many changes.
    public Queue<GameObject> selectedObjects = new Queue<GameObject>();
    public int MAX_QUEUED_OBJECTS = 2;

    private DiffusionRequest diffusionRequest;

    public CombineImagesGadgetMechanism(Gadget gadget) : base(gadget)
    {        
        mechanismText = MECHANISM_PRETEXT + "Combine Two Images";
        buttonText = "Combine";

        diffusionRequest = new DiffusionRequest();
        diffusionRequest.positivePrompt = "Beautiful";
        diffusionRequest.negativePrompt = "watermark";
        diffusionRequest.numOfVariations = 5;
        diffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
        diffusionRequest.diffusionJsonType = diffusionWorkflows.combineImages;
        diffusionRequest.diffusionModel = diffusionModels.ghostmix;
    }

    public override void OnUIHoverEntered(UIHoverEventArgs args)
    {
        if (args == null || args.uiObject == null)
        {
            return;
        }

        /*if (args.uiObject.TryGetComponent<UnityEngine.UI.Image>(out UnityEngine.UI.Image innerImg))
        {
            GetComponent<Image>().sprite = innerImg.sprite;
        }*/
    }

    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform)
        {
            return;
        }

        if (selectedObjects.Contains(args.interactableObject.transform.gameObject))
        {
            return;
        }
        // Creates pre-selection outline
        gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform)
        {

            return;
        }

        // Remove pre-selection outline
        if (selectedObjects.Contains(args.interactableObject.transform.gameObject))
        {
            return;
        }
        gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform)
        {
            return;
        }

        if (selectedObjects.Contains(args.interactableObject.transform.gameObject))
        {
            return;
        }
        // Adds to queue of selected objects
        if (selectedObjects.Count >= MAX_QUEUED_OBJECTS)
        {
            GameObject dequeObject = selectedObjects.Dequeue();
            gadget.ChangeOutline(dequeObject, GadgetSelection.unSelected);
        }

        selectedObjects.Enqueue(args.interactableObject.transform.gameObject);
        // Creates selection outline
        gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
    }

    public override void OnClick()
    {
        GetTexturesFromSelected();
    }

    public void GetTexturesFromSelected()
    {
        if (selectedObjects.Count != MAX_QUEUED_OBJECTS)
        {
            return;
        }

        GameObject firstGameObject = selectedObjects.Dequeue();
        GameObject secondGameObject = selectedObjects.Dequeue();
        Texture go1Text = firstGameObject.GetComponent<Renderer>().material.mainTexture;
        Texture go2Text = secondGameObject.GetComponent<Renderer>().material.mainTexture;

        gadget.ChangeOutline(firstGameObject, GadgetSelection.unSelected);
        gadget.ChangeOutline(secondGameObject, GadgetSelection.unSelected);

        Texture2D copyTexture = GameManager.getInstance().comfySceneLibrary.toTexture2D(go1Text);
        Texture2D secondCopyTexture = GameManager.getInstance().comfySceneLibrary.toTexture2D(go2Text);

        string uniqueName = GameManager.getInstance().comfyOrganizer.UniqueImageName();
        copyTexture.name = uniqueName + ".png";
        secondCopyTexture.name = uniqueName + "_2" + ".png";

        diffusionRequest.uploadImage = copyTexture;
        diffusionRequest.secondUploadImage = secondCopyTexture;

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }
}

public class CameraGadgetMechanism : GadgetMechanism
{    
    // todo break into two parts, one the input the other the output through the comfy lib
    private ScreenRecorder screenRecorder;
    private Camera mechanismCamera;
    private Camera XRCamera;
    private DiffusionRequest diffusionRequest;    

    public bool takingPicture = true;
    public CameraGadgetMechanism(Gadget gadget, ScreenRecorder screenRecorder, Camera camera, Camera xrCamera) : base(gadget)
    {
        this.screenRecorder = screenRecorder;
        this.mechanismCamera = camera;
        this.XRCamera = xrCamera;
        this.mechanismText = MECHANISM_PRETEXT + "Camera";
        this.buttonText = "Generate";

        diffusionRequest = new DiffusionRequest();
        diffusionRequest.positivePrompt = "Beautiful scene";
        diffusionRequest.negativePrompt = "watermark";
        diffusionRequest.numOfVariations = 5;
        diffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
        diffusionRequest.diffusionModel = diffusionModels.ghostmix;
        diffusionRequest.denoise = 0.4f;
    }

    public override void OnClick()
    {
        // TODO change mechanismText to correspond to the current takingPicture status?
        // TODO add base call to OnClick for sound? need sound of click(camera sound)
        if (takingPicture)
        {
            takingPicture = false;
        }
        else
        {
            takingPicture = true;
        }
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {        
        if (takingPicture)
        {
            // TODO add DiffusableObject data entry for diffusionrequest when taking a picture of stuff
            screenRecorder.CaptureScreenshot(diffusionRequest);
            mechanismCamera.enabled = false;
            XRCamera.enabled = true;

        }
        else
        {
            Texture2D curTexture = gadget.getGeneratedTexture();            
            if (curTexture == null)
            {
                Debug.LogError("Tried to add a textures from the Gadget camera without textures in the Queue");
                return;
            }

            // Perform the raycast
            Ray ray = new Ray(mechanismCamera.transform.position, mechanismCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit GameObject has the TextureScript component
                

                //TODO maybe this is a wrong choice to make ANOTHER diffusion request because this one isn't really a diffusion request at all, just a transfer from one
                // texturechange to another
                if (hit.collider.gameObject.TryGetComponent<DiffusionTextureChanger>(out DiffusionTextureChanger dtc))
                {
                    Debug.Log("wewew");
                    dtc.AddTexture(new List<Texture2D>() { curTexture }, false);
                }
                Debug.Log("kklk");
            }
        }        
    }
}

public class ThrowingGadgetMechanism : GadgetMechanism
{
    // TODO Do I even need diffusionlist when I have  GeneralGameScript.instance.diffusables??

    private bool allowCollision = false;
    private DiffusionRequest diffusionRequest = null;

    public ThrowingGadgetMechanism(Gadget gadget) : base(gadget)
    {
        this.mechanismText = MECHANISM_PRETEXT + "Throw an Object";
        this.buttonText = "Generate"; // TODO deccide if throwing mechanism is per object or from the gadget, this will determine button text too

        diffusionRequest = new DiffusionRequest();
        diffusionRequest.positivePrompt = "Beautiful";
        diffusionRequest.negativePrompt = "watermark";
        diffusionRequest.numOfVariations = 5;
        diffusionRequest.targets.Add(GameManager.getInstance().radiusDiffusionTexture);
        diffusionRequest.diffusionModel = diffusionModels.nano;
    }

    public void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if(args.interactableObject == null)
        {
            return;
        }
        diffusionRequest.diffusableObject = args.interactableObject.transform.gameObject.GetComponent<DiffusableObject>();
        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);        
    }
    public void DiffusableUnGrabbed(SelectExitEventArgs args)
    {
        if (args.interactableObject == null)
        {
            return;
        }

        if (args.interactableObject.transform.gameObject.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
        {
            var emission = ps.emission;
            emission.enabled = false;
        }
    }
}