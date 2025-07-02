using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Creates a mesh for the 3D Brush cursor used in the original CavePainting, 2001 paper.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("MinVR Interaction/Cursors/CavePainting Brush")]
    public class CavePaintingBrushCursor : MonoBehaviour
    {
        // Vertices that make up the brush geometry
        public static Vector3[] origVertices = new[] {
        new Vector3( 0.5f,   0.0f,   -0.0f),  // 0
        new Vector3(-0.5f,   0.0f,   -0.0f),  // 1
        
        new Vector3( 0.5f,   0.1f,   -0.25f), // 2
        new Vector3(-0.5f,   0.1f,   -0.25f), // 3
        new Vector3( 0.5f,   0.1f,   -0.75f), // 4
        new Vector3(-0.5f,   0.1f,   -0.75f), // 5
        new Vector3( 0.1f,   0.06f,  -1.0f),  // 6
        new Vector3(-0.1f,   0.06f,  -1.0f),  // 7
        new Vector3( 0.15f,  0.1f,   -1.75f), // 8
        new Vector3(-0.15f,  0.1f,   -1.75f), // 9
        
        new Vector3( 0.0f,   0.0f,   -1.85f), // 10
        
        new Vector3( 0.5f,  -0.1f,   -0.25f), // 11
        new Vector3(-0.5f,  -0.1f,   -0.25f), // 12
        new Vector3( 0.5f,  -0.1f,   -0.75f), // 13
        new Vector3(-0.5f,  -0.1f,   -0.75f), // 14
        new Vector3( 0.1f,  -0.06f,  -1.0f),  // 15
        new Vector3(-0.1f,  -0.06f,  -1.0f),  // 16
        new Vector3( 0.15f, -0.1f,   -1.75f), // 17
        new Vector3(-0.15f, -0.1f,   -1.75f)  // 18
    };


        // Vertex indices arranged into triangles (clockwise ordering)
        public static int[] origIndices = new[] {
        // top
        0, 2, 1,
        1, 2, 3,

        2, 4, 3,
        3, 4, 5,

        4, 6, 5,
        5, 6, 7,

        6, 8, 7,
        7, 8, 9,

        8, 10, 9,
        
        // bottom
        
        0, 1, 12,
        11, 0, 12,

        11, 12, 14,
        13, 11, 14,

        13, 14, 16,
        15, 13, 16,

        15, 16, 18,
        17, 15, 18,

        18, 10, 17,
        
        // one side
        11, 2, 0,

        11, 4, 2,
        4, 11, 13,

        13, 6, 4,
        6, 13, 15,

        15, 8, 6,
        8, 15, 17,

        17, 10, 8,
        
        // other side
        3, 12, 1,

        3, 14, 12,
        14, 3, 5,

        5, 16, 14,
        16, 5, 7,

        7, 18, 16,
        18, 7, 9,

        9, 10, 18,
    };


        void Start()
        {
            // vertices and indices defined above are for a flat shaded model.  to use with modern shaders
            // that assume smooth shading, we just make the mesh a bit bigger by duplicating vertices so that
            // each triangle has 3 unique vertices and when the normals are calculated, the normals will be the
            // same for all 3 vertices of each triangle, and we'll get flat shading.

            vertices = new Vector3[origIndices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = origVertices[origIndices[i]];
            }

            indices = new int[origIndices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            m_Mesh = new Mesh();
            m_Mesh.name = "Brush";
            m_Mesh.vertices = vertices;
            m_Mesh.triangles = indices;
            m_Mesh.RecalculateNormals();
            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                // Using built-in pipeline
                GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
            }
            else
            {
                GetComponent<MeshRenderer>().material = GraphicsSettings.defaultRenderPipeline.defaultMaterial;
            }
        }

        public Vector3[] vertices;
        public int[] indices;
        private Mesh m_Mesh;
    }
}