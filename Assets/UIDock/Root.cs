// <copyright file="Root.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>04/12/2020</date>
// <summary>
// The Root loccation for the UIDock system.
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIDock
    {
        public class Root : UnityEngine.UI.Graphic
        {
            /// <summary>
            /// Target references to be used with Docks to specify where
            /// a new window should be docked.
            /// </summary>
            public enum DropType
            { 
                /// <summary>
                /// Not used as an input - only as a return value.
                /// </summary>
                Invalid,

                /// <summary>
                /// Dock content into the root.
                /// </summary>
                Root,

                /// <summary>
                /// To the left of the coupled dock.
                /// </summary>
                Left,

                /// <summary>
                /// To the right of the coupled dock.
                /// </summary>
                Right,

                /// <summary>
                /// To the top of the coupled dock.
                /// </summary>
                Top,

                /// <summary>
                /// To the bottom of the coupled dock.
                /// </summary>
                Bottom,

                /// <summary>
                /// Into the same region as a the coupled dock.
                /// </summary>
                Into
            }

            /// <summary>
            /// The target for where to dock a floating window.
            /// </summary>
            public struct DragTarget
            { 
                /// <summary>
                /// An existing reference dock.
                /// </summary>
                public Dock target;

                /// <summary>
                /// The location in respect to the target dock.
                /// </summary>
                public DropType type;

                /// <summary>
                /// The region of space that was/should-be hovered
                /// 
                /// The variable is only relelvant when querying the target
                /// from a mouse position.
                /// </summary>
                public Rect region;

                /// <summary>
                /// True if the region of space was hovered over.
                /// 
                /// The variable is only relevant when querying the target
                /// from a mouse position.
                /// </summary>
                public bool ontop;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="target">The reference target.</param>
                /// <param name="type">The side of the reference.</param>
                /// <param name="region">The hit region.</param>
                /// <param name="ontop">The cursor hover value.</param>
                public DragTarget(Dock target, DropType type, Rect region, bool ontop)
                { 
                    this.target     = target;
                    this.type       = type;
                    this.region     = region;
                    this.ontop      = ontop;

                }

                /// <summary>
                /// Generate an invalid DragTarget.
                /// </summary>
                /// <returns></returns>
                public static DragTarget Invalid()
                { 
                    return new DragTarget(null, DropType.Invalid, new Rect(), false);
                }

                /// <summary>
                /// Create a clone with a specific ontop value.
                /// </summary>
                /// <param name="forcedOntop">The ontop value to override</param>
                /// <returns>A clone of the invoking object, but with a forced ontop value.</returns>
                public DragTarget OnTop(bool forcedOntop)
                { 
                    DragTarget dt = new DragTarget();
                    dt.ontop = forcedOntop;
                    dt.region = this.region;
                    dt.target = this.target;
                    dt.type = this.type;
                    return dt;
                }
            }

            /// <summary>
            /// The sharable properties defining the parameterized behaviours 
            /// and aethestics.
            /// </summary>
            public DockProps props;

            /// <summary>
            /// The root Dock. If the graph is empty, null.
            /// </summary>
            Dock root = null;

            /// <summary>
            /// All the windows managed by the system. Both docked and floating.
            /// </summary>
            Dictionary<RectTransform, Window> windowLookup = 
                new Dictionary<RectTransform, Window>();

            /// <summary>
            /// All the docked windows managed by the system.
            /// </summary>
            Dictionary<RectTransform, Dock> dockLookup = 
                new Dictionary<RectTransform, Dock>();

            /// <summary>
            /// All the floating windows managed by the system.
            /// </summary>
            List<Window> floatingWindows = new List<Window>();

            /// <summary>
            /// The starting offset of where new windows are added - before
            /// cascading is applied.
            /// </summary>
            public Vector2 winSpawnStart = Vector2.zero;

            /// <summary>
            /// The amount of offset applied per cascade value.
            /// </summary>
            public Vector2 winSpawnOffset = new Vector2(10.0f, 10.0f);

            /// <summary>
            /// The number of times new windows can cascade in their 
            /// default starting position
            /// </summary>
            public int spawnWrap = 5;

            /// <summary>
            /// The current wrappable cascade value.
            /// </summary>
            int spawnWrapIt = 0;

            /// <summary>
            /// The parent RectTransforms for floating windows of the
            /// system. If set to null, the RectTransform of the Root
            /// will be defaulted to.
            /// </summary>
            public RectTransform floatWindowHome = null;

            /// <summary>
            /// The window that's currently maximized. If a window
            /// isn't maximized, null.
            /// </summary>
            private Window maximized = null;

            /// <summary>
            /// When a window is maximized, cache the pre-maximized position, so
            /// that when it is restored, we know what to set it to. This is
            /// more relevant for floating windows than for docked windows.
            /// </summary>
            private Vector2 origMaxPos;

            /// <summary>
            /// When a window is maximized, cache the pre-maximized size, so
            /// that when it is restored, we know what to set it to. This is
            /// more relevant for floating windows than for docked windows.
            /// </summary>
            private Vector2 origMaxSize;

            /// <summary>
            /// Dirty flag when the sash needs construction.
            /// </summary>
            bool dirtySashConstruction = false;

            /// <summary>
            /// Dirty flag when the sash needs to be repositions. This flag isn't
            /// relevant if dirtySashConstruction is true because reconstruction of
            /// sashes will place them in the correct locations.
            /// </summary>
            bool dirtySashPosition = false;

            /// <summary>
            /// The coroutine handling the dirty state.dirtSashConstruction and 
            /// dirtySashPosition will be resolved when the coroutine is finished. The
            /// coroutine is also in charge of nulling the dirtyHandle when the 
            /// processing is finished.
            /// </summary>
            Coroutine dirtyHandle = null;

            /// <summary>
            /// The window currently being dragged.
            /// </summary>
            Window windowDragged = null;

            /// <summary>
            /// The image used to show the preview for drag-and-drop docking of windows.
            /// </summary>
            UnityEngine.UI.Image dropVisual = null;

            /// <summary>
            /// The managed sashes.
            /// </summary>
            List<DockSash> sashes = new List<DockSash>();

            /// <summary>
            /// If true, windows can be docked on top of each other to make notebooked
            /// tabbed layouts. Else if false, notebook tab docking is not allowed.
            /// </summary>
            public bool allowTabbedDocking = true;

            /// <summary>
            /// The windowing assets for tabbed contents.
            /// </summary>
            Dictionary<Dock, DockedTab> tabAssets = new Dictionary<Dock, DockedTab>();

            /// <summary>
            /// Is there a maximized window.
            /// </summary>
            /// <returns>True if there is a maximized window; else false.</returns>
            public bool IsMaximized()
            { 
                return this.maximized != null;
            }

            /// <summary>
            /// Checks if a specific window is maximized.
            /// </summary>
            /// <param name="window">The window to check the maximized state of.</param>
            /// <returns>True if the specified window is maximized; else false.</returns>
            public bool IsMaximized(Window window)
            { 
                return this.maximized == window;
            }

            /// <summary>
            /// Create a Window that contains a specified RectTransform.
            /// </summary>
            /// <param name="rt">The RectTransform to contain.</param>
            /// <param name="title">The titlebar test.</param>
            /// <param name="flags">The style flags.</param>
            /// <returns></returns>
            public Window WrapIntoWindow(RectTransform rt, string title, Window.Flag flags = Window.DefaultFlags)
            { 
                if(this.windowLookup.ContainsKey(rt) == true)
                    return null;

                GameObject goWindow = new GameObject("DockWindow");
                Window window = goWindow.AddComponent<Window>();
                window.Initialize(this, rt, title, flags);
                window.NotifyFloating();

                window.rectTransform.anchoredPosition = 
                    this.GetNewSpawnSpot();

                window.UpdateShadow();

                this.floatingWindows.Add(window);
                this.windowLookup.Add(rt, window);

                return window;
            }

            /// <summary>
            /// Returns the value for the parent of floating windows. The function
            /// handles defaulting to the Root's RectTransform is null.
            /// </summary>
            /// <returns>
            /// The RectTransform where floating windows should be parented to.
            /// </returns>
            RectTransform FloatingWindowContainer()
            { 
                if(this.floatWindowHome == null)
                    return this.rectTransform;

                return this.floatWindowHome;
            }

            /// <summary>
            /// Dock a window managed by the system.
            /// </summary>
            /// <param name="win">The floating window to dock.</param>
            /// <param name="dst">The reference Dock of where to dock the window.</param>
            /// <param name="dt">The relative position to the reference of where to dock to.</param>
            /// <returns>True if the docking operation was successful; else false.</returns>
            public bool DockWindow(Window win, Dock dst, DropType dt)
            { 
                Dock d = DockWindowImpl(win, dst, dt);
                if(d == null)
                    return false;

                // If it's in a tab, it need to remain borderless.
                if(d.parent == null || d.parent.dockType != Dock.Type.Tab)
                    d.window.NotifyDocked();

                this.dockLookup.Add(win.Win, d);
                this.floatingWindows.Remove(win);
                win.DisableShadow();
                this.SetDirtySashReconstr();
                return true;
            }

            /// <summary>
            /// A part of the implementation of DockWindow() nested into its own function for
            /// organizational reasons.
            /// </summary>
            /// <param name="win">The window to dock.</param>
            /// <param name="dst">The reference Dock of where to dock the window; or null, for referencing the root.</param>
            /// <param name="dt">The relative position to the reference of where to dock to.</param>
            /// <returns>The Dock created holding the win parameter. Or null if the operation fails.</returns>
            private Dock DockWindowImpl(Window win, Dock dst, DropType dt)
            { 
                if(dt == DropType.Invalid)
                    return null;

                if (this.root == null)
                { 
                    Dock newRoot = new Dock(win);
                    this.root = newRoot;
                    this.SetDirty();
                    return newRoot;
                }

                if(dst == null)
                    dst = this.root;

                Dock parent = null;
                if(dst != null)
                    parent = dst.parent;

                if(dt == DropType.Into)
                { 
                    if(this.allowTabbedDocking == false)
                    {
                        Debug.LogError("Attempting to dock tabbed notebooks on a layout system with tabs disabled.");
                        return null;
                    }

                    // Edge case for docking to the root.
                    if (dst.parent == null && this.root == dst)
                    {
                        if(dst.dockType != Dock.Type.Window && dst.dockType != Dock.Type.Tab)
                        {
                            Debug.LogError("Attempting to create a docked tab into the root when not allowed.");
                            return null;
                        }

                    }
                    else if(dst.dockType == Dock.Type.Tab)
                    { } // Do nothing - eat up if-else condition
                    else 
                    {
                        // If we're trying to dock on to a window, while note exactly allowed, it could
                        // be that its parent is a notebook tabbed collection that we want to drag on to.
                        // We only do this once because notebook tabs can only contain windows so the
                        // max depth should be 1.
                        if (dst.dockType == Dock.Type.Window && dst.parent.dockType == Dock.Type.Tab)
                            dst = dst.parent;

                        if (dst.dockType == Dock.Type.Horizontal || dst.dockType == Dock.Type.Vertical)
                        {
                            Debug.LogError("Attempting to create a docked tab into a destination that doesn't allow tabs to exist.");
                            return null;
                        }
                    }


                    // With the error checking at the top, 1 of two valid conditions now exist, we're dropping
                    // the window onto another window to CREATE a tab notebook system,
                    // ... or ...
                    // we're APPENDING to an existing tab system.
                    Dock newDock = new Dock(win, false);

                    if (dst.dockType == Dock.Type.Window)
                    { 
                        Dock oldParent = dst.parent;
                        Dock newTabDock = new Dock(Dock.Type.Tab, new Dock[]{newDock, dst });
                        newTabDock.parent = oldParent;

                        if (oldParent == null)
                            this.root = newTabDock;
                        else
                        { 
                            int idx = oldParent.children.IndexOf(dst);
                            oldParent.children[idx] = newTabDock;
                        }

                        dst.window.ChangeStyle(DockProps.WinType.Borderless);
                    }
                    else // if(dst.dockType == Dock.Type.Tab)
                    { 
                        dst.children.Add(newDock);
                        newDock.parent = dst;
                    }
                    // Layout everything to force creating/deleting tab assets and
                    // to re-align them.
                    newDock.parent.activeTab = newDock;
                    win.ChangeStyle(DockProps.WinType.Borderless);
                    this.SetDirty();
                    return newDock;
                }

                if (dst.dockType == Dock.Type.Window || dst.dockType == Dock.Type.Tab)
                {   // The top/bottom/left/right handlers

                    // if our destination is a child of a tab, we're more interested
                    // in the operation being relative to the tab system (or else it
                    // would be an illegal layout operation).
                    if(dst.parent != null && dst.parent.dockType == Dock.Type.Tab)
                    {
                        dst = dst.parent;
                        parent = dst.parent;
                    }

                    if (dt == DropType.Top)
                    {
                        if (dst == this.root)
                        {
                            Dock newDock = new Dock(win, false);
                            Dock branch = new Dock(Dock.Type.Vertical, newDock, dst);
                            dst.cachedPlace = new Rect();
                            this.root = branch;
                            return newDock;
                        }
                        else if (dst.parent.dockType == Dock.Type.Vertical)
                        {
                            int idx = dst.parent.children.IndexOf(dst);
                            
                            Dock newDock = new Dock(win);
                            parent.children.Insert(idx, newDock);
                            newDock.parent = dst.parent;
                            return newDock;
                        }
                        else if (dst.parent.dockType == Dock.Type.Horizontal)
                        {
                            Dock newDock = new Dock(win);
                            
                            int idx = parent.children.IndexOf(dst);
                            Dock branch = new Dock(Dock.Type.Vertical, newDock, dst);
                            branch.cachedPlace = dst.cachedPlace;
                            dst.cachedPlace = new Rect();
                            dst.parent = branch;
                            parent.children[idx] = branch;
                            branch.parent = parent;
                            return newDock;
                        }
                    }
                    else if(dt == DropType.Bottom)
                    { 
                        if(dst == this.root)
                        { 
                            Dock newDock = new Dock(win, false);
                            Dock branch = new Dock(Dock.Type.Vertical, dst, newDock);
                            dst.cachedPlace = new Rect();
                            this.root = branch;
                            return newDock;
                        }
                        else if(dst.parent.dockType == Dock.Type.Vertical)
                        { 
                            int idx = dst.parent.children.IndexOf(dst);

                            Dock newDock = new Dock(win);
                            parent.children.Insert(idx + 1, newDock);
                            newDock.parent = dst.parent;
                            return newDock;
                        }
                        else if(dst.parent.dockType == Dock.Type.Horizontal)
                        { 
                            Dock newDock = new Dock(win);

                            int idx = dst.parent.children.IndexOf(dst);
                            Dock branch = new Dock(Dock.Type.Vertical, dst, newDock);
                            branch.cachedPlace = dst.cachedPlace;
                            dst.cachedPlace = new Rect();
                            dst.parent = branch;
                            parent.children[idx] = branch;
                            branch.parent = parent;
                            return newDock;
                        }
                    }
                    else if(dt == DropType.Left)
                    {
                        if (dst == this.root)
                        {
                            Dock newDock = new Dock(win, false);
                            Dock branch = new Dock(Dock.Type.Horizontal, newDock, dst);
                            dst.cachedPlace = new Rect();
                            this.root = branch;
                            return newDock;
                        }
                        else if(dst.parent.dockType == Dock.Type.Vertical)
                        { 
                            Dock newDock = new Dock(win);

                            int idx = dst.parent.children.IndexOf(dst);
                            Dock branch = new Dock(Dock.Type.Horizontal, newDock, dst);
                            branch.cachedPlace = dst.cachedPlace;
                            dst.cachedPlace = new Rect();
                            parent.children[idx] = branch;
                            branch.parent = parent;
                            return newDock;
                        }
                        else if(dst.parent.dockType == Dock.Type.Horizontal)
                        { 
                            int idx = dst.parent.children.IndexOf(dst);
                            Dock newDock = new Dock(win);
                            //
                            parent.children.Insert(idx, newDock);
                            newDock.parent = dst.parent;
                            return newDock;
                        }
                    }
                    else if(dt == DropType.Right)
                    {
                        if (dst == this.root)
                        {
                            Dock newDock = new Dock(win, false);
                            Dock branch = new Dock(Dock.Type.Horizontal, dst, newDock);
                            dst.cachedPlace = new Rect();
                            this.root = branch;
                            return newDock;
                        }
                        else if (dst.parent.dockType == Dock.Type.Vertical)
                        {
                            Dock newDock = new Dock(win);

                            int idx = dst.parent.children.IndexOf(dst);
                            Dock branch = new Dock(Dock.Type.Horizontal, dst, newDock);
                            branch.cachedPlace = dst.cachedPlace;
                            dst.cachedPlace = new Rect();
                            parent.children[idx] = branch;
                            branch.parent = parent;
                            return newDock;
                        }
                        else if (dst.parent.dockType == Dock.Type.Horizontal)
                        {
                            int idx = dst.parent.children.IndexOf(dst);
                            Dock newDock = new Dock(win);
                            //
                            parent.children.Insert(idx + 1, newDock);
                            newDock.parent = dst.parent;
                            return newDock;
                        }
                    }
                }

                // If we're docking to a sizer, we do similar logic as window/tab docking
                // for the top/bottom/left/right above. So there's probably an elegant way
                // to unify a lot of logic between top/left/right/bottom destinations and 
                // window/tab/vertical/horizontal docks, but for now we're just getting the
                // basics to work.

                if (dst.dockType == Dock.Type.Vertical)
                {
                    if(dt == DropType.Left)
                    {
                        if (this.root == dst)
                        {
                            Dock newDock = new Dock(win, false);
                            Dock newVert = new Dock(Dock.Type.Horizontal, newDock, dst);
                            this.root = newVert;
                            return newDock;
                        }
                        else if(dst.parent.dockType == Dock.Type.Horizontal)
                        {
                            Dock newDock = new Dock(win, false);
                            newDock.parent = dst.parent;
                            int dstIdx = dst.parent.children.IndexOf(dst);
                            dst.parent.children.Insert(dstIdx, newDock);
                            return newDock;
                        }
                    }
                    else if(dt == DropType.Right)
                    {
                        if (this.root == dst)
                        {
                            Dock newDock = new Dock(win, false);
                            Dock newVert = new Dock(Dock.Type.Horizontal, dst, newDock);
                            this.root = newVert;
                            return newDock;
                        }
                        else if (dst.parent.dockType == Dock.Type.Horizontal)
                        {
                            Dock newDock = new Dock(win, false);
                            newDock.parent = dst.parent;
                            int dstIdx = dst.parent.children.IndexOf(dst);
                            dst.parent.children.Insert(dstIdx + 1, newDock);
                            return newDock;
                        }
                    }
                    else if(dt == DropType.Top)
                    {
                        Dock newDock = new Dock(win, false);
                        dst.children.Insert(0, newDock);
                        newDock.parent = dst;
                        return newDock;
                    }
                    else if(dt == DropType.Bottom)
                    { 
                        Dock newDock = new Dock(win, false);
                        dst.children.Add(newDock);
                        newDock.parent = dst;
                        return newDock;
                    }
                    
                    Debug.LogError("Attempting illegal docking operation onto vertical container.");
                    return null;
                }
                if(dst.dockType == Dock.Type.Horizontal)
                {
                    if (dt == DropType.Left)
                    {
                        Dock newDock = new Dock(win, false);
                        newDock.parent = dst;
                        dst.children.Insert(0, newDock);
                        return newDock;
                    }
                    else if (dt == DropType.Right)
                    {
                        Dock newDock = new Dock(win, false);
                        newDock.parent = dst;
                        dst.children.Add(newDock);
                        return newDock;
                    }
                    else if (dt == DropType.Top)
                    {
                        if(this.root == dst)
                        {
                            Dock newDock = new Dock(win, false);
                            Dock newVert = new Dock(Dock.Type.Vertical, newDock, dst);
                            this.root = newVert;
                            return newDock;
                        }
                        else if(dst.parent.dockType == Dock.Type.Vertical)
                        {
                            Dock newDock = new Dock(win, false);
                            newDock.parent = dst.parent;
                            int dstIdx = dst.parent.children.IndexOf(dst);
                            dst.parent.children.Insert(dstIdx, newDock);
                            return newDock;
                        }
                    }
                    else if (dt == DropType.Bottom)
                    {
                        if (this.root == dst)
                        {
                            Dock newDock = new Dock(win, false);
                            Dock newVert = new Dock(Dock.Type.Vertical, dst, newDock);
                            this.root = newVert;
                            return newDock;
                        }
                        else if (dst.parent.dockType == Dock.Type.Vertical)
                        {
                            Dock newDock = new Dock(win, false);
                            newDock.parent = dst.parent;
                            int dstIdx = dst.parent.children.IndexOf(dst);
                            dst.parent.children.Insert(dstIdx + 1, newDock);
                        }
                    }
                    
                    Debug.LogError("Attempting illegal docking operation into horizontal container.");
                    return null;
                }
                return null;
            }

            /// <summary>
            /// Undock a window managed by the system. The window will be converted to a floating
            /// window.
            /// </summary>
            /// <param name="win">The system to undock.</param>
            /// <returns>True if the window was undocked successfully; else false.</returns>
            /// <remarks>While undocking will remove it from undocked datastructures, the Window will
            /// still be tracked by the system as a floating Window.</remarks>
            public bool UndockWindow(Window win)
            { 
                Dock dock;
                if(this.dockLookup.TryGetValue(win.Win, out dock) == false)
                    return false;

                Dock parent = dock.parent;
                if(dock == this.root)
                { 
                    this.root = null;
                }
                else if(parent.IsContainerType() == true)
                {
                    parent.children.Remove(dock);
                    bool isTab = parent.dockType == Dock.Type.Tab;
                    if(parent.children.Count == 1)
                    {
                        ManageCollapse(parent);

                        if (isTab == true)
                        {
                            // If we collapse tabs by removing enough children, it no longer exists, so
                            // its assets should be destroyed.
                            DockedTab dtab;
                            if (this.tabAssets.TryGetValue(parent, out dtab) == true)
                            {
                                dtab.Destroy();
                                this.tabAssets.Remove(parent);
                            }
                        }
                    }
                    else
                    { 
                        if(isTab == true && dock == parent.activeTab)
                        { 
                            // TODO: Make this more intelligent, maybe set it to the tab before
                            // the one we closed.
                            parent.activeTab = parent.children[0];
                        }
                    }
                }
                else
                    return false;

                this.dockLookup.Remove(win.Win);

                if(this.maximized == win)
                    win.NotifyMaximized();
                else
                    win.NotifyFloating();

                this.floatingWindows.Add(win);
                win.rectTransform.SetAsLastSibling();
                win.gameObject.SetActive(true);
                win.EnableShadow();
                win.UpdateShadow();

                this.SetDirtySashReconstr();

                return true;
            }

            /// <summary>
            /// Handle collapsing a parent node because it has all its children removed until
            /// it only had 1 remaining child.
            /// </summary>
            /// <param name="colParent">
            /// The parent that's being collapse. It should be a container dock with only 1 child.
            /// </param>
            void ManageCollapse(Dock colParent)
            {
                if(colParent.dockType == Dock.Type.Window)
                    return; // Illegal

                if(colParent.children.Count != 1)
                    return;

                Dock single = null;
                if (colParent == this.root)
                {
                    // If the deletion leave us with a root container
                    // with only 1 child, that 1 child becomes the new root.
                    this.root = colParent.children[0];
                    this.root.parent = null;
                    return;
                }
                else
                {
                    // If the removal leaves a parent with only 1 item, then
                    // we cascade the deletion by also deleting the parent and 
                    // leaving the single child in its place.
                    int idx = colParent.parent.children.IndexOf(colParent);
                    single = colParent.children[0];
                    colParent.parent.children[idx] = single;
                    single.parent = colParent.parent;
                }

                if (single != null && single.dockType == Dock.Type.Window)
                {
                    // This is most important for tabs. If we close the 2nd to last tab and it
                    // was visible, we need the singled item to be visible, and we need to 
                    //restore the style.
                    //
                    // We could if statement this for just Tab parented things,
                    // but for now it's everything indiscriminantly.
                    single.window.ChangeStyle(DockProps.WinType.Docked, true);
                    single.window.gameObject.SetActive(true);
                }

                // The parent should no longer exist, but this means we put single
                // into another container that we didn't add with the same protections
                // with normal docking. 
                //
                // So we need to check if additional collapsing can occur from edge
                // cases.

                if (
                    single.IsContainerType() &&
                    single.parent != null &&
                    single.dockType == colParent.parent.dockType)
                { 
                    // At this point, colParent's parent can either be a horizontal or vertical
                    // grained container. If it maches the same grain as the parent, we need to
                    // make the equivalent layout, but by collapsing the contents of single into
                    // its parent.

                    int idx = single.parent.children.IndexOf(single);
                    single.parent.children.RemoveAt(idx);
                    single.parent.children.InsertRange(idx, single.children);
                    foreach(Dock d in single.parent.children)
                        d.parent = single.parent;

                }


            }

            /// <summary>
            /// Request a new cadcading spawn position, and increment
            /// the cascading location variable.
            /// </summary>
            /// <returns>The location to move a newly added and floating
            /// Window to.</returns>
            public Vector2 GetNewSpawnSpot()
            { 
                Vector2 ret = 
                    this.winSpawnStart + winSpawnOffset * this.spawnWrapIt;

                this.spawnWrapIt = 
                    (this.spawnWrapIt + 1) % this.spawnWrap;

                return ret;
            }

            /// <summary>
            /// Get an iterator through all the docked windows.
            /// </summary>
            /// <returns>Iterator of docked windows.</returns>
            public IEnumerable<Window> DockedWindows()
            { 
                foreach(Dock d in this.dockLookup.Values)
                { 
                    if(d.dockType == Dock.Type.Window)
                        yield return d.window;
                }
            }

            /// <summary>
            /// Get an iterator through all the floating windows.
            /// </summary>
            /// <returns>Iterator of the floating windows.</returns>
            public IEnumerable<Window> FloatingWindows()
            { 
                return this.floatingWindows;
            }

            /// <summary>
            /// Maximizes a window.
            /// </summary>
            /// <param name="window">The window to maximize.</param>
            /// <returns>True if the window was successfully maximized, else false.</returns>
            public bool MaximizeWindow(Window window)
            { 
                if(window == null)
                    return false;

                if(this.maximized == window)
                    return true;

                if(this.maximized != null)
                    this.RestoreWindow();

                this.maximized = window;
                foreach (KeyValuePair<RectTransform, Window> kvp in this.windowLookup)
                {
                    Window winIt = kvp.Value;
                    winIt.DisableShadow();

                    if (winIt != this.maximized)
                        winIt.gameObject.SetActive(false);
                    
                }

                foreach(DockSash ds in this.sashes)
                    ds.gameObject.SetActive(false);


                RectTransform retMax = window.rectTransform;
                this.maximized      = window;
                this.origMaxPos     = retMax.anchoredPosition;
                this.origMaxSize    = retMax.sizeDelta;

                if(retMax.parent != this)
                { 
                    retMax.SetParent(this.rectTransform);
                    Window.PrepareChild(retMax);
                }

                retMax.anchorMin    = Vector2.zero;
                retMax.anchorMax    = Vector2.one;
                retMax.offsetMin    = Vector2.zero;
                retMax.offsetMax    = Vector2.zero;

                window.NotifyMaximized();

                return true;
            }

            /// <summary>
            /// Restore a window.
            /// </summary>
            /// <param name="window">
            /// The window requesting to be restored. While only window will ever be
            /// maximized, the requesting window will pass itself as a parameter as a 
            /// safegaurd that the operation is in the correct state that the invoking
            /// windows beleives the Root is in.</param>
            /// <returns>True if the restore operation happened successfuly.</returns>
            public bool RestoreWindow(Window window)
            { 
                if(this.maximized != window)
                    return false;

                this.maximized = null;

                // If before it was maximized, it was a dock
                if(this.dockLookup.ContainsKey(window.Win) == true)
                {
                    window.transform.SetParent(this.rectTransform);
                    Window.PrepareChild(window.rectTransform);

                    window.NotifyDocked();
                }
                else
                { 
                    // Or else it was floating
                    window.transform.SetParent(this.FloatingWindowContainer());
                    Window.PrepareChild(window.rectTransform);

                    window.rectTransform.anchoredPosition = origMaxPos;
                    window.rectTransform.sizeDelta = origMaxSize;

                    window.NotifyFloating();

                }

                foreach(KeyValuePair<RectTransform, Dock> kvp in this.dockLookup)
                { 
                    Dock d = kvp.Value;
                    d.window.gameObject.SetActive(true);
                    d.window.rectTransform.SetAsFirstSibling();
                }
                
                foreach(Window w in this.floatingWindows)
                { 
                    w.gameObject.SetActive(true);
                    w.EnableShadow();
                    w.rectTransform.SetAsLastSibling();
                    w.UpdateShadow();
                }
                this.SetDirtySashReconstr();
                return true;
            }

            /// <summary>
            /// Restore the maximized window.
            /// </summary>
            /// <returns>If true, the maximized window was restored. If false,
            /// the operation failed, most likely because there wasn't a window
            /// maximized.</returns>
            public bool RestoreWindow()
            { 
                if(this.maximized == null)
                    return false;

                return this.RestoreWindow(this.maximized);
            }

            /// <summary>
            /// Close a window.
            /// </summary>
            /// <param name="window">The window to close.</param>
            /// <returns>If true, the window was successfully closed. If false,
            /// there was a state error or invalid window paramter.</returns>
            /// <remarks>Windows will delegate their closing to this function 
            /// because the Root has extra state information is needs to 
            /// maintain.</remarks>
            public bool CloseWindow(Window window)
            {
                if(this.windowLookup.Remove(window.Win) == false)
                    return false;

                if(this.windowDragged == window)
                    this.windowDragged = null;

                if (this.maximized == window)
                    this.RestoreWindow(window);

                this.UndockWindow(window);
                this.floatingWindows.Remove(window);

                this.windowLookup.Remove(window.Win);
                GameObject.Destroy(window.shadow.gameObject);
                GameObject.Destroy(window.gameObject);

                this.SetDirty();
                return true;
            }

            /// <summary>
            /// Update docked content in a window.
            /// 
            /// Also handles clearing the dirty flag and substates.
            /// </summary>
            /// <param name="moveSashes">If true, moves the sashes to the correct place.</param>
            /// <param name="regenSashes">If true, destroys the current sashes and regenerates new ones.</param>
            private void UpdateWindowsManagement(bool moveSashes, bool regenSashes)
            { 
                if(this.maximized != null)
                { 
                    foreach(KeyValuePair<RectTransform, Window> kvp in this.windowLookup)
                    { 
                        if(kvp.Key != this.maximized)
                            kvp.Key.gameObject.SetActive(false);

                        kvp.Value.DisableShadow();
                    }
                    this.maximized.gameObject.SetActive(true);

                    return;
                }

                this.UpdateDockedLayout();

                if(regenSashes == true)
                { 
                    this.RegenerateSashes();

                    this.dirtySashConstruction = false;
                    this.dirtySashPosition = false;
                }
                else if(moveSashes == true)
                { 
                    this.RealignSashes();

                    this.dirtySashPosition = false;
                }


            }

            /// <summary>
            /// A utility struct used to represent a Dock and where
            /// it has been calculated to reside inside an alloted space.
            /// </summary>
            public struct LayoutEntry
            { 
                /// <summary>
                /// The Dock being referenced.
                /// </summary>
                public Dock dock;

                /// <summary>
                /// The position where the dock should be placed. It uses
                /// a convention where the origin is at the top left.
                /// </summary>
                public Rect rect;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="dock">The Dock to set to.</param>
                /// <param name="rect">The Rect to set to.</param>
                public LayoutEntry(Dock dock, Rect rect)
                { 
                    this.dock = dock;
                    this.rect = rect;
                }
            }

            /// <summary>
            /// Update the layout of docked content.
            /// </summary>
            /// <remarks>While this function is called to resolve the dirty
            /// flag, it does not clear it.</remarks>
            private void UpdateDockedLayout()
            { 
                Vector2 sz = this.rectTransform.rect.size;

                // If we have a correct state and this.dockLookup is not empty,
                // this.root should be non-null.
                if(this.dockLookup.Count == 0)
                    return;

                this.root.CalculateMinsize(this.props);

                LayoutEntry rootLE = new LayoutEntry(this.root, new Rect(0.0f, 0.0f, sz.x, sz.y));
                this.UpdateDockedLayoutHeirarchy(rootLE);
            }

            /// <summary>
            /// Recursively update a branch of docks, based on the specified
            /// Dock's current cached placement info.
            /// </summary>
            /// <param name="d">The Dock to update the hierarchy of.</param>
            public void UpdateDockedBranch(Dock d)
            { 
                LayoutEntry branchLE = new LayoutEntry(d, d.cachedPlace);
                this.UpdateDockedLayoutHeirarchy(branchLE);
            }

            /// <summary>
            /// Give a LayoutEntry, recursively update the hierarchy.
            /// </summary>
            /// <param name="leBranch">
            /// The specified Dock to update the heirarchy of, and the location
            /// it should take up.</param>
            private void UpdateDockedLayoutHeirarchy(LayoutEntry leBranch)
            {
                Queue< LayoutEntry> q = new Queue<LayoutEntry>();
                q.Enqueue(leBranch);

                HashSet<Dock> processedTabs = new HashSet<Dock>();

                while (q.Count != 0)
                { 
                    LayoutEntry le = q.Dequeue();

                    le.dock.cachedPlace = le.rect;

                    if(le.dock.dockType == Dock.Type.Window)
                    {
                        le.dock.window.rectTransform.anchoredPosition = new Vector2(le.rect.x, -le.rect.y);
                        le.dock.window.rectTransform.sizeDelta = le.rect.size;
                        le.dock.window.rectTransform.SetAsFirstSibling();

                        le.dock.window.DisableShadow();
                    }
                    else if(le.dock.dockType == Dock.Type.Tab)
                    {
                        processedTabs.Add(le.dock);

                        if(le.dock.activeTab == null)
                            le.dock.activeTab = le.dock.children[0];

                        DockedTab dtabs;
                        if (this.tabAssets.TryGetValue(le.dock, out dtabs) == false)
                        {
                            dtabs = new DockedTab(le.dock, this);
                            dtabs.CreateAssets();
                            this.tabAssets.Add(le.dock, dtabs);
                        }

                        dtabs.HandleDock();

                    }
                    else if(le.dock.dockType == Dock.Type.Horizontal)
                    { 
                        float allowableSpace = (le.rect.width - this.props.sashWidth * (le.dock.children.Count - 1));
                        float avgAllowable = allowableSpace / (float)le.dock.children.Count;

                        float total = 0.0f;
                        // Get total used size
                        foreach(Dock d in le.dock.children)
                        { 
                            if(d.cachedPlace.width <= 0.0f)
                                d.cachedPlace.width = avgAllowable;

                            total += d.cachedPlace.width;
                        }

                        float x = le.rect.x;
                        foreach(Dock d in le.dock.children)
                        { 
                            float allocWidth = d.cachedPlace.width / total * allowableSpace;
                            q.Enqueue( 
                                new LayoutEntry(
                                    d, 
                                    new Rect(
                                        x, 
                                        le.rect.y,
                                        allocWidth,
                                        le.rect.height)));

                            x += allocWidth;
                            x += this.props.sashWidth;
                        }
                        
                    }
                    else if(le.dock.dockType == Dock.Type.Vertical)
                    { 
                        float allowableSpace = (le.rect.height - this.props.sashWidth * (le.dock.children.Count - 1));
                        float avgAllowable = allowableSpace / (float)le.dock.children.Count;

                        float total = 0.0f;
                        foreach(Dock d in le.dock.children)
                        { 
                            if(d.cachedPlace.height <= 0.0f)
                                d.cachedPlace.height = avgAllowable;

                            total += d.cachedPlace.height;
                        }

                        float y = le.rect.y;
                        foreach(Dock d in le.dock.children)
                        { 
                            float allocHeight = d.cachedPlace.height / total * allowableSpace;
                            q.Enqueue(
                                new LayoutEntry(
                                    d, 
                                    new Rect(
                                        le.rect.x,
                                        y, 
                                        le.rect.width,
                                        allocHeight)));

                            y += allocHeight;
                            y += this.props.sashWidth;
                        }
                    }
                }
            }

            /// <summary>
            /// Given a cursor position with the convention of the origin at the top left,
            /// check if the mouse is in a location that assets a docking operation should
            /// happen.
            /// </summary>
            /// <param name="v2">The cursor position.</param>
            /// <returns>
            /// The results of if a docking operation should occur, and if so, where.
            /// </returns>
            public DragTarget QueryDropTarget(Vector2 v2)
            {
                // Don't attempt any docking while maximized
                if(this.maximized != null)
                    return new DragTarget(null, DropType.Invalid, new Rect(), false);

                float dropDiam = this.props.dockIntoDim;
                float dropRadi = dropDiam * 0.5f;

                if (this.root == null)
                {
                    Vector2 cen = this.rectTransform.rect.size * 0.5f;
                    Rect r =
                        new Rect(
                            cen.x - dropRadi,
                            cen.y - dropRadi,
                            dropDiam,
                            dropDiam);

                    return new DragTarget(null, DropType.Root, r, r.Contains(v2));
                }

                Dock d = this.root;
                while(true)
                { 
                    if(d.cachedPlace.Contains(v2) == true)
                    { 
                        if(d.dockType == Dock.Type.Window || d.dockType == Dock.Type.Tab)
                        {   // We finally found a window region the mouse fits in

                            
                            List<DragTarget> lefts      = GetItemsInDirection(d, DropType.Left, this.props.dockSideDim);
                            List<DragTarget> rights     = GetItemsInDirection(d, DropType.Right, this.props.dockSideDim);
                            List<DragTarget> tops       = GetItemsInDirection(d, DropType.Top, this.props.dockSideDim);
                            List<DragTarget> bottoms    = GetItemsInDirection(d, DropType.Bottom, this.props.dockSideDim);

                            // Check if the cursor is in the center, but the center is going to be relative go these GetItems* results.
                            Vector2 cen = 
                                new Vector2(
                                    (lefts[lefts.Count - 1].region.xMax + rights[rights.Count - 1].region.x) * 0.5f,
                                    (tops[tops.Count - 1].region.yMax + bottoms[bottoms.Count - 1].region.y) * 0.5f);

                            Rect rCen = new Rect(cen.x - dropRadi, cen.y - dropRadi, dropDiam, dropDiam);

                            if(rCen.Contains(v2) == true)
                                return new DragTarget(d, DropType.Into, rCen, true);

                            // Find what side we're furthest past the boundary of
                            List < DragTarget > sideToCheck = lefts;
                            float checkSideDst = lefts[lefts.Count - 1].region.xMax - v2.x;
                            int axisCheck = 0; //x
                            //
                            float dst = v2.x - rights[rights.Count - 1].region.x;
                            if(dst > checkSideDst)
                            {
                                checkSideDst = dst;
                                sideToCheck = rights;
                            }
                            //
                            dst = tops[tops.Count - 1].region.y - v2.y;
                            if(dst > checkSideDst)
                            { 
                                checkSideDst = dst;
                                sideToCheck = tops;
                                axisCheck = 1; // set to y
                            }
                            //
                            dst = v2.y - bottoms[bottoms.Count - 1].region.yMax;
                            if(dst > checkSideDst)
                            { 
                                checkSideDst = dst;
                                sideToCheck = bottoms;
                                axisCheck = 1; // set to y
                            }

                            foreach(DragTarget dt in sideToCheck)
                            { 
                                if(dt.region.Contains(v2) == true)
                                    return dt;
                            }

                            // If it's not at the center or and edge, at least draw a preview. But we need to figure
                            // which preview to draw.
                            //
                            // We're just going to use the center, or else if we want he closest point on the rectangle,
                            // we need to know what kind of side it is which is even more branching.
                            float curToRgn = Mathf.Abs(v2[axisCheck] - sideToCheck[sideToCheck.Count - 1].region.center[axisCheck]);
                            float curToCen = Mathf.Abs(v2[axisCheck] - cen[axisCheck]);
                            //
                            if(curToRgn < curToCen)
                                return sideToCheck[sideToCheck.Count - 1].OnTop(false);
                            else
                                return new DragTarget(d, DropType.Into, rCen, false);
                        }
                        else if(d.dockType != Dock.Type.Tab && d.children != null)
                        { 
                            bool found = false;
                            foreach(Dock dc in d.children)
                            { 
                                if(dc.cachedPlace.Contains(v2) == true)
                                {
                                    d = dc;
                                    found = true;
                                    break;
                                }
                            }
                            if(found == false)
                                return DragTarget.Invalid();
                        }
                    }
                    else
                        break;
                }

                return DragTarget.Invalid();
            }

            /// <summary>
            /// Find all the possible drop targets from a node's direction.
            /// </summary>
            /// <param name="starting">A window or tab node to start scanning from.</param>
            /// <param name="dir">The direction to process. Only Top/Bottom/Left/Right are supported.</param>
            /// <param name="diam"></param>
            /// <returns>The drop targets for a given diretion.</returns>
            List<DragTarget> GetItemsInDirection(Dock starting, DropType dir, float side)
            { 
                // This feels a little unelegant, like there's some kind of pattern that could 
                // be leveraged - but instead we're just coding unrolled permutations.

                List<DragTarget> lst = new List<DragTarget>();

                lst.Add(new DragTarget(starting, dir, new Rect(), true));
                Dock it = starting;

                if (dir == DropType.Left)
                { 
                    while (it != null && it.parent != null)
                    {
                        if (it.parent.dockType == Dock.Type.Horizontal)
                        {
                            if (it.parent.children.IndexOf(it) != 0)
                                break;

                            it = it.parent;
                        }
                        else if (it.parent.dockType == Dock.Type.Vertical)
                        {
                            lst.Add(new DragTarget(it.parent, dir, new Rect(), true));
                            it = it.parent;
                        }
                        else
                            break; // saftey
                    }
                    // We want the list going outer-to-inner, but we traversed from inner to outer.
                    lst.Reverse();

                    float fx = starting.cachedPlace.x;
                    for(int i = 0; i < lst.Count; ++i)
                    { 
                        DragTarget dt = lst[i];
                        dt.region = new Rect(fx, dt.target.cachedPlace.y, side, dt.target.cachedPlace.height);
                        fx += side;
                        lst[i] = dt;
                    }
                }
                else if(dir == DropType.Right)
                {
                    while (it != null && it.parent != null)
                    {
                        if (it.parent.dockType == Dock.Type.Horizontal)
                        {
                            if (it.parent.children.IndexOf(it) != it.parent.children.Count - 1)
                                break;

                            it = it.parent;
                        }
                        else if (it.parent.dockType == Dock.Type.Vertical)
                        {
                            lst.Add(new DragTarget(it.parent, dir, new Rect(), true));
                            it = it.parent;
                        }
                        else
                            break; //saftey
                    }
                    // We want the list going outer-to-inner, but we traversed from inner to outer.
                    lst.Reverse();

                    float fx = starting.cachedPlace.xMax;
                    for (int i = 0; i < lst.Count; ++i)
                    {
                        DragTarget dt = lst[i];
                        dt.region = new Rect(fx - side, dt.target.cachedPlace.y, side, dt.target.cachedPlace.height);
                        fx -= side;
                        lst[i] = dt;
                    }

                }
                else if(dir == DropType.Top)
                {
                    while (it != null && it.parent != null)
                    {
                        if (it.parent.dockType == Dock.Type.Vertical)
                        {
                            if (it.parent.children.IndexOf(it) != 0)
                                break;

                            it = it.parent;
                        }
                        else if (it.parent.dockType == Dock.Type.Horizontal)
                        {
                            lst.Add(new DragTarget(it.parent, dir, new Rect(), true));
                            it = it.parent;
                        }
                        else
                            break; // saftey
                    }
                    // We want the list going outer-to-inner, but we traversed from inner to outer.
                    lst.Reverse();

                    float fy = starting.cachedPlace.y;
                    for (int i = 0; i < lst.Count; ++i)
                    {
                        DragTarget dt = lst[i];
                        dt.region = new Rect(dt.target.cachedPlace.x, fy, dt.target.cachedPlace.width, side);
                        fy += side;
                        lst[i] = dt;
                    }
                }
                else if(dir == DropType.Bottom)
                {
                    while (it != null && it.parent != null)
                    {
                        if (it.parent.dockType == Dock.Type.Vertical)
                        {
                            if (it.parent.children.IndexOf(it) != it.parent.children.Count - 1)
                                break;

                            it = it.parent;
                        }
                        else if (it.parent.dockType == Dock.Type.Horizontal)
                        {
                            lst.Add(new DragTarget(it.parent, dir, new Rect(), true));
                            it = it.parent;
                        }
                        else
                            break; //saftey
                    }
                    // We want the list going outer-to-inner, but we traversed from inner to outer.
                    lst.Reverse();

                    float fy = starting.cachedPlace.yMax;
                    for (int i = 0; i < lst.Count; ++i)
                    {
                        DragTarget dt = lst[i];
                        dt.region = new Rect(dt.target.cachedPlace.x, fy - side, dt.target.cachedPlace.width, side);
                        fy -= side;
                        lst[i] = dt;
                    }
                }

                return lst;
            }

            /// <summary>
            /// Handler for when a window starts being dragged. This function is called
            /// from a Window delegating their start drag messages.
            /// </summary>
            /// <param name="window">The window the message is being delegated from.</param>
            /// <param name="eventData">The event data.</param>
            public void StartWindowDrag(Window window, UnityEngine.EventSystems.PointerEventData eventData)
            { 
                this.windowDragged = window;

                this.UndockWindow(window);

                Vector2 v2 = ConvertMousePointToCoord(eventData);
                this.HandleDragPreview(v2);
            }

            /// <summary>
            /// Handler for when a window stops being dragged. This function is called
            /// from a Window delegating their end drag messages.
            /// </summary>
            /// <param name="window">The window the message is being delegated from.</param>
            /// <param name="eventData">The event data.</param>
            public void EndWindowDrag(Window window, UnityEngine.EventSystems.PointerEventData eventData)
            { 
                if(this.windowDragged != window)
                    return;

                this.windowDragged = window;

                if(this.dropVisual != null)
                {
                    GameObject.Destroy(this.dropVisual.gameObject);
                    this.dropVisual = null;
                }

                Vector2 v2 = ConvertMousePointToCoord(eventData);
                DragTarget drt = this.QueryDropTarget(v2);

                if(drt.type == DropType.Invalid || drt.ontop == false)
                    return;

                this.DockWindow(window, drt.target, drt.type);
            }

            /// <summary>
            /// Handle a window being dragged. This function is called from
            /// Windows delegating their drag messages.
            /// </summary>
            /// <param name="window">The window the message is being delegated from.</param>
            /// <param name="eventData">The event data.</param>
            public void HandleWindowDrag(Window window, UnityEngine.EventSystems.PointerEventData eventData)
            { 
                if(this.windowDragged != window)
                    return;

                Vector2 v2 = ConvertMousePointToCoord(eventData);
                this.HandleDragPreview(v2);
            }

            /// <summary>
            /// Handler for when a window is clicked on. This function is called
            /// from Windows delegating their click messages.
            /// </summary>
            public void HandleWindowMouseDown(Window window, UnityEngine.EventSystems.PointerEventData eventData)
            { 
                if(this.floatingWindows.Count == 0)
                    return;

                if(this.floatingWindows[this.floatingWindows.Count - 1] == window)
                    return;

                if(this.floatingWindows.Remove(window) == true)
                {
                    this.floatingWindows.Add(window);
                    window.rectTransform.SetAsLastSibling();
                    window.UpdateShadow();
                }
            }

            /// <summary>
            /// Convert the mouse coordinate from a PointerEventData into a vector convention 
            /// that HandleDragPreview() expects.
            /// </summary>
            /// <param name="eventData">
            /// The mouse pointer callback with the mouse position expected to be converted.
            /// </param>
            /// <returns>
            /// The converted mouse position, where the origin of the position will be at the
            /// top left of this RectTransform. 
            /// </returns>
            public Vector2 ConvertMousePointToCoord(UnityEngine.EventSystems.PointerEventData eventData)
            {
                Vector2 szd = this.rectTransform.rect.size;
                Vector2 v = this.transform.worldToLocalMatrix.MultiplyPoint(eventData.position);

                v.x += szd.x * this.rectTransform.pivot.x;
                v.y += szd.y * this.rectTransform.pivot.y;
                v.y = szd.y - v.y;

                return v;
            }

            /// <summary>
            /// Handle managing the docking preview based off the mouse cursor.
            /// </summary>
            /// <param name="v2">
            /// The position of the mouse in the coordinate frame where the origin 
            /// of the window is the top left of this RectTransform.</param>
            public void HandleDragPreview(Vector2 v2)
            { 
                if(this.dropVisual == null)
                { 
                    GameObject goPD = new GameObject("PreviewDrop");

                    goPD.transform.SetParent(this.transform);
                    this.dropVisual = goPD.AddComponent<UnityEngine.UI.Image>();
                    Window.PrepareChild(this.dropVisual.rectTransform);
                    this.dropVisual.color = this.props.dockHover;
                }


                DragTarget drt = this.QueryDropTarget(v2);
                if (drt.type != DropType.Invalid) // Into is for tabbed containers, which are not currently supported.
                { 
                    if(drt.type != DropType.Into || this.allowTabbedDocking == true)
                    {
                        this.dropVisual.gameObject.SetActive(true);

                        this.dropVisual.rectTransform.anchoredPosition = 
                            new Vector2(drt.region.x, -drt.region.y);

                        this.dropVisual.rectTransform.sizeDelta = drt.region.size;

                        if(drt.ontop == true)
                            this.dropVisual.color = this.props.dockHover;
                        else
                            this.dropVisual.color = this.props.dockUnhover;
                    }
                }
                else
                {
                    this.dropVisual.gameObject.SetActive(false);
                }
            }

            /// <summary>
            /// Set the dirty flag, and the dirty sash positions substate.
            /// </summary>
            public void SetDirtySashPos()
            { 
                this.dirtySashPosition = true;
                this.SetDirty();
            }

            /// <summary>
            /// Set the dirty flag, and sashs reconstruction substate.
            /// </summary>
            public void SetDirtySashReconstr()
            { 
                this.dirtySashConstruction = true;
                this.SetDirty();
            }

            /// <summary>
            /// Set the dirty flag, and queue a layout operation.
            /// </summary>
            /// <returns></returns>
            public bool SetDirty()
            {
                if (this.dirtyHandle != null)
                    return false;

                this.dirtyHandle =
                    this.StartCoroutine(HandleDirtyStateEnum());

                return true;
            }

            /// <summary>
            /// Coroutine to handle switching the dirty flag.
            /// </summary>
            /// <returns></returns>
            IEnumerator HandleDirtyStateEnum()
            { 
                yield return new WaitForEndOfFrame();
                UpdateWindowsManagement(this.dirtySashPosition, this.dirtySashConstruction);
                this.dirtyHandle = null;
            }

            /// <summary>
            /// Destroy all the sashes.
            /// </summary>
            private void ClearSashes()
            { 
                foreach(DockSash ds in this.sashes)
                    GameObject.Destroy(ds.gameObject);

                this.sashes.Clear();
            }

            /// <summary>
            /// Clears all existing sashes, and 
            /// </summary>
            private void RegenerateSashes()
            { 
                // This function assumes the docking has been 
                // processed and is not dirty.

                this.ClearSashes();

                Queue<Dock> docksToProcess = 
                    new Queue<Dock>();

                if(this.root != null)
                    docksToProcess.Enqueue(this.root);

                while(docksToProcess.Count > 0)
                { 
                    Dock d = docksToProcess.Dequeue();
                    if(d.dockType == Dock.Type.Tab || d.children == null)
                        continue;

                    foreach (Dock dc in d.children)
                        docksToProcess.Enqueue(dc);

                    if (d.dockType == Dock.Type.Horizontal)
                    { 
                        for(int i = 0; i < d.children.Count - 1; ++i)
                        { 
                            Dock da = d.children[i + 0];
                            Dock db = d.children[i + 1];
                            DockSash ds = this.AllocateSash(da, db, DockSash.Grain.Horizontal);
                            ds.Align();
                        }
                    }
                    else if(d.dockType == Dock.Type.Vertical)
                    {
                        for (int i = 0; i < d.children.Count - 1; ++i)
                        {
                            Dock da = d.children[i + 0];
                            Dock db = d.children[i + 1];
                            DockSash ds = this.AllocateSash(da, db, DockSash.Grain.Vertical);
                            ds.Align();
                        }
                    }
                }
            }

            /// <summary>
            /// Create a sash in the correct parent and register it with the system.
            /// </summary>
            /// <param name="a">The left/top dock attached to the sash.</param>
            /// <param name="b">The right/bottom dock attached to the sash.</param>
            /// <param name="grain">The grain of the sash.</param>
            /// <returns>The created sash.</returns>
            private DockSash AllocateSash(Dock a, Dock b, DockSash.Grain grain)
            {
                GameObject goSash = new GameObject("sash");
                goSash.transform.SetParent(this.transform);

                DockSash ds = goSash.AddComponent<DockSash>();
                ds.dockA = a;
                ds.dockB = b;
                ds.grain = grain;
                ds.system = this;
                this.props.sashSprite.ApplySliced(ds);
                Window.PrepareChild(ds.rectTransform);
                ds.rectTransform.SetAsFirstSibling();

                this.sashes.Add(ds);
                this.props.sashSprite.ApplySliced(ds);

                return ds;
            }

            /// <summary>
            /// Move all existing sashes to their correct locations.
            /// </summary>
            /// <remarks>Assumes the docks are in the correct location before calling.</remarks>
            public void RealignSashes()
            { 
                foreach(DockSash ds in this.sashes)
                    ds.Align();
            }

            /// <summary>
            /// Clear all content from the system.
            /// </summary>
            public void Clear()
            { 
                foreach(Window w in this.windowLookup.Values)
                {
                    GameObject.Destroy(w.shadow.gameObject);
                    GameObject.Destroy(w.gameObject);
                }
                this.windowLookup.Clear();
                this.dockLookup.Clear();
                this.floatingWindows.Clear();
                this.root = null;

                this.ClearSashes();

                foreach(KeyValuePair<Dock, DockedTab> kvp in this.tabAssets)
                    kvp.Value.Destroy();

                this.tabAssets.Clear();
            }

            /// <summary>
            /// Unity RectTransform callback.
            /// </summary>
            protected override void OnRectTransformDimensionsChange()
            {
                base.OnRectTransformDimensionsChange();

                this.SetDirtySashPos();
            }

        }
    }
}