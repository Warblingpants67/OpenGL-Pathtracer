#include <iostream>

#include <glad/glad.h>
#include <GLFW/glfw3.h>

#include <glm/glm/glm.hpp>
#include <glm/glm/gtc/matrix_transform.hpp>
#include <glm/glm/gtc/type_ptr.hpp>

#include <shader.h>
#include <camera.h>

void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void processInput(GLFWwindow *window);
void setViewParamsAndScreenSize();

// settings
const unsigned int SCR_WIDTH = 800;
const unsigned int SCR_HEIGHT = 600;

// camera
Camera camera(glm::vec3(0.0f, 0.0f, 0.0f));
float lastX = SCR_WIDTH / 2.0f;
float lastY = SCR_HEIGHT / 2.0f;
bool firstMouse = true;

// timing
float deltaTime = 0.0f;
float lastFrame = 0.0f;

// window
unsigned int screenWidth;
unsigned int screenHeight;

// shaders
Shader* pathtracingShader;

struct Material {
    glm::vec4 color;
    glm::vec4 emission;
};

struct Sphere {
    glm::vec3 position;
    float radius;
    Material material;
};

struct SceneBuffer {
    int numSpheres;
    float padding[3];
    Sphere spheres[];
};

std::vector<Sphere> spheres = {
    { glm::vec3(0, 0, 6), 0.6f, { glm::vec4(1, 0, 0, 1), glm::vec4(0, 0, 0, 0) } },
    { glm::vec3(2, 0, 6), 0.6f, { glm::vec4(0, 1, 0, 1), glm::vec4(0, 0, 0, 0) } },
    { glm::vec3(-2, 0, 6), 0.6f, { glm::vec4(0, 0, 1, 1), glm::vec4(0, 0, 0, 0) } },
    { glm::vec3(0, 2, 6), 0.2f, { glm::vec4(1, 1, 1, 1), glm::vec4(1, 1, 1, 1) } },
    { glm::vec3(0, -1000, 6), 1000, { glm::vec4(1, 1, 1, 1), glm::vec4(1, 1, 1, 0) } }
};  

int main()
{
    glfwInit();
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 6);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_COMPAT_PROFILE);

    GLFWwindow* window = glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "Raytracer", NULL, NULL);
    if (window == NULL)
    {
        std::cout << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }
    glfwMakeContextCurrent(window);
    glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);

    screenWidth = SCR_WIDTH;
    screenHeight = SCR_HEIGHT;

    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
    {
        std::cout << "Failed to initialize GLAD" << std::endl;
        return -1;
    }

    pathtracingShader = new Shader("shaders/vertex.vs", "shaders/fragment.fs");

    float vertices[] = {
        -1.0f, -1.0f, 0.0f,     // bottom left  
        1.0f, -1.0f, 0.0f,      // bottom right 
        1.0f,  1.0f, 0.0f,      // top right
        -1.0f, -1.0f, 0.0f,     // bottom left  
        1.0f,  1.0f, 0.0f,      // top right
        -1.0f,  1.0f, 0.0f      // top left
    };

    unsigned int VBO, VAO;
    glGenVertexArrays(1, &VAO);
    glGenBuffers(1, &VBO);
    glBindVertexArray(VAO);

    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 0, (void*)0);
    glEnableVertexAttribArray(0);

    SceneBuffer sceneBuffer;
    sceneBuffer.numSpheres = spheres.size();
    std::copy(spheres.begin(), spheres.end(), sceneBuffer.spheres);

    unsigned int sceneDataBuffer;
    glGenBuffers(1, &sceneDataBuffer);
    glBindBuffer(GL_SHADER_STORAGE_BUFFER, sceneDataBuffer);
    glBufferData(GL_SHADER_STORAGE_BUFFER, 16 + spheres.size() * sizeof(Sphere), &sceneBuffer, GL_STATIC_DRAW);
    glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 0, sceneDataBuffer);

    unsigned int frameCount = 0;
    while (!glfwWindowShouldClose(window))
    {
        float currentFrame = static_cast<float>(glfwGetTime());
        deltaTime = currentFrame - lastFrame;
        lastFrame = currentFrame;

        std::cout << "\rFPS: " << (1 / deltaTime);

        processInput(window);

        glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT);

        pathtracingShader->use();
        pathtracingShader->setMat4("camLocalToWorldMatrix", camera.GetLocalToWorldMatrix());
        pathtracingShader->setVec3("camPosWorld", camera.Position);
        setViewParamsAndScreenSize();
        pathtracingShader->setInt("frameIndex", frameCount);

        glBindVertexArray(VAO);
        glDrawArrays(GL_TRIANGLES, 0, 6);

        glfwSwapBuffers(window);
        glfwPollEvents();

        frameCount++;
    }

    glDeleteVertexArrays(1, &VAO);
    glDeleteBuffers(1, &VBO);

    glfwTerminate();
    return 0;
}

void processInput(GLFWwindow *window)
{
    if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
        glfwSetWindowShouldClose(window, true);

    if (glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS)
        camera.ProcessKeyboard(FORWARD, deltaTime);
    if (glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS)
        camera.ProcessKeyboard(BACKWARD, deltaTime);
    if (glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS)
        camera.ProcessKeyboard(LEFT, deltaTime);
    if (glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS)
        camera.ProcessKeyboard(RIGHT, deltaTime);
}

void mouse_callback(GLFWwindow* window, double xposIn, double yposIn)
{
    float xpos = static_cast<float>(xposIn);
    float ypos = static_cast<float>(yposIn);

    if (firstMouse)
    {
        lastX = xpos;
        lastY = ypos;
        firstMouse = false;
    }

    float xdelta = xpos - lastX;
    float ydelta = lastY - ypos;

    lastX = xpos;
    lastY = ypos;

    camera.ProcessMouseMovement(xdelta, ydelta);
}

void framebuffer_size_callback(GLFWwindow* window, int width, int height)
{
    glViewport(0, 0, width, height);
    screenWidth = width;
    screenHeight = height;
}

void setViewParamsAndScreenSize()
{
    float aspect = (float)screenHeight / screenWidth;
    pathtracingShader->setVec3("viewParams", glm::vec3(1, aspect, 1));
    pathtracingShader->setVec2("screenSize", glm::vec2(screenWidth, screenHeight));
}