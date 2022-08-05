
@rem Start one graphics program per wall
START "Front Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrconfig CaveFrontWall_Top
SLEEP 5
START "Front Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrconfig CaveFrontWall_Bottom
SLEEP 5
START "Left Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrconfig CaveLeftWall_Top
SLEEP 5
START "Left Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrconfig CaveLeftWall_Bottom
SLEEP 5
START "Right Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrconfig CaveRightWall_Top
SLEEP 5
START "Right Wall" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrconfig CaveRightWall_Bottom
SLEEP 5
START "Floor" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrconfig CaveFloor_Top
SLEEP 5
START "Floor" C:\Users\vrdemo\Desktop\Cave\MinVRUnity\Build\MinVRUnity.exe -vrmode stereo -vrconfig CaveFloor_Bottom

