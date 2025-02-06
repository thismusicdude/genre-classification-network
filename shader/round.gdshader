shader_type canvas_item;

uniform float time : hint_range(0.0, 1000.0) = 0.0; // Zeit für Animation
uniform float noise_scale : hint_range(0.5, 5.0) = 2.0; // Skalierung des Rauschens
uniform float shape_density : hint_range(0.1, 2.0) = 1.0; // Dichte der abstrakten Formen
uniform vec4 base_color : source_color = vec4(0.1, 0.2, 0.3, 1.0); // Basisfarbe
uniform vec4 accent_color : source_color = vec4(1.0, 0.5, 0.2, 1.0); // Akzentfarbe

void fragment() {
    // Normale UV-Koordinaten
    vec2 uv = UV * noise_scale;

	 vec2 center = vec2(0.5); // Mitte des Sprites
    float radius = 0.5; // Radius des Kreises

    // Berechne den Abstand von der Mitte
    float dist = distance(UV, center);

    // Wenn der Abstand größer als der Radius ist, mach transparent
    if (dist > radius) {
        discard;
    }
}
