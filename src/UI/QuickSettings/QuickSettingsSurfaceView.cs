using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class QuickSettingsSurfaceView
    {
        private const int PanelDepth = -1000;

        private static readonly GUILayoutOption[] ExpandHeightOptions =
        {
            GUILayout.ExpandHeight(true)
        };
        private static readonly GUILayoutOption[] SelectorButtonOptions =
        {
            GUILayout.Width(32f)
        };

        private readonly float _headerHeight;
        private readonly CharacterSelectionModel _selection;
        private readonly LiveStatusPresenter _statusPresenter;
        private readonly MotionSettingsView _motionView;
        private readonly IrisSettingsView _irisView;
        private readonly VisibilitySettingsView _visibilityView;
        private readonly ExpressionWorkspaceView _expressionWorkspace;
        private readonly QuickSettingsNavigationView _navigation;
        private readonly QuickSettingsFooterView _footer;

        private Vector2 _scrollPosition;
        private string _navigationFeedback = string.Empty;

        internal QuickSettingsSurfaceView(
            float headerHeight,
            CharacterSelectionModel selection,
            LiveStatusPresenter statusPresenter,
            MotionSettingsView motionView,
            IrisSettingsView irisView,
            ExpressionSettingsView expressionView,
            VisibilitySettingsView visibilityView,
            ExpressionLinkEditorView expressionLinkEditor,
            Action apply,
            Action discard,
            Action writeDiagnostics,
            Func<bool> hasUnsavedSettings,
            Action close)
        {
            _headerHeight = Mathf.Max(0f, headerHeight);
            _selection = Require(selection, "selection");
            _statusPresenter = Require(statusPresenter, "statusPresenter");
            _motionView = Require(motionView, "motionView");
            _irisView = Require(irisView, "irisView");
            _visibilityView = Require(visibilityView, "visibilityView");
            _expressionWorkspace = new ExpressionWorkspaceView(
                Require(expressionLinkEditor, "expressionLinkEditor"),
                Require(expressionView, "expressionView"));
            _navigation = new QuickSettingsNavigationView();
            _footer = new QuickSettingsFooterView(
                Require(apply, "apply"),
                Require(discard, "discard"),
                Require(writeDiagnostics, "writeDiagnostics"),
                Require(hasUnsavedSettings, "hasUnsavedSettings"),
                Require(close, "close"));
        }

        internal void Draw(QuickSettingsWindowBounds bounds)
        {
            float headerHeight = Mathf.Min(
                _headerHeight,
                Mathf.Max(0f, bounds.Height));
            Rect bodyBounds = new Rect(
                bounds.X,
                bounds.Y + headerHeight,
                bounds.Width,
                Mathf.Max(1f, bounds.Height - headerHeight));
            int previousDepth = GUI.depth;
            try
            {
                GUI.depth = PanelDepth;
                QuickSettingsGui.BeginSurfaceArea(bodyBounds);
                try
                {
                    DrawContent(bodyBounds.width);
                }
                finally
                {
                    QuickSettingsGui.EndSurfaceArea();
                }
            }
            finally
            {
                GUI.depth = previousDepth;
            }
        }

        private void DrawContent(float width)
        {
            QuickSettingsGui.BeginVertical();
            try
            {
                bool compactLayout = width < 420f;
                EyeMotionCharacterController controller =
                    DrawControllerSelector();
                bool preventLeavingCurrentView =
                    _navigation.Destination ==
                        QuickSettingsDestination.Expressions &&
                    _expressionWorkspace.HasUnsavedChanges;
                if (!preventLeavingCurrentView)
                {
                    _navigationFeedback = string.Empty;
                }

                if (_navigation.Draw(
                        preventLeavingCurrentView,
                        HandleNavigationBlocked))
                {
                    _scrollPosition = Vector2.zero;
                    _navigationFeedback = string.Empty;
                }

                if (_navigationFeedback.Length > 0)
                {
                    QuickSettingsGui.Warning(_navigationFeedback);
                }

                _scrollPosition = GUILayout.BeginScrollView(
                    _scrollPosition,
                    ExpandHeightOptions);
                try
                {
                    LiveStatusSnapshot status =
                        _statusPresenter.GetSnapshot(controller);
                    DrawSelectedView(
                        controller,
                        status,
                        compactLayout);
                    _footer.DrawTroubleshooting();
                }
                finally
                {
                    GUILayout.EndScrollView();
                }

                if (_footer.DrawActions(true, compactLayout))
                {
                    _scrollPosition = new Vector2(
                        0f,
                        float.MaxValue);
                }
            }
            finally
            {
                QuickSettingsGui.EndVertical();
            }
        }

        private EyeMotionCharacterController DrawControllerSelector()
        {
            EyeMotionCharacterController controller = _selection.Resolve();
            QuickSettingsGui.BeginToolbar();
            if (QuickSettingsGui.Button("<", SelectorButtonOptions))
            {
                SelectController(-1);
                controller = _selection.Resolve();
            }

            LiveStatusSnapshot status =
                _statusPresenter.GetSnapshot(controller);
            QuickSettingsGui.ToolbarLabel(status.SelectedCharacterLine);

            if (QuickSettingsGui.Button(">", SelectorButtonOptions))
            {
                SelectController(1);
                controller = _selection.Resolve();
            }

            QuickSettingsGui.EndToolbar();
            return controller;
        }

        private void SelectController(int direction)
        {
            if (_expressionWorkspace.HasUnsavedChanges)
            {
                _navigationFeedback =
                    "Save or cancel the current expression edits first.";
                _expressionWorkspace.RejectControllerChange();
                return;
            }

            _navigationFeedback = string.Empty;
            GUIUtility.keyboardControl = 0;
            if (direction < 0)
            {
                _selection.SelectPrevious();
            }
            else
            {
                _selection.SelectNext();
            }
        }

        private void DrawSelectedView(
            EyeMotionCharacterController controller,
            LiveStatusSnapshot status,
            bool compactLayout)
        {
            switch (_navigation.Destination)
            {
                case QuickSettingsDestination.EyeSize:
                    _irisView.Draw(status);
                    return;
                case QuickSettingsDestination.Expressions:
                    _expressionWorkspace.Draw(controller);
                    return;
                case QuickSettingsDestination.Visibility:
                    _visibilityView.Draw(
                        controller,
                        status,
                        compactLayout);
                    return;
                default:
                    _motionView.Draw(
                        controller,
                        status,
                        compactLayout);
                    return;
            }
        }

        private void HandleNavigationBlocked()
        {
            _expressionWorkspace.RejectNavigationChange();
            _navigationFeedback =
                "Save or cancel the current expression edits first.";
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
