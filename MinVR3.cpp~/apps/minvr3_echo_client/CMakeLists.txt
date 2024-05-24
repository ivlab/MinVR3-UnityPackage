# This file is part of the MinVR3 cmake build system.  
# See the main ../CMakeLists.txt file for details.

project(minvr3_echo_client)


# Source:
set (SOURCEFILES
  main.cpp
)
set (HEADERFILES
)



# Define the target
add_executable(${PROJECT_NAME} ${HEADERFILES} ${SOURCEFILES})


# Add dependency on libMinVR3:
target_include_directories(${PROJECT_NAME} PUBLIC ../../src)
target_link_libraries(${PROJECT_NAME} PUBLIC MinVR3)


# Installation:
install(TARGETS ${PROJECT_NAME}
        RUNTIME DESTINATION ${INSTALL_BIN_DEST}
        COMPONENT Apps)


# For better organization when using an IDE with folder structures:
set_property(TARGET ${PROJECT_NAME} PROPERTY FOLDER "Apps")
source_group("Header Files" FILES ${HEADERFILES})
