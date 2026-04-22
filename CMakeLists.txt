cmake_minimum_required(VERSION 3.10)
project(RayTracer VERSION 1.0 LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

find_package(OpenGL REQUIRED)
find_package(glfw3 REQUIRED)

include_directories(include)

add_executable(${PROJECT_NAME} 
    src/main.cpp 
    src/glad.c
)

target_link_libraries(${PROJECT_NAME} 
    glfw 
    OpenGL::GL 
    ${CMAKE_DL_LIBS}
)

if(NOT CMAKE_BUILD_TYPE)
  set(CMAKE_BUILD_TYPE Release)
endif()

set(ASSET_FOLDER "${CMAKE_CURRENT_SOURCE_DIR}/shaders")

add_custom_target(sync_shaders ALL
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        "${ASSET_FOLDER}"
        "${CMAKE_CURRENT_BINARY_DIR}/shaders"
    COMMENT "Syncing shaders to build directory..."
)

add_dependencies(${PROJECT_NAME} sync_shaders)

target_compile_options(${PROJECT_NAME} PRIVATE -Wall -Wextra -O3)