using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;


public class Zones : MonoBehaviour
{
    public TypeZones typeZones;
    public List<Transform> PointsToGo;
    public int maxCap;
    public int actualCap;

    private void GetCap()
    {
        maxCap = GameManager.Instance.GetZoneCapacity(typeZones);
    }
}
