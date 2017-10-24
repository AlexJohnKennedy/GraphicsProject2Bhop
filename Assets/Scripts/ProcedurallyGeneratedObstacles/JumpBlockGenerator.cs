using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBlockGenerator {

    public static GameObject generateJumpBlock(float lenOffset, float courseWidth, float floorheight, float height, float depth, float width, float angle, Material material) {
        GameObject gameObj = new GameObject("Wall Obstacle");     //Will be the newly created game object just for this section!

        //Setup new gameobject!
        MeshFilter filter = gameObj.AddComponent<MeshFilter>();
        MeshCollider collider = gameObj.AddComponent<MeshCollider>();
        MeshRenderer renderer = gameObj.AddComponent<MeshRenderer>();

        float center, ran;
        
        //Allow the wall to go anywhere..
        ran = Random.Range(-0.45f, 0.45f);
        center = ran * courseWidth;

        //Create the mesh and assign it to the gameobject's meshfilter and collider!
        Mesh mesh = createWallMesh(lenOffset, width, center, depth, floorheight, height, angle);
        filter.mesh = mesh;
        collider.sharedMesh = mesh;

        //Setup the renderer
        renderer.material = material;

        return gameObj;
    }

    private static Mesh createWallMesh(float lenOffset, float width, float center, float depth, float floorheight, float height, float angle) {
        Mesh mesh = new Mesh();
        mesh.name = "wallObs" + lenOffset;

        mesh.vertices = buildVerts(lenOffset, width, center, depth, floorheight, height, angle);
        mesh.triangles = buildTriangles();
        //uvs = CubeUvCalculator.CalculateUVs(mesh.vertices, 0.1f);
        Vector2[] uvs = new Vector2[20];

        mesh.uv = CubeUvCalculator.getUVMappingsNonuniform(mesh.vertices, 1);
        mesh.RecalculateNormals();

        return mesh;
    }

    private static Vector3[] buildVerts(float lenOffset, float width, float center, float depth, float floorheight, float height, float angle) {
        //For this course, to make uv mapping way easier we are going to make duplicate verticies for each face rather than share them.
        //That way we can map a differnt textrue to each face cleanly.
        Vector3[] verts = new Vector3[20];     //5 faces with 4 vertices each.

        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        float r = width / 2;

        //Front face x coords
        float v0x = center + r*cos;
        float v1x = center - r*cos;
        //Back face x coords
        float v4x = v0x - depth * sin;
        float v5x = v1x - depth * sin;

        //Front face z coords
        float v0z = lenOffset + r * sin;
        float v1z = lenOffset - r * sin;
        //Back face z coords
        float v4z = v0z + (depth * cos);
        float v5z = v1z + (depth * cos);

        //Front face
        verts[0] = new Vector3(v0x, floorheight, v0z);
        verts[1] = new Vector3(v1x, floorheight, v1z);
        verts[2] = new Vector3(v1x, floorheight + height, v1z);
        verts[3] = new Vector3(v0x, floorheight + height, v0z);

        //Back face
        verts[4] = new Vector3(v4x, floorheight, v4z);
        verts[5] = new Vector3(v5x, floorheight, v5z);
        verts[6] = new Vector3(v5x, floorheight + height, v5z);
        verts[7] = new Vector3(v4x, floorheight + height, v4z);
        
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
