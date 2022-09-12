using System;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Dialogue;
using AnodyneSharp.UI;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.States
{
    public class CreditsState : State
    {
        private List<UILabel> _labels;

        public CreditsState()
        {
            _labels = new List<UILabel>();

            int y = 0;

            for (int i = 0; i < 24; i++)
            {
                y += 180;

                string text = DialogueManager.GetDialogue("misc", "any", "ending", i);

                _labels.Add(new UILabel(new Vector2(0, y), true, text, centerText: true));
            }
        }

        public override void Update()
        {
            base.Update();

            float speed = 15 * GameTimes.DeltaTime;

            foreach (var label in _labels)
            {
                label.Position += new Vector2(0, -speed);
            }
        }

        public override void DrawUI()
        {
            base.DrawUI();

            foreach (var label in _labels)
            {
                label.Draw();
            }
        }
    }
}
