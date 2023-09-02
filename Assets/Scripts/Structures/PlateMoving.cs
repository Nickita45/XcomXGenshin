using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateMoving : MonoBehaviour
{
    [SerializeField]
    private Material usualMat, chargeMat;
    public bool IsCharge { get; private set; } //impossibly make like gameObject.GetComponent<MeshRenderer>().material == chargeMat
                                               //because unity creates Instance of chargeMat

    public void SetCharge(bool set)
    {
        if(set) 
            gameObject.GetComponent<MeshRenderer>().material = chargeMat;
        else
            gameObject.GetComponent<MeshRenderer>().material = usualMat;

        IsCharge = set;
    }
}
