#version 330 core
out vec4 FragColor;  

in vec2 screenPos;
  
void main()
{
    FragColor = vec4(screenPos, 0, 1.0);
}