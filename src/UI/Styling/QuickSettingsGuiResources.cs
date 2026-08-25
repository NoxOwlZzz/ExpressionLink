using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsGuiResources : IDisposable
    {
        private GUISkin _skin;
        private QuickSettingsTextureCatalog _textures;

        internal GUIStyle Surface { get; private set; }
        internal GUIStyle Toolbar { get; private set; }
        internal GUIStyle Footer { get; private set; }
        internal GUIStyle PropertyRow { get; private set; }
        internal GUIStyle Label { get; private set; }
        internal GUIStyle ToolbarLabel { get; private set; }
        internal GUIStyle Heading { get; private set; }
        internal GUIStyle Help { get; private set; }
        internal GUIStyle Warning { get; private set; }
        internal GUIStyle Button { get; private set; }
        internal GUIStyle PrimaryButton { get; private set; }
        internal GUIStyle Tab { get; private set; }
        internal GUIStyle SelectedTab { get; private set; }
        internal GUIStyle SecondaryTab { get; private set; }
        internal GUIStyle SelectedSecondaryTab { get; private set; }
        internal GUIStyle Disclosure { get; private set; }
        internal GUIStyle TextField { get; private set; }
        internal GUIStyle Toggle { get; private set; }
        internal GUIStyle Slider { get; private set; }
        internal GUIStyle SliderThumb { get; private set; }
        internal GUIStyle ValueLabel { get; private set; }
        internal GUIStyle SelectedSegment { get; private set; }
        internal GUIStyle StatusBadge { get; private set; }

        internal void Ensure(GUISkin skin)
        {
            if (_skin == skin && Surface != null)
            {
                return;
            }

            Release();
            _skin = skin;
            Build(skin ?? GUI.skin);
        }

        public void Dispose()
        {
            Release();
            _skin = null;
        }

        private void Build(GUISkin skin)
        {
            _textures = new QuickSettingsTextureCatalog();

            Surface = QuickSettingsStyleFactory.CreatePanel(
                skin.box,
                _textures.Surface,
                QuickSettingsTheme.Metrics.ContentPadding,
                QuickSettingsTheme.Metrics.ContentPadding,
                QuickSettingsTheme.Metrics.ContentPadding,
                QuickSettingsTheme.Metrics.ContentPadding,
                QuickSettingsTheme.Metrics.PanelSlice);
            Toolbar = QuickSettingsStyleFactory.CreatePanel(
                skin.box,
                _textures.ElevatedSurface,
                4,
                4,
                1,
                1,
                QuickSettingsTheme.Metrics.ControlSlice);
            Footer = QuickSettingsStyleFactory.CreatePanel(
                skin.box,
                _textures.ElevatedSurface,
                4,
                4,
                3,
                3,
                QuickSettingsTheme.Metrics.ControlSlice);
            PropertyRow = QuickSettingsStyleFactory.CreatePanel(
                skin.box,
                _textures.PropertyRow,
                5,
                5,
                1,
                1,
                QuickSettingsTheme.Metrics.ControlSlice);

            Label = new GUIStyle(skin.label);
            QuickSettingsStyleFactory.ConfigureText(
                Label,
                QuickSettingsTheme.Colors.PrimaryText);
            Label.alignment = TextAnchor.MiddleLeft;
            Label.padding = new RectOffset(2, 2, 1, 1);

            ToolbarLabel = new GUIStyle(Label);
            ToolbarLabel.alignment = TextAnchor.MiddleCenter;
            ToolbarLabel.clipping = TextClipping.Clip;
            ToolbarLabel.wordWrap = false;
            ToolbarLabel.stretchWidth = true;

            Help = new GUIStyle(Label);
            Help.fontSize = Math.Max(11, Label.fontSize - 1);
            Help.fontStyle = FontStyle.Normal;
            Help.normal.textColor = QuickSettingsTheme.Colors.SecondaryText;
            Help.wordWrap = true;
            Help.margin = new RectOffset(5, 5, 2, 4);

            Warning = new GUIStyle(Help);
            Warning.normal.background = _textures.WarningSurface;
            Warning.normal.textColor = QuickSettingsTheme.Colors.WarningText;
            Warning.padding = new RectOffset(7, 7, 4, 4);
            Warning.margin = new RectOffset(3, 3, 3, 4);
            Warning.border = QuickSettingsStyleFactory.CreateBorder(
                QuickSettingsTheme.Metrics.ControlSlice);

            Heading = new GUIStyle(Label);
            Heading.normal.background = _textures.Section;
            Heading.normal.textColor =
                QuickSettingsTheme.Colors.PrimaryText;
            Heading.fontStyle = FontStyle.Bold;
            Heading.fixedHeight = QuickSettingsTheme.Metrics.SectionHeight;
            Heading.padding = new RectOffset(8, 6, 2, 2);
            Heading.margin = new RectOffset(0, 0, 4, 1);
            Heading.border = QuickSettingsStyleFactory.CreateBorder(
                QuickSettingsTheme.Metrics.ControlSlice);

            Button = QuickSettingsStyleFactory.CreateButton(
                skin.button,
                _textures.Button,
                _textures.ButtonHover,
                _textures.ButtonPressed,
                QuickSettingsTheme.Colors.PrimaryText);
            PrimaryButton = QuickSettingsStyleFactory.CreateButton(
                skin.button,
                _textures.Selected,
                _textures.SelectedHover,
                _textures.Accent,
                QuickSettingsTheme.Colors.PrimaryText);
            PrimaryButton.fontStyle = FontStyle.Bold;

            Tab = QuickSettingsStyleFactory.CreateButton(
                skin.button,
                _textures.Button,
                _textures.ButtonHover,
                _textures.ButtonPressed,
                QuickSettingsTheme.Colors.SecondaryText);
            Tab.fixedHeight = QuickSettingsTheme.Metrics.TabHeight;
            SelectedTab = QuickSettingsStyleFactory.CreateButton(
                skin.button,
                _textures.Selected,
                _textures.SelectedHover,
                _textures.ButtonPressed,
                QuickSettingsTheme.Colors.PrimaryText);
            SelectedTab.fixedHeight = QuickSettingsTheme.Metrics.TabHeight;
            SelectedTab.fontStyle = FontStyle.Bold;

            SecondaryTab = new GUIStyle(Tab);
            SecondaryTab.fixedHeight = QuickSettingsTheme.Metrics.RowHeight;
            SecondaryTab.fontSize = Math.Max(11, Tab.fontSize - 1);
            SelectedSecondaryTab = new GUIStyle(SelectedTab);
            SelectedSecondaryTab.fixedHeight =
                QuickSettingsTheme.Metrics.RowHeight;
            SelectedSecondaryTab.fontSize =
                Math.Max(11, SelectedTab.fontSize - 1);

            Disclosure = QuickSettingsStyleFactory.CreateButton(
                skin.button,
                _textures.Section,
                _textures.SectionHover,
                _textures.ButtonPressed,
                QuickSettingsTheme.Colors.PrimaryText);
            Disclosure.alignment = TextAnchor.MiddleLeft;
            Disclosure.fontStyle = FontStyle.Bold;
            Disclosure.fixedHeight =
                QuickSettingsTheme.Metrics.SectionHeight;
            Disclosure.padding = new RectOffset(8, 6, 2, 2);
            Disclosure.margin = new RectOffset(0, 0, 4, 1);

            TextField = new GUIStyle(skin.textField);
            QuickSettingsStyleFactory.ConfigureInput(
                TextField,
                _textures.Input,
                _textures.InputFocused);

            Toggle = new GUIStyle(skin.toggle);
            QuickSettingsStyleFactory.ConfigureText(
                Toggle,
                QuickSettingsTheme.Colors.PrimaryText);
            Toggle.alignment = TextAnchor.MiddleLeft;
            Toggle.padding = new RectOffset(
                Math.Max(20, Toggle.padding.left),
                3,
                1,
                1);

            Slider = new GUIStyle(skin.horizontalSlider);
            QuickSettingsStyleFactory.ConfigureSliderTrack(
                Slider,
                _textures.SliderTrack);
            Slider.fixedHeight = 8f;
            Slider.margin = new RectOffset(4, 4, 8, 5);
            SliderThumb = new GUIStyle(skin.horizontalSliderThumb);
            QuickSettingsStyleFactory.ConfigureSliderThumb(
                SliderThumb,
                _textures.Accent,
                _textures.SelectedHover,
                _textures.ButtonPressed);

            ValueLabel = new GUIStyle(Label);
            ValueLabel.normal.background = _textures.Input;
            ValueLabel.alignment = TextAnchor.MiddleCenter;
            ValueLabel.fixedHeight = 20f;
            ValueLabel.padding = new RectOffset(3, 3, 1, 1);
            ValueLabel.border = QuickSettingsStyleFactory.CreateBorder(
                QuickSettingsTheme.Metrics.ControlSlice);

            SelectedSegment = new GUIStyle(SelectedSecondaryTab);
            StatusBadge = new GUIStyle(Label);
            StatusBadge.normal.background = _textures.Selected;
            StatusBadge.normal.textColor =
                QuickSettingsTheme.Colors.PrimaryText;
            StatusBadge.alignment = TextAnchor.MiddleCenter;
            StatusBadge.fontStyle = FontStyle.Bold;
            StatusBadge.fixedHeight = 20f;
            StatusBadge.padding = new RectOffset(6, 6, 1, 1);
            StatusBadge.margin = new RectOffset(3, 3, 2, 2);
            StatusBadge.border = QuickSettingsStyleFactory.CreateBorder(
                QuickSettingsTheme.Metrics.ControlSlice);
        }

        private void Release()
        {
            if (_textures != null)
            {
                _textures.Dispose();
                _textures = null;
            }

            Surface = null;
            Toolbar = null;
            Footer = null;
            PropertyRow = null;
            Label = null;
            ToolbarLabel = null;
            Heading = null;
            Help = null;
            Warning = null;
            Button = null;
            PrimaryButton = null;
            Tab = null;
            SelectedTab = null;
            SecondaryTab = null;
            SelectedSecondaryTab = null;
            Disclosure = null;
            TextField = null;
            Toggle = null;
            Slider = null;
            SliderThumb = null;
            ValueLabel = null;
            SelectedSegment = null;
            StatusBadge = null;
        }
    }
}
