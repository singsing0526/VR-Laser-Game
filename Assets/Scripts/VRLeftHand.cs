using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRLeftHand : MonoBehaviour
{
    public VRRightHand rightHand;
    public SteamVR_Action_Boolean zoom;

    private void Update()
    {
            if (rightHand.holdingObject != null && rightHand.holdingObjectInteraction == VRRightHand.HoldingObjectType.pickup)
            {
                if (zoom.stateDown) {
                    rightHand.PushHoldingObject();
                }
            }
    }
}
