using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Farm : MonoBehaviour
{
    const float validityCheckTime = 5f;
    float currentTime = 0f;

    List<Field> fields = new(9);
    public const float fieldSize = 6;
    public int level; // 0 = water well, 1 = mill house, 2 = stables

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

        var fEnumerable = fields.Where(f => f != null && f.linkedFarmer == null && f.isValid);
        if (fEnumerable.Count() > 0)
        {
            field = fEnumerable.ToArray()[Random.Range(0, fEnumerable.Count())];
            return true;
        }

        //search all possible spaces
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0 ||
                    level == 0 && !(x == 0 || y == 0)) continue;

                int index = x + 1 + 3 * (y + 1);
                if (fields[index] == null)
                {
                    if (!Physics.CheckBox(transform.position + new Vector3(x * fieldSize, 1, y * fieldSize), new Vector3(fieldSize / 2f, 2, fieldSize / 2f), transform.rotation))
                    {
                        field = Instantiate(ObjectReferences.Instance.field, transform.position + new Vector3(x * fieldSize, 1, y * fieldSize), Quaternion.Euler(0, Random.Range(0, 4) * 90, 0) * transform.rotation, transform).GetComponent<Field>();
                        return true;
                    }
                }
            }
        }

        field = null;
        return false;
    }
}
