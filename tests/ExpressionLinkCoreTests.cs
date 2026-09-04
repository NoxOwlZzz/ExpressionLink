using System;
using System.Collections.Generic;

namespace NightOwlZzz.Koikatsu.EyeMotion.Tests
{
    internal static class ExpressionLinkCoreTests
    {
        private static int _checks;
        private static int _failures;

        internal static int Run()
        {
            int ignored;
            return Run(out ignored);
        }

        internal static int Run(out int checks)
        {
            _checks = 0;
            _failures = 0;

            TestDefaultsAndValidation();
            TestEvaluator();
            TestConflictResolution();
            TestCodecRoundTrip();
            TestCodecCorruption();
            TestLimits();

            checks = _checks;
            return _failures;
        }

        private static void TestDefaultsAndValidation()
        {
            ExpressionLinkDefinition link = ExpressionLinkDefinition.CreateDefault();
            Check("default ID", link.Id != Guid.Empty);
            Check("default enabled", link.Enabled);
            Check("default scope", link.Scope == ExpressionTargetScope.Any);
            Check("default slot", link.SlotIndex == -1);
            Check("default component", link.ComponentIndex == -1);
            Check("default mode", link.Mode == ExpressionLinkMode.Binary);
            Near("default threshold", 0.001f, link.Threshold);
            Near("default input minimum", 0f, link.InputMin);
            Near("default input maximum", 1f, link.InputMax);
            Near("default output minimum", 0f, link.OutputMin);
            Near("default output maximum", 100f, link.OutputMax);

            link.Name = "  Ears  ";
            link.Source = "  eyes:4  ";
            link.BlendshapeName = "  ears_happy  ";
            ExpressionLinkDefinition normalized;
            string error;
            Check(
                "valid link normalizes",
                ExpressionLinkValidator.TryNormalize(
                    link,
                    out normalized,
                    out error));
            Check("normalization error empty", string.IsNullOrEmpty(error));
            Check("name trimmed", normalized.Name == "Ears");
            Check("source trimmed", normalized.Source == "eyes:4");
            Check("blendshape trimmed", normalized.BlendshapeName == "ears_happy");
            Check("normalization clones", !object.ReferenceEquals(link, normalized));

            ExpressionLinkDefinition clone = normalized.Clone();
            clone.Name = "Changed";
            Check("clone independent", normalized.Name == "Ears");

            ExpressionLinkDefinition emptyName = ExpressionLinkDefinition.CreateDefault();
            emptyName.Name = "  ";
            Check(
                "empty name receives default",
                ExpressionLinkValidator.TryNormalize(
                    emptyName,
                    out normalized,
                    out error) &&
                normalized.Name == "Expression Link");
        }

        private static void TestEvaluator()
        {
            ExpressionLinkDefinition binary = ExpressionLinkDefinition.CreateDefault();
            binary.Threshold = 0.25f;
            binary.OutputMin = 10f;
            binary.OutputMax = 80f;
            Near("binary inactive", 10f, ExpressionLinkEvaluator.Evaluate(binary, 0.1f));
            Near("binary threshold is inactive", 10f, ExpressionLinkEvaluator.Evaluate(binary, 0.25f));
            Near("binary active", 80f, ExpressionLinkEvaluator.Evaluate(binary, 0.251f));
            Near("invalid source inactive", 10f, ExpressionLinkEvaluator.Evaluate(binary, float.NaN));

            ExpressionLinkDefinition follow = ExpressionLinkDefinition.CreateDefault();
            follow.Mode = ExpressionLinkMode.FollowSource;
            follow.InputMin = 0.2f;
            follow.InputMax = 0.8f;
            follow.OutputMin = 20f;
            follow.OutputMax = 80f;
            Near("follow lower clamp", 20f, ExpressionLinkEvaluator.Evaluate(follow, 0f));
            Near("follow midpoint", 50f, ExpressionLinkEvaluator.Evaluate(follow, 0.5f));
            Near("follow upper clamp", 80f, ExpressionLinkEvaluator.Evaluate(follow, 1f));

            follow.OutputMin = 100f;
            follow.OutputMax = 0f;
            Near("follow inverted midpoint", 50f, ExpressionLinkEvaluator.Evaluate(follow, 0.5f));
            Near("follow inverted maximum", 0f, ExpressionLinkEvaluator.Evaluate(follow, 1f));
        }

        private static void TestConflictResolution()
        {
            Check(
                "higher priority wins",
                ExpressionLinkConflictResolver.CandidateWins(1, 100f, 2, 1f));
            Check(
                "lower priority loses",
                !ExpressionLinkConflictResolver.CandidateWins(2, 1f, 1, 100f));
            Check(
                "higher weight breaks priority tie",
                ExpressionLinkConflictResolver.CandidateWins(2, 20f, 2, 21f));
            Check(
                "equal conflict remains stable",
                !ExpressionLinkConflictResolver.CandidateWins(2, 20f, 2, 20f));
        }

        private static void TestCodecRoundTrip()
        {
            ExpressionLinkDefinition first = ExpressionLinkDefinition.CreateDefault();
            first.Name = "Happy ears";
            first.Source = "eyes:4";
            first.Scope = ExpressionTargetScope.Hair;
            first.SlotIndex = 2;
            first.RendererPath = "chaF_001/ct_hairB/hair_mesh";
            first.ComponentIndex = 1;
            first.RendererHint = "hair_mesh";
            first.MeshHint = "hair_mesh.asset";
            first.BlendshapeName = "ears_happy";
            first.Mode = ExpressionLinkMode.FollowSource;
            first.Threshold = 0.1f;
            first.InputMin = 0.2f;
            first.InputMax = 0.8f;
            first.OutputMin = 12.5f;
            first.OutputMax = 87.5f;
            first.SmoothingSpeed = 18f;
            first.Priority = 12;

            ExpressionLinkDefinition second = ExpressionLinkDefinition.CreateDefault();
            second.Enabled = false;
            second.Name = "Blink target";
            second.Source = "eyes:0";
            second.Scope = ExpressionTargetScope.Head;
            second.BlendshapeName = "blink_extra";
            second.OutputMin = 100f;
            second.OutputMax = 0f;
            second.Priority = -4;

            byte[] encoded = ExpressionLinkBinaryCodec.Encode(
                new ExpressionLinkDefinition[] { first, second });
            Check("codec emits payload", encoded.Length > 8);

            ExpressionLinkDefinition[] decoded;
            string error;
            Check(
                "codec round-trip decodes",
                ExpressionLinkBinaryCodec.TryDecode(
                    encoded,
                    out decoded,
                    out error));
            Check("codec round-trip error empty", string.IsNullOrEmpty(error));
            Check("codec round-trip count", decoded.Length == 2);
            Check("codec ID", decoded[0].Id == first.Id);
            Check("codec name", decoded[0].Name == first.Name);
            Check("codec source", decoded[0].Source == first.Source);
            Check("codec scope", decoded[0].Scope == first.Scope);
            Check("codec slot", decoded[0].SlotIndex == first.SlotIndex);
            Check("codec path", decoded[0].RendererPath == first.RendererPath);
            Check("codec component", decoded[0].ComponentIndex == first.ComponentIndex);
            Check("codec renderer hint", decoded[0].RendererHint == first.RendererHint);
            Check("codec mesh hint", decoded[0].MeshHint == first.MeshHint);
            Check("codec blendshape", decoded[0].BlendshapeName == first.BlendshapeName);
            Check("codec mode", decoded[0].Mode == first.Mode);
            Near("codec threshold", first.Threshold, decoded[0].Threshold);
            Near("codec input minimum", first.InputMin, decoded[0].InputMin);
            Near("codec input maximum", first.InputMax, decoded[0].InputMax);
            Near("codec output minimum", first.OutputMin, decoded[0].OutputMin);
            Near("codec output maximum", first.OutputMax, decoded[0].OutputMax);
            Near("codec smoothing", first.SmoothingSpeed, decoded[0].SmoothingSpeed);
            Check("codec priority", decoded[0].Priority == first.Priority);
            Check("codec disabled flag", !decoded[1].Enabled);
            Check("codec negative priority", decoded[1].Priority == -4);

            byte[] empty = ExpressionLinkBinaryCodec.Encode(null);
            Check(
                "empty codec payload",
                ExpressionLinkBinaryCodec.TryDecode(empty, out decoded, out error) &&
                decoded.Length == 0);
        }

        private static void TestCodecCorruption()
        {
            ExpressionLinkDefinition first = ExpressionLinkDefinition.CreateDefault();
            first.Name = "A";
            ExpressionLinkDefinition second = first.Clone();
            second.Id = Guid.NewGuid();
            second.Name = "B";
            byte[] valid = ExpressionLinkBinaryCodec.Encode(
                new ExpressionLinkDefinition[] { first, second });

            ExpressionLinkDefinition[] decoded;
            string error;
            Check(
                "null payload rejected",
                !ExpressionLinkBinaryCodec.TryDecode(null, out decoded, out error));
            Check(
                "empty payload rejected",
                !ExpressionLinkBinaryCodec.TryDecode(
                    new byte[0], out decoded, out error));

            byte[] corrupt = Copy(valid);
            corrupt[0] = (byte)'X';
            Check(
                "bad magic rejected",
                !ExpressionLinkBinaryCodec.TryDecode(corrupt, out decoded, out error));

            corrupt = Copy(valid);
            corrupt[4] = 99;
            Check(
                "future version rejected",
                !ExpressionLinkBinaryCodec.TryDecode(corrupt, out decoded, out error));

            corrupt = Copy(valid);
            corrupt[5] = 1;
            Check(
                "unknown header flags rejected",
                !ExpressionLinkBinaryCodec.TryDecode(corrupt, out decoded, out error));

            corrupt = Copy(valid);
            corrupt[29] = 128;
            Check(
                "unknown record flags rejected",
                !ExpressionLinkBinaryCodec.TryDecode(corrupt, out decoded, out error));

            byte[] truncated = new byte[valid.Length - 1];
            Array.Copy(valid, truncated, truncated.Length);
            Check(
                "truncated record rejected",
                !ExpressionLinkBinaryCodec.TryDecode(truncated, out decoded, out error));

            byte[] trailing = new byte[valid.Length + 1];
            Array.Copy(valid, trailing, valid.Length);
            Check(
                "trailing byte rejected",
                !ExpressionLinkBinaryCodec.TryDecode(trailing, out decoded, out error));

            corrupt = Copy(valid);
            int firstRecordLength = BitConverter.ToInt32(corrupt, 8);
            int secondRecordStart = 12 + firstRecordLength;
            int firstGuidStart = 13;
            int secondGuidStart = secondRecordStart + 5;
            Array.Copy(corrupt, firstGuidStart, corrupt, secondGuidStart, 16);
            Check(
                "duplicate IDs rejected on decode",
                !ExpressionLinkBinaryCodec.TryDecode(corrupt, out decoded, out error));

            corrupt = Copy(valid);
            corrupt[8] = 1;
            corrupt[9] = 0;
            corrupt[10] = 0;
            corrupt[11] = 0;
            Check(
                "undersized record rejected",
                !ExpressionLinkBinaryCodec.TryDecode(corrupt, out decoded, out error));
        }

        private static void TestLimits()
        {
            ExpressionLinkDefinition invalid = ExpressionLinkDefinition.CreateDefault();
            ExpressionLinkDefinition normalized;
            string error;

            invalid.Id = Guid.Empty;
            Check(
                "empty ID rejected",
                !ExpressionLinkValidator.TryNormalize(
                    invalid,
                    out normalized,
                    out error));

            invalid = ExpressionLinkDefinition.CreateDefault();
            invalid.Threshold = float.NaN;
            Check(
                "NaN threshold rejected",
                !ExpressionLinkValidator.TryNormalize(
                    invalid,
                    out normalized,
                    out error));

            invalid = ExpressionLinkDefinition.CreateDefault();
            invalid.InputMin = 0.8f;
            invalid.InputMax = 0.2f;
            Check(
                "reversed input range rejected",
                !ExpressionLinkValidator.TryNormalize(
                    invalid,
                    out normalized,
                    out error));

            invalid = ExpressionLinkDefinition.CreateDefault();
            invalid.OutputMax = 101f;
            Check(
                "weight outside range rejected",
                !ExpressionLinkValidator.TryNormalize(
                    invalid,
                    out normalized,
                    out error));

            invalid = ExpressionLinkDefinition.CreateDefault();
            invalid.Name = new string('a', ExpressionLinkLimits.MaximumNameBytes + 1);
            Check(
                "oversized name rejected",
                !ExpressionLinkValidator.TryNormalize(
                    invalid,
                    out normalized,
                    out error));

            invalid = ExpressionLinkDefinition.CreateDefault();
            invalid.Source = "\ud800";
            Check(
                "invalid Unicode rejected",
                !ExpressionLinkValidator.TryNormalize(
                    invalid,
                    out normalized,
                    out error));

            List<ExpressionLinkDefinition> tooMany =
                new List<ExpressionLinkDefinition>();
            for (int i = 0; i <= ExpressionLinkLimits.MaximumLinks; i++)
            {
                tooMany.Add(ExpressionLinkDefinition.CreateDefault());
            }

            ExpressionLinkDefinition[] normalizedList;
            Check(
                "too many links rejected",
                !ExpressionLinkValidator.TryNormalizeList(
                    tooMany,
                    out normalizedList,
                    out error));

            ExpressionLinkDefinition duplicate = ExpressionLinkDefinition.CreateDefault();
            Check(
                "duplicate IDs rejected by list validator",
                !ExpressionLinkValidator.TryNormalizeList(
                    new ExpressionLinkDefinition[]
                    {
                        duplicate,
                        duplicate.Clone()
                    },
                    out normalizedList,
                    out error));

            bool encodeRejected = false;
            try
            {
                ExpressionLinkBinaryCodec.Encode(
                    new ExpressionLinkDefinition[]
                    {
                        duplicate,
                        duplicate.Clone()
                    });
            }
            catch (ArgumentException)
            {
                encodeRejected = true;
            }

            Check("duplicate IDs rejected by encoder", encodeRejected);

            List<ExpressionLinkDefinition> oversizedForCard =
                new List<ExpressionLinkDefinition>();
            int oversizedLinkCount =
                (ExpressionLinkLimits.MaximumPayloadBytes /
                    ExpressionLinkLimits.MaximumRendererPathBytes) + 1;
            for (int i = 0; i < oversizedLinkCount; i++)
            {
                ExpressionLinkDefinition candidate =
                    ExpressionLinkDefinition.CreateDefault();
                candidate.RendererPath = new string(
                    'r',
                    ExpressionLinkLimits.MaximumRendererPathBytes);
                oversizedForCard.Add(candidate);
            }

            Check(
                "individually valid links can exceed card payload",
                ExpressionLinkValidator.TryNormalizeList(
                    oversizedForCard,
                    out normalizedList,
                    out error));

            byte[] oversizedEncoding;
            bool oversizedEncodingRejected =
                !ExpressionLinkBinaryCodec.TryEncode(
                    oversizedForCard,
                    out oversizedEncoding,
                    out error);
            Check(
                "encoder reports an oversized aggregate payload",
                oversizedEncodingRejected &&
                oversizedEncoding.Length == 0 &&
                error.IndexOf("maximum size", StringComparison.Ordinal) >= 0);

            ExpressionLinkDefinition[] decoded;
            Check(
                "oversized payload rejected before parsing",
                !ExpressionLinkBinaryCodec.TryDecode(
                    new byte[ExpressionLinkLimits.MaximumPayloadBytes + 1],
                    out decoded,
                    out error));
        }

        private static byte[] Copy(byte[] source)
        {
            byte[] copy = new byte[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }

        private static void Near(string name, float expected, float actual)
        {
            _checks++;
            if (Math.Abs(expected - actual) <= 0.0001f)
            {
                return;
            }

            _failures++;
            Console.Error.WriteLine(
                "FAIL " + name + ": expected " + expected +
                ", actual " + actual);
        }

        private static void Check(string name, bool condition)
        {
            _checks++;
            if (condition)
            {
                return;
            }

            _failures++;
            Console.Error.WriteLine("FAIL " + name);
        }
    }
}
