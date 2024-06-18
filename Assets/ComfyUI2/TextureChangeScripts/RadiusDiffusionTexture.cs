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
    //public DiffusionRequest diffusionRequest;
    public List<DiffusionRing> radiusDiffusionRings = new List<DiffusionRing>();

    private GameObject grabbedObject = null;

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

    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        // TODO think if this line is even useful in this script
        base.AddTexture(diffusionRequest);

        if (diffusionRequest.diffusableObject.grabbed)
        {
            if (diffusionRequest.diffusableObject.gameObject.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                var emission = ps.emission;
                emission.enabled = true;
            }            
        }

        DiffusionRing newDiffusionRing = new DiffusionRing();
        foreach (Texture2D texture in diffusionRequest.textures)
        {
            newDiffusionRing.diffusionTextureList.Add(texture);
        }
        radiusDiffusionRings.Add(newDiffusionRing);
        Debug.Log("added diffusion ring");

        // todo needs to be bool??
        return true;
    }

    public void addRadiusGameObjects(float curRadius, Vector3 position)
    {
        if (radiusDiffusionRings.Count <= 0)
        {
            return;
        }
        DiffusionRing dr = radiusDiffusionRings[radiusDiffusionRings.Count - 1];
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
        foreach (GameObject go in GeneralGameScript.instance.diffusionList)
        {
            if (Vector3.Distance(go.transform.position, position) <= curRadius)
            {
                radiusGameObjects.Add(go);
            }
        }

        return radiusGameObjects;
    }


    public void DiffusableObjectCollided(Collision collision)
    {
        // todo delete, and delete collision in diffusionrequest??
        //diffusionRequest.collision = collision;

        // TODO change radius over time
        addRadiusGameObjects(3, collision.transform.position);
    }
       
}
