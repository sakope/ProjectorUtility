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
        float2 maskTopLeft;
        float2 maskTopRight;
        float2 maskBottomLeft;
        float2 maskBottomRight;
    };

    StructuredBuffer<ProjectorUtilityBuffer> buf;
    uint _numOfColPrjctrs;
    uint _numOfRowPrjctrs;

    sampler2D _MainTex;
    float _blackness, _power;

    fixed4 frag(v2f_img vf) : COLOR {
        float2 coord = vf.uv;
        fixed  col   = 1.0;

        uint colID = floor(coord.x / (1.0 / _numOfColPrjctrs));
        uint rowID = floor(coord.y / (1.0 / _numOfRowPrjctrs));
        //screenID. It's row order from upper left. (just like Z form)
        uint SID   = colID + rowID * _numOfColPrjctrs;

        fixed hrztlLen = 1.0 / (float)_numOfColPrjctrs;
        fixed vrtclLen = 1.0 / (float)_numOfRowPrjctrs;
        
        //// uv shift for overlap region. ////
        int leftOLCount  = (int)ceil(_numOfColPrjctrs / 2) - (int)colID;
        int rightOLCount = (int)colID + 1 - (int)floor(_numOfColPrjctrs / 2);
        int upperOLCount = (int)ceil(_numOfRowPrjctrs / 2) - (int)rowID;
        int lowerOLCount = (int)rowID + 1 - (int)floor(_numOfRowPrjctrs / 2);
        //even is 1, odd is 0.
        fixed isEvenCol = 1 - _numOfColPrjctrs % 2;
        fixed isEvenRow = 1 - _numOfRowPrjctrs % 2;

        for (int i = 0; i < leftOLCount; i++)
        {
            //if proxies (for performance)
            fixed ifEdge = step(0.1, i);
            fixed ifCenter = step((float)i, (float)_numOfColPrjctrs / 2 - (float)colID - 0.6 + isEvenCol);
            //add every left overlap width from center
            vf.uv.x += buf[SID + i].overlapLeft * ifEdge + buf[SID + i].overlapRight * ifCenter;
        }
        for (int j = 0; j < rightOLCount; j++)
        {
            fixed ifEdge = step(0.1, j);
            fixed ifCenter = step((float)j, colID - _numOfColPrjctrs / 2 + isEvenCol);
            //add every right overlap width from center
            vf.uv.x -= buf[SID - j].overlapRight * ifEdge + buf[SID - j].overlapLeft * ifCenter;
        }
        for (int k = 0; k < upperOLCount; k++)
        {
            fixed ifEdge = step(0.1, k);
            fixed ifCenter = step((float)k, (float)_numOfRowPrjctrs / 2 - (float)rowID - 0.6 + isEvenRow);
            //add every lower overlap width from center
            vf.uv.y += buf[SID + k * _numOfColPrjctrs].overlapTop * ifEdge + buf[SID + k * _numOfColPrjctrs].overlapBottom * ifCenter;
        }
        for (int m = 0; m < lowerOLCount; m++)
        {
            fixed ifEdge = step(0.1, m);
            fixed ifCenter = step((float)m, rowID - _numOfRowPrjctrs / 2 + isEvenRow);
            //add every upper overlap width from center
            vf.uv.y -= buf[SID - m * _numOfColPrjctrs].overlapBottom * ifEdge + buf[SID - m * _numOfColPrjctrs].overlapTop * ifCenter;
        }

        //// custom uv shift region. ////
        vf.uv += float2(buf[SID].uvShiftX, buf[SID].uvShiftY);

        //// overlap gradient region. ////
        // if flag proxies for performance.
        fixed isZeroOLL = step(0.99999, 1-buf[SID].overlapLeft);
        fixed isZeroOLR = step(0.99999, 1-buf[SID].overlapRight);
        fixed isZeroOLT = step(0.99999, 1-buf[SID].overlapTop);
        fixed isZeroOLB = step(0.99999, 1-buf[SID].overlapBottom);
        col *= saturate((coord.x - (float)colID / (float)_numOfColPrjctrs) / buf[SID].overlapLeft + isZeroOLL);
        col *= saturate(abs(float(colID + 1) / (float)_numOfColPrjctrs - coord.x) / buf[SID].overlapRight + isZeroOLR);
        col *= saturate((coord.y - (float)rowID / (float)_numOfRowPrjctrs) / buf[SID].overlapTop + isZeroOLT);
        col *= saturate(abs(float(rowID + 1) / (float)_numOfRowPrjctrs - coord.y) / buf[SID].overlapBottom + isZeroOLB);

        //// overlap color tuning region. ////
        col = saturate(1.0 - (1.0 - col) * _blackness);
        col = pow(col, _power);

        //// mask region. ////
        fixed topMask = step(vrtclLen * rowID + buf[SID].maskTop, coord.y);
        fixed btmMask = step(coord.y, vrtclLen * (rowID + 1) - buf[SID].maskBottom);
        fixed lftMask = step(hrztlLen * colID + buf[SID].maskLeft, coord.x);
        fixed rgtMask = step(coord.x, hrztlLen * (colID + 1) - buf[SID].maskRight);
        col *= topMask.xxxx * btmMask.xxxx * lftMask.xxxx * rgtMask.xxxx;

		//// corner mask region. ////
		fixed topLeftMask = buf[SID].maskTopLeft.x * (1 - saturate((coord.y - vrtclLen * rowID) / buf[SID].maskTopLeft.y));
		topLeftMask = step(topLeftMask + hrztlLen * colID, coord.x);
		fixed topRightMask = buf[SID].maskTopRight.x * (1 - saturate((coord.y - vrtclLen * rowID) / buf[SID].maskTopRight.y));
		topRightMask = step(coord.x, hrztlLen * (colID + 1) - topRightMask);
		fixed bottomLeftMask = buf[SID].maskBottomLeft.x * (1 - saturate((vrtclLen * (rowID + 1) - coord.y) / buf[SID].maskBottomLeft.y));
		bottomLeftMask = step(bottomLeftMask + hrztlLen * colID, coord.x);
		fixed bottomRightMask = buf[SID].maskBottomRight.x * (1 - saturate((vrtclLen * (rowID + 1) - coord.y) / buf[SID].maskBottomRight.y));
		bottomRightMask = step(coord.x, hrztlLen * (colID + 1) - bottomRightMask);
		col *= (topLeftMask * topRightMask * bottomLeftMask * bottomRightMask).xxxx;

		//// color result. ////
        return col * tex2D(_MainTex, vf.uv);
    }
    ENDCG

    SubShader {
        ZTest Always Cull Off ZWrite Off
        Fog{ Mode off }
        ColorMask RGB

        pass {
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}
