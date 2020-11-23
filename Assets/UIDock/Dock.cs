// <copyright file="Dock.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>04/12/2020</date>
// <summary>
// Holds the definition of the Dock datastructure.
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{ 
    namespace UIDock
    { 
        /// <summary>
        /// The tree datastructure used to represented the layout
        /// of Docked UIDock.Windows.
        /// </summary>
        public class Dock
        {
            /// <summary>
            /// The 
            /// </summary>
            public enum Type
            { 
                // Default value to detect unset dock types.
                Void,
                // Will always be a leaf node
                Window,
                // Will always have at least 2 children, and none 
                // of those will be another horizontal.
                Horizontal,
                // Will always have at least 2 children, and none
                // of those will be another vertical.
                Vertical,
                // Will always have at least 2 children, and they
                // will always be windows.
                Tab
            }

            /// <summary>
            /// The parent node.
            /// </summary>
            public Dock parent;

            /// <summary>
            /// The type of node the Dock is.
            /// </summary>
            public Type dockType = Type.Void;

            /// <summary>
            /// If the type is a Window, the value of the window.
            /// Else, null.
            /// </summary>
            public Window window = null;

            /// <summary>
            /// If the type is a container (Horizontal, Vertical, or
            /// Tab, the children within the node
            /// </summary>
            public List<Dock> children = null;

            /// <summary>
            /// The cached in the UI. 
            /// 
            /// Takes into account 
            /// </summary>
            public Rect cachedPlace;

            /// <summary>
            /// The minimum size the window can be, taking into
            /// account the children.
            /// </summary>
            public Vector2 minSize = Vector2.zero;

            /// <summary>
            /// Constructor for the Window type.
            /// </summary>
            /// <param name="window">The window to manage.</param>
            /// <param name="cacheDim">If true, the cached dimension
            /// is set to the window's current dimensions.</param>
            public Dock(Window window, bool cacheDim = true)
            { 
                this.window = window;
                this.dockType = Type.Window;

                if(cacheDim == true)
                    cachedPlace.size = window.Win.rect.size;
            }

            /// <summary>
            /// Constructor for container types.
            /// </summary>
            /// <param name="type">The type. This is expected to be a container
            /// type.</param>
            /// <param name="children">The children. The array is expected to have
            /// at least 2 items.</param>
            public Dock(Type type, params Dock [] children)
            { 
                this.window = null;
                this.children = new List<Dock>(children);
                this.dockType = type;
                foreach(Dock d in children)
                    d.parent = this;
            }

            public Vector2 CalculateMinsize(DockProps dp, bool cache = true)
            {
                Vector2 ret = this.CalculateMinsizeImpl(dp, cache);

                if(cache == true)
                    this.minSize = ret;

                return ret;
            }

            /// <summary>
            /// Recursively calculate the minimum size.
            /// </summary>
            /// <param name="dp">
            /// The properties with the minium size information of leaf nodes.</param>
            /// <returns>The calculated minimum size. It also takes into account
            /// the size needed for sashes.</returns>
            private Vector2 CalculateMinsizeImpl(DockProps dp, bool cache = true)
            { 
                switch( dockType)
                { 
                    case Type.Horizontal:
                        { 
                            Vector2 reth = Vector2.zero;
                            bool alo = false; // At least once
                            foreach(Dock d in this.children)
                            { 
                                Vector2 vd = d.CalculateMinsize(dp, cache);
                                reth.x += vd.x;
                                reth.y = Mathf.Max(reth.y, vd.y);

                                if(alo == false)
                                    alo = true;
                                else
                                    reth.x += dp.sashWidth;
                            }
                            return reth;
                        }
                        
                    case Type.Vertical:
                        {
                            Vector2 retv = Vector2.zero;
                            bool alo = false;
                            foreach(Dock d in this.children)
                            { 
                                Vector2 vd = d.CalculateMinsize(dp, cache);
                                retv.x = Mathf.Max(retv.x, vd.x);
                                retv.y += vd.y;

                                if(alo == false)
                                    alo = true;
                                else
                                    retv.y += dp.sashWidth;
                            }
                            return retv;
                        }

                    case Type.Tab:
                        return dp.minsizeTabs;

                    case Type.Window:
                        return dp.minsizeWindow;

                }

                return Vector2.zero;
            }

            public bool IsContainerType()
            { 
                return 
                    this.dockType == Type.Horizontal || 
                    this.dockType == Type.Vertical || 
                    this.dockType == Type.Tab;
            }
        }
    }
}