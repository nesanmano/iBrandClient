int coverlay(int b, int t) { return (b > 128) ? 255 - 2 * (255 - t) * (255 - b) / 255 : (b * t * 2) / 255; }

kernel void GreenScreen(int          width,
                       int           height,
                       global uchar* pInput0,
                       int           inputStride0,
                       global uchar* pOutput,
                       int           outputStride,
					   float		 r,
					   float		 g,
					   float		 b,
					   float		 opacity)
{
	size_t x = get_global_id(0);
	size_t y = get_global_id(1);
	global uchar4* pI0 = (global uchar4*)(pInput0+y*inputStride0);
	global uchar4* pO = (global uchar4*)(pOutput+y*outputStride);
	uchar4 mixed;

	float4 f4 = convert_float4(pI0[x]);
    float4 overlay;

	overlay.z = coverlay(convert_int_sat_rte(f4.z), convert_int_sat_rte(r));
	overlay.y = coverlay(convert_int_sat_rte(f4.y), convert_int_sat_rte(g));
	overlay.x = coverlay(convert_int_sat_rte(f4.x), convert_int_sat_rte(b));
	overlay.w = f4.w;

	opacity = clamp(opacity, 0.0f, 1.0f);

	f4 = overlay * opacity + (f4 * (1.0f - opacity));
    
	mixed = convert_uchar4_sat_rte( f4 );

	pO[x] = mixed;
}

