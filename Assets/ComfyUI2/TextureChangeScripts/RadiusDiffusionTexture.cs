using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.XR.Interaction.Toolkit;

// TODO create a class like this DiffusionGroup which will be the father class without radius?
public class DiffusionRing
{
    public float maxRadius = 1;
    public float curRadius = 0;
    public float changeMaxTime = 1;
    public float curChangeTime = 0;
    public bool changeTextures = false;
    public List<GameObject> gameObjects = new List<GameObject>();
    public List<Texture2D> diffusionTextureList = new List<Texture2D>();
    public int diffusionTextureIndex = 0;
}

public class RadiusDiffusionTexture : DiffusionTextureChanger
{
    public DiffusionRequest diffusionRequest;
    
    private bool allowCollision = false;
    private GameObject grabbedObject = null;


    // TODO Do I even need diffusionlist when I have  GeneralGameScript.instance.diffusables??
    private List<GameObject> diffusionList = new List<GameObject>();

    public List<DiffusionRing> radiusDiffusionRings = new List<DiffusionRing>();

    private void Start()
    {
        foreach (Transform diffusionTransform in GeneralGameScript.instance.diffusables.transform)
        {
            diffusionList.Add(diffusionTransform.gameObject);
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        if (diff_Textures.Count > 0)
        {
            foreach (DiffusionRing dr in radiusDiffusionRings)
            {
                dr.curChangeTime += Time.deltaTime;
                if (dr.curChangeTime > dr.maxRadius && dr.changeTextures)
                {
                    foreach (GameObject diffusionGO in dr.gameObjects)
                    {
                        // TODO add timer(?) to changeTextureOn
                        changeTextureOn(diffusionGO, dr.diffusionTextureList[dr.diffusionTextureIndex]);
                    }
                    // TODO change curRadius of dr over time
                    dr.diffusionTextureIndex++;
                    dr.diffusionTextureIndex %= dr.diffusionTextureList.Count;

                    dr.curChangeTime = 0;
                }
            }
            
        }
    }

    // TODO instead of each throwable being a radiusDiffusionTexture, let them have a simpler script to send a request to a central one that is responsible for the whole scene
    private void OnCollisionEnter(Collision collision)
    {
        if (!allowCollision || grabbedObject != null)
        {
            return;
        }

        // TODO change radius over time
        addRadiusGameObjects(3, collision.transform.position);

        // TODO create some sort of feedback that makes it obvious that holding it is what causes the collision
        allowCollision = false;
    }

    public void addRadiusGameObjects(float curRadius, Vector3 position)
    {
        if (radiusDiffusionRings.Count <= 0)
        {
            return;
        }
        DiffusionRing dr = radiusDiffusionRings[radiusDiffusionRings.Count-1];
        if (dr == null)
        {
            return;
        }
        if (dr.gameObjects.Count > 0)
        {
            return;
        }

        dr.gameObjects = gameObjectsInRadius(curRadius, position);
        dr.changeTextures = true;
    }

    private List<GameObject> gameObjectsInRadius(float curRadius, Vector3 position)
    {        
        List<GameObject> radiusGameObjects = new List<GameObject>();
        foreach (GameObject go in diffusionList)
        {
            if (Vector3.Distance(go.transform.position, position) <= curRadius)
            {
                radiusGameObjects.Add(go);
            }            
        }

        return radiusGameObjects;
    }

    public override bool AddTexture(List<Texture2D> newDiffTextures, bool addToTextureTotal)
    {
        // TODO think if this line is even useful in this script
        base.AddTexture(newDiffTextures, addToTextureTotal);

        if (grabbedObject != null)
        {
            if (grabbedObject.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                var emission = ps.emission;
                emission.enabled = true;
            }
            

            DiffusionRing newDiffusionRing = new DiffusionRing();
            foreach (Texture2D texture in newDiffTextures)
            {
                newDiffusionRing.diffusionTextureList.Add(texture);
            }            
            radiusDiffusionRings.Add(newDiffusionRing);
            Debug.Log("added diffusion ring");
            return true;
        }
        

        return false;
    }
}
