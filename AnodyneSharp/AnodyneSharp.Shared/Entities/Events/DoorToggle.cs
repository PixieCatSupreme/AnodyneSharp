using AnodyneSharp.Entities.Gadget;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Events
{
    //Toggles the state of a door
    [Collision(typeof(Door))]
    public class DoorToggle : Entity
    {
        public DoorToggle(Vector2 pos, int w, int h) : base(pos, w, h, Drawing.DrawOrder.ENTITIES)
        {
            visible = false;
        }

        public override void Update()
        {
            //Update happens after collision check, allows for multiple doors to be toggled with this
            exists = false;
        }

        public override void Collided(Entity other)
        {
            Door d = (Door)other;
            d.Active = !d.Active;
        }
    }
}
