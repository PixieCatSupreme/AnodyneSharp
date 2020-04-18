using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace AnodyneSharp.UI
{

    public class HealthBar
    {
        private Vector2 _pos;
        private List<HealthBarPiece> _healthPieces;

        private int _lastAmount;

        public HealthBar(Vector2 pos)
        {
            _pos = pos;

            _lastAmount = GlobalState.CUR_HEALTH;

            _healthPieces = new List<HealthBarPiece>();

            for (int i = 0; i < GlobalState.MAX_HEALTH; i++)
            {
                MakeBox(i);
            }
        }

        public void MakeBox(int num)
        {
            Vector2 pos = new Vector2(
                _pos.X - HealthBarPiece.BOX_WIDTH - 8 * (7 - num % 8) - 7 * (num / 8),
                _pos.Y + num / 8 * (HealthBarPiece.BOX_HEIGHT + 1));

            _healthPieces.Add(new HealthBarPiece(pos));
        }

        public void Update()
        {
            foreach (var hp in _healthPieces)
            {
                hp.Update();
                hp.PostUpdate();
            }
        }

        public void Draw()
        {
            foreach (var hp in _healthPieces)
            {
                hp.Draw();
            }
        }

        /// <summary>
        /// Updates health to the current value. Returns false if player died, true if full and null otherwise.
        /// </summary>
        /// <returns></returns>
        public bool? UpdateHealth()
        {
            bool? result = null;
            int change = GlobalState.CUR_HEALTH - _lastAmount;

            if (change == 0)
            {
                return null;
            }
            else if (change < 0)
            {
                result = LowerHealth(change) ? false : result;
            }
            else
            {
                result = IncreaseHealth(change) ? true : result;
            }

            _lastAmount = GlobalState.CUR_HEALTH;

            if ( _lastAmount == 1)
            {
                _healthPieces[0].Play("flash");
            }
            else if(_lastAmount > 0)
            {
                _healthPieces[0].Play("full");
            }

            return result;
        }


        private bool LowerHealth(int change)
        {
            int cur_health = GlobalState.CUR_HEALTH;


            for (int i = cur_health - change; i > cur_health; i--)
            {
                if (i <= 0)
                {
                    return true;
                }
                var h = _healthPieces[i-1];
                h.Play("empty");
            }

            return false;
        }

        private bool IncreaseHealth(int change)
        {
            int cur_health = GlobalState.CUR_HEALTH;


            for (int i = cur_health; i > cur_health - change; i--)
            {
                if (i > GlobalState.MAX_HEALTH)
                {
                    GlobalState.CUR_HEALTH = GlobalState.MAX_HEALTH;
                    return true;
                }
                var h = _healthPieces[i-1];
                h.Play("full");
            }

            return false;
        }
    }
}
