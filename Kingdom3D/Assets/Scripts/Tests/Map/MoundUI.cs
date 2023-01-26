using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoundUI : MonoBehaviour
{
    const float scaleTime = 1;
    float currentTime;

    private void Start()
    {
        currentTime = scaleTime;
    }

    private void Update()
    {
        currentTime -= Time.deltaTime;

        if (currentTime < 0)
            currentTime = 0;

        transform.localScale = Vector3.one * Mathf.Lerp(1f, 1.4f, currentTime / scaleTime);
    }
}
