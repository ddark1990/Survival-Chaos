using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class MouseCursorManager : MonoBehaviour
    {
        public static MouseCursorManager Instance;

        [SerializeField] private List<CursorAnimation> cursorAnimationList;

        private CursorAnimation cursorAnimation;
        private int currentFrame;
        private int frameCount;
        private float frameTimer;

        public enum CursorType
        {
            Default,
            Emote,
            Grab,
            Attack
        }

        private void Start() {
            Instance = this;

            SetActiveCursorType(CursorType.Default);
        }

        private void Update() {
            frameTimer -= Time.deltaTime;
            if(frameTimer <= 0){
                frameTimer += cursorAnimation.frameRate;
                currentFrame = (currentFrame + 1) % frameCount;
                Cursor.SetCursor(cursorAnimation.textureArray[currentFrame], cursorAnimation.offset, CursorMode.Auto);
            }
        }

        public void SetActiveCursorType(CursorType cursorType)
        {
            SetActiveCursorAnimation(GetCursorAnimation(cursorType));
        }

        private CursorAnimation GetCursorAnimation(CursorType cursorType)
        {
            foreach (var cursorAnimation in cursorAnimationList)
            {
                if(cursorAnimation.cursorType == cursorType)
                {
                    return cursorAnimation;
                }
            }

            //cannot find cursor type
            return null;
        }

        private void SetActiveCursorAnimation(CursorAnimation cursorAnimation)
        {
            this.cursorAnimation = cursorAnimation;
            currentFrame = 0;
            frameTimer = cursorAnimation.frameRate;
            frameCount = cursorAnimation.textureArray.Length;
        }

        [Serializable]
        public class CursorAnimation
        {
            public CursorType cursorType;
            public Texture2D[] textureArray;
            public float frameRate;
            public Vector2 offset;

        }
    }
}