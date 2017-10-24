using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObstacleGenerator {
    public static GameObject generateWall(float lenOffset, float courseWidth, float courseHeight, float depth, Material material) {
        GameObject gameObj = new GameObject("Wall Obstacle");     //Will be the newly created game object just for this section!

        //Setup new gameobject!
        MeshFilter filter = gameObj.AddComponent<MeshFilter>();
        MeshCollider collider = gameObj.AddComponent<MeshCollider>();
        MeshRenderer renderer = gameObj.AddComponent<MeshRenderer>();

        //Generate a 'width' value for the new wall. It should be a MAX of 3/4 of the course width.
        //The MIN width should be 1/4 of the course length.
        //If the width > half the course width, we'll force the wall to be 'stuck' to one side or the other (only one gap).
        //If the width <= half the course, we'll allow if to spawn anyway along the width axis.

        float width = Random.Range(0.25f, 0.75f) * courseWidth;     //Width of the wall.
        float center, ran;
        if (width > 0.5 * courseWidth) {
            //Force the wall to spawn connected to edge of course, but randomly generate which side!
            ran = Random.Range(-1f, 1f);
            if (ran < 0) {
                //LEFT WALL
                center = -(courseWidth - width) / 2;
            }
            else {
                //RIGHT WALL
                center = (courseWidth - width) / 2;
            }
        }
        else {
            //Allow the wall to go anywhere..
            ran = Random.Range(-0.25f, 0.25f);
            center = ran * courseWidth;

            //If the wall is clipping through, clamp the center point to be stuck to the wall..
            if (Mathf.Abs(center) + width / 2 > courseWidth / 2) {
                if (ran < 0) {
                    //LEFT WALL
                    center = -(courseWidth - width) / 2;
                }
                else {
                    //RIGHT WALL
                    center = (courseWidth - width) / 2;
                }
            }
        }

        //Create the mesh and assign it to the gameobject's meshfilter and collider!
        Mesh mesh = createWallMesh(lenOffset, width, center, depth, courseHeight);
        filter.mesh = mesh;
        collider.sharedMesh = mesh;

        //Setup the renderer
        renderer.material = material;

        return gameObj;
    }

    private static Mesh createWallMesh(float lenOffset, float width, float center, float depth, float courseHeight) {
        Mesh mesh = new Mesh();
        mesh.name = "wallObs" + lenOffset;

        mesh.vertices = buildVerts(lenOffset, width, center, depth, courseHeight);
        mesh.triangles = buildTriangles();
        //uvs = CubeUvCalculator.CalculateUVs(mesh.vertices, 0.1f);
        Vector2[] uvs = new Vector2[20];

        mesh.uv = CubeUvCalculator.getUVMappingsNonuniform(mesh.vertices, 1);
        mesh.RecalculateNormals();

        return mesh;
    }

    /* c2 --------- c3  Each plane is 'arranged' like this.
     * |             |  We will need to remember where each corner was in the vertex array,
     * |             |  so that we can correctly apply the uv map values in the right order
     * |             |  in the UV array!
     * c1 --------- c0
     */
    private static Vector3[] buildVerts(float lenOffset, float width, float center, float depth, float courseHeight) {
        //For this course, to make uv mapping way easier we are going to make duplicate verticies for each face rather than share them.
        //That way we can map a differnt textrue to each face cleanly.
        Vector3[] verts = new Vector3[20];     //5 faces with 4 vertices each.

        //Front face
        verts[0] = new Vector3(center + width / 2, 0, lenOffset);
        verts[1] = new Vector3(center - width / 2, 0, lenOffset);
        verts[2] = new Vector3(center - width / 2, courseHeight, lenOffset);
        verts[3] = new Vector3(center + width / 2, courseHeight, lenOffset);

        //Back face
        verts[4] = new Vector3(center + width / 2, 0, lenOffset + depth);
        verts[5] = new Vector3(center - width / 2, 0, lenOffset + depth);
        verts[6] = new Vector3(center - width / 2, courseHeight, lenOffset + depth);
        verts[7] = new Vector3(center + width / 2, courseHeight, lenOffset + depth);

        //Left face
        verts[8] = new Vector3(verts[5].x, verts[5].y, verts[5].z);
        verts[9] = new Vector3(verts[1].x, verts[1].y, verts[1].z);
        verts[10] = new Vector3(verts[2].x, verts[2].y, verts[2].z);
        verts[11] = new Vector3(verts[6].x, verts[6].y, verts[6].z);

        //right face
        verts[12] = new Vector3(verts[4].x, verts[4].y, verts[4].z);
        verts[13] = new Vector3(verts[0].x, verts[0].y, verts[0].z);
        verts[14] = new Vector3(verts[3].x, verts[3].y, verts[3].z);
        verts[15] = new Vector3(verts[7].x, verts[7].y, verts[7].z);

        //Top face
        verts[16] = new Vector3(verts[2].x, verts[2].y, verts[2].z);
        verts[17] = new Vector3(verts[3].x, verts[3].y, verts[3].z);
        verts[18] = new Vector3(verts[7].x, verts[7].y, verts[7].z);
        verts[19] = new Vector3(verts[6].x, verts[6].y, verts[6].z);

        return verts;
    }

    private static int[] buildTriangles() {
        int[] trigs = new int[30];

        //Front face
        trigs[0] = 0;
        trigs[1] = 1;
        trigs[2] = 3;
        trigs[3] = 1;
        trigs[4] = 2;
        trigs[5] = 3;

        //Back face
        trigs[6] = 4;
        trigs[7] = 7;
        trigs[8] = 5;
        trigs[9] = 5;
        trigs[10] = 7;
        trigs[11] = 6;

        //Left face
        trigs[18] = 9;
        trigs[19] = 8;
        trigs[20] = 11;
        trigs[21] = 9;
        trigs[22] = 11;
        trigs[23] = 10;

        //Right face
        trigs[24] = 13;
        trigs[25] = 14;
        trigs[26] = 15;
        trigs[27] = 13;
        trigs[28] = 15;
        trigs[29] = 12;

        //Top face
        trigs[12] = 17;
        trigs[13] = 16;
        trigs[14] = 18;
        trigs[15] = 16;
        trigs[16] = 19;
        trigs[17] = 18;

        return trigs;
    }
}
