using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity, Collision(PartOfMap = true)]
    public class Gate : Entity
    {
        private EntityPreset _preset;
        private bool IsTemp { get { return _preset.Frame >= 10; } }

        private bool HeldDown;
        private Player _player;

        public static AnimatedSpriteRenderer GetSprite(int closed) => new("gates", 16, 16,
            new Anim("still", new int[] { closed },1),
            new Anim("opened", new int[] { closed + 3 },1),
            new Anim("close", new int[] { closed + 3, closed + 2, closed + 1, closed }, closed == 0 ? 10 : 8, false),
            new Anim("open", new int[] { closed, closed + 1, closed + 2, closed + 3 }, closed == 0 ? 10 : 4, false)
            );

        public Gate(EntityPreset preset, Player p) : base(preset.Position, GetSprite(int.Parse(preset.TypeValue)), DrawOrder.ENTITIES)
        {
            _preset = preset;
            _player = p;
            immovable = true;
            
            //Player holds down the gate when they stand on it on entering the screen
            HeldDown = p.Hitbox.Intersects(Hitbox);
            if(HeldDown && !ConditionSatisfied())
            {
                Play("opened");
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
            else if(CurAnimName == "still" || CurAnimName == "close")
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
