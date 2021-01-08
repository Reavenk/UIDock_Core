// <copyright file="Window.cs" company="Pixel Precision LLC">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>William Leu</author>
// <date>04/12/2020</date>
// <summary>
// A window, managing a RectTransform, that in turn is meant to
// be managed by a UIDock.Root.
// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace UIDock
    {
        public class Window : 
            UnityEngine.UI.Image,
            UnityEngine.EventSystems.IBeginDragHandler,
            UnityEngine.EventSystems.IEndDragHandler,
            UnityEngine.EventSystems.IDragHandler,
            UnityEngine.EventSystems.IPointerDownHandler,
            UnityEngine.EventSystems.IPointerUpHandler,
            UnityEngine.EventSystems.IPointerClickHandler
        {
            /// <summary>
            /// The different style flags used to defined
            /// properties for a window.
            /// </summary>
            [System.Flags]
            public enum Flag
            { 
                /// <summary>
                /// If set, the window has a close button.
                /// </summary>
                HasClose    = 1 << 0,

                /// <summary>
                /// If set, the window has a pin button
                /// </summary>
                HasPin      = 1 << 1,

                /// <summary>
                /// If set, the window can be made floatable.
                /// </summary>
                Floatable   = 1 << 2,

                /// <summary>
                /// If set, the window can be resized.
                /// </summary>
                Resizeable  = 1 << 3
            }

            /// <summary>
            /// An enum used to describe what kind of dragging
            /// operations is being done to the variable 
            /// this.dragWindow.
            /// </summary>
            /// <remarks>The siz of </remarks>
            [System.Flags]
            public enum FrameDrag
            { 
                /// <summary>
                /// A frame is not being 
                /// </summary>
                None        = 0,

                /// <summary>
                /// The top is being dragged.
                /// </summary>
                Top         = 1 << 0,

                /// <summary>
                /// The left is being dragged.
                /// </summary>
                Left        = 1 << 1,

                /// <summary>
                /// The bottom is being dragged.
                /// </summary>
                Bottom      = 1 << 2,

                /// <summary>
                /// The right is being dragged.
                /// </summary>
                Right       = 1 << 3,

                /// <summary>
                /// The position is being moved from dragging the
                /// titlebar.
                /// </summary>
                Position    = 1 << 4
            }

            /// <summary>
            /// The part of the frame on this.dragWindow being draged.
            /// </summary>
            static FrameDrag drag = FrameDrag.None;

            /// <summary>
            /// The window being dragged by the mouse.
            /// </summary>
            static Window dragWindow = null;

            /// <summary>
            /// The location where the mouse drag was started.
            /// </summary>
            static Vector2 localDragStart;



            /// <summary>
            /// A collection of variables for the titlebar buttons.
            /// </summary>
            public struct ButtonInfo
            { 
                /// <summary>
                /// The button.
                /// </summary>
                public UnityEngine.UI.Button btn;

                /// <summary>
                /// The outer button container.
                /// </summary>
                public UnityEngine.UI.Image plate;

                /// <summary>
                /// The inside button icon.
                /// </summary>
                public UnityEngine.UI.Image icon;
            }

            /// <summary>
            /// The default flags for a standard window.
            /// </summary>
            public const Flag DefaultFlags = 
                Flag.HasClose | Flag.HasPin | Flag.Floatable | Flag.HasPin;

            /// <summary>
            /// A cached copy of the window's style flags.
            /// </summary>
            private DockProps.WinType style = 
                DockProps.WinType.Float;

            /// <summary>
            /// The Root managing the window.
            /// </summary>
            public Root system = null;

            /// <summary>
            /// The shadow sprite shown underneath when the window is floating.
            /// </summary>
            public UnityEngine.UI.Image shadow = null;

            /// <summary>
            /// The close titlebar button.
            /// </summary>
            ButtonInfo btnClose;

            /// <summary>
            /// The pin titlebar button.
            /// </summary>
            ButtonInfo btnPin;

            /// <summary>
            /// The restore/maximize titlebar button.
            /// </summary>
            ButtonInfo btnRestMax;

            /// <summary>
            /// The UI element being managed.
            /// </summary>
            private RectTransform win = null;

            /// <summary>
            /// read-only access to the window.
            /// </summary>
            public RectTransform Win {get{return this.win; } }

            /// <summary>
            /// The titlebar text.
            /// </summary>
            UnityEngine.UI.Text titlebar;

            /// <summary>
            /// Timer used to tracking the distance of clicks.
            /// Used with lastClickWin to detect double clicks.
            /// </summary>
            static float lastClickTime = -999.0f;

            /// <summary>
            /// The last window clicked, used with lastClickTime
            /// to track double clicks.
            /// </summary>
            static Window lastClickWin = null;

            /// <summary>
            /// Reset the double clicks tracking variables.
            /// </summary>
            static void ResetDoubleClick()
            { 
                lastClickTime = -999.0f;
                lastClickWin = null;
            }

            /// <summary>
            /// Initialize the window.
            /// </summary>
            /// <param name="parent">The Root that the window will be managed by.</param>
            /// <param name="rt">The UI being managed.</param>
            /// <param name="titlebar">The titlebar text.</param>
            /// <param name="flags">The style flags.</param>
            public void Initialize(
                Root parent, 
                RectTransform rt, 
                string titlebar,
                Flag flags = DefaultFlags)
            {
                this.system = parent;
                DockProps props = system.props;

                props.floatWin.spriteFrame.ApplySliced(this);
                this.transform.SetParent(parent.rectTransform);
                PrepareChild(this.rectTransform);

                this.win = rt;

                rt.SetParent(this.rectTransform);
                PrepareChild(rt);

                GameObject goShadow = new GameObject("Shadow_" + rt.gameObject.name);
                goShadow.transform.SetParent(parent.rectTransform);
                this.shadow = goShadow.AddComponent<UnityEngine.UI.Image>();
                this.shadow.color = props.shadowColor;
                props.shadow.ApplySliced(this.shadow);
                PrepareChild(this.shadow.rectTransform);

                GameObject goTitleTest = new GameObject("Text_" + rt.gameObject.name);
                goTitleTest.transform.SetParent(this.transform);
                UnityEngine.UI.Text title = goTitleTest.AddComponent<UnityEngine.UI.Text>();
                PrepareChild(title.rectTransform);
                title.font = props.titleFont;
                title.fontSize = props.titleFontSize;
                title.color = props.titleLabelColor;
                this.titlebar = title;

                this.rectTransform.sizeDelta = 
                    CalculateSizeFromInnerRect();

                if((flags & Flag.HasClose) != 0)
                {
                    this.btnClose = this.AddButton(props.spriteBtnClose.sprite);
                    this.btnClose.btn.onClick.AddListener(()=>{ this.OnTitlebarButton_Close(); });
                }

                if((flags & Flag.HasPin) != 0)
                {
                    this.btnPin = this.AddButton(props.spriteBtnPin.sprite);
                    this.btnPin.btn.onClick.AddListener(()=>{ this.OnTitlebarButton_Pin(); });
                }

                if((flags & Flag.Floatable) != 0)
                {
                    this.btnRestMax = this.AddButton(props.spriteBtnMax.sprite);
                    this.btnRestMax.btn.onClick.AddListener(()=>{ this.OnTitlebarButton_RestMax(); });
                }

                this.TitlebarText = titlebar;

                this.PlaceContentWin();
            }

            /// <summary>
            /// Callback for the close window.
            /// </summary>
            void OnTitlebarButton_Close()
            {
                this.CloseWindow();
            }

            /// <summary>
            /// Callback for the restore/maximize window.
            /// </summary>
            void OnTitlebarButton_RestMax()
            { 
                this.MaximizeWindow();
            }

            void OnTitlebarButton_Pin()
            { 
                this.PinWindow();
            }

            /// <summary>
            /// Access to the titlebar text.
            /// </summary>
            public string TitlebarText 
            {
                get => this.titlebar.text;
                set
                { 
                    this.titlebar.text = value;
                } 
            }

            /// <summary>
            /// Calcuate how window should be to properly managed the ui content without
            /// changing the ui content's size.
            /// </summary>
            /// <returns></returns>
            Vector2 CalculateSizeFromInnerRect()
            {
                DockProps props = system.props;
                Vector2 dimRt = this.win.rect.size;

                Vector2 size =
                    new Vector2(
                        dimRt.x + props.winPadding * 2.0f,
                        dimRt.y + props.winPadding + props.floatWin.titlebarHeight);

                return size;
            }

            /// <summary>
            /// Calculate how big the managed content should be to maintain the window's
            /// current size while still properly containing the ui content.
            /// </summary>
            /// <returns></returns>
            Vector2 CalculateInnerRectFromSize()
            {
                DockProps props = system.props;
                Vector2 dimRt = this.rectTransform.rect.size;

                Vector2 size =
                    new Vector2(
                        dimRt.x - props.winPadding * 2.0f,
                        dimRt.y - (props.winPadding + props.floatWin.titlebarHeight));

                return size;
            }

            /// <summary>
            /// Create button assets in the window.
            /// </summary>
            /// <param name="iconSprite">The icon.</param>
            /// <returns>The button information.</returns>
            ButtonInfo AddButton(Sprite iconSprite)
            { 
                GameObject goPlate = new GameObject("Button");
                goPlate.transform.SetParent( this.transform );
                UnityEngine.UI.Image plate = goPlate.AddComponent<UnityEngine.UI.Image>();
                PrepareChild(plate.rectTransform);

                UnityEngine.UI.Button btn = goPlate.AddComponent<UnityEngine.UI.Button>();
                btn.targetGraphic = plate;

                ButtonInfo ret = new ButtonInfo();
                ret.btn = btn;
                ret.plate = plate;

                if(iconSprite != null)
                {
                    GameObject goIcon = new GameObject("Icon");
                    goIcon.transform.SetParent(goPlate.transform);
                    UnityEngine.UI.Image icon = goIcon.AddComponent<UnityEngine.UI.Image>();

                    icon.sprite = iconSprite;
                    icon.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    icon.rectTransform.pivot = new Vector2(0.5f, 0.5f);

                    Vector2 halfDim = iconSprite.rect.size / 2.0f;
                    icon.rectTransform.offsetMin = new Vector2(-halfDim.x, -halfDim.y);
                    icon.rectTransform.offsetMax = new Vector2(halfDim.x, halfDim.y);

                    ret.icon = icon;
                }

                return ret;
            }

            IEnumerable<ButtonInfo> EnuerateButtons()
            {
                if (this.btnClose.plate != null)
                    yield return this.btnClose;

                if (this.btnRestMax.plate != null)
                    yield return this.btnRestMax;

                if (this.btnPin.plate != null)
                    yield return this.btnPin;
            }

            /// <summary>
            /// Match the client content to take up the full windowed area. Used
            /// for docked content to hide the border.
            /// </summary>
            void PlaceContentBorderless()
            {
                this.win.anchorMin = Vector2.zero;
                this.win.anchorMax = Vector2.one;
                this.win.offsetMin = Vector2.zero;
                this.win.offsetMax = Vector2.zero;

                foreach (ButtonInfo bi in this.EnuerateButtons())
                    bi.plate.gameObject.SetActive(false);
            }

            /// <summary>
            /// Move the inner content around in the window to be placed. This
            /// does not only account for the managed ui, but for all the other
            /// things (titlebar buttons, titlebar text, etc).
            /// </summary>
            void PlaceContentWin()
            {
                DockProps props = system.props;
                DockProps.WindowSetting winset = this.GetWindowSetting();

                Vector2 clientDim = this.CalculateInnerRectFromSize();

                this.win.anchorMin = Vector2.zero;
                this.win.anchorMax = Vector2.one;
                this.win.offsetMin = new Vector2(props.winPadding, props.winPadding);
                this.win.offsetMax = new Vector2(-props.winPadding, -winset.titlebarHeight);

                float ffromR = props.winPadding;
                float buttonY = (props.floatWin.titlebarHeight - props.floatWin.btnHeight) * 0.5f;
                foreach(ButtonInfo bi in this.EnuerateButtons())
                {
                    bi.plate.gameObject.SetActive(true);

                    bi.plate.rectTransform.sizeDelta = 
                        new Vector2(
                            props.floatWin.btnWidth, 
                            props.floatWin.btnHeight);

                    ffromR += props.floatWin.btnWidth;

                    bi.plate.rectTransform.anchorMin = new Vector2(1.0f, 1.0f);
                    bi.plate.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);

                    bi.plate.rectTransform.anchoredPosition =
                        new Vector2(-ffromR, -buttonY);
                }
                float fend = this.rectTransform.rect.width - ffromR;

                TextGenerationSettings tgs =
                    this.titlebar.GetGenerationSettings(
                        new Vector2(
                            float.PositiveInfinity,
                            float.PositiveInfinity));

                TextGenerator tg = this.titlebar.cachedTextGenerator;

                Vector2 titlebarTextSz =
                    new Vector2(
                        tg.GetPreferredWidth(this.titlebar.text, tgs),
                        tg.GetPreferredHeight(this.titlebar.text, tgs));

                this.titlebar.alignment = TextAnchor.MiddleLeft;
                RectTransform rtTitle = this.titlebar.rectTransform;
                rtTitle.pivot = new Vector2(0.5f, 0.5f);
                rtTitle.anchorMin = new Vector2(0.0f, 1.0f);
                rtTitle.anchorMax = new Vector2(1.0f, 1.0f);
                rtTitle.offsetMin = new Vector2(props.winPadding, -winset.titlebarHeight);
                rtTitle.offsetMax = new Vector2(0.0f, 0.0f);
                this.UpdateShadow();
            }

            /// <summary>
            /// Update the shadow. This includes size, position and z-ordering.
            /// </summary>
            public void UpdateShadow()
            { 
                if(this.shadow.gameObject.activeSelf == false)
                    return;

                DockProps props = system.props;
                this.shadow.rectTransform.sizeDelta = this.rectTransform.sizeDelta;
                this.shadow.rectTransform.anchoredPosition = this.rectTransform.anchoredPosition + props.shadowOffset;

                int thisSibIdx = this.rectTransform.GetSiblingIndex();
                if(thisSibIdx == 0)
                    this.shadow.rectTransform.SetAsFirstSibling();
                else
                    this.shadow.rectTransform.SetSiblingIndex(thisSibIdx - 1);
            }

            /// <summary>
            /// Apply standard UI setup work to a RectTransform.
            /// 
            /// It's assumed the input RectTransform is already
            /// parented correctly.
            /// </summary>
            /// <param name="rt"></param>
            public static void PrepareChild(RectTransform rt)
            {
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
                rt.pivot = new Vector2(0.0f, 1.0f);
                rt.anchorMin = new Vector2(0.0f, 1.0f);
                rt.anchorMax = new Vector2(0.0f, 1.0f);
            }

            /// <summary>
            /// Toggle the window's shadow.
            /// </summary>
            /// <param name="shadow">
            /// If true, the shadow is turned on; 
            /// if false, the shadow is turned off.</param>
            public void EnableShadow(bool shadow = true)
            { 
                if(this.shadow != null)
                    this.shadow.gameObject.SetActive(shadow);
            }

            /// <summary>
            /// Turn off the window's shadow.
            /// </summary>
            public void DisableShadow()
            { 
                this.EnableShadow(false);
            }

            /// <summary>
            /// Maximize the window.
            /// </summary>
            private void MaximizeWindow()
            { 
                if(this.system.IsMaximized(this) == true)
                    this.system.RestoreWindow(this);
                else
                    this.system.MaximizeWindow(this);
            }

            /// <summary>
            /// 
            /// </summary>
            private void RestoreWindow()
            { 
                this.system.RestoreWindow(this);
            }

            /// <summary>
            /// Close and destroy the window.
            /// </summary>
            private void CloseWindow()
            { 
                this.system.CloseWindow(this);
            }

            /// <summary>
            /// Undock the window.
            /// </summary>
            private void PinWindow()
            {
                this.system.UndockWindow(this);
            }

            /// <summary>
            /// Get the window settings, based on the visual style of the window.
            /// </summary>
            /// <returns>The window settings.</returns>
            /// <remarks>The visual style should not be confused with the style flags.</remarks>
            DockProps.WindowSetting GetWindowSetting()
            {
                return this.system.props.GetWindowSetting(this.style);
            }

            public static void _StartOutsideDrag(FrameDrag frame, Window dragWin, Vector2 offset)
            { 
                drag = frame;
                dragWindow = dragWin;
                localDragStart = offset;
            }

            /// <summary>
            /// Unity interface method for starting a UI drag.
            /// </summary>
            void UnityEngine.EventSystems.IBeginDragHandler.OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
            { 
                if(dragWindow == this && drag == FrameDrag.Position)
                    this.system.StartWindowDrag(this, eventData);
            }

            /// <summary>
            /// Unity interface method for ending a UI drag.
            /// </summary>
            void UnityEngine.EventSystems.IEndDragHandler.OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
            {
                drag = FrameDrag.None;
                dragWindow = null;
                this.system.EndWindowDrag(this, eventData);
            }

            /// <summary>
            /// Unity interface method for dragging.
            /// </summary>
            void UnityEngine.EventSystems.IDragHandler.OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
            { 
                if(dragWindow != this)
                    return;

                if(drag == FrameDrag.None)
                    return;

                if(this.system.IsMaximized(this) == true)
                {
                    if(drag != FrameDrag.Position)
                        return;

                    this.RestoreWindow();
                }

                DockProps props = this.system.props;

                if(drag == FrameDrag.Position)
                { 
                    this.rectTransform.anchoredPosition += eventData.delta;
                    this.system.HandleWindowDrag(this, eventData);
                }
                else
                {
                    // TODO: Min size enforcement

                    Vector2 rtpos = this.rectTransform.anchoredPosition;
                    Vector2 rtsize = this.rectTransform.sizeDelta;

                    if((drag & FrameDrag.Top) != 0)
                    { 
                        float dy = eventData.delta.y;
                        float newSz = Mathf.Max(rtsize.y + dy, props.minsizeWindow.y);
                        dy = newSz - rtsize.y;

                        rtpos.y += dy;
                        rtsize.y += dy;

                    }
                    else if((drag & FrameDrag.Bottom) != 0)
                    {

                        float dy = eventData.delta.y;
                        float newSz = Mathf.Max(rtsize.y - dy, props.minsizeWindow.y);
                        dy = newSz - rtsize.y;

                        rtsize.y += dy;
                    }

                    if((drag & FrameDrag.Left) != 0)
                    { 
                        float dx = eventData.delta.x;
                        float newSz = Mathf.Max(rtsize.x - dx, props.minsizeWindow.x);
                        dx = rtsize.x - newSz;

                        rtpos.x += dx;
                        rtsize.x -= dx;
                    }
                    else if((drag & FrameDrag.Right) != 0)
                    {
                        float dx = eventData.delta.x;
                        float newSz = Mathf.Max(rtsize.x + dx, props.minsizeWindow.x);
                        dx = newSz - rtsize.x;

                        rtsize.x += dx;
                    }

                    this.rectTransform.anchoredPosition = rtpos;
                    this.rectTransform.sizeDelta = rtsize;
                    this.PlaceContentWin();
                }

                this.UpdateShadow();
            }

            /// <summary>
            /// Unity interface method for clicking the mouse down.
            /// </summary>
            void UnityEngine.EventSystems.IPointerDownHandler.OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
            {
                dragWindow = this;
                localDragStart = this.transform.worldToLocalMatrix.MultiplyPoint(eventData.position);

                DockProps props = system.props;
                DockProps.WindowSetting winSetting = this.GetWindowSetting();

                Vector2 sz = this.rectTransform.rect.size;

                drag = FrameDrag.None;

                this.system.HandleWindowMouseDown(this, eventData);

                if(this.style == DockProps.WinType.Float)
                {
                    if (localDragStart.y >= -props.winPadding)
                        drag |= FrameDrag.Top;
                    else if(localDragStart.y <= -sz.y + props.winPadding)
                        drag |= FrameDrag.Bottom;

                    if(localDragStart.x <= props.winPadding)
                        drag |= FrameDrag.Left;
                    else if(localDragStart.x >= sz.x - props.winPadding)
                        drag |= FrameDrag.Right;
                }

                if(
                    drag == FrameDrag.None && 
                    localDragStart.y >= -winSetting.titlebarHeight)
                {
                    drag = FrameDrag.Position;
                }
            }

            /// <summary>
            /// Unity interface method for release the mouse click.
            /// </summary>
            void UnityEngine.EventSystems.IPointerUpHandler.OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
            { 
                drag = FrameDrag.None;
                dragWindow = null;
            }

            /// <summary>
            /// Unity interface method for clicking.
            /// </summary>
            void UnityEngine.EventSystems.IPointerClickHandler.OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
            { 
                // The only thing we check here ATM is double clicking
                // in the titlebar area.
                float dCR = this.system.props.doubleClickRate;
                DockProps.WindowSetting winSetting = this.GetWindowSetting();
                if (localDragStart.y >= -winSetting.titlebarHeight)
                {
                    if(lastClickWin != this)
                    { 
                        lastClickWin = this;
                        lastClickTime = Time.time;
                    }
                    else if(Time.time > lastClickTime  + dCR)
                        ResetDoubleClick();
                    else
                    {
                        if(this.system.IsMaximized(this) == true)
                            this.system.RestoreWindow(this);
                        else
                            this.system.MaximizeWindow(this);

                        ResetDoubleClick();
                    }
                }
                else
                    ResetDoubleClick();
            }

            /// <summary>
            /// Called on the window when changed to be maximized.
            /// </summary>
            public void NotifyMaximized()
            {
                this.ChangeStyle(DockProps.WinType.Maximized);

                if(this.btnPin.plate != null)
                    this.btnPin.plate.gameObject.SetActive(false);

                this._SetRestoreButton();
            }

            /// <summary>
            /// Called on the window when changed to be floating.
            /// </summary>
            public void NotifyFloating()
            {
                this.ChangeStyle(DockProps.WinType.Float);

                if (this.btnPin.plate != null)
                    this.btnPin.plate.gameObject.SetActive(false);

                this._SetMaximizeButton();
            }

            /// <summary>
            /// Called on the window when changed to be docked.
            /// </summary>
            public void NotifyDocked()
            {
                if (this.btnPin.plate != null)
                    this.btnPin.plate.gameObject.SetActive(true);

                this._SetMaximizeButton();

                this.ChangeStyle(DockProps.WinType.Docked);
            }

            /// <summary>
            /// Change the visual style of the window.
            /// </summary>
            /// <param name="winType">The window style to change to.</param>
            /// <param name="placeContent">If true, PlaceContent() will be called afterwards.</param>
            public void ChangeStyle(DockProps.WinType winType, bool placeContent = true)
            { 
                if(this.style == winType)
                    return;

                this.style = winType;

                if(winType == DockProps.WinType.Borderless)
                { 
                    this.PlaceContentBorderless();
                }
                else
                {
                    DockProps.WindowSetting ws = this.GetWindowSetting();
                    this.sprite = ws.spriteFrame.sprite;
                    this.color = ws.spriteFrame.color;

                    if(this.btnClose.plate != null)
                        ws.spriteBtnPlate.ApplySliced(this.btnClose.plate);

                    if(this.btnPin.plate != null)
                        ws.spriteBtnPlate.ApplySliced(this.btnPin.plate);

                    if(this.btnRestMax.plate != null)
                        ws.spriteBtnPlate.ApplySliced(this.btnRestMax.plate);

                    if(placeContent == true)
                        this.PlaceContentWin();
                }
            }

            /// <summary>
            /// Set the maximize/restore button have to have maximize icon.
            /// </summary>
            void _SetMaximizeButton()
            { 
                if(this.btnRestMax.icon != null)
                {
                    this.system.props.spriteBtnMax.ApplySimple(this.btnRestMax.icon);
                }
            }

            /// <summary>
            /// Set the maximize/restore button to have the restore icon.
            /// </summary>
            void _SetRestoreButton()
            { 
                if(this.btnRestMax.icon != null)
                { 
                    this.system.props.spriteBtnRestore.ApplySimple(this.btnRestMax.icon);
                }
            }
        }
    }
}