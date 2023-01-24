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
        //private VideoCapture videoCaptureObject = null;

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
                BdkLogger.Log("CameraRecordingUtils is busy!! No new recording started. photoCaptureObject = " + (photoCaptureObject == null));
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
                BdkLogger.Log("CameraRecordingUtils: Photo captured");
                m_PictureCaptured.Invoke();
                if (saveImage) SaveImage();
            } else
            {
                BdkLogger.Log("CameraRecordingUtils: Unable to start photo mode!");
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
            string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            BdkLogger.Log("CameraRecordingUtils.SaveImage: saving to " + filePath + filename);
            photoCaptureObject.TakePhotoAsync(filePath, outputFileFormat, OnCapturedPhotoToDisk);
        }
        private void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
        {
            if(result.success)
            {
                BdkLogger.Log("CameraRecordingUtils: Saved Photo to disk!");
                photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
                m_PictureSaved.Invoke();
            } else
            {
                BdkLogger.Log("CameraRecordingUtils: Failed to save Photo to disk");
                m_PictureSaveFailed.Invoke();
            }
        }

    }
}
