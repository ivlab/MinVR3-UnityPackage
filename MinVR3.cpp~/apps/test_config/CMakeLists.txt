# This file is part of the MinVR3 cmake build system.  
# See the main ../CMakeLists.txt file for details.

project(test_config)


# Source:
set (SOURCEFILES
  main.cpp
)
set (HEADERFILES
)

set(EXTRAFILES
   example-minvr3-config.txt
)


set_source_files_properties(${EXTRAFILES} PROPERTIES HEADER_FILE_ONLY TRUE)


# Define the target
add_executable(${PROJECT_NAME} ${HEADERFILES} ${SOURCEFILES} ${EXTRAFILES})


# Add dependency on libMinVR3:
target_include_directories(${PROJECT_NAME} PUBLIC ../../src)
target_link_libraries(${PROJECT_NAME} PUBLIC MinVR3)


# Installation:
install(TARGETS ${PROJECT_NAME}
        RUNTIME DESTINATION ${INSTALL_BIN_DEST}
        COMPONENT Tests)


# For better organization when using an IDE with folder structures:
set_property(TARGET ${PROJECT_NAME} PROPERTY FOLDER "Tests")
source_group("Header Files" FILES ${HEADERFILES})
