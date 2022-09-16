using UnityEngine;
using System.Linq;
using UnityEngine.Windows.WebCam;
using Bouvet.DevelopmentKit.Internal.Utils;
using TMPro;

public class PhotoVideoCaptureHololens : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    private GameObject previewImage;
    [SerializeField]
    private TextMeshPro informationText;
    [Header("Camera Settings")]
    [SerializeField]
    [Range(0, 100)]
    private float hologramOpacity = 80f;
    [SerializeField]
    private PhotoCaptureFileOutputFormat outputFileFormat = PhotoCaptureFileOutputFormat.PNG;

    private PhotoCapture photoCaptureObject = null;
    private VideoCapture videoCaptureObject = null;

    public void TakePhotoHololens()
    {
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }
    private void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = hologramOpacity;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }
    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        informationText.text = "Take a photo or start recording.";
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            informationText.text = "Taking a picture, hold still";
            photoCaptureObject.TakePhotoAsync(filePath, outputFileFormat, OnCapturedPhotoToDisk);
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            BdkLogger.Log("Unable to start photo mode!");
        }
    }
    private void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            BdkLogger.Log("Saved Photo to disk!");
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        else
        {
            BdkLogger.Log("Failed to save Photo to disk");
        }
    }
    private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            previewImage.GetComponent<Renderer>().material.SetTexture("_MainTex", targetTexture);
            informationText.text = "The Photo have been saved to disk, find it under this location:\nUser Folders -> LocalAppData -> bdk(name of project) -> LocalState";
        }
    }

    public void RecordVideoHololens()
    {
        VideoCapture.CreateAsync(false, OnVideoCaptureCreated);
    }
    private void OnVideoCaptureCreated(VideoCapture videoCapture)
    {
        if (videoCapture != null)
        {
            videoCaptureObject = videoCapture;

            Resolution cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();

            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = hologramOpacity;
            cameraParameters.frameRate = cameraFramerate;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            videoCaptureObject.StartVideoModeAsync(cameraParameters, VideoCapture.AudioState.None, OnStartedVideoCaptureMode);
        }
        else
        {
            BdkLogger.Log("Failed to create VideoCapture Instance!");
        }
    }
    private void OnStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format("MyVideo_{0}.mp4", Time.time);
            string filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            videoCaptureObject.StartRecordingAsync(filepath, OnStartedRecordingVideo);
        }
    }
    private void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        BdkLogger.Log("Started Recording Video!");
        informationText.text = "Recording a video, you can stop by clicking the stop button.";
    }
    public void StopRecordingVideo()
    {
        videoCaptureObject.StopRecordingAsync(OnStoppedRecordingVideo);
    }
    private void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        BdkLogger.Log("Stopped Recording Video!");
        videoCaptureObject.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }

    void OnStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        informationText.text = "Take a photo or start recording.";
        videoCaptureObject.Dispose();
        videoCaptureObject = null;
    }
}