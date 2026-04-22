cmake_minimum_required(VERSION 3.10)

# 1. Project Metadata
project(RayTracer VERSION 1.0 LANGUAGES C CXX)

# 2. Set C++ Standard (important for ray tracing performance features)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# 3. Find Packages
# On Ubuntu: sudo apt install libglfw3-dev libgles2-mesa-dev
find_package(OpenGL REQUIRED)
find_package(glfw3 REQUIRED)

# 4. Include Directories
# Tell CMake where your headers and GLAD headers are
include_directories(include)

# 5. Define the Executable
# List all your source files here. 
# Don't forget glad.c!
add_executable(${PROJECT_NAME} 
    src/main.cpp 
    src/glad.c
)

# 6. Link Libraries
# This tells the linker to connect your code with the OpenGL and GLFW binaries
target_link_libraries(${PROJECT_NAME} 
    glfw 
    OpenGL::GL 
    ${CMAKE_DL_LIBS}
)

# 7. Optimization (Crucial for Ray Tracing)
if(NOT CMAKE_BUILD_TYPE)
  set(CMAKE_BUILD_TYPE Release)
endif()

# Define the assets folder
set(ASSET_FOLDER "${CMAKE_CURRENT_SOURCE_DIR}/shaders")

# Create a custom target that always syncs shaders
add_custom_target(sync_shaders ALL
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        "${ASSET_FOLDER}"
        "${CMAKE_CURRENT_BINARY_DIR}/shaders"
    COMMENT "Syncing shaders to build directory..."
)

# Make the executable depend on syncing shaders
add_dependencies(${PROJECT_NAME} sync_shaders)

target_compile_options(${PROJECT_NAME} PRIVATE -Wall -Wextra -O3)