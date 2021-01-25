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
        private static readonly Vector2 Position = new Vector2(155, 2);

        private List<HealthBarPiece> _healthPieces;

        private int _lastAmount;

        public HealthBar()
        {
            _lastAmount = GlobalState.CUR_HEALTH;

            _healthPieces = new List<HealthBarPiece>();

            CreateHealthBoxes();
        }

        public void CreateHealthBoxes()
        {
            _healthPieces.Clear();

            for (int i = 0; i < GlobalState.MAX_HEALTH; i++)
            {
                _healthPieces.Add(new HealthBarPiece(GetHealthPiecePos(i)));
            }
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
        /// Updates health to the current value.
        /// </summary>
        /// <returns></returns>
        public void UpdateHealth()
        {
            int change = GlobalState.CUR_HEALTH - _lastAmount;
            
            if (change < 0)
            {
                LowerHealth(change);
            }
            else
            {
                IncreaseHealth(change);
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
        }


        private void LowerHealth(int change)
        {
            int cur_health = GlobalState.CUR_HEALTH;

            for (int i = cur_health - change; i > cur_health; i--)
            {
                var h = _healthPieces[i-1];
                h.Play("empty");
            }
        }

        private void IncreaseHealth(int change)
        {
            int cur_health = GlobalState.CUR_HEALTH;


            for (int i = cur_health; i > cur_health - change; i--)
            {
                var h = _healthPieces[i-1];
                h.Play("full");
            }
        }

        public static Vector2 GetHealthPiecePos(int num)
        {
            return new Vector2(
                Position.X - HealthBarPiece.BOX_WIDTH - 8 * (7 - num % 8) - 7 * (num / 8),
                Position.Y + num / 8 * (HealthBarPiece.BOX_HEIGHT + 1));
        }
    }
}
