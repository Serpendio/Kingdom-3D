using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapToggle : MonoBehaviour
{
    [SerializeField] bool isToggled;
    [SerializeField] GameObject checkBox;
    [SerializeField] Map.ObjectTypes objectType;

    private void OnValidate()
    {
        checkBox.SetActive(isToggled);
    }

    private void Start()
    {
        Map.Instance.ChangeVisibility(objectType, isToggled);
    }

    public void Toggle()
    {
        isToggled = !isToggled;
        Map.Instance.ChangeVisibility(objectType, isToggled);
        checkBox.SetActive(isToggled);
    }

    public void Toggle(bool value)
    {
        if (isToggled == value) return;

        isToggled = !isToggled;
        Map.Instance.ChangeVisibility(objectType, isToggled);
        checkBox.SetActive(isToggled);
    }
}
