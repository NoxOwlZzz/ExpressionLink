using System;

namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class SliderHighlightRendererOwnershipTests
    {
        private static int _checks;
        private static int _failures;

        internal static int Run(out int checks)
        {
            _checks = 0;
            _failures = 0;
            try
            {
                TestMetadataContract();
                TestOwnership();
            }
            catch (Exception exception)
            {
                _failures++;
                Console.Error.WriteLine(
                    "FAIL SliderHighlight renderer ownership suite: " +
                    exception.GetBaseException().Message);
            }
            finally
            {
                ValidOwner.Set(null, null);
            }

            checks = _checks;
            return _failures;
        }

        private static void TestMetadataContract()
        {
            Type rendererType = typeof(FakeRenderer);
            Check("null plugin type is unavailable",
                SliderHighlightRendererOwnership.TryCreate(null, rendererType) == null);
            Check("null renderer type is unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(ValidOwner), null) == null);
            Check("missing fields are unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(object), rendererType) == null);
            Check("missing body field is unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(FaceOnlyOwner), rendererType) == null);
            Check("missing face field is unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(BodyOnlyOwner), rendererType) == null);
            Check("wrong face field type is unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(WrongFaceTypeOwner), rendererType) == null);
            Check("wrong body field type is unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(WrongBodyTypeOwner), rendererType) == null);
            Check("public face field is unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(PublicFaceOwner), rendererType) == null);
            Check("public body field is unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(PublicBodyOwner), rendererType) == null);
            Check("instance face field is unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(InstanceFaceOwner), rendererType) == null);
            Check("instance body field is unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(InstanceBodyOwner), rendererType) == null);
            Check("inherited fields are unavailable",
                SliderHighlightRendererOwnership.TryCreate(typeof(InheritedOwner), rendererType) == null);
            Check("field type must match the renderer type exactly",
                SliderHighlightRendererOwnership.TryCreate(typeof(ValidOwner), typeof(object)) == null);
        }

        private static void TestOwnership()
        {
            SliderHighlightRendererOwnership ownership =
                SliderHighlightRendererOwnership.TryCreate(
                    typeof(ValidOwner), typeof(FakeRenderer));
            Check("declared private static renderer fields are supported", ownership != null);
            if (ownership == null)
            {
                return;
            }

            FakeRenderer face = new FakeRenderer("Highlight_cf_O_face_rend");
            FakeRenderer body = new FakeRenderer("Highlight_o_body_a_rend");
            FakeRenderer real = new FakeRenderer("cf_O_face");
            FakeRenderer matchingName = new FakeRenderer(face.Name);
            ValidOwner.Set(face, body);
            Check("face owner fixture is initialized", ReferenceEquals(ValidOwner.Face, face));
            Check("body owner fixture is initialized", ReferenceEquals(ValidOwner.Body, body));
            Check("owned face is excluded from automatic discovery",
                ownership.ShouldExclude(face, false, false));
            Check("owned body is excluded from automatic discovery",
                ownership.ShouldExclude(body, false, false));
            Check("real renderer remains eligible",
                !ownership.ShouldExclude(real, false, false));
            Check("matching renderer name is not ownership",
                !ownership.ShouldExclude(matchingName, false, false));
            Check("fixture equality differs from reference identity", face.Equals(real));
            Check("value equality is not ownership",
                !ownership.ShouldExclude(real, false, false));
            Check("null renderer remains eligible",
                !ownership.ShouldExclude(null, false, false));
            Check("unrelated object remains eligible",
                !ownership.ShouldExclude(new object(), false, false));
            Check("explicit path preserves owned face selection",
                !ownership.ShouldExclude(face, true, false));
            Check("explicit name preserves owned face selection",
                !ownership.ShouldExclude(face, false, true));
            Check("both explicit selectors preserve owned face selection",
                !ownership.ShouldExclude(face, true, true));
            Check("explicit path preserves owned body selection",
                !ownership.ShouldExclude(body, true, false));
            Check("explicit name preserves owned body selection",
                !ownership.ShouldExclude(body, false, true));

            FakeRenderer nextFace = new FakeRenderer(face.Name);
            FakeRenderer nextBody = new FakeRenderer(body.Name);
            ValidOwner.Set(nextFace, nextBody);
            Check("replacement face is read on each query",
                ownership.ShouldExclude(nextFace, false, false));
            Check("replacement body is read on each query",
                ownership.ShouldExclude(nextBody, false, false));
            Check("released face remains eligible",
                !ownership.ShouldExclude(face, false, false));
            Check("released body remains eligible",
                !ownership.ShouldExclude(body, false, false));

            ValidOwner.Set(null, nextBody);
            Check("null face owner releases face exclusion",
                !ownership.ShouldExclude(nextFace, false, false));
            Check("body ownership is independent of a null face owner",
                ownership.ShouldExclude(nextBody, false, false));
            ValidOwner.Set(nextFace, null);
            Check("null body owner releases body exclusion",
                !ownership.ShouldExclude(nextBody, false, false));
            Check("face ownership is independent of a null body owner",
                ownership.ShouldExclude(nextFace, false, false));
            ValidOwner.Set(null, null);
            Check("null owners never exclude a null renderer",
                !ownership.ShouldExclude(null, false, false));
            Check("null owners leave renderers eligible",
                !ownership.ShouldExclude(real, false, false));
        }

        private static void Check(string name, bool condition)
        {
            _checks++;
            if (!condition)
            {
                _failures++;
                Console.Error.WriteLine("FAIL SliderHighlight ownership: " + name);
            }
        }

        private sealed class FakeRenderer
        {
            internal readonly string Name;

            internal FakeRenderer(string name) { Name = name; }
            public override bool Equals(object obj) { return obj is FakeRenderer; }
            public override int GetHashCode() { return 0; }
        }

        private class ValidOwner
        {
            private static FakeRenderer _smrFac;
            private static FakeRenderer _smrBod;
            internal static FakeRenderer Face { get { return _smrFac; } }
            internal static FakeRenderer Body { get { return _smrBod; } }

            internal static void Set(FakeRenderer face, FakeRenderer body)
            {
                _smrFac = face;
                _smrBod = body;
            }
        }

        private sealed class InheritedOwner : ValidOwner { }

        private sealed class FaceOnlyOwner
        {
            private static FakeRenderer _smrFac = null;
            internal static object[] Fields { get { return new object[] { _smrFac }; } }
        }

        private sealed class BodyOnlyOwner
        {
            private static FakeRenderer _smrBod = null;
            internal static object[] Fields { get { return new object[] { _smrBod }; } }
        }

        private sealed class WrongFaceTypeOwner
        {
            private static object _smrFac = null;
            private static FakeRenderer _smrBod = null;
            internal static object[] Fields { get { return new object[] { _smrFac, _smrBod }; } }
        }

        private sealed class WrongBodyTypeOwner
        {
            private static FakeRenderer _smrFac = null;
            private static object _smrBod = null;
            internal static object[] Fields { get { return new object[] { _smrFac, _smrBod }; } }
        }

        private sealed class PublicFaceOwner
        {
            public static FakeRenderer _smrFac = null;
            private static FakeRenderer _smrBod = null;
            internal static object[] Fields { get { return new object[] { _smrFac, _smrBod }; } }
        }

        private sealed class PublicBodyOwner
        {
            private static FakeRenderer _smrFac = null;
            public static FakeRenderer _smrBod = null;
            internal static object[] Fields { get { return new object[] { _smrFac, _smrBod }; } }
        }

        private sealed class InstanceFaceOwner
        {
            private FakeRenderer _smrFac = null;
            private static FakeRenderer _smrBod = null;
            internal object[] Fields { get { return new object[] { _smrFac, _smrBod }; } }
        }

        private sealed class InstanceBodyOwner
        {
            private static FakeRenderer _smrFac = null;
            private FakeRenderer _smrBod = null;
            internal object[] Fields { get { return new object[] { _smrFac, _smrBod }; } }
        }
    }
}
