# This file is part of the MinVR3 cmake build system.  
# See the main MinVR3/CMakeLists.txt file for details.


set(HEADERFILES
    src/config_val.h
    src/min_net.h
    src/minvr3.h
    src/minvr3_net.h
    src/minvr3_utils.h
    src/net_headers.h
    src/vr_event.h
)

set(SOURCEFILES
    src/config_val.cpp
    src/min_net.cpp
    src/minvr3_net.cpp
    src/minvr3_utils.cpp
    src/vr_event.cpp
)

set(JSON_HEADERFILES
    src/json/json.h
    src/json/json-forwards.h
)

set(JSON_SOURCEFILES
    src/json/jsoncpp.cpp
)

set(EXTRAFILES
)


set_source_files_properties(${EXTRAFILES} PROPERTIES HEADER_FILE_ONLY TRUE)

source_group("Header Files" FILES ${HEADERFILES} ${JSON_HEADERFILES})
source_group("Source Files" FILES ${SOURCEFILES} ${JSON_SOURCEFILES})

add_library(MinVR3 ${HEADERFILES} ${JSON_HEADERFILES} ${SOURCEFILES} ${JSON_SOURCEFILES} ${EXTRAFILES} ${SHADERFILES})


# Using target_include_directories() rather than just include_directories() is
# critical in order to support generating a MinVR3Config.cmake file.  It supports
# generator expressions, so we can point to two different include dirs depending
# on whether building or using the installed version.
target_include_directories(MinVR3 PUBLIC
    $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/src>  # for headers when building
    $<INSTALL_INTERFACE:${INSTALL_INCLUDE_DEST}>        # for client in install mode
)


install(TARGETS MinVR3 EXPORT MinVR3Targets COMPONENT CoreLib
  LIBRARY DESTINATION "${INSTALL_LIB_DEST}"
  ARCHIVE DESTINATION "${INSTALL_LIB_DEST}"
  RUNTIME DESTINATION "${INSTALL_BIN_DEST}"
)

install(FILES ${HEADERFILES} DESTINATION "${INSTALL_INCLUDE_DEST}" COMPONENT CoreLib)
install(FILES ${JSON_HEADERFILES} DESTINATION "${INSTALL_INCLUDE_DEST}/json" COMPONENT CoreLib)

