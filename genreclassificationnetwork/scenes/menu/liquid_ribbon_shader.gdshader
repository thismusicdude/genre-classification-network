// Inspired by FencerDevLog
shader_type canvas_item;

uniform float zoom: hint_range(1.0, 200.0, 0.1) = 50;
uniform float wave_crop: hint_range(0.0, 1.0, 0.01) = 0.8;
uniform float brightness: hint_range(0.0, 2.0, 0.1) = 0.5;
uniform float speed: hint_range(0.0, 10.0, 0.01) = 1;
uniform int iterations: hint_range(0, 100, 1) = 5;
uniform float freq_start: hint_range(0.0, 50.0, 0.01) = 20;
uniform float freq_coeff: hint_range(0.0, 20.0, 0.01) = 1;

void fragment() {
	vec2 uv = UV -0.5;
	float time = TIME * speed * 0.01;
	
	// fractal brownian motion without fractals
	float freq = freq_start;
	for (int i = 0; i < iterations; i++) {
		uv.y += sin(uv.x * 4.0 + time) / freq;
		uv.x += cos(uv.y * 8.0 + time) / freq;
		freq += freq_coeff;
	}
	
	float wave = sin(uv.y * zoom) * 0.5 + 0.5;
	wave = 1.0 - smoothstep(0.0, 0.5, wave);
	float hard_limit = step(wave_crop, 1.0 - abs(uv.y));
	float soft_limit = smoothstep(wave_crop, wave_crop + 0.1, 1.0 - abs(uv.y));
	wave *= hard_limit;
	vec3 color = vec3(0.1) * wave;
	color.gb += brightness * vec2(uv.x + 0.5, uv.y + 0.5) * soft_limit;
	COLOR = vec4(color, 1.0);
}

// fratal brownian motion