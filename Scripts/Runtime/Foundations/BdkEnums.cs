using System;

namespace Bouvet.DevelopmentKit
{
    /// <summary>
    /// Enum of the different kinds of input sources in BouvetDevelopmentKit
    /// </summary>
    [Flags]
    public enum InputSourceKind
    {
        None = 0,
        HandLeft = 1,
        HandRight = 2,
        EyeGaze = 4,
        HeadGaze = 8,
        InteractionBeamLeft = 16,
        InteractionBeamRight = 32,
        Voice = 64,
        Hololens = 128,
        IndexFingerRight = 256,
        IndexFingerLeft = 512
    }

    /// <summary>
    /// Emum of the different joint names in BouvetDevelopmentKit
    /// </summary>
    public enum JointName
    {
        Palm = 0,
        Wrist = 1,
        ThumbMetacarpal = 2,
        ThumbProximal = 3,
        ThumbDistal = 4,
        ThumbTip = 5,
        IndexMetacarpal = 6,
        IndexProximal = 7,
        IndexIntermediate = 8,
        IndexDistal = 9,
        IndexTip = 10,
        MiddleMetacarpal = 11,
        MiddleProximal = 12,
        MiddleIntermediate = 13,
        MiddleDistal = 14,
        MiddleTip = 15,
        RingMetacarpal = 16,
        RingProximal = 17,
        RingIntermediate = 18,
        RingDistal = 19,
        RingTip = 20,
        LittleMetacarpal = 21,
        LittleProximal = 22,
        LittleIntermediate = 23,
        LittleDistal = 24,
        LittleTip = 25
    }

    /// <summary>
    /// Enum of the different visual states of the cursors.
    /// </summary>
    public enum CursorState
    {
        None = 0,
        InteractionBeamCursor = 1,
        IndexFingerCursor = 2,
        HeadCursor = 3,
        EyeCursor = 4
    }

    /// <summary>
    /// Enum of the different hand interaction an object can allow.
    /// </summary>
    [Flags]
    public enum HandInteractionMode
    {
        None = 0,
        Right = 1,
        Left = 2,
        Everything = 3
    }

    /// <summary>
    /// Enum for the different air tap interactions for buttons
    /// </summary>
    public enum ButtonInteractionBeamMode
    {
        None = 0,
        OnInputDown = 1,
        OnInputUp = 2,
        OnInputUpLocked = 3
    }
}