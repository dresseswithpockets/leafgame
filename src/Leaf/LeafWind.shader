shader_type spatial;
render_mode cull_disabled;

uniform float TimeScale;
uniform vec2 WindDir;
uniform vec3 EdgeScale;
uniform sampler2D EdgeMask;
uniform vec3 NoiseScale;
uniform sampler2D WindNoise;

uniform vec3 DirectWindNoiseOffset;
uniform vec3 DirectWindNoiseScale;
uniform vec2 DirectWindNoiseDir;
uniform sampler2D DirectWindNoise;
uniform vec3 DirectWindStrength;

uniform sampler2D Albedo : hint_albedo;
uniform vec4 Color : hint_color;

void fragment() {
    vec4 col = texture(Albedo, UV) * Color;
    ALBEDO = col.rgb;
    //ALPHA = col.a;
}

void vertex() {
    vec3 noise = (texture(WindNoise, UV + (normalize(WindDir) * TimeScale * TIME)).xyz - vec3(0.5)) * NoiseScale;
    vec3 edge_contribution = texture(EdgeMask, UV).xyz * EdgeScale;
    vec3 wind_contribution = (texture(DirectWindNoise, UV + (normalize(DirectWindNoiseDir) * TimeScale * TIME)).xyz + DirectWindNoiseOffset) * DirectWindNoiseScale;
    vec3 offset = noise * edge_contribution + DirectWindStrength * wind_contribution * edge_contribution;
    VERTEX += offset;
}