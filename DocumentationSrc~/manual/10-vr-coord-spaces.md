# VR Coordinate Spaces

- VR Coordinate Spaces
  - (A More Careful) Introduction to Unity's Coordinate Spaces
    - Local vs. World vs. Parent in the Hierarchy
      - Unity's hierarchy is a scene graph that uses Transforms as the basic node
      - However, this can be confusing if you have worked with scene graphs before because the transform class API
        and even the editor API expose "world" coordinates/routines not routines to go from the local space to parent space
      - "position" in the editor is action "localPosition".
      - when you get the localToWorld, this transforms localPosition ALL THE WAY to world coordinates, not to the parent space
        like a typical scene graph.
      - if you want to transform from on GameObject's space to another at some other point in the hierarchy, you need to go through
        world space.
      - when you add a child to a transform, it converts its coordinates as if they were in world, unless you add the optional "false" parameter
      - The API for Transform includes 40% of the functions you might expect for converting between local, parent, and world space for points and vectors.
        MinVR add extensions to fill in the rest. 

    - Common Confusions with Unity's Transform Class
      - Extensions to the MinVR Transform class



  - MinVRRoot (RoomSpace Origin)
    - Link to VRGems article

  - Working with Coordinate Spaces in Code
    - Conversion
    - Naming Variables to Help

    name according to how the points in a mesh that you draw in local space get transformed, e.g., 

    - Vector3 pointInLocalSpace = ...
    - Vector3 pointInWorldSpace = localToWorld * pointInLocalSpace;
    - Matrix4 worldToRoom = roomToWorld.inverse();
    - Vector3 pointInRoomSpace = worldToRoom * pointInWorldSpace;


