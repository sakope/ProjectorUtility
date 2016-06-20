using UnityEngine;
using System.Runtime.InteropServices;
using Common;

using ProjectorUtility.Controller;

namespace ProjectorUtility.Core
{
    /// <summary>
    /// compute buffer
    /// </summary>
    struct ProjectorUtilityBuffer
    {
        public uint screenID;
        public float overlapTop, overlapBottom, overlapLeft, overlapRight;
        public float uvShiftX, uvShiftY;
        public float maskTop, maskBottom, maskLeft, maskRight;
        public Vector2 maskTopLeft, maskTopRight, maskBottomLeft, maskBottomRight;

        public ProjectorUtilityBuffer(uint screenID, float overlapTop, float overlapBottom, float overlapLeft, float overlapRight, float uvShiftX, float uvShiftY, float maskTop, float maskBottom, float maskLeft, float maskRight, Vector2 maskTopLeft, Vector2 maskTopRight, Vector2 maskBottomLeft, Vector2 maskBottomRight)
        {
            this.screenID = screenID;
            this.overlapTop = overlapTop;
            this.overlapBottom = overlapBottom;
            this.overlapLeft = overlapLeft;
            this.overlapRight = overlapRight;
            this.uvShiftX = uvShiftX;
            this.uvShiftY = uvShiftY;
            this.maskTop = maskTop;
            this.maskBottom = maskBottom;
            this.maskLeft = maskLeft;
            this.maskRight = maskRight;
            this.maskTopLeft = maskTopLeft;
            this.maskTopRight = maskTopRight;
            this.maskBottomLeft = maskBottomLeft;
            this.maskBottomRight = maskBottomRight;
        }
    }
    
    /// <summary>
    /// Handle shader.
    /// </summary>
    public class ProjectorUtilityBlender : SingletonMonoBehaviour<ProjectorUtilityBlender>
    {
        [SerializeField]
        Shader _shader;

        Material _mat;
        ComputeBuffer _computeBuffer;

        void OnEnable()
        {
            _mat = new Material(_shader);
        }

        void OnDisable()
        {
            _computeBuffer.Release();
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (_mat != null)
            {
                Graphics.Blit(src, dest, _mat);
            }
        }

        public void SetBuffer()
        {
            if (_computeBuffer != null)
            {
                _computeBuffer.Release();
            }

            _computeBuffer = new ComputeBuffer(ProjectorUtilityController.Instance.NumOfScreen, Marshal.SizeOf(typeof(ProjectorUtilityBuffer)));
            ProjectorUtilityBuffer[] buf = new ProjectorUtilityBuffer[_computeBuffer.count];

            ProjectorUtilityController.Instance.GetScreenSettingEntities.ForEach(entity =>
            {
                buf[entity.ID] = new ProjectorUtilityBuffer(
                    (uint)entity.ID,
                    entity.TopBlend.Value,
                    entity.BottomBlend.Value,
                    entity.LeftBlend.Value,
                    entity.RightBlend.Value,
                    entity.uvShift.Value.x,
                    entity.uvShift.Value.y,
                    entity.topMask.Value,
                    entity.bottomMask.Value,
                    entity.leftMask.Value,
                    entity.rightMask.Value,
                    entity.topLeftMask.Value,
                    entity.topRightMask.Value,
                    entity.bottomLeftMask.Value,
                    entity.bottomRightMask.Value
                    );
            });

            _computeBuffer.SetData(buf);

            _mat.SetBuffer("buf", _computeBuffer);
            _mat.SetInt("_numOfColPrjctrs", ProjectorUtilityController.Instance.GetCommonSettingEntity.NumOfColProjectors.Value);
            _mat.SetInt("_numOfRowPrjctrs", ProjectorUtilityController.Instance.GetCommonSettingEntity.NumOfRowProjectors.Value);
            _mat.SetFloat("_blackness", ProjectorUtilityController.Instance.GetCommonSettingEntity.Blackness.Value);
            _mat.SetFloat("_power", ProjectorUtilityController.Instance.GetCommonSettingEntity.Curve.Value);
        }
    }
}