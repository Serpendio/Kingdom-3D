using UnityEngine;

public class DirtMound : MonoBehaviour
{
    public Gate.ConnectionInfo connectionInfo;
    GameObject moundUI;

    public void BuildZone()
    {
        connectionInfo.zone1.BuildZone();
    }

    private void Start()
    {
        moundUI = Map.Instance.CreateMapObject(Map.ObjectTypes.Mound, 0);
        moundUI.transform.localRotation = Quaternion.Euler(0, 0, -transform.rotation.eulerAngles.y);
        moundUI.transform.localPosition = new(transform.position.x, transform.position.z);
    }

    private void OnDestroy()
    {
        Destroy(moundUI);
    }
}
