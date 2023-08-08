using UnityEngine;

namespace IVLab.MinVR3
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets the path of this GameObject in the scene hierarchy
        /// </summary>
        public static string GetScenePath(this GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }
    }
}