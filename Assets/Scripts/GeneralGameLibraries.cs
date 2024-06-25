using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A collection of Classes and functions that are useful throughout the project and are based on default Unity types of Objects.
/// </summary>
public class GeneralGameLibraries : Object
{
    /// <summary>
    /// Opens an folder and retreives all the Audio Clips within for easy use through its Dictionary
    /// </summary>
    public class AudioClipsLibrary : Object
    {
        public string AudioClipFolder;
        public Dictionary<string, AudioClip> AudioClips;

        public AudioClipsLibrary(string curAudioClipFolder = "Sounds/Voiceover")
        {
            if (curAudioClipFolder == "")
            {
                Debug.LogError("Choose a proper Audio Clip Folder");
                return;
            }
            AudioClipFolder = curAudioClipFolder;
            AudioClips = new Dictionary<string, AudioClip>();

            GetAudioClips(AudioClipFolder);
        }

        /// <summary>
        /// Gets all the Audio Clips from the given folder and adds it to the AudioClips Dictionary
        /// </summary>
        /// <param name="audioClipFolder">Folder to get Audio Clips from</param>
        private void GetAudioClips(string audioClipFolder)
        {
            // TODO what if not mp3 wav?
            //var audioClipFileNames = Directory.GetFiles(audioClipFolder, "*.wav");
            //var audioClipFileNames = Directory.GetFiles(audioClipFolder, "*.mp3");      
            var audioClipFileNames = Resources.LoadAll(audioClipFolder, typeof(AudioClip));

            foreach (AudioClip audioClip in audioClipFileNames)
            {
                AudioClips[audioClip.name] = Resources.Load<AudioClip>(audioClipFolder + "/" + audioClip.name);
            }
        }
    }

    /// <summary>
    /// Provides functions for general Texture manipulations
    /// </summary>
    public class TextureManipulationLibrary : Object
    {
        //https://stackoverflow.com/questions/44264468/convert-rendertexture-to-texture2d
        public static Texture2D toTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            var old_rt = RenderTexture.active;
            RenderTexture.active = rTex;

            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            RenderTexture.active = old_rt;
            return tex;
        }

        public static Texture2D toTexture2D(Texture inTex)
        {
            RenderTexture rTex = new RenderTexture(inTex.width, inTex.height, 4);
            Graphics.Blit(inTex, rTex);

            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            // ReadPixels looks at the active RenderTexture.
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            return tex;
        }

        public static Texture2D DeCompress(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
    }
}
