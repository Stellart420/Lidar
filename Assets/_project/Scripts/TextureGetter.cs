using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TextureGetter : Singleton<TextureGetter>
{
    [SerializeField] private RawImage _showTextureImage;
    [SerializeField] private ARCameraManager _cameraManager;



    public void GetImageAsync()
    {
        // Get information about the device camera image.
        if (_cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            // If successful, launch a coroutine that waits for the image
            // to be ready, then apply it to a texture.
            StartCoroutine(ProcessImage(image));

            // It's safe to dispose the image before the async operation completes.
            //image.Dispose();
        }
    }

    IEnumerator ProcessImage(XRCpuImage image)
    {
        Debug.Log($"Image: {image.width}/{image.height}. Camera: {Camera.main.pixelWidth}/{Camera.main.pixelHeight}");

        // Create the async conversion request.
        var request = image.ConvertAsync(new XRCpuImage.ConversionParams
        {
            // Use the full image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            // Color image format.
            outputFormat = TextureFormat.RGB24,

            // Flip across the Y axis.
            transformation = XRCpuImage.Transformation.MirrorY
        });

        // Wait for the conversion to complete.
        while (!request.status.IsDone())
            yield return null;

        // Check status to see if the conversion completed successfully.
        if (request.status != XRCpuImage.AsyncConversionStatus.Ready)
        {
            // Something went wrong.
            Debug.LogErrorFormat("Request failed with status {0}", request.status);

            // Dispose even if there is an error.
            request.Dispose();
            yield break;
        }

        // Image data is ready. Let's apply it to a Texture2D.
        var rawData = request.GetData<byte>();


        var texture = new Texture2D(
                request.conversionParams.outputDimensions.x,
                request.conversionParams.outputDimensions.y,
                request.conversionParams.outputFormat,
                false);

        // Copy the image data into the texture.
        texture.LoadRawTextureData(rawData);
        texture.Apply();

        _showTextureImage.texture = texture;

        // Need to dispose the request to delete resources associated
        // with the request, including the raw data.
        request.Dispose();
    }
}
