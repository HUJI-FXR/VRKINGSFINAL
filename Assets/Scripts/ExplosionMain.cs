using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public enum ExplosionMode
{
    random,
    audioReactive
}

/// <summary>
/// Class responsible for everything to do with the final explosion
/// </summary>
public class ExplosionMain : MonoBehaviour
{
    public ExplosionMode explosionMode;
    private ExplosionMode m_explosionMode;

    public List<Texture2D> diffusionTextures;
    public List<GameObject> diffusableGameObjects;

    public AudioReact audioReact;

    // The minimal time it takes between two audioReactions
    public float audioReactBreathingRoom = 0.1f;
    private float m_audioReactBreathingRoom = 0.1f;

    // The percentage of blocks that are effected by triggers
    [Range(0f, 1f)]
    public float diffusedPercentage;

    public float explosionTickLength = 5;

    private void Start()
    {
        StartCoroutine(WhenLoaded());
    }

    IEnumerator WhenLoaded()
    {
        while (GameManager.getInstance() == null)
        {
            yield return new WaitForSeconds(1f);
        }

        diffusableGameObjects = new List<GameObject>();
        foreach (Transform TN in GameManager.getInstance().diffusables.transform)
        {
            diffusableGameObjects.Add(TN.gameObject);
        }

        // TODO delete this line 
        //StartExplosion();

        
        TransitionExplosionMode(explosionMode);
    }

    private void Update()
    {
        // Updates all relevant information when changing the current ExplosionMode
        if (m_explosionMode != explosionMode)
        {
            TransitionExplosionMode(explosionMode);
        }

        // When the mode is in audio reactivity, in accordance to the AudioReact parameters at a certain time, the transitions are triggered
        if (explosionMode == ExplosionMode.audioReactive)
        {
            if (audioReact == null) return;
            if (!audioReact.wentOverThreshold) return;

            // A basic timer for a minimal amount of time before two audio reactions
            if (m_audioReactBreathingRoom > 0)
            {
                m_audioReactBreathingRoom -= Time.deltaTime;
                return;
            }
            m_audioReactBreathingRoom = audioReactBreathingRoom;

            // A random selection of Game Objects will be effected by the audio reaction
            foreach (GameObject GO in diffusableGameObjects)
            {
                if (Random.value <= diffusedPercentage)
                {
                    if (GO.TryGetComponent<TextureTransition>(out TextureTransition TT))
                    {
                        TT.TriggerNextTexture();
                    }
                }                
            }
        }
    }

    public void StartExplosion()
    {
        InvokeRepeating("ExplosionTick", 0, explosionTickLength);
    }

    public void StopExplosion()
    {
        CancelInvoke("ExplosionTick");
    }

    public void ExplosionTick()
    {
        if (diffusionTextures == null || diffusableGameObjects == null) return;

        foreach (GameObject GO in diffusableGameObjects) {
            if (Random.value > diffusedPercentage) continue;
            if (GO.TryGetComponent<TextureTransition>(out TextureTransition TT))
            {
                // TODO how many textures are sent to each block at each tick?? cur = 5
                TT.textures = new List<Texture> ();
                foreach (Texture2D texture in GeneralGameLibraries.GetRandomElements(diffusionTextures, 5))
                {
                    TT.textures.Add (texture);
                }
            }
        }        
    }

    public void TransitionExplosionMode(ExplosionMode curExplosionMode)
    {
        explosionMode = curExplosionMode;
        m_explosionMode = curExplosionMode;


        switch (explosionMode)
        {
            case ExplosionMode.random:
                foreach (GameObject GO in diffusableGameObjects)
                {
                    if (GO.TryGetComponent<TextureTransition>(out TextureTransition TT))
                    {
                        TT.textures = null;
                        TT.constantTransition = true;
                        TT.transition = Random.value;
                    }
                }
                break;

            case ExplosionMode.audioReactive:
                foreach (GameObject GO in diffusableGameObjects)
                {
                    if (GO.TryGetComponent<TextureTransition>(out TextureTransition TT))
                    {
                        TT.textures = new List<Texture>();
                        foreach (Texture2D texture in GeneralGameLibraries.GetRandomElements(diffusionTextures, 5))
                        {
                            TT.textures.Add(texture);
                        }

                        TT.constantTransition = false;
                        //StopExplosion(); 
                    }
                }
                break;
        }
    }
}
