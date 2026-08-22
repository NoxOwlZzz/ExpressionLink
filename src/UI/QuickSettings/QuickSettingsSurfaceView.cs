using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsSurfaceView
    {
        private const int PanelDepth = -1000;
        private const float FixedVerticalContentHeight = 126f;
        private const int MotionTab = 0;
        private const int IrisTab = 1;
        private const int ExpressionsTab = 2;
        private const int VisibilityTab = 3;
        private const int LinksTab = 4;

        private static readonly GUILayoutOption[] SelectorButtonOptions =
        {
            GUILayout.Width(32f)
        };

        private readonly CharacterSelectionModel _selection;
        private readonly LiveStatusPresenter _statusPresenter;
        private readonly MotionSettingsView _motionView;
        private readonly IrisSettingsView _irisView;
        private readonly ExpressionSettingsView _expressionView;
        private readonly VisibilitySettingsView _visibilityView;
        private readonly ExpressionLinkEditorView _expressionLinkEditor;
        private readonly Action _apply;
        private readonly Action _reload;
        private readonly Action _writeDiagnostics;
        private readonly Action _close;
        private readonly GUIContent _windowContent;
        private readonly GUILayoutOption[] _scrollViewOptions =
            new GUILayoutOption[1];

        private Vector2 _scrollPosition;
        private float _scrollViewOptionHeight = float.NaN;
        private int _selectedTab;

        internal QuickSettingsSurfaceView(
            string windowTitle,
            CharacterSelectionModel selection,
            LiveStatusPresenter statusPresenter,
            MotionSettingsView motionView,
            IrisSettingsView irisView,
            ExpressionSettingsView expressionView,
            VisibilitySettingsView visibilityView,
            ExpressionLinkEditorView expressionLinkEditor,
            Action apply,
            Action reload,
            Action writeDiagnostics,
            Action close)
        {
            _selection = Require(selection, "selection");
            _statusPresenter = Require(statusPresenter, "statusPresenter");
            _motionView = Require(motionView, "motionView");
            _irisView = Require(irisView, "irisView");
            _expressionView = Require(expressionView, "expressionView");
            _visibilityView = Require(visibilityView, "visibilityView");
            _expressionLinkEditor = Require(
                expressionLinkEditor,
                "expressionLinkEditor");
            _apply = Require(apply, "apply");
            _reload = Require(reload, "reload");
            _writeDiagnostics = Require(
                writeDiagnostics,
                "writeDiagnostics");
            _close = Require(close, "close");
            _windowContent = new GUIContent(windowTitle ?? string.Empty);
        }

        internal void Draw(QuickSettingsWindowBounds bounds)
        {
            Rect guiBounds = new Rect(
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height);
            int previousDepth = GUI.depth;
            try
            {
                GUI.depth = PanelDepth;
                GUILayout.BeginArea(
                    guiBounds,
                    _windowContent,
                    GUI.skin.window);
                try
                {
                    DrawContent(bounds);
                }
                finally
                {
                    GUILayout.EndArea();
                }
            }
            finally
            {
                GUI.depth = previousDepth;
            }
        }

        private void DrawContent(QuickSettingsWindowBounds bounds)
        {
            QuickSettingsGui.BeginVertical();
            EyeMotionCharacterController controller = DrawControllerSelector();
            DrawTabs();

            _scrollPosition = GUILayout.BeginScrollView(
                _scrollPosition,
                GetScrollViewOptions(bounds.Height));
            DrawSelectedTab(
                controller,
                _statusPresenter.GetSnapshot(controller),
                bounds.Width < 520f);
            GUILayout.EndScrollView();

            DrawFooter();
            QuickSettingsGui.EndVertical();
        }

        private EyeMotionCharacterController DrawControllerSelector()
        {
            EyeMotionCharacterController controller = _selection.Resolve();
            QuickSettingsGui.BeginHorizontal();
            if (GUILayout.Button("<", SelectorButtonOptions))
            {
                SelectController(-1);
                controller = _selection.Resolve();
            }

            LiveStatusSnapshot status =
                _statusPresenter.GetSnapshot(controller);
            QuickSettingsGui.Label(status.SelectedCharacterLine);

            if (GUILayout.Button(">", SelectorButtonOptions))
            {
                SelectController(1);
                controller = _selection.Resolve();
            }

            QuickSettingsGui.EndHorizontal();
            return controller;
        }

        private void SelectController(int direction)
        {
            if (_expressionLinkEditor.HasUnsavedChanges)
            {
                _expressionLinkEditor.RejectControllerChange();
                _selectedTab = LinksTab;
                _scrollPosition = Vector2.zero;
                return;
            }

            if (direction < 0)
            {
                _selection.SelectPrevious();
            }
            else
            {
                _selection.SelectNext();
            }
        }

        private void DrawTabs()
        {
            QuickSettingsGui.BeginHorizontal();
            DrawTabButton(MotionTab, "Motion");
            DrawTabButton(IrisTab, "Iris");
            DrawTabButton(ExpressionsTab, "Expressions");
            DrawTabButton(VisibilityTab, "Visibility");
            DrawTabButton(LinksTab, "Links");
            QuickSettingsGui.EndHorizontal();
        }

        private void DrawTabButton(int tab, string label)
        {
            string text = _selectedTab == tab
                ? "[" + label + "]"
                : label;
            if (!QuickSettingsGui.Button(text) || _selectedTab == tab)
            {
                return;
            }

            _selectedTab = tab;
            _scrollPosition = Vector2.zero;
        }

        private void DrawSelectedTab(
            EyeMotionCharacterController controller,
            LiveStatusSnapshot status,
            bool compactLayout)
        {
            switch (_selectedTab)
            {
                case IrisTab:
                    _irisView.Draw(status);
                    return;
                case ExpressionsTab:
                    _expressionView.Draw(controller);
                    return;
                case VisibilityTab:
                    _visibilityView.Draw(
                        controller,
                        status,
                        compactLayout);
                    return;
                case LinksTab:
                    _expressionLinkEditor.Draw(controller);
                    return;
                default:
                    _motionView.Draw(controller, status, compactLayout);
                    return;
            }
        }

        private void DrawFooter()
        {
            QuickSettingsGui.BeginHorizontal();
            if (_selectedTab != LinksTab)
            {
                if (QuickSettingsGui.Button("Apply"))
                {
                    _apply();
                }

                if (QuickSettingsGui.Button("Reload"))
                {
                    _reload();
                }
            }

            if (QuickSettingsGui.Button("Diagnostics"))
            {
                _writeDiagnostics();
            }

            if (QuickSettingsGui.Button("Close"))
            {
                _close();
            }

            QuickSettingsGui.EndHorizontal();
        }

        private GUILayoutOption[] GetScrollViewOptions(float windowHeight)
        {
            float height = Mathf.Max(
                80f,
                windowHeight - FixedVerticalContentHeight);
            if (_scrollViewOptionHeight != height)
            {
                _scrollViewOptionHeight = height;
                _scrollViewOptions[0] = GUILayout.Height(height);
            }

            return _scrollViewOptions;
        }

        private static T Require<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }
    }
}
