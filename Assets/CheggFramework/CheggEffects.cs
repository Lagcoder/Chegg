using System;
using System.Collections.Generic;

namespace Chegg.Framework
{
    public enum EffectKind
    {
        Poison,
        Fire,
        Slowness,
        Weakness,
        Hunger,
        BadOmen,
        Stunned,
        Speed,
        Strength,
        Resistance,
        WaterBreathing
    }

    public enum EffectPolarity
    {
        Negative,
        Positive
    }

    public enum EffectRemovalRule
    {
        DurationEnds,
        EnterWaterOrDurationEnds,
        EnterOwnSpawnZone,
        AfterOneUse
    }

    [Serializable]
    public sealed class EffectDefinition
    {
        public EffectKind Kind;
        public EffectPolarity Polarity;
        public EffectRemovalRule RemovalRule;
        public int DefaultDurationTurns;
        public string MechanicDescription = string.Empty;
    }

    [Serializable]
    public sealed class EffectInstance
    {
        public EffectKind Kind;
        public int RemainingTurns;
        public bool Consumed;

        public EffectInstance(EffectKind kind, int durationTurns)
        {
            Kind = kind;
            RemainingTurns = durationTurns;
        }
    }

    public static class CheggEffectCatalog
    {
        public static readonly Dictionary<EffectKind, EffectDefinition> Definitions = Build();

        private static Dictionary<EffectKind, EffectDefinition> Build()
        {
            return new Dictionary<EffectKind, EffectDefinition>
            {
                { EffectKind.Poison, new EffectDefinition
                    {
                        Kind = EffectKind.Poison,
                        Polarity = EffectPolarity.Negative,
                        RemovalRule = EffectRemovalRule.DurationEnds,
                        DefaultDurationTurns = 1,
                        MechanicDescription = "Minion is removed at the end of its next turn."
                    }
                },
                { EffectKind.Fire, new EffectDefinition
                    {
                        Kind = EffectKind.Fire,
                        Polarity = EffectPolarity.Negative,
                        RemovalRule = EffectRemovalRule.EnterWaterOrDurationEnds,
                        DefaultDurationTurns = 2,
                        MechanicDescription = "Minion is removed after 2 turns unless it enters water."
                    }
                },
                { EffectKind.Slowness, new EffectDefinition
                    {
                        Kind = EffectKind.Slowness,
                        Polarity = EffectPolarity.Negative,
                        RemovalRule = EffectRemovalRule.DurationEnds,
                        DefaultDurationTurns = 1,
                        MechanicDescription = "Minion cannot Dash while active."
                    }
                },
                { EffectKind.Weakness, new EffectDefinition
                    {
                        Kind = EffectKind.Weakness,
                        Polarity = EffectPolarity.Negative,
                        RemovalRule = EffectRemovalRule.DurationEnds,
                        DefaultDurationTurns = 1,
                        MechanicDescription = "Minion cannot Attack while active."
                    }
                },
                { EffectKind.Hunger, new EffectDefinition
                    {
                        Kind = EffectKind.Hunger,
                        Polarity = EffectPolarity.Negative,
                        RemovalRule = EffectRemovalRule.EnterOwnSpawnZone,
                        DefaultDurationTurns = 0,
                        MechanicDescription = "Minion cannot use free move until it re-enters own spawn zone."
                    }
                },
                { EffectKind.BadOmen, new EffectDefinition
                    {
                        Kind = EffectKind.BadOmen,
                        Polarity = EffectPolarity.Negative,
                        RemovalRule = EffectRemovalRule.EnterOwnSpawnZone,
                        DefaultDurationTurns = 0,
                        MechanicDescription = "Minion cannot put king in check or kill king until it re-enters own spawn zone."
                    }
                },
                { EffectKind.Stunned, new EffectDefinition
                    {
                        Kind = EffectKind.Stunned,
                        Polarity = EffectPolarity.Negative,
                        RemovalRule = EffectRemovalRule.DurationEnds,
                        DefaultDurationTurns = 1,
                        MechanicDescription = "Minion loses entire turn."
                    }
                },
                { EffectKind.Speed, new EffectDefinition
                    {
                        Kind = EffectKind.Speed,
                        Polarity = EffectPolarity.Positive,
                        RemovalRule = EffectRemovalRule.DurationEnds,
                        DefaultDurationTurns = 1,
                        MechanicDescription = "Minion may use free move twice each turn."
                    }
                },
                { EffectKind.Strength, new EffectDefinition
                    {
                        Kind = EffectKind.Strength,
                        Polarity = EffectPolarity.Positive,
                        RemovalRule = EffectRemovalRule.DurationEnds,
                        DefaultDurationTurns = 1,
                        MechanicDescription = "Minion may attack twice each turn."
                    }
                },
                { EffectKind.Resistance, new EffectDefinition
                    {
                        Kind = EffectKind.Resistance,
                        Polarity = EffectPolarity.Positive,
                        RemovalRule = EffectRemovalRule.AfterOneUse,
                        DefaultDurationTurns = 0,
                        MechanicDescription = "Negates next lethal enemy hit and is consumed."
                    }
                },
                { EffectKind.WaterBreathing, new EffectDefinition
                    {
                        Kind = EffectKind.WaterBreathing,
                        Polarity = EffectPolarity.Positive,
                        RemovalRule = EffectRemovalRule.DurationEnds,
                        DefaultDurationTurns = 1,
                        MechanicDescription = "Immune to drowning and drowning turn-skip while active."
                    }
                }
            };
        }
    }
}
