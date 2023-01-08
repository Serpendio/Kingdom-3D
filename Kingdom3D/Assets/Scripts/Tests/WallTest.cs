using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTest : MonoBehaviour
{
    public int numMounds = 3;
    public float rotation;
    public float radius;
    public float wallWidth = 2;
    float circumference;

    private void OnDrawGizmosSelected()
    {
        if (numMounds <= 0 || radius <= 0 || wallWidth <= 0) return;

        circumference = 2 * Mathf.PI * radius;
        int numWalls = (int)(circumference / wallWidth);
        float widthMultiplier = circumference / numWalls / wallWidth;
        float degreePerWall = 360 * wallWidth * widthMultiplier / circumference;
        int wallsPerMound = numWalls / numMounds;
        int missingWalls = numWalls - wallsPerMound * numMounds;
        for (int i = 0; i < numMounds; i++)
        {
            Random.InitState(i);
            Color moundColour = Random.ColorHSV();
            for (int o = 0; o < wallsPerMound + (i == (numMounds - 1) ? missingWalls : 0); o++)
            {
                Gizmos.color = o == wallsPerMound / 2 ? Color.red : moundColour;
                float angle = 360f / numMounds * i // mound displacement
                    + (o - wallsPerMound / 2) * degreePerWall // wall displacement
                    - (i + 1) * degreePerWall * missingWalls / (float)numMounds // account for the missing walls
                    + rotation;
                Vector3 wallPos = Quaternion.Euler(0, angle, 0) * Vector3.forward * radius + Vector3.up / 2f;
                Gizmos.matrix = Matrix4x4.TRS(wallPos, Quaternion.Euler(0, angle, 0), transform.lossyScale); //Matrix4x4.Rotate(Quaternion.Euler(0, angle, 0)) * startMatrix;
                Gizmos.DrawCube(
                    Vector3.zero,
                    new Vector3(wallWidth, 1, 1) * widthMultiplier
                    );
            }
        }
    }
}
