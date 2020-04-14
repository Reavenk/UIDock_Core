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
        public class DockSash : 
            UnityEngine.UI.Image,
            UnityEngine.EventSystems.IDragHandler
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
            /// The system.
            /// </summary>
            public Root system;
            

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

            void UnityEngine.EventSystems.IDragHandler.OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
            { 
                Vector2 delta = eventData.delta;
                if (this.grain == Grain.Horizontal)
                { 
                    if(delta.x == 0.0f)
                        return;

                    float moveAmt = 0.0f;
                    if(delta.x < 0.0f)
                    {
                        if(this.dockA.minSize.x >= this.dockA.cachedPlace.width)
                            return;

                        float endSize = this.dockA.cachedPlace.width + delta.x;
                        endSize = Mathf.Max(endSize, this.dockA.minSize.x);
                        moveAmt = Mathf.Min(0.0f, endSize - this.dockA.cachedPlace.width);
                    }
                    else
                    { 
                        if(this.dockB.minSize.x >= this.dockB.cachedPlace.width)
                            return;

                        float endSize = this.dockB.cachedPlace.width - delta.x;
                        endSize = Mathf.Max(endSize, this.dockB.minSize.x);
                        moveAmt = Mathf.Max(0.0f, this.dockB.cachedPlace.width - endSize);
                    }

                    if(moveAmt == 0.0f)
                        return;

                    this.dockA.cachedPlace.width += moveAmt;
                    this.dockB.cachedPlace.width -= moveAmt;
                    this.dockB.cachedPlace.x += moveAmt;
                    this.system.UpdateDockedBranch(this.dockA);
                    this.system.UpdateDockedBranch(this.dockB);
                    this.Align();
                }
                else if(this.grain == Grain.Vertical)
                { 
                    if(delta.y == 0.0)
                        return;
                    
                    float moveAmt = 0.0f;
                    if(delta.y > 0.0f)
                    {
                        if(this.dockA.minSize.y >= this.dockA.cachedPlace.height)
                            return;

                        float endSize = this.dockA.cachedPlace.height - delta.y;
                        endSize = Mathf.Max(endSize, this.dockA.minSize.y);
                        moveAmt = Mathf.Min(0.0f, endSize - this.dockA.cachedPlace.height);
                    }
                    else
                    { 
                        if(this.dockB.minSize.y >= this.dockB.cachedPlace.height)
                            return;

                        float endSize = this.dockB.cachedPlace.height + delta.y;
                        endSize = Mathf.Max(endSize, this.dockB.minSize.y);
                        moveAmt = Mathf.Max(0.0f, this.dockB.cachedPlace.height - endSize);
                    }

                    if(moveAmt == 0.0f)
                        return;

                    this.dockA.cachedPlace.height += moveAmt;
                    this.dockB.cachedPlace.height -= moveAmt;
                    this.dockB.cachedPlace.y += moveAmt;
                    this.system.UpdateDockedBranch(this.dockA);
                    this.system.UpdateDockedBranch(this.dockB);
                    this.Align();
                }
            }
        }

        
    }
}