#version 330 core

out vec4 FragColor;

uniform vec2 ures;
uniform float urandom;
uniform int uID;


// Hash function with time-based movement
vec2 hash(vec2 p) {
    p = vec2(dot(p, vec2(127.1 * urandom, 311.7 * urandom)) * urandom, dot(p * urandom, vec2(269.5 * urandom, 183.3)));
    return fract(sin(p) * 43758.5453); // Time affects point positions
}

void main() {
    if (uID == 1)
    {
        vec2 fragCoord = gl_FragCoord.xy;
        vec2 uv = fragCoord / ures; // Normalize coordinates
        uv *= 10.0; // Scale for multiple Voronoi cells
        vec2 i_uv = floor(uv);
        vec2 f_uv = fract(uv);

        float minDist = 10.0;
        vec3 color = vec3(0.0);

        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                vec2 neighbor = vec2(float(x), float(y));
                vec2 point = hash(i_uv + neighbor) + neighbor;
                float d = length(f_uv - point);
                if (d <= minDist) {
                    minDist = d;
                    color = vec3(hash(i_uv + neighbor), 0.5);
                }
            }
        }

        FragColor = vec4(color, 1.0);
    }
    else if (uID == 2)
    {
        vec2 uv = gl_FragCoord.xy / ures * 10.0; // Scale up for multiple cells
        vec2 cell = floor(uv);  // Get integer cell position
        vec2 localPos = fract(uv); // Get position inside the cell

        float minDist = 10.0; 
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                vec2 neighborCell = cell + vec2(x, y);
                vec2 point = hash(neighborCell); 
                float dist = length(localPos - (point + vec2(x, y))); // Distance to point
                minDist = min(minDist, dist);
            }
        }

        FragColor = vec4(vec3(minDist), 1.0); // Use distance for grayscale shading
    }
}
