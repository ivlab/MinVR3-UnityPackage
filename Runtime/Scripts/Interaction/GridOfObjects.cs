using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IVLab.MinVR3
{
    /// <summary>
    /// A grid of GameObjects, where the grid can be defined different ways (e.g., on a plane,
    /// on the surface of a spherical dome, ...)
    /// </summary>
    [AddComponentMenu("MinVR Interaction/Building Blocks/Grid of Objects")]
    public class GridOfObjects : MonoBehaviour
    {
        // applies to all grids
        public List<GameObject> m_Objects;
        public bool m_CreateTestObjects = false;

        public int m_NumRows = 1;
        public int m_NumColumns = 1;

        // define different grid types
        public enum GridType
        {
            Planar = 0,
            Spherical = 1
        };

        public GridType m_GridType = GridType.Planar;

        // specific to planar grids
        public Vector3 m_GridCenterPoint;
        public Vector3 m_GridRowDir;
        public Vector3 m_GridColumnDir;


        private void Reset()
        {
            m_Objects = null;
            m_NumRows = 1;
            m_NumColumns = 1;
            m_CreateTestObjects = false;
            m_GridType = GridType.Planar;
            m_GridCenterPoint = Vector3.zero;
            m_GridRowDir = Vector3.right;
            m_GridColumnDir = -Vector3.up;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (m_CreateTestObjects)
            {
                m_Objects = new List<GameObject>();
                for (int r = 0; r < m_NumRows; r++)
                {
                    for (int c = 0; c < m_NumColumns; c++)
                    {
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        go.name = "Grid Object (" + c + ", " + r + ")";
                        go.transform.SetParent(transform, false);
                        m_Objects.Add(go);
                    }
                }
            }

            UpdateObjectTransforms();
        }

        void UpdateObjectTransforms()
        {
            for (int i = 0; i < m_Objects.Count; i++)
            {
                Vector3 pos = m_GridCenterPoint;
                Quaternion rot = Quaternion.identity;
                int c = ObjectIDToCol(i);
                int r = ObjectIDToRow(i);
                Vector3 xOffset = m_GridColumnDir * c;
                Vector3 yOffset = m_GridRowDir * r;


                m_Objects[i].transform.localPosition = pos + xOffset + yOffset;
                m_Objects[i].transform.localRotation = rot;
            }
        }


        int ObjectIDToCol(int id)
        {
            return id % m_NumColumns;
        }

        int ObjectIDToRow(int id)
        {
            return (int)Mathf.Floor(id / m_NumColumns);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}