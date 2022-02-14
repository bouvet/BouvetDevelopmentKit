using Bouvet.DevelopmentKit.Input;
using System;
using System.Collections;
using Bouvet.DevelopmentKit.Functionality.Hands;
using Bouvet.DevelopmentKit.Input.Hands;
using UnityEngine;
using UnityEngine.UI;

namespace Bouvet.DevelopmentKit.Tools.Hands
{
    public class OverHandMenu : MonoBehaviour
    {
        public GameObject menu;
        public GameObject handAnchor;
        public GameObject handAnchorVisuals;
        public Transform handLineRendererAnchor;
        public Transform menuLineRendererAnchor;
        public LineRenderer lineRenderer;
        public float height = 0.2f;
        public bool tryingToActivate;
        public Grabbable grabbable;
        public Image radialProgressBar;
        public float timeUntilActivation;

        private InputManager im;
        private Vector3 pos;
        private Quaternion rot;
        private bool loading;
        private bool followHand;
        private float timeLoadingBegan;

        private void Start()
        {
            im = InputManager.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            if (CheckHoldingState())
            {
                return;
            }
            CheckHandRotations();
            CheckLoadingState();
            MoveMenu();        
        }

        private void MoveMenu()
        {
            if (followHand)
            {
                handAnchorVisuals.SetActive(true);
                menu.transform.position = Vector3.Lerp(menu.transform.position, handAnchor.transform.position + Vector3.up * height, Time.deltaTime * 10f);
                menu.transform.LookAt(im.Hololens);
                menu.transform.localEulerAngles = Vector3.up * menu.transform.localEulerAngles.y;
                lineRenderer.SetPosition(0, handLineRendererAnchor.position);
                lineRenderer.SetPosition(1, menuLineRendererAnchor.position);
            }
            else if (handAnchorVisuals.activeSelf)
            {
                handAnchorVisuals.SetActive(false);
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, Vector3.zero);
            }
        }

        private void CheckLoadingState()
        {
            if (tryingToActivate && !loading)
            {
                loading = true;
                timeLoadingBegan = Time.time;
                Invoke(nameof(ActivateMenu), timeUntilActivation);
                StartCoroutine(Loading());
            }
            else if (!tryingToActivate && loading)
            {
                CancelInvoke(nameof(ActivateMenu));
                StopCoroutine(Loading());
                loading = false;
                radialProgressBar.fillAmount = 0f;
            }
            else if (!tryingToActivate && followHand)
            {
                followHand = false;
            }
        }

        private void CheckHandRotations()
        {
    #if !UNITY_EDITOR
            tryingToActivate = false;
    #endif
            if (im.TryGetHandJointTransform(Bouvet.DevelopmentKit.InputSourceKind.HandLeft, Bouvet.DevelopmentKit.JointName.Palm, out pos, out rot, true))
            {
                handAnchor.transform.position = pos;
                handAnchor.transform.rotation = rot;
                if (Vector3.Angle(-handAnchor.transform.up, Vector3.up) < 40)
                {
                    tryingToActivate = true;
                }
            }
            if (!tryingToActivate && im.TryGetHandJointTransform(Bouvet.DevelopmentKit.InputSourceKind.HandRight, Bouvet.DevelopmentKit.JointName.Palm, out pos, out rot, true))
            {
                handAnchor.transform.position = pos;
                handAnchor.transform.rotation = rot;
                if (Vector3.Angle(-handAnchor.transform.up, Vector3.up) < 40)
                {
                    tryingToActivate = true;
                }
            }
        }

        private bool CheckHoldingState()
        {
            if(grabbable && grabbable.beingHeld)
            {
                if (handAnchorVisuals.activeSelf)
                {
                    handAnchorVisuals.SetActive(false);
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.zero);
                }
                return true;
            }
            return false;
        }

        private void ActivateMenu()
        {
            followHand = true;
        }

        protected virtual IEnumerator Loading()
        {
            while (tryingToActivate)
            {
                radialProgressBar.fillAmount = Mathf.Clamp((Time.time - timeLoadingBegan) / timeUntilActivation, 0f, 1f);
                yield return null;
            }
            radialProgressBar.fillAmount = 0f;
        }
    }
}