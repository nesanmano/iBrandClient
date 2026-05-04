float4 RGBToHSL(float4 pixel)
{
	float4 hsl;
    pixel /=  256.0f;
    float minVal, maxVal;

    minVal = fmin(pixel.x, pixel.y);
    minVal = fmin(minVal, pixel.z);

    maxVal = fmax(pixel.x, pixel.y);
    maxVal = fmax(maxVal, pixel.z);

    //h
    if (minVal == maxVal)
    {
        hsl.x = 0;
    }
    else if (maxVal == pixel.z)
    {
        hsl.x = fmod((60 * ((pixel.y - pixel.x) / (maxVal - minVal)) + 360), 360);
    }
    else if (maxVal == pixel.y)
    {
        hsl.x = 60 * ((pixel.x - pixel.z) / (maxVal - minVal)) + 120;
    }
    else
    {
        hsl.x = 60 * ((pixel.z - pixel.y) / (maxVal - minVal)) + 240;
    }

    //l
    hsl.z = (maxVal + minVal) / 2.0f;

    //s
    if (maxVal == minVal)
    {
        hsl.y = 0;
    }
    else if (hsl.z <= 0.5f)
    {
        hsl.y = (maxVal - minVal) / (2.0f * hsl.z);
    }
    else
    {
        hsl.y = (maxVal - minVal) / (2.0f - (2.0f * hsl.z));
    }
	hsl.w = pixel.w;

    return hsl;
}

float4 HSLtoRGB(float4 hsl)
{
    float p, q, hk;
    float4 t;
    float4 adjust;
	adjust.x = 1 / 3.0f;
	adjust.y = 0;
	adjust.z = 0 - (1 / 3.0f);
    float4 rgb;

    if (hsl.y != 0.0f)
    {
        if (hsl.z < 0.5f)
        {
            q = hsl.z * (1 + hsl.y);
        }
        else
        {
            q = hsl.z + hsl.y - (hsl.z * hsl.y);
        }

        p = 2 * hsl.z - q;

        hk = hsl.x / 360.0f;

        t.x = hk + adjust.x;
        if(t.x < 0.0f) {
            t.x += 1;
        } else if(t.x > 1.0f) {
            t.x -= 1;
        }
        
        if(6*t.x < 1.0f) {
            // p + ((q-p)*6*tc)
            rgb.z = (p + ((q-p)*6*t.x)) * 255.0f;
			
        } else if(2*t.x < 1.0f) {
            // q 
            rgb.z = q * 255.0f;
        } else if(3*t.x < 2.0f) {
            // p+((q-p)*6*((2/3)-tc)) 
            rgb.z = (p+((q-p)*6*((2.0f/3.0f)-t.x))) * 255.0f;
        } else {
            rgb.z= p * 255.0f;
        }
        
        t.y = hk + adjust.y;
        if(t.y < 0.0f) {
            t.y += 1;
        } else if(t.y > 1.0f) {
            t.y -= 1;
        }
        
        if(6*t.y < 1.0f) {
            // p + ((q-p)*6*tc)
            rgb.y = (p + ((q-p)*6*t.y)) * 255.0f;
        } else if(2*t.y < 1.0f) {
            // q
            rgb.y = q * 255.0f;
        } else if(3*t.y < 2.0f) {
            // p+((q-p)*6*((2/3)-tc))
            rgb.y = (p+((q-p)*6*((2/3.0f)-t.y))) * 255.0f;
        } else {
            rgb.y= p * 255.0f;
        }
        
        t.z = hk + adjust.z;
        if(t.z < 0.0f) {
            t.z += 1;
        } else if(t.z > 1.0f) {
            t.z -= 1;
        }
        
        if(6*t.z < 1.0f) {
            // p + ((q-p)*6*tc)
            rgb.x = (p + ((q-p)*6*t.z)) * 255.0f;
        } else if(2*t.z < 1.0f) {
            // q
            rgb.x = q * 255.0f;
        } else if(3*t.z < 2.0f) {
            // p+((q-p)*6*((2/3)-tc))
            rgb.x = (p+((q-p)*6*((2/3.0f)-t.z))) * 255.0f;
        } else {
            rgb.x= p * 255.0f;
        }
    }
    else
    {
        rgb.x = hsl.z;
        rgb.y = hsl.z;
        rgb.z = hsl.z;
		rgb.x = 0.0f;
		rgb.y = 0.0f;
		rgb.z = 255.0f;

    }
	rgb.w = hsl.w;

    return rgb;
}

kernel void GreenScreen(int          width,
                       int           height,
                       global uchar* pInput0,
                       int           inputStride0,
                       global uchar* pOutput,
                       int           outputStride,
					   float			 lowerHue,
					   float			 higherHue,
					   float		 lowerSaturation,
					   float		 higherSaturation,
					   float		 lowerLuminance,
					   float		 higherLuminance,
					   float			 semiLowerHue,
					   float			 semiHigherHue,
					   float		 semiLowerSaturation,
					   float		 semiHigherSaturation,
					   float		 semiLowerLuminance,
					   float		 semiHigherLuminance )
{
	size_t x = get_global_id(0);
	size_t y = get_global_id(1);
	global uchar4* pI0 = (global uchar4*)(pInput0+y*inputStride0);
	global uchar4* pO = (global uchar4*)(pOutput+y*outputStride);
	uchar4 mixed;

	float4 f4 = convert_float4(pI0[x]);
	float4 hsl = RGBToHSL(f4);
    float a = 255.0f;
    
    if( hsl.x <= higherHue && hsl.x >= lowerHue && hsl.z <= higherLuminance && hsl.z >= lowerLuminance && hsl.y <= higherSaturation && hsl.y >= lowerSaturation) {
        a = 0.0f;
        f4.x = 0.0f;
        f4.y = 0.0f;
        f4.z = 0.0f;
        f4.w = a;
    } else {
        a = 255.0f;

		bool estUnPeuVert = false;
        if( hsl.x >=semiLowerHue && hsl.x <=lowerHue) {
            hsl.x = semiLowerHue;
            estUnPeuVert = true;
        } else if (hsl.x >= higherHue && hsl.x <= semiHigherHue) {
            hsl.x = semiHigherHue;
            estUnPeuVert = true;
        } else if(hsl.x < higherHue && hsl.x > lowerHue) {
            estUnPeuVert = false;
            hsl.y=0.0f;
        }
        
        if(estUnPeuVert) {
            if( hsl.y >=semiLowerSaturation && hsl.y <=lowerSaturation) {
                hsl.y = semiLowerSaturation;
				
            } else if (hsl.y >= higherSaturation && hsl.y <= semiHigherSaturation) {
                hsl.y = semiHigherSaturation;
            }
            if( hsl.z >=semiLowerLuminance && hsl.z <=lowerLuminance) {
                hsl.z = semiLowerLuminance;
            } else if (hsl.z >= higherLuminance && hsl.z <= semiHigherLuminance) {
                hsl.z = semiHigherLuminance;
            }
        }
        
        /*float4 result = HSLtoRGB(hsl);
        f4.x = result.x;
        f4.y = result.y;
        f4.z = result.z;
        f4.w = a;*/
    }

	mixed = convert_uchar4_sat_rte( f4 );

	pO[x] = mixed;
}

