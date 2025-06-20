shader_type spatial;

// Novalogic terrain textures
uniform sampler2D polytrn_colormap:filter_linear_mipmap_anisotropic;        // Dvxi5_c.tga - base color map
uniform sampler2D polytrn_detailmap:filter_linear_mipmap_anisotropic;       // Det_d1.tga - base detail
uniform sampler2D polytrn_detailmap_c1:filter_linear_mipmap_anisotropic;    // Det_dg1.tga - dirt-grass
uniform sampler2D polytrn_detailmap_c2:filter_linear_mipmap_anisotropic;    // Det_ds2.tga - dirt-stone
uniform sampler2D polytrn_detailmap_c3:filter_linear_mipmap_anisotropic;    // Det_mr2.tga - mud-rock
uniform sampler2D polytrn_detailmap2:filter_linear_mipmap_anisotropic;      // Det_d1d.tga - secondary detail
uniform sampler2D polytrn_detailmapdist:filter_linear_mipmap_anisotropic;   // DetgrsD2.tga - distance map
uniform sampler2D polytrn_tilestrip:filter_linear_mipmap_anisotropic;       // trntile10.tga - texture atlas
uniform sampler2D polytrn_detailblendmap:filter_linear_mipmap_anisotropic;  // Dvxi5_d1.tga - blend control

// Water parameters
uniform float water_height : hint_range(0.0, 100.0) = 21.0;
uniform float water_murk : hint_range(0.0, 1.0) = 0.3;
uniform vec3 water_color : source_color = vec3(0.424, 0.318, 0.188);

// Terrain parameters
uniform int sector_index : hint_range(1, 4) = 1;  // Which sector (1-4) this mesh represents
uniform float detail_scale1 : hint_range(1.0, 512.0) = 128.0;
uniform float detail_scale2 : hint_range(1.0, 64.0) = 8.0;
uniform bool debug_sectors;  // Enable sector color debugging

varying vec3 world_pos;
varying vec2 terrain_uv;

void vertex(){
    // Calculate world position using the mesh vertices directly
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;

    // Map sector UV (0-1 within sector) to the correct quarter of the 1024x1024 texture
    // Each sector gets a 0.5x0.5 portion of the texture
    vec2 sector_uv = UV;  // Use the mesh UVs (0-1 within this sector)

    // Map to correct quarter based on sector index
    vec2 sector_offset = vec2(0.0);
    if (sector_index == 1) {
        sector_offset = vec2(0.0, 0.0);    // Top-left
    } else if (sector_index == 2) {
        sector_offset = vec2(0.0, 0.5);    // Bottom-left
    } else if (sector_index == 3) {
        sector_offset = vec2(0.5, 0.0);    // Top-right
    } else if (sector_index == 4) {
        sector_offset = vec2(0.5, 0.5);    // Bottom-right
    }

    // Scale sector UV to 0.5x0.5 and offset to correct quarter
    terrain_uv = sector_uv * 0.5 + sector_offset;

    // Use the mesh's existing normals
    NORMAL = NORMAL;
}

void fragment(){
    vec3 albedo = vec3(1.0);

    // Calculate detail texture UVs based on world position
    vec2 detailUV1 = world_pos.xz / detail_scale1;  // Main details: configurable scale
    vec2 detailUV2 = world_pos.xz / detail_scale2;  // Secondary detail: configurable scale

    // Get the blend map for terrain texture mixing
    vec4 blendMap = texture(polytrn_detailblendmap, terrain_uv);

    // Get the base color map
    vec3 baseColor = texture(polytrn_colormap, terrain_uv).rgb;

    // Sample detail textures at two different scales
    vec3 detail1 = texture(polytrn_detailmap, detailUV1).rgb;
    vec3 detailBlend = detail1;  // Start with base detail

    // Apply terrain type blending based on blend map channels
    vec3 detail1C1 = texture(polytrn_detailmap_c1, detailUV1).rgb;
    vec3 detail1C2 = texture(polytrn_detailmap_c2, detailUV1).rgb;
    vec3 detail1C3 = texture(polytrn_detailmap_c3, detailUV1).rgb;

    if(blendMap.r > 0.0)
        detailBlend = mix(detailBlend, detail1C1, blendMap.r);  // Add grass variant
    if(blendMap.g > 0.0)
        detailBlend = mix(detailBlend, detail1C2, blendMap.g);  // Add stone variant
    if(blendMap.b > 0.0)
        detailBlend = mix(detailBlend, detail1C3, blendMap.b);  // Add mud variant

    // Second detail layer for large scale variation
    vec3 detail2 = texture(polytrn_detailmap2, detailUV2).rgb;
    vec3 detailBlend2 = detail2;

    vec3 detail2C1 = texture(polytrn_detailmap_c1, detailUV2).rgb;
    vec3 detail2C2 = texture(polytrn_detailmap_c2, detailUV2).rgb;
    vec3 detail2C3 = texture(polytrn_detailmap_c3, detailUV2).rgb;

    if(blendMap.r > 0.0)
        detailBlend2 = mix(detailBlend2, detail2C1, blendMap.r);  // Add grass variant
    if(blendMap.g > 0.0)
        detailBlend2 = mix(detailBlend2, detail2C2, blendMap.g);  // Add stone variant
    if(blendMap.b > 0.0)
        detailBlend2 = mix(detailBlend2, detail2C3, blendMap.b);  // Add mud variant

    // Combine base color with detail layers
    albedo = baseColor * (detailBlend * detailBlend2);

    // Debug: Override with sector colors if enabled
    if (debug_sectors) {
        if (sector_index == 1) {
            albedo = vec3(1.0, 0.0, 0.0);  // Red
        } else if (sector_index == 2) {
            albedo = vec3(0.0, 1.0, 0.0);  // Green
        } else if (sector_index == 3) {
            albedo = vec3(0.0, 0.0, 1.0);  // Blue
        } else if (sector_index == 4) {
            albedo = vec3(1.0, 1.0, 0.0);  // Yellow
        }
    }

    ALBEDO = albedo;
}