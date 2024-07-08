using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RunningTradeNPCs
{
    public class MiaoGraphics : Entity
    {
        public static AnimatedSpriteRenderer GetSprite() => new("fields_npcs", 16, 16,
            new Anim("walk_d", new int[] { 0, 1 }, 4),
            new Anim("walk_r", new int[] { 2, 3 }, 4),
            new Anim("walk_u", new int[] { 4, 5 }, 4),
            new Anim("walk_l", new int[] { 6, 7 }, 4)
            );

        public MiaoGraphics(Vector2 position) : base(position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            width = height = 12;
            offset.X = offset.Y = 2;
        }
    }

    [NamedEntity("Trade_NPC", null, 0), Collision(typeof(Player))]
    public class MiaoXiao : MiaoGraphics, Interactable
    {
        MiaoXiaoFollower follower;

        public MiaoXiao(EntityPreset preset, Player p) : base(preset.Position)
        {
            immovable = true;
            follower = new(preset.Position, p);
            if(p.follower != null)
            {
                exists = false;
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(follower, 1);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if(GlobalState.events.SpookedMonster && !DialogueManager.IsSceneFinished("miao", "philosophy"))
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("miao", "philosophy");
            }
            else
            {
                //Ask to shadow, then swap places
                GlobalState.Dialogue = DialogueManager.GetDialogue("miao", "init");
                exists = false;
                follower.Activate();
            }
            return true;
        }
    }

    //Follower logic
    [Collision(MapCollision = true), Events(typeof(StartWarp),typeof(EndScreenTransition), typeof(StartScreenTransition))]
    public class MiaoXiaoFollower : MiaoGraphics
    {
        static readonly Dictionary<Point, (string name, string eventCheck, int diagId)> DialogueScreens = new()
        {
            [new Point(3,2)] = ("shopkeep", "shopkeep.init",0),
            [new Point(1,2)] = ("mitra", "mitra.fieldinit", 1),
            [new Point(2,1)] = ("nexus", null, 2)
        };


        const float dialogue_screen_timeout = 1.8f;
        const float hint_timeout = 70f;

        float timer = hint_timeout;
        (string name, string eventCheck, int diagId) currentDiag = (null,null,-1);
        string wait_on_event = null;

        Player follow;

        Entity dust = new(Vector2.Zero, Dust.GetSprite(), Drawing.DrawOrder.BG_ENTITIES);

        bool on_conveyor_this_frame = false;

        Rectangle boundaries;
        Point ScreenOffset => new(GlobalState.CURRENT_GRID_X - boundaries.X, GlobalState.CURRENT_GRID_Y - boundaries.Y);

        public MiaoXiaoFollower(Vector2 pos, Player p) : base(pos)
        {
            follow = p;
            dust.exists = false;
            dust.visible = false;
            dust.Play("poofed");

            exists = false;

            boundaries = new Rectangle(GlobalState.CURRENT_GRID_X, GlobalState.CURRENT_GRID_Y-1, 4, 3);
        }
        
        public void Activate()
        {
            exists = true;
            follow.follower = this;
            dust.exists = true;
        }

        public override void Update()
        {
            base.Update();
            
            Vector2 offset = follow.Center - Center;
            if(offset.LengthSquared() > 16*16)
            {
                offset.Normalize();
                velocity = offset * 70;
            }
            else
            {
                velocity = Vector2.Zero;
            }

            FaceTowards(follow.Position);
            PlayFacing("walk");
            
            if(!on_conveyor_this_frame && dust.CurAnimName == "unpoof")
            {
                dust.Play("poof");
            }
            dust.Position = Position - new Vector2(2, 0);
            on_conveyor_this_frame = false;

            if(wait_on_event != null && GlobalState.events.GetEvent(wait_on_event) != 0)
            {
                currentDiag = DialogueScreens[ScreenOffset];
                timer = dialogue_screen_timeout;
                wait_on_event = null;
            }

            timer -= GameTimes.DeltaTime;
            if(timer < 0 && !GlobalState.ScreenTransition && GlobalState.LastDialogueFinished)
            {
                int id = currentDiag.diagId;
                if(currentDiag.name != null)
                {
                    GlobalState.events.IncEvent($"miao.{currentDiag.name}");
                }
                else if(!GlobalState.events.SpookedMonster)
                {
                    //Ask where Icky is
                    id = 3;
                }
                else if(DialogueScreens.Values.All((s) => GlobalState.events.GetEvent($"miao.{s.name}") != 0))
                {
                    //Random talk when all other dialogues have happened
                    id = GlobalState.RNG.Next(5,8);
                }
                
                if (id != -1)
                {
                    GlobalState.Dialogue = DialogueManager.GetDialogue("miao", "randoms", id);
                }

                timer = hint_timeout;
                currentDiag = (null, null, -1);
            }
        }

        public override void OnEvent(GameEvent e)
        {
            if(e is StartWarp || (e is EndScreenTransition && !boundaries.Contains(GlobalState.CURRENT_GRID_X,GlobalState.CURRENT_GRID_Y)))
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("miao", "randoms", 4);
                exists = false;
                dust.exists = false;
                follow.follower = null;
            }
            else if(e is EndScreenTransition)
            {
                if(DialogueScreens.TryGetValue(ScreenOffset,out (string name, string eventCheck, int diagId) screen)) {
                    if(GlobalState.events.GetEvent(screen.eventCheck) == 0)
                    {
                        wait_on_event = screen.eventCheck;
                    }
                    if((screen.eventCheck == null || GlobalState.events.GetEvent(screen.eventCheck) != 0) && GlobalState.events.GetEvent($"miao.{screen.name}") == 0)
                    {
                        timer = dialogue_screen_timeout;
                        currentDiag = screen;
                    }
                }
            }
            else if(e is StartScreenTransition)
            {
                if (currentDiag.name != null)
                {
                    timer = hint_timeout;
                    currentDiag = (null, null, -1);
                }
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(dust, 1);
        }

        public override void Conveyor(Touching direction)
        {
            base.Conveyor(direction);
            if(dust.CurAnimName != "unpoof")
            {
                dust.Play("unpoof");
                dust.visible = true;
            }
            on_conveyor_this_frame = true;
        }

    }
}
