using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


namespace IVLab.MinVR3
{
    /// <summary>
    /// These constants and functions help to add MinVR items to Unity's internal GameObject, Component,
    /// and Asset menus in a consistent way.  Use these both within the main MinVR package and plugins.
    /// </summary>
    public class MenuHelpers : MonoBehaviour
    {
        // CONSTANTS FOR MINVR MENU ITEM PRIORITIES

        // Note: Setting up the priorities to get the menu structure looking the way we want is difficult,
        // involving lots of trial and error!  When adding items to the "MinVR" menus, use these constants
        // to help position the menu items correctly.  When debugging menu placement, note that restarting
        // Unity is (sometimes?) required.  Don't trust that the placement is correct until after a restart,
        // and if a change to the priority is made but does not seem to have made a difference, try restarting
        // Unity.


        // Use this priority for the first item listed in the "MinVR" and "MinVR Interaction" menus, this places
        // these menus at the end of the first big section in Unity's GameObject menu.  For the "MinVR" menu,
        // the first item is the special "Get Started" > "Create VREngine and Room Space Origin" option.
        public const int gameObjectMenuPriority = 11;


        // Use this priority for the "New VRConfig (Create Your Own From Template)" item.  Because its priority
        // is more than 10 higher than that of the "Get Started" item, a divider line appears under the "Get
        // Started" item and before this item.
        public const int vrConfigSec1Priority = gameObjectMenuPriority + 11;

        // Use this priority for items inside Section 2 of the VRConfigs submenu.  Because it is more than 10 higher
        // than Section 1, a divider line appears between Section 1 and 2.
        public const int vrConfigSec2Priority = vrConfigSec1Priority + 11;

        // Use this priority for items inside Section 3 of the VRConfigs submenu.  Because it is more than 10 higher
        // than Section 2, a divider line appears between Section 2 and 3.
        public const int vrConfigSec3Priority = vrConfigSec2Priority + 11;


        // Use this priority for all items in the MinVR menu other than the special cases of the "Get Started"
        // item and the "VRConfigs" submenu  described above.  This is more than 10 higher than the "Get Started"
        // item, so these items will go in Section2 of the GameObject > MinVR menu.  The priority just 1
        // greater than the first item in the VRConfigs submenu, so these items should be placed in the same
        // section, but just lower than the VRConfigs subment.Note: Menu items with the same priority seem to be
        // listed in the order encountered in these C# files.
        public const int minVRSec2Priority = vrConfigSec1Priority + 1;


        // Use this priority for all other items in the "MinVR Interaction" menu.  This menu does not have any
        // subsections, so the priorities are easier, everything gets a priority one greater than the first
        // item in this submenu. Note: Menu items with the same priority seem to be listed in the order
        // encountered in these C# files.
        public const int mvriItemPriority = gameObjectMenuPriority + 1;



        // HELPER FUNCTIONS FOR ADDING MENU ITEMS

        /// <summary>
        /// Adds two EventAlias components to the game object, one for a Position event, one for a Rotation event.
        /// </summary>
        /// <param name="go">Game Object to add the components to.</param>
        /// <param name="aliasBaseName">Name for the alias event without /Position or /Rotation</param>
        /// <param name="origEventBaseName">Name for the existing event that should be renamed with the alias,
        /// without the trailing /Position or /Rotation.</param>
        public static void AddTrackingAliases(GameObject go, string aliasBaseName, string origEventBaseName)
        {
            VREventAlias posAlias = go.AddComponent<VREventAlias>();
            posAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            posAlias.aliasEventName = aliasBaseName + "/Position";
            posAlias.originalEvents = new List<VREventPrototypeAny>()
                { VREventPrototypeAny.Create<Vector3>(origEventBaseName + "/Position") };

            VREventAlias rotAlias = go.AddComponent<VREventAlias>();
            rotAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            rotAlias.aliasEventName = aliasBaseName + "/Rotation";
            rotAlias.originalEvents = new List<VREventPrototypeAny>()
                { VREventPrototypeAny.Create<Quaternion>(origEventBaseName + "/Rotation") };
        }
        
        /// <summary>
        /// (For the Meta Quest devices) Adds two EventAlias components to the game object, one for a Position event, one for a Rotation event.
        /// </summary>
        /// <param name="go">Game Object to add the components to.</param>
        /// <param name="aliasBaseName">Name for the alias event without /Position or /Rotation</param>
        /// <param name="origEventBaseName">Name for the existing event that should be renamed with the alias,
        /// without the trailing /Position or /Rotation.</param>
        public static void AddQuestTrackingAliases(GameObject go, string aliasBaseName, string origEventBaseName)
        {
            VREventAlias posAlias = go.AddComponent<VREventAlias>();
            posAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            posAlias.aliasEventName = aliasBaseName + "/Position";
            posAlias.originalEvents = new List<VREventPrototypeAny>()
                { VREventPrototypeAny.Create<Vector3>(origEventBaseName + "/Pointer/Position") };

            VREventAlias rotAlias = go.AddComponent<VREventAlias>();
            rotAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            rotAlias.aliasEventName = aliasBaseName + "/Rotation";
            rotAlias.originalEvents = new List<VREventPrototypeAny>()
                { VREventPrototypeAny.Create<Quaternion>(origEventBaseName + "/Pointer/Rotation") };
        }

        /// <summary>
        /// Adds two EventAlias components to the game object, one for a Down event, one for an Up event.
        /// </summary>
        /// <param name="go">Game Object to add the components to.</param>
        /// <param name="aliasBaseName">Name for the alias event without /Down or /Up</param>
        /// <param name="origEventBaseName">Name for the existing event that should be renamed with the alias,
        /// without the trailing /Down or /Up.</param>
        public static void AddButtonAliases(GameObject go, string aliasBaseName, string origEventBaseName)
        {
            VREventAlias downAlias = go.AddComponent<VREventAlias>();
            downAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            downAlias.aliasEventName = aliasBaseName + "/Down";
            downAlias.originalEvents = new List<VREventPrototypeAny>()
                { VREventPrototypeAny.Create(origEventBaseName + "/Down") };

            VREventAlias upAlias = go.AddComponent<VREventAlias>();
            upAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            upAlias.aliasEventName = aliasBaseName + "/Up";
            upAlias.originalEvents = new List<VREventPrototypeAny>()
                { VREventPrototypeAny.Create(origEventBaseName + "/Up") };
        }
        
        /// <summary>
        /// Adds one EventAlias components to the game object
        /// </summary>
        /// <param name="go">Game Object to add the components to.</param>
        /// <param name="aliasBaseName">Name for the alias event</param>
        /// <param name="origEventBaseName">Name for the existing event that should be renamed with the alias</param>
        public static VREventAlias AddButtonAlias(GameObject go, string aliasBaseName, string origEventBaseName)
        {
            VREventAlias buttonAlias = go.AddComponent<VREventAlias>();
            buttonAlias.aliasStrategy = VREventAlias.AliasStrategy.RenameClone;
            buttonAlias.aliasEventName = aliasBaseName;
            buttonAlias.originalEvents = new List<VREventPrototypeAny>() { VREventPrototypeAny.Create(origEventBaseName) };
            return buttonAlias;
        }

        /// <summary>
        /// True if the GameObject is a child of the MinVR Room Space Origin.
        /// </summary>
        public static bool IsUnderRoomSpace(GameObject go)
        {
            return go.GetComponentInParent<RoomSpaceOrigin>() != null;
        }

        /// <summary>
        /// MinVR requires one object in the scene to be identified as the Room Space Origin.  This routine
        /// creates that object at the root of the hierarchy if it is not already found in the hierarchy.
        /// </summary>
        /// <returns>The found or newly created GameObject with RoomSpaceOrigin attached.</returns>
        public static GameObject CreateRoomSpaceOriginIfNeeded()
        {
            RoomSpaceOrigin rso = FindObjectOfType<RoomSpaceOrigin>();
            if (rso != null) {
                return rso.gameObject;
            } else {
                return CreateAndPlaceGameObject("Room Space", null, typeof(RoomSpaceOrigin));
            }
        }

        /// <summary>
        /// MinVR requires the singleton VREngine class to exist in the hierarchy.  This routine creates it at the
        /// root of the hierarchy if needed.  Following MinVR's convention, it also creates a
        /// minvr-configvals-default.txt config file in the Assets folder.
        /// </summary>
        /// <returns>The found or newly created GameObject with VREngine attached.</returns>
        public static GameObject CreateVREngineIfNeeded()
        {
            VREngine engine = FindObjectOfType<VREngine>();
            if (engine != null) {
                return engine.gameObject;
            } else {
                GameObject engineGO = CreateAndPlaceGameObject("VREngine", null, typeof(VREngine));
                engine = engineGO.GetComponent<VREngine>();

                // create a config file as well using the following template
                string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath("Assets/minvr-configvals-default.txt");
                string configText =
                    $"# MinVR3 ConfigVal File ({uniqueFileName})\n" +
                    "# This file is parsed when VREngine starts up.\n" +
                    "# \n" +
                    "# Notes: The typical use of this file is to define default settings for ConfigVals, where\n" +
                    "# \"default\" means the value works well for multiple VRConfigs.  For example, a good size in\n" +
                    "# meters for a VR menu might be the same for many VR HMDs, CAVEs, or Powerwalls so a default\n" +
                    "# size could be defined here like `MENU_SIZE = 1.5`.  MENU_SIZE would not be redefined in any\n" +
                    "# VRConfig-specific config files (e.g., settings-cave.minvr.txt) where 1.5 is a good size,\n" +
                    "# but MENU_SIZE would be redefined in VRConfig-specific config files where the size should\n" +
                    "# be different.  For example, the menu would need to be much smaller than 1.5 meters when\n" +
                    "# running in Desktop or zSpace modes, so these config files could overwrite the MENU_SIZE\n" +
                    "# setting by including a line like `MENU_SIZE = 0.25`.  In the menu's Start() function,\n" +
                    "# Programmers would apply the setting to the menu by writing something like:\n" +
                    "#    this.size = ConfigVal(\"MENU_SIZE\", 1.5);\n" +
                    "# Note, the second argument to the ConfigVal command is the default value to use if an\n" +
                    "# entry for MENU_SIZE is not found.\n" +
                    "\n" +
                    "MENU_SIZE = 1.5\n" +
                    "\n";
                File.WriteAllText(uniqueFileName, configText);
                AssetDatabase.ImportAsset(uniqueFileName);

                TextAsset newConfigFile = (TextAsset)AssetDatabase.LoadAssetAtPath(uniqueFileName, typeof(TextAsset));
                engine.configManager.AddConfigFile(newConfigFile);

                return engineGO;
            }
        }

        /// <summary>
        /// Creates a standard layout for MinVR VRConfig nodes.  Includes a root GameObject named VRConfig_Name and
        /// three child objects (Event Aliases, Input Devices, Display Devices).  Following MinVR's convention, it
        /// also creates a minvr-configvals-name.txt config file in the Assets folder. The child objects will not
        /// (yet) have any specific input, display, or alias components attached.
        /// </summary>
        /// <returns>The created VRConfig_Name GameObject.</returns>
        public static GameObject CreateVRConfigTemplate(MenuCommand command, string name,
            ref GameObject inputDevChild, ref GameObject displayDevChild, ref GameObject eventAliasesChild)
        {
            CreateVREngineIfNeeded();
            CreateRoomSpaceOriginIfNeeded();

            // unless user has explicitly parented to another object, VRConfigs should be parented
            // to the Room Space Origin.
            GameObject parent = command.context as GameObject;
            if (parent == null) {
                // find RoomSpaceOrigin should never fail here since we create it if needed above
                parent = FindObjectOfType<RoomSpaceOrigin>().gameObject;
            }

            GameObject vrConfigObj = CreateAndPlaceGameObject("VRConfig_" + name, parent, typeof(VRConfig));
            VRConfig vrConfig = vrConfigObj.GetComponent<VRConfig>();
            eventAliasesChild = CreateAndPlaceGameObject("Event Aliases", vrConfigObj, new Type[] { });
            inputDevChild = CreateAndPlaceGameObject("Input Devices", vrConfigObj, new Type[] { });
            displayDevChild = CreateAndPlaceGameObject("Display Devices", vrConfigObj, new Type[] { });

            // create a config file as well using the following template
            string vrConfigNameLower = name.ToLower().Replace(" ", "");
            string uniqueFileName = AssetDatabase.GenerateUniqueAssetPath($"Assets/minvr-configvals-{vrConfigNameLower}.txt");
            string configText =
                $"# MinVR3 ConfigVal File ({uniqueFileName})\n" +
                $"# This file is only parsed when VRConfig_{name} is used.\n" +
                $"# When parsed, settings in this file will override those with the same name in minvr-configvals-default.txt\n" +
                "\n" +
                "MENU_SIZE = 0.25\n" +
                "\n";
            File.WriteAllText(uniqueFileName, configText);
            AssetDatabase.ImportAsset(uniqueFileName);

            TextAsset newConfigFile = (TextAsset)AssetDatabase.LoadAssetAtPath(uniqueFileName, typeof(TextAsset));
            vrConfig.AddConfigFile(newConfigFile);

            Selection.activeGameObject = vrConfigObj;
            return vrConfigObj;
        }

        /// <summary>
        /// Creates a new GameObject and adds it to the hierarchy.
        /// </summary>
        /// <param name="name">Name for the new object</param>
        /// <param name="parent">New object's parent or 'null' to place in the root of the hierarchy.</param>
        /// <param name="componentTypes">Array of zero or more components to attach to the object.  To attach
        /// zero components use an empty array, like 'new Type[] {}'.</param>
        /// <returns>The newly created GameObject.</returns>
        public static GameObject CreateAndPlaceGameObject(string name, GameObject parent, params Type[] componentTypes)
        {
            GameObject go = ObjectFactory.CreateGameObject(name, componentTypes);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            if (parent != null) {
                Undo.SetTransformParent(go.transform, parent.transform, "Reparenting");
                ResetTransform(go.transform);
                go.layer = parent.gameObject.layer;
            }
            GameObjectUtility.EnsureUniqueNameForSibling(go);
            Selection.activeGameObject = go;
            return go;
        }

        /// <summary>
        /// Creates one of Unity's built-in Primitive GameObjects and adds it to the hierarchy.  
        /// </summary>
        /// <param name="name">Name for the new object</param>
        /// <param name="parent">New object's parent or 'null' to place in the root of the hierarchy.</param>
        /// <param name="primitiveType">Cube, sphere, capsule, plane, etc.</param>
        /// <returns>The newly created Primitive GameObject.</returns>
        public static GameObject CreateAndPlacePrimitive(string name, GameObject parent, PrimitiveType primitiveType)
        {
            GameObject go = ObjectFactory.CreatePrimitive(primitiveType);
            go.name = name;
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            if (parent != null) {
                Undo.SetTransformParent(go.transform, parent.transform, "Reparenting");
                ResetTransform(go.transform);
                go.layer = parent.gameObject.layer;
            }
            GameObjectUtility.EnsureUniqueNameForSibling(go);
            Selection.activeGameObject = go;
            return go;
        }

        /// <summary>
        /// Resets the transform to the identity
        /// </summary>
        public static void ResetTransform(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (transform.parent is RectTransform) {
                var rectTransform = transform as RectTransform;
                if (rectTransform != null) {
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.sizeDelta = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// Finds a prefab in the AssetDatabase and adds an instance of it to the hierarchy.
        /// </summary>
        /// <param name="parent">Parent for the new prefab or null to place at the root of the hierarchy.</param>
        /// <param name="searchStr">Identifies the prefab by path or type.  Several options are possible.  See
        /// Unity's AssetDatabase.FindAssets() for details.</param>
        public static GameObject InstatiatePrefabFromAsset(GameObject parent, string searchStr)
        {
            UnityEngine.Object prefabAsset = null;
            string[] guids = AssetDatabase.FindAssets(searchStr);
            if (guids.Length > 0) {
                string fullPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                prefabAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullPath);
            }

            Debug.Assert(prefabAsset != null, "Cannot find requested prefab in the AssetDatabase using search string '" + searchStr + "'.");
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, parent);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
            return go;
        }
    }

} // end namespace
