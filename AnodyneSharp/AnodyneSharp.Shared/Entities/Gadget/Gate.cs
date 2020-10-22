using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity, Collision(PartOfMap = true)]
    public class Gate : Entity
    {
        private EntityPreset _preset;
        private bool IsTemp { get { return _preset.Frame >= 10; } }
        private int ClosedFrame;

        private bool HeldDown;
        private Player _player;

        public Gate(EntityPreset preset, Player p) : base(preset.Position, "gates", 16, 16, DrawOrder.ENTITIES)
        {
            _preset = preset;
            _player = p;
            immovable = true;

            ClosedFrame = int.Parse(preset.TypeValue);
            bool fast = ClosedFrame == 0;
            
            AddAnimation("still", CreateAnimFrameArray(ClosedFrame));
            AddAnimation("close", CreateAnimFrameArray(ClosedFrame + 3, ClosedFrame + 2, ClosedFrame + 1, ClosedFrame), fast ? 10 : 8, false);
            AddAnimation("open", CreateAnimFrameArray(ClosedFrame, ClosedFrame + 1, ClosedFrame + 2, ClosedFrame + 3), fast ? 10 : 4, false);


            //Player holds down the gate when they stand on it on entering the screen
            HeldDown = p.Hitbox.Intersects(Hitbox);
            if(HeldDown && !ConditionSatisfied())
            {
                SetFrame(ClosedFrame+3);
                Solid = false;
            }
            else
            {
                Play("still");
            }
        }

        private bool ConditionSatisfied()
        {
            // 0-4,10-14 are enemies killed, 5-7,15-17 are puzzles solved
            int check = (_preset.Frame % 10 >= 5) ? GlobalState.PUZZLES_SOLVED : GlobalState.ENEMIES_KILLED;
            return check > (_preset.Frame % 5);
        }

        public override void Update()
        {
            if(HeldDown)
            {
                if(!_player.Hitbox.Intersects(Hitbox))
                {
                    HeldDown = false;
                    Play("close");
                    SoundManager.PlaySoundEffect("hit_ground_1");
                    Solid = true;
                }
            }
            else if(_curAnim.name == "still" || _curAnim.name == "close")
            {
                if (ConditionSatisfied())
                {
                    Play("open");
                    SoundManager.PlaySoundEffect("open");
                    if (!IsTemp)
                    {
                        _preset.Alive = false;
                    }
                    Solid = false;
                }
            }

            base.Update();

        }

        public override void Collided(Entity other)
        {
            if(Solid)
            {
                Separate(this, other);
            }
        }
    }
}
