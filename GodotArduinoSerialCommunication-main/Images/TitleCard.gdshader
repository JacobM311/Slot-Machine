shader_type spatial;

void fragment() {
    // Output color is set to black (0.0, 0.0, 0.0, 1.0)
    ALBEDO = vec3(0.0, 0.0, 0.0);
    // Setting alpha to 1.0 for full opacity
    ALPHA = 1.0;
}