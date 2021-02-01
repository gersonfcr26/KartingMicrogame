using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DriftType
{
    Entrance,
    Exit
}

public class DriftTrigger : MonoBehaviour
{
    [SerializeField]
    private DriftType m_driftType;

    private void OnTriggerEnter(Collider other)
    {
        // Check for collectable items
        if (other.TryGetComponent(out IDriftable racer))
        {
            // Apply effect
            switch (m_driftType)
            {
                case DriftType.Entrance:
                {
                    racer.DriftInput(true);
                    break;
                }
                case DriftType.Exit:
                {
                    racer.DriftOutput();
                    break;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check for collectable items
        if (other.TryGetComponent(out IDriftable racer))
        {
            // Apply effect
            switch (m_driftType)
            {
                case DriftType.Entrance:
                {
                    racer.DriftInput(false);
                    break;
                }
            }
        }
    }
}
