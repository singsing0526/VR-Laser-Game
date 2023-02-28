using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class VRRightHand : MonoBehaviour
{
    public SteamVR_Action_Boolean select;
    public LineRenderer lineRenderer;
    public GameObject holdingObject, endPointer;
    public BuildObjectList buildObjectList;
    private Vector3 linePosition;
    private float originalDistance, newDistance;
    private float maxRayDistance;
    private Rigidbody rb;
    private bool containRB;
    private int playerMask, ignoreBothMask;
    private Laser rotateLaser;
    public LaserInteraction laserInteraction;
    public enum HoldingObjectType { none, pickup, rotate};
    public HoldingObjectType holdingObjectInteraction = HoldingObjectType.none;
    private AudioSource holdingObjectAudio;
    public AudioLibrary audioLibrary;

    private void Awake()
    {
        lineRenderer.positionCount = 2;
        playerMask = ~(1 << LayerMask.NameToLayer("Player"));
        ignoreBothMask = ~(1 << LayerMask.NameToLayer("Player") | 1 <<  LayerMask.NameToLayer("Holding"));
        maxRayDistance = 1.5f;
    }

    private bool IsObjectPickable(string name)
    {
        for (int i = 0; i < buildObjectList.pickableObjectList.Count; i++)
        {
            if (name == buildObjectList.pickableObjectList[i])
            {
                return true;
            }
        }
        return false;
    }

    private bool IsObjectRotatable(string name)
    {
        for (int i = 0; i < buildObjectList.rotatableObjectList.Count; i++)
        {
            if (name == buildObjectList.rotatableObjectList[i])
            {
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        lineRenderer.SetPosition(0, transform.position + transform.forward * -0.1f);
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, playerMask))
        {
            if (select.stateDown == true && holdingObject == null)
            {
                holdingObjectInteraction = HoldingObjectType.none;
                containRB = false;
                if (IsObjectPickable(hit.transform.name)){
                    holdingObjectInteraction = HoldingObjectType.pickup;
                }
                else if (IsObjectRotatable(hit.transform.name))
                {
                    holdingObjectInteraction = HoldingObjectType.rotate;
                }

                if (holdingObjectInteraction != HoldingObjectType.none) {
                    if (holdingObjectInteraction == HoldingObjectType.pickup)
                    {
                        holdingObject = hit.transform.gameObject;
                        originalDistance = SerializedClasses.GetDistance(transform.position, linePosition);
                        holdingObject.layer = 10;
                        holdingObjectAudio = holdingObject.GetComponent<AudioSource>();
                        holdingObjectAudio.clip = audioLibrary.audioClips[2];
                        holdingObjectAudio.Play();
                    }
                    else if (holdingObjectInteraction == HoldingObjectType.rotate)
                    {
                        holdingObject = hit.transform.gameObject;
                        rotateLaser = Instantiate(laserInteraction.laserPrefab).GetComponent<Laser>();
                        rotateLaser.SetLaser(LaserColorList.LaserType.NA, holdingObject, holdingObject.transform.forward, Laser.FindBestBound(holdingObject.transform, holdingObject.transform.forward, holdingObject.GetComponent<BoxCollider>().bounds.extents), 4, in laserInteraction);
                        holdingObjectAudio = holdingObject.GetComponent<AudioSource>();
                        holdingObjectAudio.clip = audioLibrary.audioClips[0];
                    }
                    if (holdingObject.GetComponent<Rigidbody>())
                    {
                        containRB = true;
                        rb = holdingObject.GetComponent<Rigidbody>();
                        rb.isKinematic = true;
                    }
                }
            }
            linePosition = hit.point;
        }
        else
        {
            if (holdingObject != null) {
                DropHoldingObject();
            }
            holdingObjectInteraction = HoldingObjectType.none;
            linePosition = ray.GetPoint(maxRayDistance);
        }

        if (holdingObject != null)
        {
            if (holdingObjectInteraction == HoldingObjectType.pickup)
            {
                if (Physics.Raycast(ray.origin, ray.direction, out hit, maxRayDistance, ignoreBothMask))
                {
                    newDistance = hit.distance;
                }
                else
                {
                    newDistance = originalDistance;
                }
                if (newDistance > originalDistance)
                {
                    holdingObject.transform.position = ray.GetPoint(originalDistance);
                }
                else
                {
                    holdingObject.transform.position = ray.GetPoint(newDistance);
                }

                if (select.stateUp == true)
                {
                    LaserInteraction.hasSceneUpdate = true;
                    float delay = GetObjectFallTime(GetObjectHeight(holdingObject.GetComponent<BoxCollider>()));
                    if (delay < 20)
                    {
                        holdingObject.layer = 0;
                        holdingObject = null;
                        StartCoroutine(WaitLaserUpdate(delay));
                    }
                    DropHoldingObject();
                }
            }
            else if (holdingObjectInteraction == HoldingObjectType.rotate)
            {
                holdingObject.transform.Rotate(Vector3.up * 30 * Time.deltaTime, Space.World);
                if (holdingObjectAudio.isPlaying == false)
                {
                    holdingObjectAudio.Play();
                }
                rotateLaser.UpdateLaserDirection(holdingObject.transform.forward);
                if (select.stateUp == true)
                {
                    LaserInteraction.hasSceneUpdate = true;
                    DropHoldingObject();
                }
            }
        }
        lineRenderer.SetPosition(1, linePosition);
        endPointer.transform.position = linePosition;;
    }

    private float GetObjectHeight(BoxCollider collider)
    {
        float objectYOffset = (collider.size.y * collider.transform.localScale.x) * 0.5f;
        Ray ray = new Ray(collider.transform.position + Vector3.down * objectYOffset, Vector3.down);
        if(Physics.Raycast(ray, out RaycastHit hit, 10))
        {
            return hit.distance;
        }
        return 10;
    }

    private float GetObjectFallTime(float height)
    {
        // h = 0.5gt^2
        // t = sqrt.(2h / g)
        return Mathf.Sqrt(2 * height / 9.7f);
    }

    IEnumerator WaitLaserUpdate(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (holdingObjectAudio.isPlaying) {
            holdingObjectAudio.clip = audioLibrary.audioClips[2];
            holdingObjectAudio.Play();
        }
        DropHoldingObject();
    }

    public void PushHoldingObject()
    {
        if (containRB)
        {
            Vector3 direction =  holdingObject.transform.position - transform.position;
            direction.Normalize();
            rb.velocity += direction * 3;
            LaserInteraction.hasSceneUpdate = true;
            StartCoroutine(WaitLaserUpdate(GetObjectFallTime(GetObjectHeight(holdingObject.GetComponent<BoxCollider>()))));
            DropHoldingObject();
        }
    }

    private void DropHoldingObject()
    {
        holdingObject.layer = 0;
        holdingObject = null;
        if (containRB)
        {
            containRB = false;
            rb.isKinematic = false;
            rb = null;
        }
        holdingObjectInteraction = HoldingObjectType.none;
        rotateLaser = null;
        LaserInteraction.hasSceneUpdate = true;
    }
}
