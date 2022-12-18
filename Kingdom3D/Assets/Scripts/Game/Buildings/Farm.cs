using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : MonoBehaviour
{
    const float validityCheckTime = 5f;
    float currentTime = 0f;

    List<Field> fields = new List<Field>();
    int level; // 0 = , 1 = , 2 = stables

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= validityCheckTime)
        {
            currentTime = 0f;
            foreach (Field field in fields)
            {
                field.CheckValidity();
            }
        }
    }

    public bool FindEmptyField(ref Field field)
    {
        foreach (Field fieldToCheck in fields)
        {
            if (fieldToCheck.linkedFarmer == null && fieldToCheck.isValid)
            {
                field = fieldToCheck;
                return true;
            }
        }

        //search all possible spaces

        return false;
    }
}
