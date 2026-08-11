using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace NightOwlZzz.Koikatsu.EyeMotion
{
    /// <summary>
    /// Optional, allocation-free frame-loop bridge to KK_ExpressionControl.
    /// The external assembly is discovered at runtime, so EyeMotion keeps no
    /// compile-time or BepInEx dependency on ExpressionControl.
    /// </summary>
    internal sealed class ExpressionControlEyeSource
    {
        private delegate object StaticObjectGetter();
        private delegate IList ListGetter(object instance);
        private delegate object ObjectGetter(object instance);
        private delegate ChaControl CharacterGetter(object instance);
        private delegate float FloatGetter(object instance);

        private const int DiscoveryRetrySamples = 256;
        private const string PluginTypeName =
            "ExpressionControl.ExpressionControlPlugin";

        private static RuntimeAccessors _runtime;
        private static int _discoveryRetryCountdown;

        private readonly ChaControl _character;
        private IList _cachedFemaleList;
        private object _cachedFemaleData;
        private int _cachedFemaleIndex = -1;

        private ExpressionControlEyeSource(ChaControl character)
        {
            _character = character;
        }

        internal static ExpressionControlEyeSource Resolve(
            ChaControl character)
        {
            return new ExpressionControlEyeSource(character);
        }

        internal bool RuntimeAvailable
        {
            get { return EnsureRuntimeAvailable(); }
        }

        internal bool CharacterAvailable
        {
            get { return _cachedFemaleData != null; }
        }

        internal bool Available
        {
            get
            {
                return _runtime != null &&
                       _cachedFemaleData != null;
            }
        }

        internal string Status
        {
            get
            {
                if (_runtime == null)
                {
                    return "Plugin unavailable";
                }

                if (_cachedFemaleData == null)
                {
                    return "Waiting for character";
                }

                return "Ready";
            }
        }

        /// <summary>
        /// Reads ExpressionControl's live IrisY and Size values for this
        /// character. Once runtime discovery succeeds, this method performs
        /// no reflection, boxing, LINQ, or managed allocation.
        /// </summary>
        internal bool TrySample(out float irisY, out float irisSize)
        {
            irisY = 0f;
            irisSize = 0f;

            if (_character == null || !EnsureRuntimeAvailable())
            {
                ClearCachedCharacter();
                return false;
            }

            RuntimeAccessors runtime = _runtime;
            try
            {
                object instance = runtime.GetInstance();
                if (instance == null)
                {
                    ClearCachedCharacter();
                    return false;
                }

                IList femaleList = runtime.GetFemaleList(instance);
                object femaleData = FindFemaleData(runtime, femaleList);
                if (femaleData == null)
                {
                    return false;
                }

                object expression = runtime.GetRealtimeExpression(femaleData);
                if (expression == null)
                {
                    return false;
                }

                irisY = runtime.GetEyeY(expression);
                irisSize = runtime.GetEyeSmall(expression);
                return true;
            }
            catch (Exception)
            {
                // ExpressionControl may recreate its state while changing
                // scenes. Drop the per-character cache and retry safely on a
                // subsequent sample without disabling the rest of EyeMotion.
                ClearCachedCharacter();
                return false;
            }
        }

        private object FindFemaleData(
            RuntimeAccessors runtime,
            IList femaleList)
        {
            if (femaleList == null)
            {
                ClearCachedCharacter();
                return null;
            }

            if (ReferenceEquals(femaleList, _cachedFemaleList) &&
                _cachedFemaleData != null &&
                _cachedFemaleIndex >= 0 &&
                _cachedFemaleIndex < femaleList.Count)
            {
                object current = femaleList[_cachedFemaleIndex];
                if (ReferenceEquals(current, _cachedFemaleData) &&
                    ReferenceEquals(
                        runtime.GetCharacter(current),
                        _character))
                {
                    return current;
                }
            }

            for (int i = 0; i < femaleList.Count; i++)
            {
                object candidate = femaleList[i];
                if (candidate != null &&
                    ReferenceEquals(
                        runtime.GetCharacter(candidate),
                        _character))
                {
                    _cachedFemaleList = femaleList;
                    _cachedFemaleData = candidate;
                    _cachedFemaleIndex = i;
                    return candidate;
                }
            }

            ClearCachedCharacter();
            return null;
        }

        private void ClearCachedCharacter()
        {
            _cachedFemaleList = null;
            _cachedFemaleData = null;
            _cachedFemaleIndex = -1;
        }

        private static bool EnsureRuntimeAvailable()
        {
            if (_runtime != null)
            {
                return true;
            }

            if (_discoveryRetryCountdown > 0)
            {
                _discoveryRetryCountdown--;
                return false;
            }

            RuntimeAccessors discovered;
            if (TryDiscoverRuntime(out discovered))
            {
                _runtime = discovered;
                return true;
            }

            _discoveryRetryCountdown = DiscoveryRetrySamples;
            return false;
        }

        private static bool TryDiscoverRuntime(
            out RuntimeAccessors runtime)
        {
            runtime = null;
            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                Type pluginType = null;
                for (int i = 0; i < assemblies.Length; i++)
                {
                    pluginType = assemblies[i].GetType(
                        PluginTypeName,
                        false,
                        false);
                    if (pluginType != null)
                    {
                        break;
                    }
                }

                if (pluginType == null)
                {
                    return false;
                }

                BindingFlags staticFlags =
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic;
                BindingFlags instanceFlags =
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic;

                FieldInfo instanceField = pluginType.GetField(
                    "inst",
                    staticFlags);
                if (instanceField == null ||
                    instanceField.FieldType.IsValueType)
                {
                    return false;
                }

                Type controllerType = instanceField.FieldType;
                FieldInfo femaleListField = controllerType.GetField(
                    "femaleList",
                    instanceFlags);
                if (femaleListField == null ||
                    !typeof(IList).IsAssignableFrom(
                        femaleListField.FieldType))
                {
                    return false;
                }

                Type femaleDataType = GetListElementType(
                    femaleListField.FieldType);
                if (femaleDataType == null)
                {
                    return false;
                }

                FieldInfo characterField = femaleDataType.GetField(
                    "female",
                    instanceFlags);
                FieldInfo expressionField = femaleDataType.GetField(
                    "expressionRealtime",
                    instanceFlags);
                if (characterField == null ||
                    !typeof(ChaControl).IsAssignableFrom(
                        characterField.FieldType) ||
                    expressionField == null ||
                    expressionField.FieldType.IsValueType)
                {
                    return false;
                }

                Type expressionType = expressionField.FieldType;
                FieldInfo eyeYField = expressionType.GetField(
                    "eyeY",
                    instanceFlags);
                FieldInfo eyeSmallField = expressionType.GetField(
                    "eyeSmall",
                    instanceFlags);
                if (!IsSingleField(eyeYField) ||
                    !IsSingleField(eyeSmallField))
                {
                    return false;
                }

                runtime = new RuntimeAccessors(
                    CreateStaticObjectGetter(instanceField),
                    CreateListGetter(femaleListField),
                    CreateCharacterGetter(characterField),
                    CreateObjectGetter(expressionField),
                    CreateFloatGetter(eyeYField),
                    CreateFloatGetter(eyeSmallField));
                return true;
            }
            catch (Exception)
            {
                runtime = null;
                return false;
            }
        }

        private static Type GetListElementType(Type listType)
        {
            if (listType.IsGenericType)
            {
                Type[] arguments = listType.GetGenericArguments();
                if (arguments.Length == 1)
                {
                    return arguments[0];
                }
            }

            return null;
        }

        private static bool IsSingleField(FieldInfo field)
        {
            return field != null && field.FieldType == typeof(float);
        }

        private static StaticObjectGetter CreateStaticObjectGetter(
            FieldInfo field)
        {
            DynamicMethod method = CreateDynamicMethod(
                "EyeMotion_GetExpressionControlInstance",
                typeof(object),
                Type.EmptyTypes,
                field.DeclaringType);
            ILGenerator il = method.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, field);
            il.Emit(OpCodes.Ret);
            return (StaticObjectGetter)method.CreateDelegate(
                typeof(StaticObjectGetter));
        }

        private static ListGetter CreateListGetter(FieldInfo field)
        {
            DynamicMethod method = CreateDynamicMethod(
                "EyeMotion_GetExpressionControlFemaleList",
                typeof(IList),
                new Type[] { typeof(object) },
                field.DeclaringType);
            ILGenerator il = method.GetILGenerator();
            EmitInstanceFieldLoad(il, field);
            il.Emit(OpCodes.Castclass, typeof(IList));
            il.Emit(OpCodes.Ret);
            return (ListGetter)method.CreateDelegate(typeof(ListGetter));
        }

        private static CharacterGetter CreateCharacterGetter(
            FieldInfo field)
        {
            DynamicMethod method = CreateDynamicMethod(
                "EyeMotion_GetExpressionControlCharacter",
                typeof(ChaControl),
                new Type[] { typeof(object) },
                field.DeclaringType);
            ILGenerator il = method.GetILGenerator();
            EmitInstanceFieldLoad(il, field);
            il.Emit(OpCodes.Ret);
            return (CharacterGetter)method.CreateDelegate(
                typeof(CharacterGetter));
        }

        private static ObjectGetter CreateObjectGetter(FieldInfo field)
        {
            DynamicMethod method = CreateDynamicMethod(
                "EyeMotion_GetExpressionControlRealtimeExpression",
                typeof(object),
                new Type[] { typeof(object) },
                field.DeclaringType);
            ILGenerator il = method.GetILGenerator();
            EmitInstanceFieldLoad(il, field);
            il.Emit(OpCodes.Ret);
            return (ObjectGetter)method.CreateDelegate(typeof(ObjectGetter));
        }

        private static FloatGetter CreateFloatGetter(FieldInfo field)
        {
            DynamicMethod method = CreateDynamicMethod(
                "EyeMotion_GetExpressionControlValue_" + field.Name,
                typeof(float),
                new Type[] { typeof(object) },
                field.DeclaringType);
            ILGenerator il = method.GetILGenerator();
            EmitInstanceFieldLoad(il, field);
            il.Emit(OpCodes.Ret);
            return (FloatGetter)method.CreateDelegate(typeof(FloatGetter));
        }

        private static DynamicMethod CreateDynamicMethod(
            string name,
            Type returnType,
            Type[] parameterTypes,
            Type owner)
        {
            return new DynamicMethod(
                name,
                returnType,
                parameterTypes,
                owner,
                true);
        }

        private static void EmitInstanceFieldLoad(
            ILGenerator il,
            FieldInfo field)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, field.DeclaringType);
            il.Emit(OpCodes.Ldfld, field);
        }

        private sealed class RuntimeAccessors
        {
            internal readonly StaticObjectGetter GetInstance;
            internal readonly ListGetter GetFemaleList;
            internal readonly CharacterGetter GetCharacter;
            internal readonly ObjectGetter GetRealtimeExpression;
            internal readonly FloatGetter GetEyeY;
            internal readonly FloatGetter GetEyeSmall;

            internal RuntimeAccessors(
                StaticObjectGetter getInstance,
                ListGetter getFemaleList,
                CharacterGetter getCharacter,
                ObjectGetter getRealtimeExpression,
                FloatGetter getEyeY,
                FloatGetter getEyeSmall)
            {
                GetInstance = getInstance;
                GetFemaleList = getFemaleList;
                GetCharacter = getCharacter;
                GetRealtimeExpression = getRealtimeExpression;
                GetEyeY = getEyeY;
                GetEyeSmall = getEyeSmall;
            }
        }
    }
}
