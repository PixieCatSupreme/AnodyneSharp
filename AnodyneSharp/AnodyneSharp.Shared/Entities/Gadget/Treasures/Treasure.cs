using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public class Treasure : BaseTreasure
    {
        private bool _flickering;
        private float _flickerTimer;
        private float _flickerFreq = 0;
        private const float FlickerLength = 0.05f;

        private Vector2 _curPos;
        private Vector2 _endPos;

        public Treasure(string textureName, Vector2 pos, int frame, int dialogueID)
            : base(textureName, pos, frame, dialogueID)
        {
            _curPos = pos;
            _endPos = new Vector2(_curPos.X, _curPos.Y - 16);
        }

        public override void Update()
        {
            if (!finished)
            {
                if (_curPos.Y > _endPos.Y)
                {
                    _curPos.Y = Math.Max(_curPos.Y - 25 * GameTimes.DeltaTime, _endPos.Y);
                    _texRect.Y = (int)_curPos.Y;
                }

                Flicker();

                if (!_flickering)
                {
                    finished = true;
                }
            }
        }

        public override void GetTreasure()
        {
            base.GetTreasure();

            _flickering = true;
            _flickerTimer = 1;

        }

        private void Flicker()
        {
            if (_flickerTimer > 0)
            {
                _flickerTimer -= GameTimes.DeltaTime;
                _flickerFreq -= GameTimes.DeltaTime;
                if (_flickerFreq <= 0)
                {
                    _flickerFreq = FlickerLength;
                    visible = !visible;
                }
            }
            else
            {
                _flickering = false;
            }
        }
    }
}
