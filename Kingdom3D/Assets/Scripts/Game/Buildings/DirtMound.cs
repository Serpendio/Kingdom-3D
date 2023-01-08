using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtMound : MonoBehaviour
{
    public int numMounds = 3;
    public float rotation;
    public float radius;
    public float wallWidth = 2;
    public int moundIndex;

    public void BuildWalls()
    {
        float circumference = 2 * Mathf.PI * radius;
        int numWalls = (int)(circumference / wallWidth);
        float widthMultiplier = circumference / numWalls / wallWidth;
        float degreePerWall = 360 * wallWidth * widthMultiplier / circumference;
        int wallsPerMound = numWalls / numMounds;
        int missingWalls = numWalls - wallsPerMound * numMounds;
        for (int o = 0; o < wallsPerMound + (moundIndex == (numMounds - 1) ? missingWalls : 0); o++)
        {
            float angle = 360 / numMounds * moundIndex // mound displacement
                + (o - wallsPerMound / 2) * degreePerWall // wall displacement
                - missingWalls / (float)numMounds * (moundIndex + 1) * degreePerWall // account for the missing walls
                + rotation;
            Vector3 wallPos = Quaternion.Euler(0, angle, 0) * Vector3.forward * radius + Vector3.up / 2f;
            Gizmos.matrix = Matrix4x4.TRS(wallPos, Quaternion.Euler(0, angle, 0), transform.lossyScale); //Matrix4x4.Rotate(Quaternion.Euler(0, angle, 0)) * startMatrix;
            Gizmos.DrawCube(
                Vector3.zero,
                new Vector3(wallWidth, 1, 1) * widthMultiplier
                );
            /*if (o == wallsPerMound / 2)
                Instantiate(ObjectReferences.Instance.wall1, )
            else
            */
        }
    }
}
