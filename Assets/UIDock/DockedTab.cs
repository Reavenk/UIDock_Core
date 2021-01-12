using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIDock
    {
        /// <summary>
        /// Management of instanced assets for the docked tab. 
        /// </summary>
        /// <remarks>
        /// Note that this content is parented directly to the dock (the same 
        /// parent as the sashes) - do not make the confusion of this being parented
        /// on to the actual windows that are docked.
        /// </remarks>
        public class DockedTab
        {
            /// <summary>
            /// Assets for each notebook tab.
            /// </summary>
            public class TabsAssets
            { 
                public UnityEngine.UI.Image notebookTab;

                public UnityEngine.UI.Button notebookButton;

                public UnityEngine.UI.Text label;

                public UnityEngine.UI.Image closeButton;

                public void Destroy()
                { 
                    if(this.notebookTab != null)
                        GameObject.Destroy(this.notebookTab.gameObject);
                }
            }

            public readonly Dock dock;
            public readonly Root root;

            private UnityEngine.UI.Image floor = null;
            //private UnityEngine.UI.Image compressLeftBtnPlate = null;
            //private UnityEngine.UI.Image compressRightBtnPlate = null;

            private Vector2 cachedLastPos;
            private Vector2 cachedLastSize;

            Dictionary<Dock, TabsAssets> assetLookup = new Dictionary<Dock, TabsAssets>();

            public DockedTab(Dock dock, Root root)
            { 
                this.dock = dock;
                this.root = root;
            }

            public void CreateAssets()
            {
                //          Floor
                ////////////////////////////////////////////////////////////////////////////////
                GameObject goFlow = new GameObject("TabsFloor");
                goFlow.transform.SetParent(this.root.rectTransform, false);
                this.floor = goFlow.AddComponent<UnityEngine.UI.Image>();
                RectTransform rtFloor = this.floor.rectTransform;
                rtFloor.anchorMin = new Vector2(0.0f, 1.0f);
                rtFloor.anchorMax = new Vector2(0.0f, 1.0f);
                rtFloor.pivot = new Vector2(0.0f, 1.0f);
                this.floor.sprite = this.root.props.tabs.tabFloor;

                // For now we'll forego tab navigation buttons.
                ////          Left compression navigation
                //////////////////////////////////////////////////////////////////////////////////
                //GameObject goCmpLeft = new GameObject("CompressLeft");
                //goCmpLeft.transform.SetParent(parent, false);
                //this.compressLeftBtnPlate = goCmpLeft.AddComponent<UnityEngine.UI.Image>();
                //RectTransform rtCmpLeft = this.compressLeftBtnPlate.rectTransform;
                //rtCmpLeft.anchorMin = new Vector2(0.0f, 1.0f);
                //rtCmpLeft.anchorMax = new Vector2(0.0f, 1.0f);
                //rtCmpLeft.pivot = new Vector2(0.0f, 1.0f);
                //
                //GameObject goCmpLeftIcon = new GameObject("CompressLeftIcon");
                //
                ////      Right compression navigation
                //////////////////////////////////////////////////////////////////////////////////
                //GameObject goCmpRight = new GameObject("CompressRight");
                //goCmpRight.transform.SetParent(parent, false);
                //this.compressRightBtnPlate = goCmpRight.AddComponent<UnityEngine.UI.Image>();
                //RectTransform rtCmpRight = this.compressRightBtnPlate.rectTransform;
                //rtCmpRight.anchorMin = new Vector2(0.0f, 1.0f);
                //rtCmpRight.anchorMax = new Vector2(0.0f, 1.0f);
                //rtCmpRight.pivot = new Vector2(0.0f, 1.0f);

                //GameObject goCmpRightIcon = new GameObject("CompressRightIcon");
            }

            public void Destroy()
            { 
                foreach(KeyValuePair<Dock, TabsAssets> kvp in this.assetLookup)
                    kvp.Value.Destroy();

                this.assetLookup.Clear();

                if(this.floor != null)
                    GameObject.Destroy(this.floor.gameObject);

                //if(this.compressLeftBtnPlate != null)
                //    GameObject.Destroy(this.compressLeftBtnPlate.gameObject);
                //
                //if(this.compressRightBtnPlate != null)
                //    GameObject.Destroy(this.compressRightBtnPlate.gameObject);
            }

            /// <summary>
            /// Retrieve a cached TabAssets for a specific window in the tab system - or
            /// create and store a new one if none currently exist.
            /// </summary>
            /// <param name="tabbedWin">The window to retrive the assets for.</param>
            /// <param name="rt">The RectTransform to put parent the assets in if they're being created. </param>
            /// <param name="props">The properties used to give the assets if they're being created.</param>
            /// <returns></returns>
            public TabsAssets GetTabAssets(Dock tabbedWin, RectTransform parent, DockProps props)
            { 
                TabsAssets ret;
                if(this.assetLookup.TryGetValue(tabbedWin, out ret) == true)
                    return ret;

                ret = new TabsAssets();

                GameObject goTab = new GameObject("Tab asset");
                goTab.transform.SetParent(parent);
                ret.notebookTab = goTab.AddComponent<UnityEngine.UI.Image>();
                ret.notebookTab.sprite = props.tabs.tabPlate;
                ret.notebookTab.type = UnityEngine.UI.Image.Type.Sliced;
                RectTransform rtTab = ret.notebookTab.rectTransform;
                rtTab.anchorMin = new Vector2(0.0f, 1.0f);
                rtTab.anchorMax = new Vector2(0.0f, 1.0f);
                rtTab.pivot = new Vector2(0.0f, 1.0f);
                goTab.AddComponent<UnityEngine.UI.Mask>();

                ret.notebookButton = goTab.AddComponent<UnityEngine.UI.Button>();
                ret.notebookButton.onClick.AddListener(
                    ()=>
                    { 
                        this.dock.activeTab = tabbedWin;
                        this.HandleDock();
                    });

                // Prevent locked windows from being ripped from the tab system. The issue here is
                // that if it's ripped, then it should turn to a floating window, which locked windows
                // can't.
                if(tabbedWin.window.Locked == false)
                {
                    // If the tag is being dragged, redirect it to the window to
                    // initiate a pull-off.
                    UnityEngine.EventSystems.EventTrigger etTab = goTab.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                    etTab.triggers = new List<UnityEngine.EventSystems.EventTrigger.Entry>();
                    UnityEngine.EventSystems.EventTrigger.Entry dragEnt = new UnityEngine.EventSystems.EventTrigger.Entry();
                    dragEnt.eventID = UnityEngine.EventSystems.EventTriggerType.BeginDrag;
                    dragEnt.callback.AddListener(
                        (x)=>
                        { 
                            UnityEngine.EventSystems.PointerEventData evt = 
                                x as UnityEngine.EventSystems.PointerEventData;

                            // Transform the point from the tab, to the window about to be ripped.
                            Vector2 mouseInTab = goTab.transform.worldToLocalMatrix.MultiplyPoint(evt.position);
                            evt.position = tabbedWin.window.transform.localToWorldMatrix.MultiplyPoint(mouseInTab);

                            // Make sure it's active before handoff. Will be inactive if not the main tab.
                            tabbedWin.window.gameObject.SetActive(true);

                            // Do handoff
                            evt.dragging = true;
                            evt.pointerDrag = tabbedWin.window.gameObject;

                            // Force titlebar drag state
                            Window._StartOutsideDrag( Window.FrameDrag.Position, tabbedWin.window, Vector2.zero);

                            // Make sure it handles OnBeginDrag - certain important drag things are 
                            // initialized there.
                            UnityEngine.EventSystems.IBeginDragHandler dragBegin = tabbedWin.window;
                            dragBegin.OnBeginDrag(evt);
                            // Reset styles + shadow
                            this.root.UndockWindow(tabbedWin.window);   

                        });
                    etTab.triggers.Add(dragEnt);
                }

                if( tabbedWin.window.Closable == true && 
                    tabbedWin.window.Locked == false)
                {
                    GameObject goCloseBtn = new GameObject("CloseButton");
                    goCloseBtn.transform.SetParent(rtTab);
                    ret.closeButton = goCloseBtn.AddComponent<UnityEngine.UI.Image>();
                    ret.closeButton.sprite = props.tabs.innerTabBtn;
                    ret.closeButton.type = UnityEngine.UI.Image.Type.Sliced;
                    RectTransform rtCloseBtn = ret.closeButton.rectTransform;
                    rtCloseBtn.anchorMin = new Vector2(1.0f, 0.0f);
                    rtCloseBtn.anchorMax = new Vector2(1.0f, 1.0f);
                    rtCloseBtn.offsetMin = 
                        new Vector2(
                            -this.root.props.tabs.closeBorderRight - this.root.props.tabs.closeWidth, 
                            this.root.props.tabs.closeBorderVert);
                    rtCloseBtn.offsetMax = 
                        new Vector2(
                            -this.root.props.tabs.closeBorderRight, 
                            -this.root.props.tabs.closeBorderVert);

                    UnityEngine.UI.Button closeBtn = goCloseBtn.AddComponent<UnityEngine.UI.Button>();
                    closeBtn.onClick.AddListener(
                        ()=>
                        { 
                            this.root.CloseWindow(tabbedWin.window);
                        });

                    if(props.tabs.closeWindowIcon != null)
                    {
                        GameObject goCloseIco = new GameObject("Close");
                        goCloseIco.transform.SetParent(rtCloseBtn);
                        UnityEngine.UI.Image imgCloseIco = goCloseIco.AddComponent<UnityEngine.UI.Image>();
                        RectTransform rtClIco = imgCloseIco.rectTransform;
                        imgCloseIco.sprite = props.tabs.closeWindowIcon;
                        rtClIco.anchorMin = new Vector2(0.5f, 0.5f);
                        rtClIco.anchorMax = new Vector2(0.5f, 0.5f);
                        rtClIco.pivot = new Vector2(0.5f, 0.5f);
                        rtClIco.anchoredPosition = Vector2.zero;
                        rtClIco.sizeDelta = props.tabs.closeWindowIcon.rect.size;
                    }
                }

                GameObject goText = new GameObject("Text");
                goText.transform.SetParent(rtTab);
                ret.label = goText.AddComponent<UnityEngine.UI.Text>();
                ret.label.text = tabbedWin.window.TitlebarText;
                ret.label.color = props.tabs.tabFontColor;
                ret.label.fontSize = props.tabs.tabFontSize;
                ret.label.verticalOverflow = VerticalWrapMode.Truncate;
                ret.label.horizontalOverflow = HorizontalWrapMode.Wrap;
                ret.label.alignment = TextAnchor.MiddleCenter;
                ret.label.font = props.tabs.tabFont;
                RectTransform rtLabel = ret.label.rectTransform;
                rtLabel.anchorMin = Vector2.zero;
                rtLabel.anchorMax = Vector2.one;
                rtLabel.offsetMin = Vector2.zero;
                rtLabel.offsetMax = new Vector2(-this.root.props.tabs.closeBorderRight - this.root.props.tabs.closeWidth, 0.0f);
                rtLabel.pivot = new Vector2(0.5f, 0.5f);

                this.assetLookup.Add(tabbedWin, ret);
                return ret;
            }

            public void ManageTabs(Vector2 pos, Vector2 size)
            {
                float height = 1.0f;
                if(this.floor.sprite != null)
                    height = Mathf.Min(size.y, this.floor.sprite.rect.height);

                this.floor.rectTransform.anchoredPosition = new Vector2(pos.x, pos.y - size.y + height);
                this.floor.rectTransform.sizeDelta = new Vector2(size.x, height);

                //this.compressLeftBtnPlate.gameObject.SetActive(false);
                //this.compressRightBtnPlate.gameObject.SetActive(false);

                int childrenCt = this.dock.children.Count;
                float totalTabsWidth = childrenCt * this.root.props.tabs.maxWidth;
                totalTabsWidth = Mathf.Min(size.x, totalTabsWidth);

                float tabWidth = totalTabsWidth / childrenCt;
                if(tabWidth < this.root.props.tabs.compactThreshold)
                { 
                    // If we're below the compact threshold, recalculate
                    // everything else if the active tab was clamped to
                    // that size.
                    tabWidth = 
                        (totalTabsWidth - this.root.props.tabs.compactThreshold)/(childrenCt - 1);
                }

                float xOff = 0.0f;

                floor.transform.SetAsFirstSibling();

                HashSet<Dock> childrenEncountered = new HashSet<Dock>();
                foreach(Dock d in this.dock.children)
                {
                    bool isActive = d == this.dock.activeTab;
                    float useTabWidth = tabWidth;

                    if(isActive == true)
                    { 
                        if(useTabWidth < this.root.props.tabs.compactThreshold)
                            useTabWidth = this.root.props.tabs.compactThreshold;
                    }

                    TabsAssets ta = GetTabAssets(d, this.root.rectTransform, this.root.props);
                    RectTransform rtt = ta.notebookTab.rectTransform;
                    rtt.anchoredPosition = new Vector2(pos.x + xOff, pos.y);
                    rtt.sizeDelta = new Vector2(useTabWidth, size.y);

                    if(isActive)
                    {
                        rtt.SetSiblingIndex(floor.transform.GetSiblingIndex() + 1);

                        if(ta.closeButton != null)
                            ta.closeButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        rtt.SetAsFirstSibling();

                        // If there's so little space, don't even show the close button
                        bool showClose = (useTabWidth >= this.root.props.tabs.minWidth);

                        if(ta.closeButton != null)
                            ta.closeButton.gameObject.SetActive(showClose);
                    }

                    if(useTabWidth <= this.root.props.tabs.compactThreshold)
                        ta.label.gameObject.SetActive(false);
                    else
                        ta.label.gameObject.SetActive(true);

                    xOff += useTabWidth;
                    childrenEncountered.Add(d);
                }


                List<Dock> currentTrackedDocks = new List<Dock>(assetLookup.Keys);
                foreach(Dock d in currentTrackedDocks)
                { 
                    if(childrenEncountered.Contains(d) == false)
                    {
                        this.assetLookup[d].Destroy();
                        this.assetLookup.Remove(d);
                    }
                }

                this.cachedLastPos = pos;
                this.cachedLastSize = size;

            }

            public void HandleDock()
            {
                float dtH = root.props.tabs.rgnHeight;
                float wp = root.props.winPadding;

                Rect rc = this.dock.cachedPlace;
                foreach (Dock d in this.dock.children)
                {
                    if (d == this.dock.activeTab)
                    {
                        d.window.rectTransform.gameObject.SetActive(true);
                        d.window.rectTransform.anchoredPosition = new Vector2(rc.x + wp, -rc.y - dtH - wp);
                        d.window.rectTransform.sizeDelta = new Vector2(rc.width - wp * 2.0f, rc.height - dtH - wp * 2.0f);
                        d.window.rectTransform.SetAsFirstSibling();
                    }
                    else
                    {
                        d.window.rectTransform.gameObject.SetActive(false);
                    }
                }

                // Everything involved with alignment is deffered to DockedTabs.
                this.ManageTabs(
                    new Vector2(rc.x, -rc.y),
                    new Vector2(rc.width, dtH));
            }

            public void Show(bool toggle = true)
            { 
                this.floor.gameObject.SetActive(toggle);

                foreach(KeyValuePair<Dock,TabsAssets> kvp in this.assetLookup)
                    kvp.Value.notebookTab.gameObject.SetActive(toggle);
            }

            public void Hide()
            { 
                this.Show(false);
            }
        } 
    }
}
