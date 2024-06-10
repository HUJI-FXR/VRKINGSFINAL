using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;

public class RadiusDiffusionTexture : DiffusionTextureChanger
{
    public float changeTextureEvery = 1;
    public GameObject DiffusionParent;
    public DiffusionRequest DiffReq;
    public ComfyOrganizer comfyOrganizer;
    
    private bool diffuse = false;
    private bool allowCollision = false;
    
    private float textureChangeDelta = 0;
    private float radius = 0;
    private List<GameObject> diffusionList = new List<GameObject>();
    public List<List<GameObject>> radiusDiffusionLists = new List<List<GameObject>>();

    private void Start()
    {
        if (DiffusionParent == null || comfyOrganizer == null)
        {
            return;
        }

        foreach (Transform diffusionTransform in DiffusionParent.transform)
        {
            diffusionList.Add(diffusionTransform.gameObject);
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        if (!diffuse)
        {
            return;
        }

        if (diff_Textures.Count > 0)
        {
            textureChangeDelta += Time.deltaTime;
            if (textureChangeDelta > changeTextureEvery)
            {
                foreach (List<GameObject> rl in radiusDiffusionLists)
                {
                    foreach (GameObject diffusionGO in rl)
                    {
                        // TODO decide how the different textures in a radius change the situation
                        changeTextureOn(diffusionGO, diff_Textures[curTextureIndex]);
                    }
                }

                curTextureIndex++;
                curTextureIndex %= diff_Textures.Count;

                textureChangeDelta = 0;
            }
        }
    }

    // TODO instead of each throwable being a radiusDiffusionTexture, let them have a simpler script to send a request to a central one that is responsible for the whole scene
    private void OnCollisionEnter(Collision collision)
    {
        if (!allowCollision)
        {
            return;
        }

        // TODO change radius over time
        addRadiusGameObjects(3, collision.transform.position);

        // TODO create some sort of feedback that makes it obvious that holding it is what causes the collision
        allowCollision = false;
    }

    public void StartDiffusion()
    {
        comfyOrganizer.SendDiffusionRequest(DiffReq);
        diffuse = true;
        allowCollision = true;
    }

    public void addRadiusGameObjects(float curRadius, Vector3 position)
    {
        List<GameObject> curRadList = gameObjectsInRadius(curRadius, position);
        radiusDiffusionLists.Add(curRadList);
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
}
