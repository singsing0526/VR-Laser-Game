using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Pointer : MonoBehaviour
{
    public float m_DefaultLength =5.0f;
    public GameObject m_dot;
    public VRInputModule m_InputModule;

    private LineRenderer m_LineRenderer = null;


    private void Awake()
    {
        m_LineRenderer = GetComponent<LineRenderer>();
    }
    void Update()
    {
        UpdateLine();
    }
    private void UpdateLine()
    {
        PointerEventData data = m_InputModule.GetData();

        float targetLength = data.pointerCurrentRaycast.distance==0? m_DefaultLength:data.pointerCurrentRaycast.distance;

        //raycast
        RaycastHit hit = CreateRaycast(targetLength);

        //default
        Vector3 endPostion = transform.position+(transform.forward*targetLength);

        //or based on hit
        if(hit.collider!=null)
            endPostion = hit.point;

        //set postion of the dot
        m_dot.transform.position = endPostion;

        //set linerenderer
        m_LineRenderer.SetPosition(0,transform.position);
        m_LineRenderer.SetPosition(1,endPostion);
    }
    private RaycastHit CreateRaycast(float length)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        Physics.Raycast(ray, out hit,m_DefaultLength);

        return hit;
    }
}
