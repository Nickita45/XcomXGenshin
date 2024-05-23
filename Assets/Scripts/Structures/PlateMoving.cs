using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateMoving : MonoBehaviour
{
    [SerializeField]
    private Material usualMat, chargeMat, summmonMat;
    public PlateMovingType GetPlateType { get; private set; } //impossibly make like gameObject.GetComponent<MeshRenderer>().material == chargeMat
                                               //because unity creates Instance of chargeMat

    public void SetType(PlateMovingType plate)
    {
        switch (plate)
        {
            case PlateMovingType.Charge:
                gameObject.GetComponent<MeshRenderer>().material = chargeMat;
                break;
            case PlateMovingType.Usual:
                gameObject.GetComponent<MeshRenderer>().material = usualMat;
                break;
            case PlateMovingType.Summon:
                gameObject.GetComponent<MeshRenderer>().material = summmonMat;
                break;
        }

        GetPlateType = plate;
    }
}

public enum PlateMovingType
{
    Usual,
    Charge,
    Summon
}
