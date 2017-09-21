using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorRaiseGenerator {

    public static GameObject generateFloorRaise(float lenOffset, float averageSpacing, float courseLength, float courseWidth, float currentFloorHeight, float jumpHeight, Stack<float> floorEndCoordStack, float angleRad, Material material) {
        GameObject gameObj = new GameObject("Floor raise obstacle");     //Will be the newly created game object just for this section!

        MeshFilter filter = gameObj.AddComponent<MeshFilter>();
        MeshCollider collider = gameObj.AddComponent<MeshCollider>();
        MeshRenderer renderer = gameObj.AddComponent<MeshRenderer>();

        //Generate a random 'distance' over which the floor raise will last
        float span = Random.Range(1f, 20f) * averageSpacing;
        float endingLen = span + lenOffset;
        if (floorEndCoordStack.Count != 0 && endingLen > floorEndCoordStack.Peek()) {
            //We over stepped the floor below.. we should clamp this span.
            endingLen = floorEndCoordStack.Peek();
        }
        else if (endingLen > courseLength - 0.5 * averageSpacing) {
            //Oops, this is spanning too far!
            endingLen = (float)(courseLength - 0.5 * averageSpacing);
        }

        //Create the mesh and assign it to the gameobjects meshFilter and mesh collider
        Mesh mesh = createFloorRaiseMesh(lenOffset, endingLen, courseWidth, currentFloorHeight, jumpHeight, angleRad);
        filter.mesh = mesh;
        collider.sharedMesh = mesh;

        //In order to keep track of the ending points of all the raised floors, we should push the floor change..
        floorEndCoordStack.Push(endingLen);
        currentFloorHeight += jumpHeight;

        //Setup the renderer
        renderer.material = material;
        
        return gameObj;
    }
    private static Mesh createFloorRaiseMesh(float lenOffset, float endingLen, float courseWidth, float currentFloorHeight, float jumpHeight, float angleRad) {

        //NO ANGLE FIX: TODO, MAYBE REIMPLEMENT
        angleRad = 0;

        Mesh mesh = new Mesh();
        mesh.name = "floorRaise" + lenOffset;

        Vector3[] verts = new Vector3[12];   //Floor raise requires 12 verticies (three faces times four verts per face)
        int[] trigs = new int[18];          //6 triangles => 18 indexing positions

        //Front face
        verts[0] = new Vector3(courseWidth / 2, currentFloorHeight, lenOffset - (courseWidth / 2) * Mathf.Tan(angleRad));
        verts[1] = new Vector3(-courseWidth / 2, currentFloorHeight, lenOffset + (courseWidth / 2) * Mathf.Tan(angleRad));
        verts[2] = new Vector3(-courseWidth / 2, currentFloorHeight + jumpHeight, lenOffset + (courseWidth / 2) * Mathf.Tan(angleRad));
        verts[3] = new Vector3(courseWidth / 2, currentFloorHeight + jumpHeight, lenOffset - (courseWidth / 2) * Mathf.Tan(angleRad));

        trigs[0] = 0;
        trigs[1] = 1;
        trigs[2] = 3;
        trigs[3] = 1;
        trigs[4] = 2;
        trigs[5] = 3;

        //Back face
        verts[4] = new Vector3(courseWidth / 2, currentFloorHeight, endingLen);
        verts[5] = new Vector3(-courseWidth / 2, currentFloorHeight, endingLen);
        verts[6] = new Vector3(-courseWidth / 2, currentFloorHeight + jumpHeight, endingLen);
        verts[7] = new Vector3(courseWidth / 2, currentFloorHeight + jumpHeight, endingLen);

        trigs[6] = 4;
        trigs[7] = 7;
        trigs[8] = 5;
        trigs[9] = 5;
        trigs[10] = 7;
        trigs[11] = 6;

        //Top Face
        verts[8] = cloneV3(verts[3]);
        verts[9] = cloneV3(verts[2]);
        verts[10] = cloneV3(verts[6]);
        verts[11] = cloneV3(verts[7]);

        //Top Face triangle assignment
        trigs[12] = 8;
        trigs[13] = 9;
        trigs[14] = 10;
        trigs[15] = 8;
        trigs[16] = 10;
        trigs[17] = 11;

        mesh.vertices = verts;
        mesh.triangles = trigs;
        mesh.uv = CubeUvCalculator.getUVMappingsNonuniform(verts, 5f);

        return mesh;
    }

    private static Vector3 cloneV3(Vector3 v) {
        return new Vector3(v.x, v.y, v.z);
    }
}
