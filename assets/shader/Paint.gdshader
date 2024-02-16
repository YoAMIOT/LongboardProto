shader_type canvas_item;

uniform float brush_size : hint_range(1.0, 10.0);
uniform float intensity : hint_range(0.0, 1.0);

void fragment() {
    vec2 uv = FRAGCOORD.xy / vec2(SCREEN_PIXEL_SIZE.x, SCREEN_PIXEL_SIZE.y);
    vec4 color = texture(TEXTURE, uv);
    
    // Convert UV coordinates to pixel coordinates
    vec2 pixel = uv * vec2(SCREEN_PIXEL_SIZE.x, SCREEN_PIXEL_SIZE.y);
    vec2 offset = vec2(1.0 / SCREEN_PIXEL_SIZE.x, 1.0 / SCREEN_PIXEL_SIZE.y) * brush_size;
    
    // Apply oil painting effect
    vec4 finalColor = vec4(0.0);
    for (float x = -brush_size; x <= brush_size; x++) {
        for (float y = -brush_size; y <= brush_size; y++) {
            finalColor += texture(TEXTURE, uv + vec2(x, y) * offset);
        }
    }
    finalColor /= (2.0 * brush_size + 1.0) * (2.0 * brush_size + 1.0);
    
    // Adjust intensity
    finalColor = mix(color, finalColor, intensity);
    
    // Output final color
    COLOR = finalColor;
}