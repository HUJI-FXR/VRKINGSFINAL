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


        // configure with raw, jpg, png, or ppm (simple raw format)
        enum Format { RAW, JPG, PNG, PPM };

        /// <summary>
        /// Captures an image with the Camera it is attached to, saves it to folder and returns it.
        /// </summary>
        public static Texture2D CaptureScreenshot(Camera camera)
        {
            // 4k = 3840 x 2160   1080p = 1920 x 1080
            int captureWidth = 512;
            int captureHeight = 512;

            Format format = Format.PNG;

            // folder to write output (defaults to data path)
            string folder = "Assets/ComfyUILib/Screenshots";

            // private vars for screenshot
            Rect rect;
            RenderTexture renderTexture = null;
            Texture2D screenShot;

            // creates off-screen render texture that can rendered into
            rect = new Rect(0, 0, captureWidth, captureHeight);
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
            screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);

            // get main camera and manually render scene into rt
            camera.targetTexture = renderTexture;
            camera.Render();

            // read pixels will read from the currently active render texture so make our offscreen 
            // render texture active and then read the pixels
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(rect, 0, 0);

            // reset active camera texture and render texture
            camera.targetTexture = null;
            RenderTexture.active = null;

            // get our unique filename
            string filename = folder + '/' + GameManager.getInstance().comfyOrganizer.UniqueImageName() + '.' + format.ToString().ToLower();

            // pull in our file header/data bytes for the specified image format (has to be done from main thread)
            byte[] fileHeader = null;
            byte[] fileData = null;
            if (format == Format.RAW)
            {
                fileData = screenShot.GetRawTextureData();
            }
            else if (format == Format.PNG)
            {
                fileData = screenShot.EncodeToPNG();
            }
            else if (format == Format.JPG)
            {
                fileData = screenShot.EncodeToJPG();
            }
            else // ppm
            {
                // create a file header for ppm formatted file
                string headerStr = string.Format("P6{0}{1}255", rect.width, rect.height);

                fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
                fileData = screenShot.GetRawTextureData();
            }

            // Create new thread to save the image to file (only operation that can be done in background)
            new System.Threading.Thread(() =>
            {
                // create file and write optional header with image bytes
                var f = System.IO.File.Create(filename);
                if (fileHeader != null) f.Write(fileHeader, 0, fileHeader.Length);
                f.Write(fileData, 0, fileData.Length);
                f.Close();
                Debug.Log(string.Format("Wrote screenshot {0} of size {1}", filename, fileData.Length));
            }).Start();

            int lastSlashIndex = filename.LastIndexOf('/');
            // Extract the file name by taking the substring after the last slash
            string cutFileName = filename.Substring(lastSlashIndex + 1);

            screenShot.name = cutFileName;

            return screenShot;
        }
    }
}
