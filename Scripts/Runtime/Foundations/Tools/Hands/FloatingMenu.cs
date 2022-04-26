using Bouvet.DevelopmentKit;
using Bouvet.DevelopmentKit.Functionality.Hands;
using Bouvet.DevelopmentKit.Input;
using Bouvet.DevelopmentKit.Input.Hands;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Tools.Hands
{
    /// <summary>
    /// Simple script for having a menu panel with buttons. The panel can be moved around or follow the player. 
    /// </summary>
    public class FloatingMenu : TwoHandGrabbable
    {
#pragma warning disable CS0649
        [Header("Floating menu behaviour")]
        [SerializeField]
        public bool menuFollow = true; // Determits if the menu follows or not

        public Vector2 offsets = new Vector2(0.1f, -0.2f);

        [SerializeField]
        protected Transform menuTransform; // Anchor point of the menu

        [SerializeField]
        protected float maxDistanceFromCenterOfFOV = 0.4f; // Max distance from center of FOV while following (in meters)

        [SerializeField]
        protected float maxDistanceFromPlayer = 0.7f; // Max distance from player while following (in meters)

        [SerializeField]
        protected float minDistanceFromPlayer = 0.4f; // Min distance from player while following (in meters)

        [SerializeField]
        protected float speed = 9.81f; // Speed of which the panel follows the player

        [SerializeField]
        protected bool fixedToOnlyRotateY; // Toggle to decide if the panel only rotates in the vertical axis

        [SerializeField]
        protected bool alwaysFaceUp; // Toggle to decide if the panel doesn't rotate in the forward axis

        [SerializeField]
        protected bool alwaysFacePlayer; // Toggle that allows the panel to face the player even if it isn't currently following the player

        [SerializeField]
        protected bool facePlayerWhileHeld; // Toggle that allows the panel to face the player even if it isn't currently following the player

        protected Transform hololensTransform; // Hololens transform
        protected Vector3 targetPosition;
        protected Vector3 removeZAxisRotation = new Vector3(1, 1, 0);
        protected Vector3 relativePos;
        protected Quaternion relativeRot;
        protected Vector3 stopPosition;
        protected bool startedMoving;
        protected bool pauseFacePlayerWhenHeld;

        public override void Initialize()
        {
            base.Initialize();
            hololensTransform = inputManager.Hololens;
        }

        /// <summary>
        /// Public function that toggles the menu follow state to its opposite
        /// </summary>
        public void ToggleLerping()
        {
            menuFollow = !menuFollow;
        }

        /// <summary>
        /// Public function that toggles whether the menu follows the player or not
        /// </summary>
        /// <param name="state"></param>
        public void SetMenuLerp(bool state)
        {
            menuFollow = state;
        }

        /// <summary>
        /// If the panel is grabbed, the panel stops following the player
        /// </summary>
        /// <param name="source"></param>
        public override void BeginInteraction(InputSource source)
        {
            if (!gameObject.Equals(source.collidedObject))
            {
                return;
            }
            base.BeginInteraction(source);
            menuFollow = false;
            if (facePlayerWhileHeld) // the error is here somewhere
            {
                stopPosition = Anchor.position;
                startedMoving = false;
            }
        }

        /// <summary>
        /// Updates the panel position and rotation if it is supposed to
        /// </summary>
        protected void Update()
        {
            if (currentState == HandInteractionMode.Everything)
            {
                pauseFacePlayerWhenHeld = true;
            }
            else if (currentState == HandInteractionMode.None)
            {
                pauseFacePlayerWhenHeld = false;
            }

            if (menuFollow)
            {
                MoveToPosition();
            }
            else if (currentState != HandInteractionMode.Everything && beingHeld && facePlayerWhileHeld && !pauseFacePlayerWhenHeld && (startedMoving || Vector3.Distance(Anchor.position, stopPosition) > 0.15f))
            {
                startedMoving = true;
                RotateTowardsPlayer();
            }

            if (menuFollow || alwaysFacePlayer)
            {
                RotateTowardsPlayer();
            }

            if (alwaysFaceUp)
            {
                Anchor.rotation = Quaternion.Euler(Vector3.Scale(Anchor.eulerAngles, removeZAxisRotation));
            }

            if (fixedToOnlyRotateY)
            {
                menuTransform.eulerAngles = Vector3.up * menuTransform.eulerAngles.y;
            }
        }

        /// <summary>
        /// Function that rotates the panel to face the player
        /// </summary>
        protected void RotateTowardsPlayer()
        {
            relativePos = hololensTransform.position - Anchor.position;
            relativeRot = Quaternion.LookRotation(relativePos, Vector3.up);
            relativeRot *= new Quaternion(90, 0, 0, 0);
            //menuTransform.LookAt(hololensTransform);
            menuTransform.rotation = Quaternion.Slerp(menuTransform.rotation, relativeRot, 0.5f);
            //menuTransform.Rotate(Vector3.up, 180f);
        }

        protected Vector3 hololensPosition;
        protected Vector3 menuPosition;
        /// <summary>
        /// Function that moves the panel to be within the set bounds of the player.
        /// </summary>
        protected void MoveToPosition()
        {
            Vector3 scale = hololensTransform.forward.XZ().normalized;

            hololensPosition = hololensTransform.position;
            targetPosition = hololensPosition + hololensTransform.right * offsets.x + scale * maxDistanceFromPlayer; // + (Vector3.up * -0.2f);
            targetPosition.y = hololensPosition.y + offsets.y;

            menuPosition = menuTransform.position;
            float distance = Vector3.Distance(targetPosition, menuPosition), playerDistance = Vector3.Distance(menuPosition, hololensPosition);

            if (distance > maxDistanceFromCenterOfFOV) // || playerDistance < minDistanceFromPlayer)
            {
                menuTransform.position = Vector3.Lerp(menuTransform.position, targetPosition, speed * (Mathf.Abs(distance - maxDistanceFromCenterOfFOV) + (playerDistance < minDistanceFromPlayer ? Mathf.Abs(minDistanceFromPlayer - playerDistance) : 0.0f)) * Time.deltaTime);
            }
        }
#pragma warning restore CS0649
    }
}