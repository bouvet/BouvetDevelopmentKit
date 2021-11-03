using Bouvet.DevelopmentKit.Input;
using Bouvet.DevelopmentKit.Input.Hands;
using Bouvet.DevelopmentKit.Internal.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CubeMenu : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField]
    private Transform mainMenuAnchor;

    [SerializeField]
    private Transform pivotAnchor;

    [SerializeField]
    private Transform[] menuSideTransforms = new Transform[4];

    [SerializeField]
    private Transform menuPanelParent;

    [SerializeField]
    private float minTimeToSwipe = 0.2f;

    [SerializeField]
    private Transform debugPanel;

    [SerializeField]
    private Transform debugPanel2;

    private List<Transform> menuStates = new List<Transform>();
    private InputManager inputManager;
    private Transform hololens;

    private int mainSideFacingHololensID;
    private int secondarySideFacingHololensID;

    private void Start()
    {
        inputManager = InputManager.Instance;
        hololens = inputManager.Hololens;
        for (int i = 0; i < menuPanelParent.childCount; i++)
        {
            menuStates.Add(menuPanelParent.GetChild(i));
        }

        //for (int i = 0; i < menuStates.Count; i++)
        //{
        //    menuStates[i].parent = menuSideTransforms[i];
        //    menuStates[i].localPosition = Vector3.zero;
        //    menuStates[i].localRotation = Quaternion.identity;
        //}
    }

    // Update is called once per frame
    private void Update()
    {
        pivotAnchor.LookAt(hololens);
        pivotAnchor.eulerAngles = Vector3.Scale(Vector3.up, pivotAnchor.eulerAngles);
        if (Input.GetKeyDown(KeyCode.J))
        {
            StopAllCoroutines();
            StartCoroutine(nameof(RotateLeft));
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            StopAllCoroutines();
            StartCoroutine(nameof(RotateRight));
        }

        UpdateSideFacingHololens();
    }

    private void UpdateSideFacingHololens()
    {
        float angle = transform.eulerAngles.y - mainMenuAnchor.eulerAngles.y;
        int tempID = mainSideFacingHololensID;
        bool changedSecondary = false;
        if (angle > 45f && angle < 135f && mainSideFacingHololensID != 1)
        {
            mainSideFacingHololensID = 1;
            if (mainSideFacingHololensID == secondarySideFacingHololensID)
            {
                secondarySideFacingHololensID = tempID;
                changedSecondary = true;
            }

            UpdatePanelPositions();
        }
        else if (angle > 135f && angle < 225f && mainSideFacingHololensID != 2)
        {
            mainSideFacingHololensID = 2;
            if (mainSideFacingHololensID == secondarySideFacingHololensID)
            {
                secondarySideFacingHololensID = tempID;
                changedSecondary = true;
            }

            UpdatePanelPositions();
        }
        else if (angle > 225f && angle < 315f && mainSideFacingHololensID != 3)
        {
            mainSideFacingHololensID = 3;
            if (mainSideFacingHololensID == secondarySideFacingHololensID)
            {
                secondarySideFacingHololensID = tempID;
                changedSecondary = true;
            }

            UpdatePanelPositions();
        }
        else if ((angle < 45f || angle > 315f) && mainSideFacingHololensID != 0)
        {
            mainSideFacingHololensID = 0;
            if (mainSideFacingHololensID == secondarySideFacingHololensID)
            {
                secondarySideFacingHololensID = tempID;
                changedSecondary = true;
            }

            UpdatePanelPositions();
        }

        if (changedSecondary)
        {
            return;
        }

        if (angle > 270f && angle < 360f && secondarySideFacingHololensID != 3)
        {
            if (mainSideFacingHololensID != 3)
            {
                secondarySideFacingHololensID = 3;
            }
            else
            {
                secondarySideFacingHololensID = 0;
            }

            UpdatePanelPositions();
        }
        else if (angle > 180f && angle < 270f && secondarySideFacingHololensID != 2)
        {
            if (mainSideFacingHololensID != 2)
            {
                secondarySideFacingHololensID = 2;
            }
            else
            {
                secondarySideFacingHololensID = 3;
            }

            UpdatePanelPositions();
        }
        else if (angle > 90f && angle < 180f && secondarySideFacingHololensID != 1)
        {
            if (mainSideFacingHololensID != 1)
            {
                secondarySideFacingHololensID = 1;
            }
            else
            {
                secondarySideFacingHololensID = 2;
            }

            UpdatePanelPositions();
        }
        else if (angle < 90f && angle > 0f && secondarySideFacingHololensID != 0)
        {
            if (mainSideFacingHololensID != 0)
            {
                secondarySideFacingHololensID = 0;
            }
            else
            {
                secondarySideFacingHololensID = 1;
            }

            UpdatePanelPositions();
        }
    }

    private void UpdatePanelPositions()
    {
        debugPanel.parent = menuSideTransforms[mainSideFacingHololensID];
        debugPanel.localPosition = Vector3.zero;
        debugPanel.localRotation = Quaternion.identity;
        debugPanel2.parent = menuSideTransforms[secondarySideFacingHololensID];
        debugPanel2.localPosition = Vector3.zero;
        debugPanel2.localRotation = Quaternion.identity;
    }

    private float currentRotation;

    private IEnumerator RotateRight()
    {
        Vector3 newRotation = mainMenuAnchor.eulerAngles + Vector3.up * -100f;
        currentRotation = 100f;
        while (Mathf.Abs(currentRotation) >= 10f)
        {
            currentRotation = Mathf.Abs(Quaternion.Angle(mainMenuAnchor.rotation, Quaternion.Euler(newRotation)));
            mainMenuAnchor.rotation = Quaternion.Slerp(mainMenuAnchor.rotation, Quaternion.Euler(newRotation), 5f * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator RotateLeft()
    {
        Vector3 newRotation = mainMenuAnchor.eulerAngles + Vector3.up * 100f;
        currentRotation = 100f;
        while (Mathf.Abs(currentRotation) >= 10f)
        {
            currentRotation = Mathf.Abs(Quaternion.Angle(mainMenuAnchor.rotation, Quaternion.Euler(newRotation)));
            mainMenuAnchor.rotation = Quaternion.Slerp(mainMenuAnchor.rotation, Quaternion.Euler(newRotation), 5f * Time.deltaTime);
            yield return null;
        }
    }

    private Indicator finger;
    private float rigthHandTime;
    private float leftHandTime;
    private bool rightHandMovingRight;
    private bool leftHandMovingRight;

    private void OnTriggerEnter(Collider other)
    {
        finger = other.gameObject.GetComponent<Indicator>();
        if (finger)
        {
            if (transform.InverseTransformPoint(other.transform.position).x < 0)
            {
                if (finger.isRightHand)
                {
                    rigthHandTime = Time.time;
                    rightHandMovingRight = true;
                }
                else
                {
                    leftHandTime = Time.time;
                    leftHandMovingRight = true;
                }
            }

            if (transform.InverseTransformPoint(other.transform.position).x > 0)
            {
                if (finger.isRightHand)
                {
                    rigthHandTime = Time.time;
                    rightHandMovingRight = false;
                }
                else
                {
                    leftHandTime = Time.time;
                    leftHandMovingRight = false;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        finger = other.gameObject.GetComponent<Indicator>();
        if (finger)
        {
            if (transform.InverseTransformPoint(other.transform.position).x > 0)
            {
                if (finger.isRightHand && rightHandMovingRight && Time.time < rigthHandTime + minTimeToSwipe)
                {
                    StopAllCoroutines();
                    StartCoroutine(nameof(RotateLeft));
                }

                if (!finger.isRightHand && leftHandMovingRight && Time.time < leftHandTime + minTimeToSwipe)
                {
                    StopAllCoroutines();
                    StartCoroutine(nameof(RotateLeft));
                }
            }

            if (transform.InverseTransformPoint(other.transform.position).x < 0)
            {
                if (finger.isRightHand && !rightHandMovingRight && Time.time < rigthHandTime + minTimeToSwipe)
                {
                    StopAllCoroutines();
                    StartCoroutine(nameof(RotateRight));
                }

                if (!finger.isRightHand && !leftHandMovingRight && Time.time < leftHandTime + minTimeToSwipe)
                {
                    StopAllCoroutines();
                    StartCoroutine(nameof(RotateRight));
                }
            }
        }
    }
#pragma warning disable CS0649
}