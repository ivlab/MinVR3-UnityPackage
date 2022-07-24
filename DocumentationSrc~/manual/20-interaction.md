# Interaction Techniques

  This work will grow into its own project.  (Tentative Title:  *Open Up: The Spatial Interaction Technique Library*.)  However, it is so closely tied to the way MinVR works and so useful to consider while developing MinVR, that we will begin by working on them together in the same repository.  The scripts inside MinVR3's `interaction` folder are like the incubator for OpenUp.

  "OpenUp" - The Spatial Interaction Technique Library
  - BuildingBlocks
    - FSM
    - SharedToken
    - SimpleEventListener
    - TrackedPoseDriver
    - Cursors
  - Desktop
    - TrackballCamera
    - ObjectManip
    - CraftCam (reimplement from MinGfx)
    - UniCam (reimplement from MinGfx)
  - Widgets
    - Menus
      - Basic Floating
      - CavePainting-style Marking Menus
      - Invisible Palete
    - Spatial Sliders, Knobs
    - ColorPicker (reimplement from CavePainting)
  - Painting/Sketching
    - CavePainting / Drawing on Air
    - Lift-Off
  - Object Manipulation
    - PRISM
    - Constraint-Based Widgets
  - Visualization
    - SliceWIM
    - Bento Box
    - Worlds in Wedges
