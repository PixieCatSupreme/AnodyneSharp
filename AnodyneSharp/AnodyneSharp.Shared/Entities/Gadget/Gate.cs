using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity, Collision(typeof(Player))]
    public class Gate : Entity
    {
        private EntityPreset _preset;
        private bool IsTemp { get { return _preset.Frame >= 10; } }
        private int ClosedFrame;

        private bool HeldDown;
        private Player _player;

        public Gate(EntityPreset preset, Player p) : base(preset.Position, "gates", 16, 16, DrawOrder.ENTITIES)
        {
            //Hack to fix missing permanence in map
            preset.Permanence = Permanence.GLOBAL;
            
            _preset = preset;
            _player = p;
            immovable = true;

            ClosedFrame = 0;
            bool fast = true;
            if(GlobalState.CURRENT_MAP_NAME == "BLANK")
            {
                ClosedFrame = 8;
                fast = false;
            }
            else if(GlobalState.CURRENT_MAP_NAME == "TRAIN")
            {
                ClosedFrame = 16;
                fast = false;
            }
            AddAnimation("still", CreateAnimFrameArray(ClosedFrame));
            AddAnimation("close", CreateAnimFrameArray(ClosedFrame + 3, ClosedFrame + 2, ClosedFrame + 1, ClosedFrame), fast ? 10 : 8, false);
            AddAnimation("open", CreateAnimFrameArray(ClosedFrame, ClosedFrame + 1, ClosedFrame + 2, ClosedFrame + 3), fast ? 10 : 4, false);


            //Player holds down the gate when they stand on it on entering the screen
            HeldDown = p.Hitbox.Intersects(Hitbox);
            if(HeldDown && !ConditionSatisfied())
            {
                SetFrame(ClosedFrame+3);
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
                if(_player.Hitbox.Intersects(Hitbox))
                {
                    HeldDown = false;
                    Play("close");
                    SoundManager.PlaySoundEffect("hit_ground_1");
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
                }
            }

            base.Update();

        }

        public override void Collided(Entity other)
        {
            if(!HeldDown && _curAnim.name != "open")
            {
                Separate(this, other);
            }
        }
    }
}
