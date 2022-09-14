using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Linq;
using UnityEngine.Windows.WebCam;
using System.Threading.Tasks;
using Bouvet.DevelopmentKit.Internal.Utils;
#if WINDOWS_UWP && !UNITY_EDITOR
using Application = UnityEngine.WSA.Application;
using Windows.ApplicationModel;
using Windows.Graphics.Holographic;
using Windows.System;
using Windows.Media.Capture;
using Windows.Foundation.Metadata;
#endif

public class PhotoVideoCaptureHololens : MonoBehaviour
{
    [SerializeField]
    private GameObject previewImage;

    private PhotoCapture photoCaptureObject = null;
    private Texture2D targetTexture = null;

    static readonly float MaxRecordingTime = 5.0f;

    VideoCapture m_VideoCapture = null;
    float m_stopRecordingTimer = float.MaxValue;

    private void Update()
    {
        if (m_VideoCapture == null || !m_VideoCapture.IsRecording)
        {
            return;
        }

        if (Time.time > m_stopRecordingTimer)
        {
            m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
        }
    }

    public void RequestCameraAccess()
    {
        _ = RequestAccessAndInitAsync(CancellationToken.None, true, false);
    }
    public static Task<bool> RequestAccessAndInitAsync(CancellationToken token, bool useCamera, bool useMicrophone)
    {
#if WINDOWS_UWP && !UNITY_EDITOR
        // Note that the UWP UI thread and the main Unity app thread are always different.
        // https://docs.unity3d.com/Manual/windowsstore-appcallbacks.html
        TaskCompletionSource<bool> permissionTcs = new TaskCompletionSource<bool>();
        Application.InvokeOnUIThread(() =>
        {
            // Request UWP access to audio or video capture. The OS may show some popup dialog to the
            // user to request permission. This will succeed only if the user grants permission.
            try
            {
                MediaCapture mediaAccessRequester = new MediaCapture();
                MediaCaptureInitializationSettings mediaSettings = new MediaCaptureInitializationSettings
                {
                    AudioDeviceId = "", // Set later
                    VideoDeviceId = "", // Set later
                    PhotoCaptureSource = PhotoCaptureSource.VideoPreview,
                    SharingMode = MediaCaptureSharingMode.SharedReadOnly // for MRC and lower res camera
                };

                if (useCamera && useMicrophone)
                {
                    mediaSettings.StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo;
                }
                else if (useCamera)
                {
                    mediaSettings.StreamingCaptureMode = StreamingCaptureMode.Video;
                }
                else if (useMicrophone)
                {
                    mediaSettings.StreamingCaptureMode = StreamingCaptureMode.Audio;
                }

                mediaAccessRequester.InitializeAsync(mediaSettings).AsTask(token).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        BdkLogger.Log($"Media access failure: {task.Exception.InnerException?.Message}.", LogSeverity.Error);
                        permissionTcs.SetResult(false);
                    }
                    else
                    {
                        permissionTcs.SetResult(true);
                    }
                }, token);
            }
            catch (Exception ex)
            {
                // Log an error and prevent activation
                BdkLogger.Log($"Media access failure: {ex.Message}.", LogSeverity.Error);
                permissionTcs.SetResult(false);
            }
        },
        false);

        return permissionTcs.Task;
#else
        return Task.FromResult(true);
#endif
    }

    public void RecordVideo()
    {

        Resolution cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        Debug.Log(cameraResolution);

        float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();
        Debug.Log(cameraFramerate);

        VideoCapture.CreateAsync(false, delegate (VideoCapture videoCapture)
        {
            if (videoCapture != null)
            {
                m_VideoCapture = videoCapture;
                Debug.Log("Created VideoCapture Instance!");

                CameraParameters cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 0.0f;
                cameraParameters.frameRate = cameraFramerate;
                cameraParameters.cameraResolutionWidth = cameraResolution.width;
                cameraParameters.cameraResolutionHeight = cameraResolution.height;
                cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

                m_VideoCapture.StartVideoModeAsync(cameraParameters,
                                                   VideoCapture.AudioState.ApplicationAndMicAudio,
                                                   OnStartedVideoCaptureMode);
            }
            else
            {
                Debug.LogError("Failed to create VideoCapture Instance!");
            }
        });
    }

    private void OnStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("Started Video Capture Mode!");
        string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
        string filename = string.Format("TestVideo_{0}.mp4", timeStamp);
        string filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        filepath = filepath.Replace("/", @"\");
        m_VideoCapture.StartRecordingAsync(filepath, OnStartedRecordingVideo);
    }

    private void OnStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        BdkLogger.Log("Stopped Video Capture Mode!");
    }

    private void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        BdkLogger.Log("Started Recording Video!");
        m_stopRecordingTimer = Time.time + MaxRecordingTime;
    }

    private void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        BdkLogger.Log("Stopped Recording Video!");
        m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }

    public void CapturePhoto()
    {
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Create a PhotoCapture object
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                // Take a picture
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            });
        });
    }

    private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        // Copy the raw image data into the target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);
        previewImage.GetComponent<Renderer>().material.SetTexture("_MainTex", targetTexture);

        // Deactivate the camera
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown the photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
}