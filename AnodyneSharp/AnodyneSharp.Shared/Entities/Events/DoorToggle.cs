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
        bool hitDoor = false;

        public DoorToggle(Vector2 pos, int w, int h) : base(pos, w, h)
        {
        }

        public override void Update()
        {
            //Update happens after collision check, allows for multiple doors to be toggled with this
            if (hitDoor)
            {
                exists = false;
            }
        }

        public override void Collided(Entity other)
        {
            Door d = (Door)other;
            d.Active = !d.Active;
            hitDoor = true;
        }
    }
}
