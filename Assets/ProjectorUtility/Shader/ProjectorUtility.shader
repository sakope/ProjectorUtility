Shader "Custom/ProjectorUtility"
{
    Properties {
        _MainTex("Base (RGB)", 2D) = "white" {}
    }
    CGINCLUDE
    #include "UnityCG.cginc"

    struct ProjectorUtilityBuffer
    {
        uint   screenID;
        float  overlapTop;
        float  overlapBottom;
        float  overlapLeft;
        float  overlapRight;
        float  uvShiftX;
        float  uvShiftY;
        float  maskTop;
        float  maskBottom;
        float  maskLeft;
        float  maskRight;
    };

    StructuredBuffer<ProjectorUtilityBuffer> buf;
    uint _numOfColPrjctrs;
    uint _numOfRowPrjctrs;

    sampler2D _MainTex;
    float _blackness, _power, _brightness;

    fixed4 frag_blend(v2f_img vf) : COLOR {
        float2 coord = vf.uv;
        fixed  col   = 1.0;

        uint colID = floor(coord.x / (1.0 / _numOfColPrjctrs));
        uint rowID = floor(coord.y / (1.0 / _numOfRowPrjctrs));
        //screenID. It's row order from upper left. (just like Z form)
        uint SID   = colID + rowID * _numOfColPrjctrs;

        float hrztlLen = 1.0 / (float)_numOfColPrjctrs;
        float vrtclLen = 1.0 / (float)_numOfRowPrjctrs;
        
        //// uv shift for overlap region. ////
        int leftOLCount  = (int)floor(_numOfColPrjctrs / 2.0) - (int)colID;
        int rightOLCount = (int)colID + 1 - (int)ceil(_numOfColPrjctrs / 2.0);
        int upperOLCount = (int)floor(_numOfRowPrjctrs / 2.0) - (int)rowID;
        int lowerOLCount = (int)rowID + 1 - (int)ceil(_numOfRowPrjctrs / 2.0);
        //even is 1, odd is 0.
        fixed isEvenCol = 1 - _numOfColPrjctrs % 2;
        fixed isEvenRow = 1 - _numOfRowPrjctrs % 2;

        for (int i = 0; i < leftOLCount; i++)
        {
            //if proxies (for performance)
			fixed ifCenterEven = step(0.001, (float)_numOfColPrjctrs / 2.0 - (float)colID - i - isEvenCol);
            //add every left overlap width from center
            vf.uv.x += buf[SID + i + 1].overlapLeft * ifCenterEven + buf[SID + i].overlapRight;
        }
        for (int j = 0; j < rightOLCount; j++)
        {
			fixed ifCenterEven = step(0.001, (float)_numOfColPrjctrs / 2.0 - ((float)_numOfColPrjctrs - (float)colID - 1) - j - isEvenCol);
            //add every right overlap width from center
            vf.uv.x -= buf[SID - j - 1].overlapRight * ifCenterEven + buf[SID - j].overlapLeft;
        }
        for (int k = 0; k < upperOLCount; k++)
        {
			fixed ifCenterEven = step(0.001, (float)_numOfRowPrjctrs / 2.0 - (float)rowID - k - isEvenRow);
            //add every lower overlap width from center
            vf.uv.y += buf[SID + (k + 1) * _numOfColPrjctrs].overlapTop * ifCenterEven + buf[SID + k * _numOfColPrjctrs].overlapBottom;
        }
        for (int m = 0; m < lowerOLCount; m++)
        {
			fixed ifCenterEven = step(0.001, (float)_numOfRowPrjctrs / 2.0 - ((float)_numOfRowPrjctrs - (float)rowID - 1) - m - isEvenRow);
            //add every upper overlap width from center
            vf.uv.y -= buf[SID - (m + 1) * _numOfColPrjctrs].overlapBottom * ifCenterEven + buf[SID - m * _numOfColPrjctrs].overlapTop;
        }

        //// custom uv shift region. ////
        vf.uv += float2(buf[SID].uvShiftX, buf[SID].uvShiftY);

        //// overlap gradient region. ////
        col *= lerp(saturate(abs(coord.x - (float)colID / (float)_numOfColPrjctrs) / buf[SID].overlapLeft), 1, buf[SID].overlapLeft == 0);
        col *= lerp(saturate(abs(((float)colID + 1.0) / (float)_numOfColPrjctrs - coord.x) / buf[SID].overlapRight), 1, buf[SID].overlapRight == 0);
        col *= lerp(saturate(abs(coord.y - (float)rowID / (float)_numOfRowPrjctrs) / buf[SID].overlapTop), 1, buf[SID].overlapTop == 0);
        col *= lerp(saturate(abs(((float)rowID + 1.0) / (float)_numOfRowPrjctrs - coord.y) / buf[SID].overlapBottom), 1, buf[SID].overlapBottom == 0);

        //// overlap color tuning region. ////
        //blackness.
        col = saturate(1.0 - (1.0 - col) * _blackness);
        //blend curve.
        col = pow(col, _power);
        //blending area brightness.
        col = lerp(col, col * _brightness, (col < 1.0));

        //// mask region. ////
        fixed topMask = step(vrtclLen * rowID + buf[SID].maskTop, coord.y);
        fixed btmMask = step(coord.y, vrtclLen * (rowID + 1) - buf[SID].maskBottom);
        fixed lftMask = step(hrztlLen * colID + buf[SID].maskLeft, coord.x);
        fixed rgtMask = step(coord.x, hrztlLen * (colID + 1) - buf[SID].maskRight);
        col *= topMask.xxxx * btmMask.xxxx * lftMask.xxxx * rgtMask.xxxx;

        //// color result. ////
        return col * tex2D(_MainTex, vf.uv);
    }

    fixed4 frag_mask(v2f_img v) : COLOR {
        return fixed4(0, 0, 0, 1);
    }

    ENDCG

    SubShader {
        Tags{ "RenderType" = "Opaque" "Queue" = "Overlay+99000" }
        ZTest Always Cull Off ZWrite Off
        Fog{ Mode off }
        ColorMask RGB

        pass {
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 5.0
            #pragma vertex vert_img
            #pragma fragment frag_blend
            ENDCG
        }

        pass {
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 5.0
            #pragma vertex vert_img
            #pragma fragment frag_mask
            ENDCG
        }
    }
}
