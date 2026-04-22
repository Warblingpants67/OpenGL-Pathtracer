#version 330 core
out vec4 FragColor;  

in vec2 uv;
uniform vec3 viewParams;
uniform vec3 camPosWorld;
uniform mat4 camLocalToWorldMatrix;

struct Ray {
    vec3 origin;
    vec3 dir;
};

struct HitInfo {
    bool hit;
    float dist;
    vec3 hitPoint;
    vec3 normal;
};

HitInfo RaySphere(Ray ray, vec3 sphereOrigin, float sphereRadius)
{
    HitInfo hitInfo;
    hitInfo.hit = false;
    vec3 rayOriginRelativeToSphere = ray.origin - sphereOrigin;

    float a = dot(ray.dir, ray.dir);
    float b = 2 * dot(rayOriginRelativeToSphere, ray.dir);
    float c = dot(rayOriginRelativeToSphere, rayOriginRelativeToSphere) - sphereRadius * sphereRadius;

    float discriminant = b * b - 4 * a * c;

    if (discriminant >= 0)
    {
        float dist = (-b - sqrt(discriminant)) / (2 * a);

        if (dist >= 0)
        {
            hitInfo.hit = true;
            hitInfo.dist = dist;
            hitInfo.hitPoint = ray.origin + ray.dir * dist;
            hitInfo.normal = normalize(hitInfo.hitPoint - sphereOrigin);
        }
    }

    return hitInfo;
}
  
void main()
{
    vec3 viewPointLocal = vec3(uv / 2, 1) * viewParams;
    vec3 viewPointWorld = (camLocalToWorldMatrix * vec4(viewPointLocal, 1)).xyz;

    Ray ray;
    ray.origin = camPosWorld;
    ray.dir = normalize(viewPointWorld - ray.origin);

    HitInfo hitSphere = RaySphere(ray, vec3(0, 0, 5), 1);
    if (hitSphere.hit) { FragColor = vec4(1, 1, 1, 1); }
    else { FragColor = vec4(ray.dir, 1); }
}