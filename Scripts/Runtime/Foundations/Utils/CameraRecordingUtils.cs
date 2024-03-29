using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows.WebCam;

namespace Bouvet.DevelopmentKit.Internal.Utils
{
    /// <summary>
    ///  Utility singleton to take a photo or record video from the default camera.
    ///  
    ///  Made to be used with Hololens, but also works in the editor
    /// </summary>

    public class CameraRecordingUtils : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance variable
        /// </summary>
        public static CameraRecordingUtils Instance { get; private set; }

        [Header("Camera Settings")]
        [SerializeField]
        private bool captureHologram = false; //* Decides if the holograms are to be included in the capture
        public bool CaptureHologram
        {
            get { return captureHologram; }
            set { captureHologram = value; }
        }

        [SerializeField]
        [Range(0f, 1f)] 
        private float hologramOpacity = 0.8f;

        [Header("File output options")]
        [SerializeField]
        private bool saveImage = true;
        [SerializeField]
        private PhotoCaptureFileOutputFormat outputFileFormat = PhotoCaptureFileOutputFormat.PNG;

        public PhotoCapture photoCaptureObject { get; private set; } = null;
        private VideoCapture videoCaptureObject = null;
        public string videoFilePath { get; private set; } = "";

        [Header("Events")]
        [SerializeField]
        private UnityEvent m_PictureCaptured;
        [SerializeField]
        private UnityEvent m_PictureCaptureFailed;
        [SerializeField]
        private UnityEvent m_PictureSaved;
        [SerializeField]
        private UnityEvent m_PictureSaveFailed;
        [SerializeField]
        private UnityEvent m_VideoCaptured;
        [SerializeField]
        private UnityEvent m_VideoCaptureFailed;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this);
            } else
            {
                Instance = this;
            }
        }

        public void TakePhoto()
        {
            if(photoCaptureObject == null)
            {
                PhotoCapture.CreateAsync(captureHologram, OnPhotoCaptureCreated);
            } else
            {
                BdkLogger.Log("CameraRecordingUtils.TakePhoto: CameraRecordingUtils is busy!! No new recording started. photoCaptureObject = " + (photoCaptureObject == null));
            }
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

        private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
        {
            if(result.success)
            {
                BdkLogger.Log("CameraRecordingUtils.OnPhotoModeStarted: Photo captured");
                m_PictureCaptured.Invoke();
                if (saveImage) SaveImage();
            } else
            {
                BdkLogger.Log("CameraRecordingUtils.OnPhotoModeStarted: Unable to start photo mode!");
            }

            if(!saveImage) photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
        {
            photoCaptureObject.Dispose();
            photoCaptureObject = null;
        }

        private void SaveImage()
        {
            string extension = "png";
            if(outputFileFormat != PhotoCaptureFileOutputFormat.PNG)
            {
                extension = "jpg";
            }
            string filename = string.Format(@"CapturedImage{0}_n." + extension, Time.time);
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            BdkLogger.Log("CameraRecordingUtils.SaveImage: saving to " + filePath);
            photoCaptureObject.TakePhotoAsync(filePath, outputFileFormat, OnCapturedPhotoToDisk);
        }
        private void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
        {
            if(result.success)
            {
                BdkLogger.Log("CameraRecordingUtils.OnCapturedPhotoToDisk: Saved Photo to disk!");
                photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
                m_PictureSaved.Invoke();
            } else
            {
                BdkLogger.Log("CameraRecordingUtils.OnCapturedPhotoToDisk: Failed to save Photo to disk");
                m_PictureSaveFailed.Invoke();
            }
        }

        public void RecordVideo()
        {
            if (videoCaptureObject == null)
            {
                VideoCapture.CreateAsync(captureHologram, OnVideoCaptureCreated);
            }
            else
            {
                BdkLogger.Log("CameraRecordingUtils.RecordVideo: CameraRecordingUtils is busy!! No new recording started. videoCaptureObject = " + (videoCaptureObject == null));
            }
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
                BdkLogger.Log("CameraRecordingUtils.OnVideoCaptureCreated: Failed to create VideoCapture Instance!");
            }
        }
        private void OnStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
        {
            if (result.success)
            {
                string filename = string.Format("MyVideo_{0}.mp4", Time.time);
                videoFilePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
                BdkLogger.Log("CameraRecordingUtils.OnStartedVideoCaptureMode: Saving to " + videoFilePath);
                videoCaptureObject.StartRecordingAsync(videoFilePath, OnStartedRecordingVideo);
            } else
            {
                BdkLogger.Log("CameraRecordingUtils.OnStartedVideoCaptureMode: video capture failed");
                m_VideoCaptureFailed?.Invoke();
            }
        }
        private void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
        {
            BdkLogger.Log("CameraRecordingUtils.OnStartedRecordingVideo");
            if(!result.success)
            {
                BdkLogger.Log("CameraRecordingUtils.OnStartedRecordingVideo: Starting video recording failed");
                m_VideoCaptureFailed?.Invoke();
            }
        }
        public void StopRecordingVideo()
        {
            videoCaptureObject.StopRecordingAsync(OnStoppedRecordingVideo);
        }
        private void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
        {
            if(result.success)
            {
                BdkLogger.Log("CameraRecordingUtils.OnStoppedRecordingVideo");
                videoCaptureObject.StopVideoModeAsync(OnStoppedVideoCaptureMode);
            } else
            {
                BdkLogger.Log("CameraRecordingUtils.OnStoppedRecordingVideo: video recording failed");
                m_VideoCaptureFailed?.Invoke();
            }
        }

        void OnStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
        {
            if(result.success)
            {
                m_VideoCaptured?.Invoke();
                videoCaptureObject.Dispose();
                videoCaptureObject = null;
            } else
            {
                BdkLogger.Log("CameraRecordingUtils.OnStoppedVideoCaptureMode: video recording failed");
                m_VideoCaptureFailed?.Invoke();
            }
        }

    }
}
