using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public LaserColorList laserColorList;
    public MeshRenderer meshRenderer;
    public LaserColorList.LaserType laserType;

    public Material FindMaterial(LaserColorList.LaserType laserType)
    {
        for (int i = 0; i < laserColorList.laserColorList.Count; i++)
        {
            if (laserColorList.laserColorList[i].laserType == laserType)
            {
                return laserColorList.laserColorList[i].material;
            }
        }
        return laserColorList.laserColorList[0].material;
    }

    private void Start()
    {
        if (GetComponent<SubscribeExtraInformation>().extraSettings.Count > 0)
        {
            List<string> extraSettings = GetComponent<SubscribeExtraInformation>().extraSettings;
            laserType = (LaserColorList.LaserType)System.Enum.Parse(typeof(LaserColorList.LaserType), extraSettings[0]);
            meshRenderer.material = FindMaterial(laserType);
        }
    }

    private void FixedUpdate()
    {
        transform.position += transform.up * Mathf.Cos(Time.timeSinceLevelLoad * 5) * 0.0025f;
    }
}
