using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalGraphics : MonoBehaviour
{
    // alembic doesn't work for webGL, add backup for that case
    // to increase performance, only one texture could to be used, however this won't work when both are visible on screen

    [SerializeField] Camera mainCam, linkedCam1, linkedCam2;
    [SerializeField] Transform surface1, surface2;
    [SerializeField] RenderTexture portalTexFile;
    private RenderTexture portalTexture1, portalTexture2;

    void Start()
    {
        if (mainCam == null)
        {
            Debug.LogWarning("No main camera has been assigned, this could affect performance");
            mainCam = Camera.main;
        }
        portalTexture1 = Instantiate(portalTexFile);
        linkedCam1.targetTexture = portalTexture1;
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        props.SetTexture("_MainTex", portalTexture1);
        surface1.GetComponent<MeshRenderer>().SetPropertyBlock(props);

        portalTexture2 = Instantiate(portalTexFile);
        linkedCam2.targetTexture = portalTexture2;
        props = new MaterialPropertyBlock();
        props.SetTexture("_MainTex", portalTexture2);
        surface2.GetComponent<MeshRenderer>().SetPropertyBlock(props);
    }

    void Update()
    {
        linkedCam1.transform.rotation = linkedCam2.transform.rotation = mainCam.transform.rotation;

        linkedCam1.transform.position = mainCam.transform.position - surface2.transform.position + surface1.transform.position;
        linkedCam2.transform.position = mainCam.transform.position - surface1.transform.position + surface2.transform.position;
    }
}
