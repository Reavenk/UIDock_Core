// <copyright file="DockProps.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>04/12/2020</date>
// <summary>
// The properties of windows and docks of the UIDock system.
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIDock
    {
        [CreateAssetMenu(menuName = "PxPre/DockProps")]
        public class DockProps : ScriptableObject
        {
            /// <summary>
            /// A struct coupling a sprite and color.
            /// </summary>
            [System.Serializable]
            public struct SpriteAndColor
            { 
                /// <summary>
                /// The sprite.
                /// </summary>
                public Sprite sprite;

                /// <summary>
                /// The color.
                /// </summary>
                public Color color;

                /// <summary>
                /// Apply the pair properties to an image and also set the image to 
                /// show the sprite as a simple type.
                /// </summary>
                /// <param name="img">The image to apply to.</param>
                public void ApplySimple(UnityEngine.UI.Image img)
                { 
                    this.Apply(img, UnityEngine.UI.Image.Type.Simple);
                }

                /// <summary>
                /// Apply the pair properties to an image and also set the image to
                /// show the sprite as a sliced type.
                /// </summary>
                /// <param name="img">The image to apply to.</param>
                public void ApplySliced(UnityEngine.UI.Image img)
                { 
                    this.Apply(img, UnityEngine.UI.Image.Type.Sliced);
                }

                /// <summary>
                /// Apple the pair properties to an image.
                /// </summary>
                /// <param name="img">The image to apply to.</param>
                public void Apply(UnityEngine.UI.Image img)
                { 
                    img.sprite = this.sprite;
                    img.color = this.color;
                }

                /// <summary>
                /// Apple the pair properties to an image and also set the image type.
                /// </summary>
                /// <param name="img">The image to apply to.</param>
                /// <param name="type">The image type to apply on the image.</param>
                public void Apply(UnityEngine.UI.Image img, UnityEngine.UI.Image.Type type)
                { 
                    this.Apply(img);
                    img.type = type;
                }
            }

            /// <summary>
            /// The setting for the window that change based on
            /// different 
            /// </summary>
            [System.Serializable]
            public class WindowSetting
            {
                /// <summary>
                /// The sprite for the plate of buttons.
                /// </summary>
                public SpriteAndColor spriteBtnPlate;

                /// <summary>
                /// The sprite for the frame of the windows.
                /// </summary>
                public SpriteAndColor spriteFrame;

                /// <summary>
                /// The width of the sizes of the window - 
                /// for floating windows, this is also the hitbox
                /// for drag regions.
                /// </summary>
                public float splitterWidth = 10.0f;

                /// <summary>
                /// The width of titlebar buttons.
                /// </summary>
                public float btnWidth = 20.0f;

                /// <summary>
                /// The height of titlebar buttons.
                /// </summary>
                public float btnHeight = 20.0f;

                /// <summary>
                /// The height of the titlebar.
                /// </summary>
                public float titlebarHeight = 24.0f;
            }

            /// <summary>
            /// The various types of windows that WindowSetting
            /// is meant to represent.
            /// </summary>
            public enum WinType
            { 
                /// <summary>
                /// Undocked windows that are floating.
                /// </summary>
                Float,

                /// <summary>
                /// Docked windows.
                /// </summary>
                Docked,

                /// <summary>
                /// The window that is maximized.
                /// </summary>
                Maximized
            }

            /// <summary>
            /// The titlebar icon sprite for the close button.
            /// </summary>
            public SpriteAndColor spriteBtnClose;

            /// <summary>
            /// The titlebar icon sprite for the restore button.
            /// </summary>
            public SpriteAndColor spriteBtnRestore;

            /// <summary>
            /// The titlebar icon sprite for the maximize button.
            /// </summary>
            public SpriteAndColor spriteBtnMax;

            /// <summary>
            /// The titlebar icon sprite for dropdowns.
            /// </summary>
            public SpriteAndColor spriteBtnPull;

            /// <summary>
            /// The titlebar icon sprite for undocking.
            /// </summary>
            public SpriteAndColor spriteBtnPin;

            /// <summary>
            /// The properties for floating windows.
            /// </summary>
            public WindowSetting floatWin = new WindowSetting();

            /// <summary>
            /// The properties for docked windows.
            /// </summary>
            public WindowSetting dockWin = new WindowSetting();

            /// <summary>
            /// The properties for maximuzed windows.
            /// </summary>
            public WindowSetting maximizeWin = new WindowSetting();

            /// <summary>
            /// The width for the padding of windows. For floating windows, 
            /// this is also the 
            /// </summary>
            public float winPadding = 5.0f;

            /// <summary>
            /// The sprite for the shadow of floating windows.
            /// </summary>
            public SpriteAndColor shadow;

            /// <summary>
            /// The offset of the shadow to the window, for floating windows.
            /// </summary>
            public Vector2 shadowOffset = new Vector2(10.0f, 10.0f);

            /// <summary>
            /// The modulating color of the shadow.
            /// </summary>
            public Color shadowColor = Color.black;

            /// <summary>
            /// Sprite info for sashes.
            /// </summary>
            public SpriteAndColor sashSprite;

            /// <summary>
            /// The width of the sashes.
            /// </summary>
            public float sashWidth = 10;

            /// <summary>
            /// The font for titles.
            /// </summary>
            public Font titleFont;

            /// <summary>
            /// The font color for titles.
            /// </summary>
            public Color titleLabelColor = Color.black;

            /// <summary>
            /// the font size for titles.
            /// </summary>
            public int titleFontSize = 14;

            /// <summary>
            /// The size of the square in the center of a window
            /// for the hitbox for docking into a window.
            /// </summary>
            public float dockIntoDim = 40.0f;

            /// <summary>
            /// The width of the hitbox for docking to the side 
            /// of a window.
            /// </summary>
            public float dockSideDim = 20.0f;

            /// <summary>
            /// The color of dock previews that are shown but not
            /// being hovered over.
            /// </summary>
            public Color dockUnhover = new Color(1.0f, 0.5f, 0.25f);

            /// <summary>
            /// The color of dock previews that are shown and being
            /// hovered over.
            /// </summary>
            public Color dockHover = new Color(0.0f, 1.0f, 0.0f);

            /// <summary>
            /// The time between clicks of titlebars to count as a double click.
            /// </summary>
            public float doubleClickRate = 0.5f;

            /// <summary>
            /// Includes frame decoration.
            /// </summary>
            public Vector2 minsizeWindow = new Vector2(50.0f, 50.0f); 

            /// <summary>
            /// Includes frame decoration.
            /// </summary>
            public Vector2 minsizeTabs = new Vector2(50.0f, 50.0f);

            /// <summary>
            /// Get the window setting based off a style enum.
            /// </summary>
            /// <param name="wt">The style enum.</param>
            /// <returns>The window setting matching the style enum.</returns>
            public WindowSetting GetWindowSetting(WinType wt)
            { 
                switch(wt)
                { 
                    case WinType.Docked:
                        return this.dockWin;

                    case WinType.Float:
                        return this.floatWin;

                    case WinType.Maximized:
                        return this.maximizeWin;
                }

                return null;
            }
        }
    }
}