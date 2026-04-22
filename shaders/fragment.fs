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

struct Material {
    vec4 color;
};

struct Sphere {
    vec3 position;
    float radius;
    Material material;
};

layout(std140) uniform SceneData {
    int numSpheres;
    Sphere spheres[];
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

HitInfo CalculateRayCollision(Ray ray)
{
    HitInfo closestHit;
    closestHit.hit = false;
    closestHit.dist = 1e20;

    for (int i = 0; i < numSpheres; i++)
    {
        HitInfo hitInfo = RaySphere(ray, spheres[i].position, spheres[i].radius);
        if (hitInfo.hit && hitInfo.dist < closestHit.dist)
        {
            closestHit = hitInfo;
            closestHit.material = spheres[i].material;
        }
    }

    return closestHit;
}
  
void main()
{
    vec3 viewPointLocal = vec3(uv / 2, 1) * viewParams;
    vec3 viewPointWorld = (camLocalToWorldMatrix * vec4(viewPointLocal, 1)).xyz;

    Ray ray;
    ray.origin = camPosWorld;
    ray.dir = normalize(viewPointWorld - ray.origin);

    HitInfo hitSphere = CalculateRayCollision(ray);
    if (hitSphere.hit) { FragColor = hitSphere.material.color; }
    else { FragColor = vec4(ray.dir, 1); }
}