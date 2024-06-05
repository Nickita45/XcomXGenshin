using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;

// A utility class to make sure the data formats are correct.
//
// In particular, this makes floating point data representation consistent
// so we can use it in the dictionary of MatrixMap.
public class CultureSetter
{
    public CultureSetter()
    {
        CultureInfo culture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }
}
