// MD Vertex Measure - classify piping / steel / equipment from names
// Author: MD (Mayank Dholariya)
using System;

namespace VertexMeasure.Core
{
    public static class CategoryHints
    {
        private static readonly string[] PipingTokens = {
            "PIPE", "ELBOW", "BEND", "TEE", "REDUC", "FLANGE", "VALVE", "GASKET",
            "CAP", "COUPLING", "OLET", "WELD", "SPOOL", "PIPING", "FITTING",
            "BRANCH", "CROSS", "UNION", "NIPPLE", "TRAP", "STRAINER", "INSTRUMENT"
        };

        private static readonly string[] SteelTokens = {
            "BEAM", "COLUMN", "STEEL", "MEMBER", "BRACE", "GIRDER", "PURLIN",
            "CHANNEL", "ANGLE", "PFC", "UB ", "UC ", "UB-", "UC-", "WIDE FLANGE",
            "HSS", "CHS", "RHS", "SHS", "PLATE", "GRATING", "HANDRAIL", "LADDER",
            "STRUCT", "W-SHAPE", "IPE", "HEA", "HEB"
        };

        private static readonly string[] EquipmentTokens = {
            "EQUIP", "VESSEL", "PUMP", "TANK", "EXCHANGER", "COMPRESSOR", "DRUM",
            "STACK", "COLUMN", "REACTOR", "FILTER", "PACKAGE", "SKID", "HEATER",
            "COOLER", "FAN", "BLOWER", "AGITATOR", "MIXER", "SILO", "HOPPER"
        };

        public static ItemCategory Classify(string displayName, string className)
        {
            string text = ((displayName ?? "") + " " + (className ?? "")).ToUpperInvariant();
            // Piping first (more specific fittings)
            if (ContainsAny(text, PipingTokens)) return ItemCategory.Piping;
            // Steel vs equipment: "COLUMN" appears in both — prefer steel if beam-like tokens else equipment
            bool steel = ContainsAny(text, SteelTokens);
            bool equip = ContainsAny(text, EquipmentTokens);
            if (steel && !equip) return ItemCategory.Steel;
            if (equip && !steel) return ItemCategory.Equipment;
            if (steel && equip)
            {
                // Distillation COLUMN vs structural COLUMN: if also vessel/tower-ish keep equipment
                if (text.Contains("VESSEL") || text.Contains("TOWER") || text.Contains("REACTOR") || text.Contains("DISTILL"))
                    return ItemCategory.Equipment;
                return ItemCategory.Steel;
            }
            return ItemCategory.Unknown;
        }

        private static bool ContainsAny(string text, string[] tokens)
        {
            foreach (string t in tokens)
                if (text.IndexOf(t, StringComparison.Ordinal) >= 0) return true;
            return false;
        }
    }
}
