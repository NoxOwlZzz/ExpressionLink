using System;
using UnityEngine;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal enum QuickSettingsDestination
    {
        EyeTracking,
        EyeSize,
        Expressions,
        Visibility
    }

    internal sealed class QuickSettingsNavigationView
    {
        private const int EyesPage = 0;
        private const int ExpressionsPage = 1;
        private const int VisibilityPage = 2;

        private const int TrackingSection = 0;
        private const int SizeSection = 1;

        private int _selectedPage = EyesPage;
        private int _selectedEyesSection = TrackingSection;

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
                    return QuickSettingsDestination.Expressions;
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
                QuickSettingsDestination.Expressions,
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
                changed |= DrawEyesSectionButton(
                    TrackingSection,
                    "Tracking",
                    QuickSettingsDestination.EyeTracking,
                    preventLeavingCurrentView,
                    navigationBlocked);
                changed |= DrawEyesSectionButton(
                    SizeSection,
                    "Size controls",
                    QuickSettingsDestination.EyeSize,
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

        private bool DrawEyesSectionButton(
            int section,
            string label,
            QuickSettingsDestination destination,
            bool preventLeavingCurrentView,
            Action navigationBlocked)
        {
            bool selected = _selectedEyesSection == section;
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

            _selectedEyesSection = section;
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

        private string GetDescription()
        {
            switch (Destination)
            {
                case QuickSettingsDestination.EyeSize:
                    return "Connect the game's eye sliders to your custom eye shapes.";
                case QuickSettingsDestination.Expressions:
                    return "Make a blendshape react to a facial expression.";
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
