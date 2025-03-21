using System;
using UnityEngine;
using UnityEngine.Rendering;
 
public class CameraManager : MonoBehaviour
{
    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += PreRender;
        RenderPipelineManager.endCameraRendering += PostRender;
    }

    [Obsolete] // Jonathan Added this tag
    private void PreRender(ScriptableRenderContext _context, Camera _camera)
    {
        if (_camera.TryGetComponent<CameraRenderControl>(out CameraRenderControl _cameraRenderControl))
        {
            _cameraRenderControl.PreRender(_context, _camera);
        }
    }
 
    private void PostRender(ScriptableRenderContext _context, Camera _camera)
    {
        if (_camera.TryGetComponent<CameraRenderControl>(out CameraRenderControl _cameraRenderControl))
        {
            _cameraRenderControl.PostRender(_context, _camera);
        }
    }
 
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= PreRender;
        RenderPipelineManager.endCameraRendering -= PostRender;
    }
}