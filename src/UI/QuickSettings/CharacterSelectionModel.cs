using System;
using System.Collections.Generic;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    internal sealed class CharacterSelectionModel
    {
        private EyeMotionCharacterController _current;
        private int _selectedInstanceId;

        internal event Action<EyeMotionCharacterController, EyeMotionCharacterController>
            SelectionChanged;

        internal EyeMotionCharacterController Current
        {
            get { return Resolve(); }
        }

        internal EyeMotionCharacterController Resolve()
        {
            return Resolve(EyeMotionCharacterController.ActiveControllers);
        }

        internal EyeMotionCharacterController Resolve(
            IList<EyeMotionCharacterController> controllers)
        {
            if (controllers == null || controllers.Count == 0)
            {
                SetCurrent(null);
                return null;
            }

            EyeMotionCharacterController first = null;
            EyeMotionCharacterController firstBound = null;
            EyeMotionCharacterController selected = null;
            for (int i = 0; i < controllers.Count; i++)
            {
                EyeMotionCharacterController controller = controllers[i];
                if (controller == null)
                {
                    continue;
                }

                if (first == null)
                {
                    first = controller;
                }

                if (firstBound == null &&
                    controller.CurrentBindingState == BindingState.Bound)
                {
                    firstBound = controller;
                }

                if (_selectedInstanceId != 0 &&
                    controller.GetInstanceID() == _selectedInstanceId)
                {
                    selected = controller;
                }
            }

            SetCurrent(selected ?? firstBound ?? first);
            return _current;
        }

        internal EyeMotionCharacterController SelectPrevious()
        {
            return Move(-1, EyeMotionCharacterController.ActiveControllers);
        }

        internal EyeMotionCharacterController SelectNext()
        {
            return Move(1, EyeMotionCharacterController.ActiveControllers);
        }

        internal EyeMotionCharacterController Move(
            int direction,
            IList<EyeMotionCharacterController> controllers)
        {
            EyeMotionCharacterController current = Resolve(controllers);
            if (current == null || controllers == null || controllers.Count == 0)
            {
                return null;
            }

            int currentIndex = FindIndex(controllers, _selectedInstanceId);
            int step = direction < 0 ? -1 : 1;
            int count = controllers.Count;
            for (int offset = 1; offset <= count; offset++)
            {
                int index = WrapIndex(currentIndex + (step * offset), count);
                EyeMotionCharacterController candidate = controllers[index];
                if (candidate != null)
                {
                    SetCurrent(candidate);
                    return _current;
                }
            }

            return _current;
        }

        internal void Clear()
        {
            SetCurrent(null);
        }

        private void SetCurrent(EyeMotionCharacterController controller)
        {
            int instanceId = controller == null ? 0 : controller.GetInstanceID();
            EyeMotionCharacterController previous = _current;
            bool changed = instanceId != _selectedInstanceId;

            _current = controller;
            _selectedInstanceId = instanceId;
            if (changed && SelectionChanged != null)
            {
                SelectionChanged(previous, controller);
            }
        }

        private static int FindIndex(
            IList<EyeMotionCharacterController> controllers,
            int instanceId)
        {
            for (int i = 0; i < controllers.Count; i++)
            {
                EyeMotionCharacterController controller = controllers[i];
                if (controller != null && controller.GetInstanceID() == instanceId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int WrapIndex(int index, int count)
        {
            while (index < 0)
            {
                index += count;
            }

            return index % count;
        }
    }
}
