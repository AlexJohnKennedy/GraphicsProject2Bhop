using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * This class simply has static methods for plainly UV mapping a cube mesh.
 * We assume that the cube is build with a fresh set of FOUR verticies for each face!
 * 
 * That is, each seet of four verticies represents an individual plane arrangement.
 * WE ASSUME THIS ARRANGMENT OF VERTS FOR EACH FACE
 * c2 --------- c3  The plane is 'arranged' like this.
 * |             |  We will need to remember where each corner was in the vertex array,
 * |             |  so that we can correctly apply the uv map values in the right order
 * |             |  in the UV array!
 * c1 --------- c0
 */
public class CubeUvCalculator {

    public static Vector2[] getUVMappingsNonuniform(Vector3[] verts, float scale) {

        Vector2[] uvs = new Vector2[verts.Length];

        //Performed iteratively. Since the verticies should be in a groups of four we can grab four at a time!
        for (int i = 0; i < verts.Length; i += 4) {
            int i1 = i + 1;             //Index values, to save reperforming additions..
            int i2 = i + 2;
            int i3 = i + 3;

            //Extract the corners of the plane in original ordering.
            Vector3[] vectors = { verts[i], verts[i1], verts[i2], verts[i3] };

            //If all four y values are equal then it is a top face, since our cubes are never slanted!
            if (equal(vectors[0].y, vectors[1].y, vectors[2].y, vectors[3].y)) {
                //TOP FACE: Use the (x,z) coordinates to map uv values!
                for (int j=0; j < 4; j++) {
                    uvs[i + j] = new Vector2(vectors[j].x / scale, vectors[j].z / scale);
                }
            }
            else {
                //Side face: the X in uv should be mapped to the DISTANCE between the (x,z) vector2 positions, and the Y in uv should just be the y coordinate
                uvs[i1] = new Vector2(0, 0);    //Just start from the corner of the texture for simplicity

                float xzdist = Vector2.Distance(new Vector2(vectors[0].x, vectors[0].z), new Vector2(vectors[1].x, vectors[1].z));
                uvs[i] = new Vector2(xzdist / scale, 0);

                uvs[i2] = new Vector2(0, vectors[2].y / scale);

                uvs[i3] = new Vector2(xzdist / scale, vectors[2].y / scale);
            }


        }

        return uvs;
    }
    
    private static bool equal(float a, float b, float c, float d) {
        return (equal(a, b) && equal(c, d) && equal(a, c));
    }

    private static bool equal(float a, float b) {
        return (Mathf.Abs(a - b) <= 0.01f);
    }
}
