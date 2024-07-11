using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coll_Key : CollectibleBase
{
    protected override void OnCollect()
    {
        // Increment key count in GlobalGameData
        GlobalGameData.Key ++;
    }

}
