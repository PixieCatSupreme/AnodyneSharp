using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities
{
    [Collision(typeof(Player))]
    public class SpriteSage : Entity
    {
        protected Player _player;
        protected bool _facePlayer = true;

        public static AnimatedSpriteRenderer GetSprite() => new("sage", 16, 16,
            new Anim("idle_d", new int[] { 6 },1),
            new Anim("idle_r", new int[] { 7 },1),
            new Anim("idle_l", new int[] { 7 },1),
            new Anim("idle_u", new int[] { 8 },1),
            new Anim("walk_d", new int[] { 0, 1 }, 6),
            new Anim("walk_r", new int[] { 2, 3 }, 6),
            new Anim("walk_l", new int[] { 2, 3 }, 6),
            new Anim("walk_u", new int[] { 4, 5 }, 6)
            );

        public SpriteSage(Vector2 pos, Player p) : base(pos, GetSprite(), DrawOrder.ENTITIES)
        {
            _player = p;

            width = height = 10;
            offset = Vector2.One * 3;
            Position += offset;
            immovable = true;
        }

        public override void Update()
        {
            base.Update();
            if (_facePlayer)
            {
                FaceTowards(_player.Position);
            }
            else
            {
                if(velocity != Vector2.Zero)
                {
                    FaceTowards(Position + velocity);
                }
            }
            PlayFacing(velocity == Vector2.Zero ? "idle" : "walk");
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }

        protected override void AnimationChanged(string name)
        {
            if (name == "walk_l" || name == "idle_l")
            {
                _flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = SpriteEffects.None;
            }
        }
    }

    public abstract class Sage : SpriteSage, Interactable
    {
        protected EntityPreset _preset;

        int _initDistance;
        int _stopDistance;
        string _scene;

        public Sage(EntityPreset preset, Player p, int initDistance, int stopDistance, string scene) : base(preset.Position, p)
        {
            _preset = preset;
            _initDistance = initDistance;
            _stopDistance = stopDistance;
            _scene = scene;
        }

        protected virtual IEnumerator<CutsceneEvent> StateLogic()
        {
            MoveTowards(_player.Position, 20);

            while ((_player.Position - Position).Length() > _stopDistance)
            {
                yield return null;
            }

            velocity = Vector2.Zero;

            yield return new DialogueEvent(DialogueManager.GetDialogue("sage", _scene));

            yield break;
        }

        public override void Update()
        {
            base.Update();
            if (!_preset.Activated && !GlobalState.ScreenTransition && 
                _player.state == PlayerState.GROUND && (_player.Position - Position).Length() < _initDistance)
            {
                _preset.Activated = true;
                GlobalState.StartCutscene = StateLogic();
            }
        }

        public virtual bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", _scene);
            return true;
        }
    }

    [NamedEntity(xmlName: "Sage", map: "NEXUS")]
    public class SageNexus : Sage
    {
        public SageNexus(EntityPreset preset, Player p) : base(preset, p, 64, 20, "enter_nexus")
        {
            if (GlobalState.events.GetEvent("SageDied") > 0)
            {
                preset.Alive = exists = false;
            }
        }

        public override bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", Scene());
            return true;
        }

        static string Scene()
        {
            if(!DialogueManager.IsSceneDirty("sage", "all_card_first") && CardDataManager.GotAllNormalCardsOfAnyMap())
            {
                return "all_card_first";
            }
            if(GlobalState.events.GetEvent("WindmillOpened") != 0)
            {
                return "after_windmill";
            }
            if(GlobalState.events.BossDefeated.Contains("REDCAVE") && GlobalState.events.BossDefeated.Contains("CROWD"))
            {
                return "before_windmill";
            }
            if(GlobalState.events.BossDefeated.Contains("BEDROOM"))
            {
                return "after_bed";
            }
            if(GlobalState.events.VisitedMaps.Contains("STREET"))
            {
                return "after_ent_str";
            }
            return "enter_nexus";
        }
    }

    [NamedEntity(xmlName: "Sage", map: "OVERWORLD")]
    public class SageOverworld : Sage
    {
        public SageOverworld(EntityPreset preset, Player p) : base(preset, p, 56, 28, "bedroom_entrance")
        {
            if (GlobalState.events.BossDefeated.Contains("BEDROOM"))
            {
                preset.Alive = exists = false;
            }
        }

        protected override IEnumerator<CutsceneEvent> StateLogic()
        {
            IEnumerator<CutsceneEvent> baseState = base.StateLogic();

            while(baseState.MoveNext())
            {
                yield return baseState.Current;
            }

            while (!_player.broom.exists)
            {
                _player.actions_disabled = false;
                yield return null;
            }

            while (_player.broom.exists)
            {
                yield return null;
            }

            yield return new DialogueEvent(DialogueManager.GetDialogue("sage", "bedroom_entrance"));

            yield break;
        }
    }

    [NamedEntity(xmlName:"Sage",map:"TERMINAL")]
    public class SageTerminal : SpriteSage, Interactable
    {
        IEnumerator state;
        EntityPreset _preset;

        public SageTerminal(EntityPreset preset, Player p) : base(preset.Position,p)
        {
            if(GlobalState.events.GetEvent("SageDied") > 0)
            {
                preset.Alive = exists = false;
            }
            state = States();
            _preset = preset;
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
        }

        IEnumerator States()
        {
            while((_player.Position - Position).Length() > 46)
                yield return null;

            if(!DialogueManager.IsSceneDirty("sage","entrance"))
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "entrance");
                while (!GlobalState.LastDialogueFinished)
                    yield return null;
            }

            if (GlobalState.inventory.CardCount < 36) yield break;

            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "etc", 0);
            while (!GlobalState.LastDialogueFinished)
                yield return null;

            GlobalState.FireEvent(new ChangeCardCount(92));

            Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
            while (_player.Position.Y > tl.Y + 55) yield return null;

            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "etc", 1);
            _preset.Alive = false;

            yield break;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            string dialog = GlobalState.inventory.CardCount switch
            {
                < 18 => DialogueManager.GetDialogue("sage", "entrance", 1),
                < 36 => DialogueManager.GetDialogue("sage", "entrance", 2),
                _ => "..."
            };
            GlobalState.Dialogue = dialog;
            return true;
        }
    }

    public class DungeonSage : Sage
    {
        public DungeonSage(EntityPreset preset, Player p, int initDistance, int stopDistance, string scene)
            : base(preset,p,initDistance,stopDistance,scene)
        {
            if (GlobalState.events.LeftAfterBoss.Contains(GlobalState.CURRENT_MAP_NAME))
            {
                preset.Alive = exists = false;
            }
        }
    }

    [NamedEntity(xmlName: "Sage", map: "BEDROOM")]
    public class SageBedroom : DungeonSage
    {
        public SageBedroom(EntityPreset preset, Player p)
            : base(preset, p, 48, 24, "after_boss")
        { }
    }

    [NamedEntity(xmlName: "Sage", map: "REDCAVE")]
    public class SageRedCave : DungeonSage
    {
        public SageRedCave(EntityPreset preset, Player p)
            : base(preset, p, 28, 16, "one")
        { }
    }

    [NamedEntity(xmlName: "Sage", map: "CROWD")]
    public class SageCrowd : DungeonSage
    {
        public SageCrowd(EntityPreset preset, Player p)
            : base(preset, p, 46, 24, "one")
        { }
    }


    [NamedEntity(xmlName: "Sage", map: "BLANK"), Events(typeof(EndScreenTransition))]
    public class SageBlank : Entity
    {
        EntityPreset _preset;
        public SageBlank(EntityPreset preset, Player p) : base(preset.Position)
        {
            visible = false;
            _preset = preset;
        }

        public override void OnEvent(GameEvent e)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "intro", _preset.Frame + 1);
            exists = _preset.Alive = false;
        }
    }
}
