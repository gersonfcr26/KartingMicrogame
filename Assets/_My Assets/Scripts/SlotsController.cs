using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotsController : MonoBehaviour
{
    /// <summary>
    /// Return all slots available.
    /// </summary>
    public List<Transform> GetSlots()
    {
        int slotsAmount = transform.childCount;
        var slots = new List<Transform>(slotsAmount);

        for(int i = 0; i < slotsAmount; i++)
        {
            slots.Add(transform.GetChild(i));
        }

        return slots;
    }
}
