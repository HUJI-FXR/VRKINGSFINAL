using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Represents a ring(technically a circle) of effected GameObjects that is effected by the Diffusion request that begins at its center
/// </summary>
public class DiffusionRing
{    
    // Max Radius of the circle
    public float maxRadius = 5;
    public float curRadius = 1;

    // Max Time must be greater than 0
    public float changeMaxTime = 5f;
    public float curChangeTime = 0;

    // If False, stops the enlargening of the Ring
    public bool changeTextures = false;

    // GameObjects effected by the Diffusion texture change
    public List<GameObject> gameObjects;

    // Textures of the Diffusion circle
    public List<Texture2D> diffusionTextureList;

    // Circle center position
    public Vector3 centerPosition;
}

/// <summary>
/// Diffusion Texture Changer that creates a Diffusion texture change in circles
/// </summary>
public class RadiusDiffusionTexture : DiffusionTextureChanger
{    
    public List<DiffusionRing> radiusDiffusionRings;

    // Max Radius of the circles that are made at the current time
    public float CurrentMaxRadius = 5;

    private void Awake()
    {
        radiusDiffusionRings = new List<DiffusionRing>();        
    }

    // Update is called once per frame
    protected void Update()
    {
        if (radiusDiffusionRings.Count <= 0) return;

        foreach (DiffusionRing dr in radiusDiffusionRings)
        {
            if (!dr.changeTextures) continue;

            dr.curChangeTime += Time.deltaTime;
            dr.curRadius = (dr.curChangeTime / dr.changeMaxTime) * dr.maxRadius;

            addRadiusGameObjects(dr);

            if (dr.curChangeTime > dr.changeMaxTime)
            {
                dr.changeTextures = false;
            }
        }
    }

    // TODO documentation

    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        // TODO think if this line is even useful in this script
        //base.AddTexture(diffusionRequest);

        if (diffusionRequest.diffusableObject.grabbed)
        {
            if (diffusionRequest.diffusableObject.gameObject.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                var emission = ps.emission;
                emission.enabled = true;
            }            
        }

        DiffusionRing newDiffusionRing = new DiffusionRing();
        newDiffusionRing.gameObjects = new List<GameObject>();
        newDiffusionRing.diffusionTextureList = new List<Texture2D>();
        newDiffusionRing.maxRadius = CurrentMaxRadius;

        foreach (Texture2D texture in diffusionRequest.textures)
        {
            newDiffusionRing.diffusionTextureList.Add(texture);
        }
        radiusDiffusionRings.Add(newDiffusionRing);

        return true;
    }

    public void addRadiusGameObjects(DiffusionRing diffusionRing)
    {        
        if (diffusionRing == null) return;       

        List<GameObject> curRadiusGameObjects = gameObjectsInRadius(diffusionRing.curRadius, diffusionRing.centerPosition);
        List<GameObject> newRadiusGameObjects = new List<GameObject>();

        foreach(GameObject GO in curRadiusGameObjects)
        {
            if (!diffusionRing.gameObjects.Contains(GO)) {
                newRadiusGameObjects.Add(GO);
            }
        }                

        foreach (GameObject GO in newRadiusGameObjects)
        {
            if (GO.TryGetComponent<TextureTransition>(out TextureTransition TT))
            {
                TT.AddTexture(diffusionRing.diffusionTextureList, true);
                diffusionRing.gameObjects.Add(GO);
            }
        }
    }

    private List<GameObject> gameObjectsInRadius(float curRadius, Vector3 position)
    {
        if (GameManager.getInstance().gadget == null) return null;

        List<GameObject> radiusGameObjects = new List<GameObject>();
        foreach (GameObject go in GameManager.getInstance().diffusionList)
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
        if (radiusDiffusionRings.Count <= 0) return;
        DiffusionRing dr = radiusDiffusionRings[radiusDiffusionRings.Count - 1];

        if (dr.gameObjects.Count > 0) return;

        dr.centerPosition = collision.transform.position;
        dr.changeTextures = true;

        // todo delete, and delete collision in diffusionrequest??

        addRadiusGameObjects(dr);
    }
       
}
