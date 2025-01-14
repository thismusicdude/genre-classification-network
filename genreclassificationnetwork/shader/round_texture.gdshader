shader_type canvas_item;

uniform float time : hint_range(0.0, 1000.0) = 0.0; // Zeit für Animation
uniform float noise_scale : hint_range(0.5, 5.0) = 2.0; // Skalierung des Rauschens
uniform float shape_density : hint_range(0.1, 2.0) = 1.0; // Dichte der abstrakten Formen
uniform vec4 base_color : source_color = vec4(0.1, 0.2, 0.3, 1.0); // Basisfarbe
uniform vec4 accent_color : source_color = vec4(1.0, 0.5, 0.2, 1.0); // Akzentfarbe

void fragment() {
    // Normale UV-Koordinaten
    vec2 uv = UV * noise_scale;

    // Basis-Rauschen erzeugen
    float noise = fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453);

    // Abstrakte Form: Kreise
    float circle1 = smoothstep(0.3, 0.35, sin(distance(uv, vec2(0.3 + time * 0.1, 0.5)) * shape_density));

    // Abstrakte Form: Linien
    float line1 = smoothstep(0.4, 0.45, abs(sin(uv.x * 10.0 + time * 0.2)));

    // Farbmix basierend auf Formen
    vec4 color = mix(base_color, accent_color, circle1 + line1);

    // Dynamisches Rauschen als Akzent
    color.rgb += noise * 0.1;

    // Glättung der Übergänge
    color.rgb = smoothstep(0.0, 1.0, color.rgb);

    // Ergebnis in die Ausgabe schreiben
    COLOR = color;
	
	 vec2 center = vec2(0.5); // Mitte des Sprites
    float radius = 0.5; // Radius des Kreises

    // Berechne den Abstand von der Mitte
    float dist = distance(UV, center);

    // Wenn der Abstand größer als der Radius ist, mach transparent
    if (dist > radius) {
        discard;
    }
}

/*shader_type canvas_item;

void fragment() {
   
}*/
