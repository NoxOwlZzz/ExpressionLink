using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal enum QuickSettingsDestination
    {
        EyeTracking,
        EyeSize,
        AutomaticExpressions,
        CustomLinks,
        Visibility
    }

    internal sealed class QuickSettingsNavigationView
    {
        private const int EyesPage = 0;
        private const int ExpressionsPage = 1;
        private const int VisibilityPage = 2;

        private const int TrackingSection = 0;
        private const int SizeSection = 1;
        private const int AutomaticSection = 0;
        private const int LinksSection = 1;

        private int _selectedPage = EyesPage;
        private int _selectedEyesSection = TrackingSection;
        private int _selectedExpressionsSection = AutomaticSection;

        internal QuickSettingsDestination Destination
        {
            get
            {
                if (_selectedPage == VisibilityPage)
                {
                    return QuickSettingsDestination.Visibility;
                }

                if (_selectedPage == ExpressionsPage)
                {
                    return _selectedExpressionsSection == LinksSection
                        ? QuickSettingsDestination.CustomLinks
                        : QuickSettingsDestination.AutomaticExpressions;
                }

                return _selectedEyesSection == SizeSection
                    ? QuickSettingsDestination.EyeSize
                    : QuickSettingsDestination.EyeTracking;
            }
        }

        internal bool Draw(
            bool preventLeavingCurrentView,
            Action navigationBlocked)
        {
            bool changed = false;
            QuickSettingsGui.BeginHorizontal();
            changed |= DrawPrimaryButton(
                EyesPage,
                "Eyes",
                GetEyesDestination(),
                preventLeavingCurrentView,
                navigationBlocked);
            changed |= DrawPrimaryButton(
                ExpressionsPage,
                "Expressions",
                GetExpressionsDestination(),
                preventLeavingCurrentView,
                navigationBlocked);
            changed |= DrawPrimaryButton(
                VisibilityPage,
                "Visibility",
                QuickSettingsDestination.Visibility,
                preventLeavingCurrentView,
                navigationBlocked);
            QuickSettingsGui.EndHorizontal();

            if (_selectedPage == EyesPage)
            {
                QuickSettingsGui.BeginHorizontal();
                changed |= DrawSecondaryButton(
                    TrackingSection,
                    "Tracking",
                    QuickSettingsDestination.EyeTracking,
                    true,
                    preventLeavingCurrentView,
                    navigationBlocked);
                changed |= DrawSecondaryButton(
                    SizeSection,
                    "Size controls",
                    QuickSettingsDestination.EyeSize,
                    true,
                    preventLeavingCurrentView,
                    navigationBlocked);
                QuickSettingsGui.EndHorizontal();
            }
            else if (_selectedPage == ExpressionsPage)
            {
                QuickSettingsGui.BeginHorizontal();
                changed |= DrawSecondaryButton(
                    AutomaticSection,
                    "Game expressions",
                    QuickSettingsDestination.AutomaticExpressions,
                    false,
                    preventLeavingCurrentView,
                    navigationBlocked);
                changed |= DrawSecondaryButton(
                    LinksSection,
                    "Custom links",
                    QuickSettingsDestination.CustomLinks,
                    false,
                    preventLeavingCurrentView,
                    navigationBlocked);
                QuickSettingsGui.EndHorizontal();
            }

            QuickSettingsGui.Help(GetDescription());
            return changed;
        }

        private bool DrawPrimaryButton(
            int page,
            string label,
            QuickSettingsDestination destination,
            bool preventLeavingCurrentView,
            Action navigationBlocked)
        {
            bool selected = _selectedPage == page;
            if (!QuickSettingsGui.TabButton(label, selected) || selected)
            {
                return false;
            }

            if (!CanNavigate(
                    destination,
                    preventLeavingCurrentView,
                    navigationBlocked))
            {
                return false;
            }

            _selectedPage = page;
            ClearKeyboardFocus();
            return true;
        }

        private bool DrawSecondaryButton(
            int section,
            string label,
            QuickSettingsDestination destination,
            bool eyesSection,
            bool preventLeavingCurrentView,
            Action navigationBlocked)
        {
            bool selected = eyesSection
                ? _selectedEyesSection == section
                : _selectedExpressionsSection == section;
            if (!QuickSettingsGui.SecondaryTabButton(label, selected) ||
                selected)
            {
                return false;
            }

            if (!CanNavigate(
                    destination,
                    preventLeavingCurrentView,
                    navigationBlocked))
            {
                return false;
            }

            if (eyesSection)
            {
                _selectedEyesSection = section;
            }
            else
            {
                _selectedExpressionsSection = section;
            }

            ClearKeyboardFocus();
            return true;
        }

        private bool CanNavigate(
            QuickSettingsDestination destination,
            bool preventLeavingCurrentView,
            Action navigationBlocked)
        {
            if (destination == Destination)
            {
                return false;
            }

            if (!preventLeavingCurrentView)
            {
                return true;
            }

            if (navigationBlocked != null)
            {
                navigationBlocked();
            }

            return false;
        }

        private QuickSettingsDestination GetEyesDestination()
        {
            return _selectedEyesSection == SizeSection
                ? QuickSettingsDestination.EyeSize
                : QuickSettingsDestination.EyeTracking;
        }

        private QuickSettingsDestination GetExpressionsDestination()
        {
            return _selectedExpressionsSection == LinksSection
                ? QuickSettingsDestination.CustomLinks
                : QuickSettingsDestination.AutomaticExpressions;
        }

        private string GetDescription()
        {
            switch (Destination)
            {
                case QuickSettingsDestination.EyeSize:
                    return "Connect the game's eye sliders to your custom eye shapes.";
                case QuickSettingsDestination.AutomaticExpressions:
                    return "Map game expressions to ExpressionMesh 01 through 04.";
                case QuickSettingsDestination.CustomLinks:
                    return "Drive a blendshape on the head, hair, body, clothes, or accessories.";
                case QuickSettingsDestination.Visibility:
                    return "Show or hide custom parts and follow Erase Highlight.";
                default:
                    return "Set up camera tracking, blinking, calibration, and smoothing.";
            }
        }

        private static void ClearKeyboardFocus()
        {
            GUIUtility.keyboardControl = 0;
        }
    }
}
