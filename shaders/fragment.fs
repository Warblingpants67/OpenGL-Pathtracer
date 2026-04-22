#version 460 core
out vec4 FragColor;  

int _MAX_BOUNCE_COUNT = 5;
int _RAYS_PER_PIXEL = 4;

in vec2 uv;
uniform vec3 viewParams;
uniform vec2 screenSize;
uniform vec3 camPosWorld;
uniform mat4 camLocalToWorldMatrix;
uniform int frameIndex;

struct Ray {
    vec3 origin;
    vec3 dir;
};

struct Material {
    vec4 color;
    vec4 emission;
};

struct Sphere {
    vec3 position;
    float radius;
    Material material;
};

struct HitInfo {
    bool hit;
    float dist;
    vec3 hitPoint;
    vec3 normal;
    Material material;
};

layout(std430, binding = 0) buffer SceneData {
    int numSpheres;
    Sphere spheres[];
};

float randUniform(inout uint state)
{
    state = state * 747796405 + 2891336453;
    uint temp = ((state >> ((state >> 28) + 4)) ^ state) * 277803737;
    return float((temp >> 22) ^ temp) / 4294967296.0;
}

float randNormal(inout uint state)
{
    float theta = 6.28318530718 * randUniform(state);
    float rho = sqrt(-2 * log(randUniform(state)));
    return rho * cos(theta);
}

vec3 randOnSphere(inout uint state)
{
    return normalize(vec3(randNormal(state), randNormal(state), randNormal(state)));
}

vec3 randHemisphere(inout uint state, vec3 normal)
{
    vec3 dir = randOnSphere(state);
    return dir * sign(dot(dir, normal));
}

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
        Sphere sphere = spheres[i];

        HitInfo hitInfo = RaySphere(ray, sphere.position, sphere.radius);
        if (hitInfo.hit && hitInfo.dist < closestHit.dist)
        {
            closestHit = hitInfo;
            closestHit.material = sphere.material;
        }
    }

    return closestHit;
}

vec3 TraceRay(Ray ray, inout uint state)
{
    vec3 incomingLight = vec3(0);
    vec3 accumulatedColor = vec3(1);

    for (int i = 0; i <= _MAX_BOUNCE_COUNT; i++)
    {
        HitInfo hit = CalculateRayCollision(ray);
        if (hit.hit)
        {
            ray.origin = hit.hitPoint + hit.normal * 0.001;
            ray.dir = randHemisphere(state, hit.normal);

            Material material = hit.material;
            vec3 emittedLight = material.emission.rgb * material.emission.a;
            incomingLight += emittedLight * accumulatedColor;
            accumulatedColor *= material.color.rgb;
        }
        else
        {
            break;
        }
    }

    return incomingLight;
}
  
void main()
{
    uvec2 numPixels = uvec2(screenSize);
    uvec2 pixelCoord = uvec2(uv * numPixels);
    uint pixelIndex = pixelCoord.y * numPixels.x + pixelCoord.x;
    uint rngState = pixelIndex + uint(frameIndex) * 239017 + uint(frameIndex) * 29;

    vec3 viewPointLocal = vec3(uv - 0.5, 1) * viewParams;
    vec3 viewPointWorld = (camLocalToWorldMatrix * vec4(viewPointLocal, 1)).xyz;

    Ray ray;
    ray.origin = camPosWorld;
    ray.dir = normalize(viewPointWorld - ray.origin);

    vec3 totalIncomingLight = vec3(0);
    for (int i = 0; i < _RAYS_PER_PIXEL; i++)
    {
        totalIncomingLight += TraceRay(ray, rngState);
    }

    FragColor = vec4(totalIncomingLight / float(_RAYS_PER_PIXEL), 1);
}