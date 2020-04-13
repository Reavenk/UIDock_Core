// <copyright file="DockSash.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>04/12/2020</date>
// <summary>
// A sash to divide docked content and to give the user an interface
// to drag it.
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIDock
    {
        public class DockSash : UnityEngine.UI.Image
        {
            /// <summary>
            /// The direction the sash is dividing.
            /// </summary>
            public enum Grain
            {
                /// <summary>
                /// The content being divided from the sash is separated horizontally.
                /// </summary>
                Horizontal,

                /// <summary>
                /// The content being divided from the sash is separated vertically.
                /// </summary>
                Vertical
            }

            /// <summary>
            /// The left or top content of the sash, depending on the grain.
            /// </summary>
            public Dock dockA;

            /// <summary>
            /// The right or bottom content of the sash, depending on the grain.
            /// </summary>
            public Dock dockB;

            /// <summary>
            /// The grain of the sash.
            /// </summary>
            public Grain grain;
            

            /// <summary>
            /// Assuming that the sash children are placed correctly, place the 
            /// dock sash in the middle of them.
            /// </summary>
            public void Align()
            {
                if(this.grain == Grain.Horizontal)
                {
                    this.rectTransform.anchoredPosition =
                        new Vector2(
                            dockA.cachedPlace.xMax,
                            -dockA.cachedPlace.y);

                    this.rectTransform.sizeDelta =
                        new Vector2(
                            dockB.cachedPlace.x - dockA.cachedPlace.xMax,
                            dockA.cachedPlace.height);
                }
                else
                { 
                    this.rectTransform.anchoredPosition = 
                        new Vector2(
                            dockA.cachedPlace.x,
                            -dockA.cachedPlace.yMax);

                    this.rectTransform.sizeDelta = 
                        new Vector2(
                            dockA.cachedPlace.width,
                            dockB.cachedPlace.y - dockA.cachedPlace.yMax);
                }
            }
        }

        
    }
}