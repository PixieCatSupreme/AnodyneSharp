using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing
{
    public enum DrawOrder
    {
        BACKGROUND,                 //Moving background textures
        MAP_BG,                     //Map layer 1
        PLAYER_REFLECTION,          //Reflection of the player
        FOOT_OVERLAY,               //Feet stuff like water wrinkles
        MAP_BG2,                    //Map layer 2
        VERY_BG_ENTITIES,           //Entities that need to go behind dust
        BG_ENTITIES,                //Background entities, animated tiles,...
        SHADOWS,                    //Shadows
        PARTICLES,                  //Things like particles, keys, blood etc
        ROLLERS,                    //Spike rollers behave weirdly with the ENTITIES layer otherwise
        ENTITIES,                   //Enemies, the player and other npcs
        FG_SPRITES,                 //Bullets and other things that have to be drawn over entities
        MAP_FG,                     //Foreground layer of the map
        DEC_OVER,                   //Screen wide effects
        DARKNESS,                   //Darkens the screen
        HITBOX,                     //Hitboxes
        CREDITS_OVERLAY,            //Credits dim overlay
        HEADER,                     //Player UI
        UI_OBJECTS,                 //Health, keys, mini mini map
        EQUIPPED_BORDER,            //The border of the equipped broom
        DUST_ICON,                  //Icon on HUD showing dust status
        HEALTH_UPGRADE,             //Health cicida. Needs to be drawn on UI so it can fly over the header
        PAUSE_BG,                   //Background for pause menu. Needs to be drawn over health upgrades. 
        EQUIPMENT_ICON,             //The inventory items for the broom types
        EQUIPPED_ICON,              //The small equipped indicator for the broom types
        AUDIO_SLIDER,               //Audio slider
        PAUSE_SELECTOR,             //Pause menu option selector
        MENUTEXT,                   //Text in pause menu
        MINIMAP,                    //Minimap in pause menu and header
        MINIMAP_PLAYER,             //Minimap player indicator
        MINIMAP_CHEST,              //Minimap chest indicator
        TEXTBOX,                    //Textboxes
        TEXT,                       //Text in dialogueBoxes
        SUBMENU_SLIDER,             //Sliders in submenu
        SUBMENU_SELECTOR,           //Equipment selector, config selector and card selector
        DEATH_FADEIN,               //A fade-in that happens on player death
        PLAYER_DIE_DUMMY,           //Player dummy who does the dying animation
        DEATH_TEXT,                 //Text that appears on player death
        BLACK_OVERLAY               //Black fadeout on map transition and when player continues after death
    }

    public static class DrawingUtilities
    {

        public static float GetDrawingZ(DrawOrder order, float gridy)
        {
            float z = (float)order;
            if(order == DrawOrder.ENTITIES)
            {
                z += gridy / (GameConstants.SCREEN_HEIGHT_IN_PIXELS + 1); //+1 to prevent z-fighting with next layer
            }
            else if((int)order > (int)DrawOrder.HITBOX)
            {
                return GetDrawingZ(order);
            }
            return 1-z/(float)DrawOrder.HITBOX;
        }

        public static float GetDrawingZ(DrawOrder order)
        {
            float z = (float)order;
            return 1-z / (float)DrawOrder.BLACK_OVERLAY;
        }
    }
}
