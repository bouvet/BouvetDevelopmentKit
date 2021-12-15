using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Bouvet.DevelopmentKit.Input.Hands
{
#pragma warning disable CS0649
    public class SwipeEvent : MonoBehaviour
    {
        [SerializeField]
        protected float minTimeBetweenSwipes;

        [SerializeField]
        protected float minSpeedToSwipe = 1.8f;

        [SerializeField]
        protected UnityEvent onRightSwipe;

        [SerializeField]
        protected UnityEvent onLeftSwipe;

        protected bool blockingSwipe;

        protected ProximityAndManipulationEvent finger;

        protected bool leftHandInField;
        protected Transform leftIndexFinger;
        protected Vector3 leftPreviousPosition;
        protected bool rightHandInField;
        protected Transform rightIndexFinger;
        protected Vector3 rightPreviousPosition;

        // Update is called once per frame
        protected virtual void Update()
        {
            if (!blockingSwipe)
            {
                CheckSwipe();
            }

            if (leftIndexFinger)
            {
                leftPreviousPosition = leftIndexFinger.position;
            }

            if (rightIndexFinger)
            {
                rightPreviousPosition = rightIndexFinger.position;
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            BdkLogger.Log("Something entered", LogSeverity.Info);
            finger = other.gameObject.GetComponent<ProximityAndManipulationEvent>();
            if (finger)
            {
                if (finger.isRightHand)
                {
                    rightHandInField = true;
                    if (!rightIndexFinger)
                    {
                        rightIndexFinger = other.gameObject.transform;
                        rightPreviousPosition = rightIndexFinger.position;
                    }
                }
                else
                {
                    leftHandInField = true;
                    if (!leftIndexFinger)
                    {
                        leftIndexFinger = other.gameObject.transform;
                        leftPreviousPosition = leftIndexFinger.position;
                    }
                }
            }
        }


        protected virtual void OnTriggerExit(Collider other)
        {
            finger = other.gameObject.GetComponent<ProximityAndManipulationEvent>();
            if (finger)
            {
                if (finger.isRightHand)
                {
                    rightHandInField = false;
                }
                else
                {
                    leftHandInField = false;
                }
            }
        }

        private void CheckSwipe()
        {
            if (rightHandInField)
            {
                if (Vector3.Distance(rightIndexFinger.position, rightPreviousPosition) / Time.deltaTime > minSpeedToSwipe
                    && Mathf.Abs(rightPreviousPosition.y - rightIndexFinger.position.y) < 0.02f)
                {
                    if (transform.InverseTransformPoint(rightPreviousPosition).x - transform.InverseTransformPoint(rightIndexFinger.position).x < 0)
                    {
                        onRightSwipe.Invoke();
                    }
                    else
                    {
                        onLeftSwipe.Invoke();
                    }

                    blockingSwipe = true;
                    Invoke(nameof(UnblockSwipe), minTimeBetweenSwipes);
                }
            }

            if (leftHandInField)
            {
                if (Vector3.Distance(leftIndexFinger.position, leftPreviousPosition) / Time.deltaTime > minSpeedToSwipe
                    && Mathf.Abs(leftPreviousPosition.y - leftIndexFinger.position.y) < 0.02f)
                {
                    if (transform.InverseTransformPoint(leftPreviousPosition).x - transform.InverseTransformPoint(leftIndexFinger.position).x < 0)
                    {
                        onRightSwipe.Invoke();
                    }
                    else
                    {
                        onLeftSwipe.Invoke();
                    }

                    blockingSwipe = true;
                    Invoke(nameof(UnblockSwipe), minTimeBetweenSwipes);
                }
            }
        }

        protected void UnblockSwipe()
        {
            blockingSwipe = false;
        }
    }
#pragma warning restore CS0649
}