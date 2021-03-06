using System;

namespace Rawr.Tree {

    public abstract class Spell {
        protected float minHeal             = 0f;
        public float    MinHeal     { get { return healModifier * (minHeal + spellPower * coefDH); } }
        public float    BaseMinHeal { get { return minHeal; } }

        protected float maxHeal             = 0f;
        public float    MaxHeal     { get { return healModifier * (maxHeal + spellPower * coefDH); } }
        public float    BaseMaxHeal { get { return maxHeal; } }

        protected int numberOfCasts = 1;
        public float NumberOfCasts { get { return numberOfCasts; } }

        public float castTime = 0f;
        public float castTimeBeforeHaste = 0f;
        public float CastTime { 
            get {
                if (castTime > gcd) { return numberOfCasts * castTime;
                } else if (gcd > 1) { return numberOfCasts * gcd;
                } else {              return numberOfCasts; }
            }
        }
        public float gcd            = 1.5f;
        public float gcdBeforeHaste = 1.5f;

        protected float manaCost    = 0f;
        virtual public float ManaCost { get { return manaCost * numberOfCasts; } }

        public float coefDH  = 0f; //coef for DirectHeal
        public float coefHoT = 0f; //coef for HoT
        protected float spellPower = 0f;
        public float SpellPower { get { return spellPower; } }
        public float speed = 1f;
        protected float critModifier = 1.5f;
        public float CritModifier { get { return critModifier; } }
        protected float critPercent = 0f;
        public float CritPercent { get { return critPercent; } }
        protected float critHoTPercent = 0f;
        public float CritHoTPercent { get { return critHoTPercent; } }
        protected float periodicTick = 0f;
        protected float periodicTicks = 0f;
        public float PeriodicTicks { get { return periodicTicks; } }
        protected float periodicTickTimeBeforeHaste = 3f; // this is only for rejuvenation at the moment
        protected float periodicTickTime = 3f;
        public float PeriodicTickTime { get { return periodicTickTime; } }

        protected float extraHealing = 0f; // for BonusHoTOnDirectHeals
        protected float healModifier = 1f; // for BonusHealingDoneMultiplier

        virtual public float AverageHealing { get { return healModifier * (extraHealing + (minHeal + maxHeal) / 2f + SpellPower * coefDH); } }

        /// <summary>
        ///  Direct heal component of this spell
        /// </summary>
        public float AverageHealingwithCrit { get { return (AverageHealing * (100f - CritPercent) + (AverageHealing * CritModifier) * CritPercent) / 100f; } }

        virtual public float PeriodicTick { get { return healModifier * (periodicTick + spellPower * coefHoT); } }

        virtual public float PeriodicTickwithCrit { get { return (PeriodicTick * (100f - critHoTPercent) + (PeriodicTick * CritModifier) * critHoTPercent) / 100f; } }

        /// <summary>
        /// Total healing generated by this spell, includes base, crit and HoT effects 
        /// </summary>
        public float TotalAverageHealing { get { return (AverageHealingwithCrit + PeriodicTickwithCrit * PeriodicTicks); } }

        public float Duration { get { return periodicTicks * periodicTickTime; } }

        // HPS: The healing per second you will get when either chaincasting or refreshing perfectly
        // HPS_DH: Direct Healing divided per cast time
        //         If you want to know the chaincast healing, use HPCT_DH
        // HPS_HOT: Rate at which HoT healing is applied
        //          Different from the rate at which it can be generated by the druid (see HPCT)
        public float HPS { get { return TotalAverageHealing / Math.Max(CastTime, Duration); } }
        public float HPS_DH { get { return AverageHealingwithCrit / Math.Max(CastTime, Duration); } }
        public float HPS_HOT { get { return periodicTickTime > 0 ? PeriodicTickwithCrit / periodicTickTime : 0; } }

        public float HPM { get { return TotalAverageHealing / ManaCost; } }
        public float HPM_DH { get { return AverageHealingwithCrit / ManaCost; } }
        public float HPM_HOT { get { return PeriodicTickwithCrit * PeriodicTicks / ManaCost; } }

        // Wildebees: 20090221 : Healing per cast time, considers direct healing and HoT parts
        //     Total healing divided by CastTime
        // This indicates the rate at which the healing is generated
        public float HPCT { get { return TotalAverageHealing / CastTime; } }
        public float HPCT_DH { get { return AverageHealingwithCrit / CastTime; } }
        public float HPCT_HOT { get { return PeriodicTickwithCrit * PeriodicTicks / CastTime; } }

        public float HPSPM { get { return HPS / ManaCost; } }
        public float HPSPM_DH { get { return HPS_DH / ManaCost; } }
        public float HPSPM_HOT { get { return HPS_HOT / ManaCost; } }

        public float HPCTPM { get { return HPCT / ManaCost; } }
        public float HPCTPM_DH { get { return HPCT_DH / ManaCost; } }
        public float HPCTPM_HOT { get { return HPCT_HOT / ManaCost; } }

        public virtual void applyStats(Stats stats)
        {
            speed = (1 + StatConversion.GetSpellHasteFromRating(stats.HasteRating)) * (1 + stats.SpellHaste);
            critModifier = 1.5f * (1f + stats.BonusCritHealMultiplier);
            healModifier = (1f + stats.BonusHealingDoneMultiplier);
            extraHealing = stats.BonusHealingReceived;
            applyHaste();
        }
        
        protected virtual void applyHaste() {
            gcd = gcdBeforeHaste / speed;
            if (gcd < 1f) { gcd = 1f; }
            castTime = (float)Math.Round(castTimeBeforeHaste / speed, 4);
        }

        protected virtual void applyHasteToPeriodicTickTime() {
            // Does Nature's Grace affect the duration of Rejuvenate?
            // It says: "casting speed" so I expect not...
            periodicTickTime = periodicTickTimeBeforeHaste / speed;
        }

        public override string ToString()
        {
            return ""+Math.Round(AverageHealingwithCrit, 0) + "*" + Math.Round(MinHeal, 0) + " - " + Math.Round(MaxHeal, 0) + " normal\n" + Math.Round(MinHeal * CritModifier, 0) + " - " + Math.Round(MaxHeal * CritModifier, 0) + " crit\n" + Math.Round(CritPercent, 2) + "% crit chance\n" + (PeriodicTick>0?Math.Round(PeriodicTick, 0)+" every "+Math.Round(PeriodicTickTime, 2)+" s.":"");
        }

    }
    public class HealingTouch : Spell {
        protected float manaCostModifier = 1f;
        protected float extraCritPercent = 0f;
        protected float extraCritModifier = 0f;
        public HealingTouch(Character character, Stats calculatedStats)
        {
            CalculationOptionsTree calcOpts = (CalculationOptionsTree)character.CalculationOptions;

            #region Base Values
            castTimeBeforeHaste = 3f;
            coefDH = 1.62f;
            minHeal = 3750f;
            maxHeal = 4428f;
            #endregion

            calculateTalents(character.DruidTalents, calcOpts);

            #region Glyph of Healing Touch
            if (character.DruidTalents.GlyphOfHealingTouch) {
                castTimeBeforeHaste -= 1.5f;
                manaCostModifier *= 1 - 0.25f;
                minHeal *= 1 - 0.5f;
                maxHeal *= 1 - 0.5f;
                coefDH *= 1 - 0.5f;
            }
            #endregion

            applyStats(calculatedStats);
        }
        public override void applyStats(Stats stats)
        {
            base.applyStats(stats);
            spellPower = stats.SpellPower + stats.HealingTouchFinalHealBonus; // Idol
            critPercent = stats.SpellCrit + extraCritPercent;
            critModifier += extraCritModifier;
            manaCost = 0.33f * TreeConstants.BaseMana - stats.SpellsManaReduction;
            manaCost *= manaCostModifier;
        }
        private void calculateTalents(DruidTalents druidTalents, CalculationOptionsTree calcOpts) {
            manaCostModifier *= 1 - (druidTalents.Moonglow * 0.03f + druidTalents.TranquilSpirit * 0.02f);

            castTimeBeforeHaste -= 0.1f * druidTalents.Naturalist;

            extraCritPercent += 2f * druidTalents.NaturesMajesty;

            //Living Seed, 30% seed, 33% * points spend (1/3)
            extraCritModifier = 0.1f * druidTalents.LivingSeed * calcOpts.Current.LivingSeedEfficiency / 100f;

            // 6% chance to get Omen of Clarity...
            manaCostModifier *= 1 - 0.06f * druidTalents.OmenOfClarity;

            minHeal *=
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            maxHeal *=
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            coefDH += (0.2f * druidTalents.EmpoweredTouch);     // ET is additive from http://elitistjerks.com/f73/t37038-restoration_glyphs/p8/#post1240879 

            coefDH *=
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);
        }
    }
    public class Regrowth : Spell {
        protected float manaCostModifier = 1f;
        protected float extraCritPercent = 0f;
        protected float extraCritModifier = 0f;
        public Regrowth(Character character, Stats stats)
        {
            InitializeRegrowth(character, stats); 
        }
        public Regrowth(Character character, Stats stats, bool withRegrowthActive, bool clipIfGlyphed)
        {
            InitializeRegrowth(character, stats);

            if (withRegrowthActive && character.DruidTalents.GlyphOfRegrowth)
            {
                minHeal *= 1.2f;
                maxHeal *= 1.2f;
                periodicTick *= 1.2f;
                coefDH *= 1.2f;
                coefHoT *= 1.2f;

                if (clipIfGlyphed)
                {
                    periodicTicks--;
                }
            }
        }
        private void InitializeRegrowth(Character character, Stats stats) {
            CalculationOptionsTree calcOpts = (CalculationOptionsTree)character.CalculationOptions;

            #region Base Values
            castTimeBeforeHaste = 2f;
            coefDH = 0.54f; 
            coefHoT = 1.316f / 7f;

            minHeal = 2234f;
            maxHeal = 2494f;
            periodicTick = 335f; // 2345 / 7
            periodicTicks = 7f;
            #endregion

            // Seems to apply before talents

            calculateTalents(character.DruidTalents, calcOpts);

            applyStats(stats);
        }
        public override void applyStats(Stats stats)
        {
            base.applyStats(stats);
            spellPower = stats.SpellPower;
            critPercent = stats.SpellCrit + extraCritPercent;
            manaCost = 0.29f * TreeConstants.BaseMana - stats.SpellsManaReduction;
            critModifier += extraCritModifier;
        }
        private void calculateTalents(DruidTalents druidTalents, CalculationOptionsTree calcOpts) {
            periodicTicks += 2 * druidTalents.NaturesSplendor;

            manaCostModifier *= (1 - 0.03f * druidTalents.Moonglow - 0.2f * druidTalents.TreeOfLife);
            // 6% chance to get Omen of Clarity...
            manaCostModifier *= 1f - 0.06f * druidTalents.OmenOfClarity;

            extraCritPercent += 5f * druidTalents.NaturesBounty;
            //Living Seed
            extraCritModifier = 0.1f * druidTalents.LivingSeed * calcOpts.Current.LivingSeedEfficiency / 100f;

            minHeal *=
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            maxHeal *=
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            coefDH *=
                (1f + 0.04f * druidTalents.EmpoweredRejuvenation) *
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            periodicTick *=
                (1f + 0.01f * druidTalents.Genesis + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            coefHoT *= 
                (1f + 0.04f * druidTalents.EmpoweredRejuvenation) *
                (1f + 0.01f * druidTalents.Genesis + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);
        }
    }
    public class Rejuvenation : Spell {
        private float manaCostModifier = 1f;
        private float extraTicks = 0f;
        private bool hasteTicks = false;
        private float periodicTickModifier = 1f;
        public Rejuvenation(Character character, Stats calculatedStats) {
            CalculationOptionsTree calcOpts = (CalculationOptionsTree)character.CalculationOptions;

            #region Base Values
            castTimeBeforeHaste = 0f;
            coefHoT = 1.879f / 5;
            #endregion

            calculateTalents(character.DruidTalents, calcOpts);

            applyStats(calculatedStats);
        }
        public override void applyStats(Stats stats)
        {
            base.applyStats(stats);

            spellPower = stats.SpellPower;

            manaCost = (0.18f * TreeConstants.BaseMana - stats.SpellsManaReduction);
            manaCost *= manaCostModifier; // from talents
            manaCost -= stats.ReduceRejuvenationCost; //  Idol of Awakening (-106 Manacost)

            periodicTick = 338f; // 1690 / 5
            periodicTick *= periodicTickModifier; // from talents
            periodicTick += stats.RejuvenationHealBonus; // Idol of Pure Thoughts

            periodicTicks = 5f + extraTicks; // from talents

            #region Tier 8 (4) SetBonus
            if (stats.RejuvenationInstantTick > 0.0f)
            {
                // Set AverageHealingwithCrit = PeriodicTick
                //Some talents doesn't apply to this instant tick, so it should actually be less than the normal tick, hopefully small enough error
                minHeal = PeriodicTick * stats.RejuvenationInstantTick;
                maxHeal = PeriodicTick * stats.RejuvenationInstantTick;
                coefDH = 0.0f; // PeriodicTick already scaled by SpellPower, so don't scale again
                critModifier = 1f;
            }
            else
            {
                minHeal = 0f;
                maxHeal = 0f;
            }
            #endregion

            #region Tier 9 (4) SetBonus
            critHoTPercent = stats.SpellCrit * stats.RejuvenationCrit;
            //Should set  critPercent = critHoTPercent;   to allow instantTick to also be crittable, but cannot have 4 piece setbonus simultanuously
            #endregion

            #region Tier 10 (4) SetBonus
            if (stats.RejuvJumpChance > 0)
            {
                // chanceOncePerRejuv = (float)(1f - Math.Pow(1f - calculatedStats.RejuvJumpChance, periodicTicks));
                float chance = periodicTicks * stats.RejuvJumpChance;
                float factor = 1f; // assume it doesn't consume existing buff, if it does then factor = 0.5f
                // assume it will jump multiple times
                periodicTicks *= (1f + factor * chance + factor * chance * chance + factor * chance * chance * chance);
                //only one jump: periodicTicks *= (1f + factor * chance);
            }
            #endregion

            if (hasteTicks) applyHasteToPeriodicTickTime();
        }
        private void calculateTalents(DruidTalents druidTalents, CalculationOptionsTree calcOpts) {
            extraTicks += 1 * druidTalents.NaturesSplendor;

            manaCostModifier *= 1 - 0.2f * druidTalents.TreeOfLife - 0.03f * druidTalents.Moonglow;
            // 6% chance to get Omen of Clarity...
            manaCostModifier *= 1 - 0.06f * druidTalents.OmenOfClarity;

            periodicTickModifier *=
                (1 + 0.01f * druidTalents.Genesis + 
                 0.05f * druidTalents.ImprovedRejuvenation + 
                 0.02f * druidTalents.GiftOfNature) *
                (1 + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1 + 0.06f * druidTalents.TreeOfLife);

            coefHoT *= 
                (1 + 0.04f * druidTalents.EmpoweredRejuvenation) *
                (1 + 0.01f * druidTalents.Genesis + 
                 0.05f * druidTalents.ImprovedRejuvenation + 
                 0.02f * druidTalents.GiftOfNature) *
                (1 + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1 + 0.06f * druidTalents.TreeOfLife);

            #region Glyph of Rapid Rejuvenation
            if (druidTalents.GlyphOfRapidRejuvenation)
            {
                hasteTicks = true;
            }
            #endregion
        }
    }
    public class Lifebloom : Spell {
        protected float idolHoTBonus = 0f;
        protected float stackScaling = 1.0f;
        protected float stackSize = 1.0f;
        protected float manaRefund = 0.0f;
        private float manaCostModifier = 1f;
        public override float PeriodicTick { get { return stackScaling * (periodicTick + (idolHoTBonus + spellPower) * coefHoT); } }
        public override float AverageHealing {  get { return stackSize * (extraHealing + (minHeal + maxHeal) / 2 + spellPower * coefDH); } }
        public override float ManaCost {get { return (base.ManaCost - stackSize * manaRefund); }/*set { manaCost = value; }*/}
        public Lifebloom(Character character, Stats stats)
        {
            CalculationOptionsTree calcOpts = (CalculationOptionsTree)character.CalculationOptions;

            #region Base Values
            castTimeBeforeHaste = 0f;
            periodicTickTime    = 1f;
            coefHoT             = 0.6684f / 7f;

            minHeal = 776f; 
            maxHeal = 776f;
            coefDH = 0.516f; 

            periodicTick = 53f;
            periodicTicks = 7f;
            #endregion

            // Seems to apply before talents

            calculateTalents(character.DruidTalents, calcOpts);

            applyStats(stats);
        }

        public Lifebloom(Character character, Stats stats, int numStacks, bool fastStack)
            : this(character, stats)
        {
            float newPeriodicTicks = periodicTicks;

            if (numStacks == 1) {
                // Do nothing, already setup
            } else if (numStacks == 2) {
                if (!fastStack) {
                    #region Slow LB stacking
                    newPeriodicTicks = periodicTicks * 2 - 1;  // Double number of ticks, but lose 1
                    // N-1 ticks of 1 stack + N ticks of 2 stacks, averaged over total ticks
                    stackScaling = ((periodicTicks - 1) + 2 * periodicTicks) / newPeriodicTicks;
                    #endregion
                } else {
                    #region Fast LB stacking
                    newPeriodicTicks = periodicTicks + 1;  // Stack every tick 
                    // 1 ticks of 1 stack + N ticks of 2 stacks, averaged over total ticks
                    stackScaling = ((1) + 2 * periodicTicks) / newPeriodicTicks;
                    #endregion
                }

                stackSize = 2.0f; // Bloom heal doubled

                periodicTicks = newPeriodicTicks;
                numberOfCasts = 2;
            } else if (numStacks == 3) {
                if (!fastStack) {
                    #region Slow LB stacking
                    newPeriodicTicks = periodicTicks * 3 - 2;  // Triple number of ticks, but lose 1 each time
                    // N-1 ticks of 1 stack + N -1 ticks of 2 stacks, averaged over total ticks
                    stackScaling = ((periodicTicks - 1) + 2 * (periodicTicks - 1) + 3 * periodicTicks) / newPeriodicTicks;
                    #endregion
                } else {
                    #region Fast LB stacking
                    newPeriodicTicks = periodicTicks + 2;  // Stack every tick 
                    // 1 ticks of 1 stack + 1 ticks of 2 stacks, averaged over total ticks
                    stackScaling = ((1) + 2 * (1) + 3 * periodicTicks) / newPeriodicTicks;
                    #endregion
                }

                stackSize *= 3.0f; // Bloom heal trippled

                periodicTicks = newPeriodicTicks;

                numberOfCasts = 3;
            }
        }
        public override void applyStats(Stats stats)
        {
            base.applyStats(stats);
            spellPower = stats.SpellPower;
            critPercent = stats.SpellCrit;
            manaCost = 0.28f * TreeConstants.BaseMana - stats.SpellsManaReduction;
            manaCost *= (1f - stats.LifebloomCostReduction);
            manaCost *= numberOfCasts;
            manaCost *= manaCostModifier;
            manaRefund = 0.14f * TreeConstants.BaseMana - 0.5f * stats.SpellsManaReduction;
            manaRefund *= (1f - stats.LifebloomCostReduction);
            manaRefund *= numberOfCasts;
            manaRefund *= manaCostModifier;
            idolHoTBonus = stats.LifebloomTickHealBonus; // Idol of the Emerald Queen
        }
        private void calculateTalents(DruidTalents druidTalents, CalculationOptionsTree calcOpts) {
            periodicTicks += 2f * druidTalents.NaturesSplendor;

            gcdBeforeHaste -= 1.5f * 0.02f * druidTalents.GiftOfTheEarthmother;

            manaCostModifier *= (1f - 0.2f * druidTalents.TreeOfLife);

            // 6% chance to get Omen of Clarity...
            manaCostModifier *= 1f - 0.06f * druidTalents.OmenOfClarity;

            periodicTick *=
                (1f + 0.01f * druidTalents.Genesis + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            coefHoT *= 
                (1f + 0.04f * druidTalents.EmpoweredRejuvenation) *
                (1f + 0.01f * druidTalents.Genesis + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            minHeal *=
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            maxHeal *=
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            coefDH *=
                (1f + 0.04f * druidTalents.EmpoweredRejuvenation) *
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            if (druidTalents.GlyphOfLifebloom) { periodicTicks += 1f; } //(calcOpts.glyphOfLifebloom)
        }
    }
    public class LifebloomStack : Lifebloom {
        public LifebloomStack(Character character, Stats stats)
            : base(character, stats)
        {
            periodicTick  *= 3f;
            periodicTicks -= 1f; // Keep a stack alive
            coefHoT       *= 3f;
            minHeal        = 0f;
            maxHeal        = 0f;
            coefDH         = 0f;
            critPercent = 0f;
            manaRefund     = 0f; // manaCost is now without refund   
        }
    }
    public class WildGrowth : Spell {
        public int maxTargets;
        private float manaCostModifier = 1f;
        private float periodickTickModifier = 1f;
        private float[] baseTick = new float[7];
        private float[] tick = new float[7];
        public float[] BaseTick { get { return tick; } }
        public float[] Tick { get { return tick; } }
        public WildGrowth(Character character, Stats stats)
        {
            CalculationOptionsTree calcOpts = (CalculationOptionsTree)character.CalculationOptions;

            #region Base Values
            castTimeBeforeHaste = 0f;
            coefHoT = 0.8057f / 7f; 

            periodicTick     = 206f; // 1442 / 7
            periodicTicks    =   7f;
            periodicTickTime =   1f;
            maxTargets       =   5;
            #endregion

            calculateTalents(character.DruidTalents, calcOpts);

            applyStats(stats);
        }
        public override void applyStats(Stats stats)
        {
            base.applyStats(stats);
            spellPower = stats.SpellPower;
            manaCost = 0.23f * TreeConstants.BaseMana - stats.SpellsManaReduction;
            manaCost *= manaCostModifier;
            periodicTick = 206f; // 1442 / 7
            #region T10 (2) SetBonus
            float healing = 0f;
            for (int i = 0; i < 7; i++)
            {
                baseTick[i] = 293f - 29f * (1f - stats.WildGrowthLessReduction) * i;
                tick[i] = baseTick[i] + coefHoT * spellPower;
                healing += baseTick[i];
            }
            periodicTick = healing / 7;
            #endregion
            periodicTick *= periodickTickModifier;
        }
        private void calculateTalents(DruidTalents druidTalents, CalculationOptionsTree calcOpts) {
            manaCostModifier *= (1f - 0.2f * druidTalents.TreeOfLife);

            // 6% chance to get Omen of Clarity...
            manaCostModifier *= 1f - 0.06f * druidTalents.OmenOfClarity;

            // Glyph of Wild Growth
            if (druidTalents.GlyphOfWildGrowth)
              maxTargets += 1;

            coefHoT *=
                (1f + 0.01f * druidTalents.Genesis + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.06f * druidTalents.TreeOfLife) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife);

            periodickTickModifier *= 
                (1f + 0.01f * druidTalents.Genesis + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.06f * druidTalents.TreeOfLife) *
                (1f + 0.02f * druidTalents.MasterShapeshifter);
        }
    }
    public class Nourish : Spell {
        private float NourishBonusPerHoTGlyphs;
        private float manaCostModifier = 1f;
        protected float extraCritPercent = 0f;
        protected float extraCritModifier = 0f;
        public Nourish(Character character, Stats stats) { InitializeNourish(character, stats); }
        public Nourish(Character character, Stats stats, int hotsActive)
        {
            InitializeNourish(character, stats);
            if (hotsActive>0) {
                minHeal *= 1.2f + (stats.NourishBonusPerHoT + NourishBonusPerHoTGlyphs) * hotsActive;
                maxHeal *= 1.2f + (stats.NourishBonusPerHoT + NourishBonusPerHoTGlyphs) * hotsActive;
                coefDH  *= 1.2f + (stats.NourishBonusPerHoT + NourishBonusPerHoTGlyphs) * hotsActive;
            }
        }
        private void InitializeNourish(Character character, Stats stats)
        {
            CalculationOptionsTree calcOpts = (CalculationOptionsTree)character.CalculationOptions;

            #region Base Values
            castTimeBeforeHaste = 1.5f;
            //coefDH = 0.6611f; // Spreadsheet says .69, wowwiki says .6611, 1.5/3.5 = .43, confused!
            //coefDH = 0.67305f; // Value used in TreeCalcs
            coefDH = 0.671429f; // Best guess based on tests reported in workitem http://rawr.codeplex.com/WorkItem/View.aspx?WorkItemId=13809

            minHeal = 1883f;
            maxHeal = 2187f;
            NourishBonusPerHoTGlyphs = 0.0f;
            #endregion

            calculateTalents(character.DruidTalents, calcOpts);

            applyStats(stats);
        }
        public override void applyStats(Stats stats)
        {
            base.applyStats(stats);
            spellPower = stats.SpellPower;
            spellPower += stats.NourishSpellpower; // Idol of Flourishing Life
            manaCost = 0.18f * TreeConstants.BaseMana - stats.SpellsManaReduction;
            manaCost *= manaCostModifier; // from talents
            critPercent = stats.SpellCrit + extraCritPercent;
            #region Tier 9 2 piece Set Bonus
            critPercent += (stats.NourishCritBonus * 100.0f); // Percent is range 0-100
            #endregion
            critModifier += extraCritModifier;
        }
        private void calculateTalents(DruidTalents druidTalents, CalculationOptionsTree calcOpts) {
            manaCostModifier *= (1f - druidTalents.TranquilSpirit * 0.02f - druidTalents.Moonglow * 0.03f);

			extraCritPercent += 2f * druidTalents.NaturesMajesty;
            extraCritPercent += 5f * druidTalents.NaturesBounty;

            //Living Seed, 30% seed, 33% * points spend (1/3)
            //if (calcOpts.useLivingSeedAsCritMultiplicator)
            extraCritModifier += 0.1f * druidTalents.LivingSeed * calcOpts.Current.LivingSeedEfficiency / 100f;

            // 6% chance to get Omen of Clarity...
            manaCostModifier *= 1f - 0.06f * druidTalents.OmenOfClarity;
                
            minHeal *=
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            maxHeal *=
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            coefDH += (0.1f * druidTalents.EmpoweredTouch);     // From 3.2 Empowered Touch also boosts Nourish
            // Assume also additive, also see http://elitistjerks.com/f73/t37038-restoration_glyphs/p8/#post1240879 
            // This is also the value TreeCalcs uses at the moment (8th december 2009)

            coefDH *=
                (1f + 0.02f * druidTalents.GiftOfNature) *
                (1f + 0.02f * druidTalents.MasterShapeshifter * druidTalents.TreeOfLife) *
                (1f + 0.06f * druidTalents.TreeOfLife);

            if (druidTalents.GlyphOfNourish) { NourishBonusPerHoTGlyphs = 0.06f; }
        }
    }
    public class Swiftmend : Spell {
        public float regrowthUseChance = 0.0f;
        public float rejuvUseChance = 0.0f;
        public float regrowthTicksLost = 0.0f;
        public float rejuvTicksLost = 0.0f;
        public Swiftmend(Character character, Stats stats, Spell Rejuv, Spell Regrowth) {
            regrowthUseChance = rejuvUseChance = regrowthTicksLost = rejuvTicksLost = 0.0f;

            #region Base Values
            castTimeBeforeHaste = 0;
            coefDH = 0;
            manaCost = 0.16f * TreeConstants.BaseMana;
            spellPower = 0f;
            critPercent = stats.SpellCrit;

            minHeal = 0f;
            maxHeal = 0f;
            #endregion

            if (character.DruidTalents.Swiftmend == 0)
            {
                Rejuv = null;
                Regrowth = null;
            }

            if (Rejuv == null) {
                if (Regrowth == null) {
                    // No HoTs, so Swiftmend not possible
                }else{
                    // Case of only Regrowth
                    minHeal = 6 * Regrowth.PeriodicTick;
                    maxHeal = minHeal;
                    regrowthUseChance = 1.0f;
                    if (!character.DruidTalents.GlyphOfSwiftmend)
                    {
                        regrowthTicksLost = (Regrowth.PeriodicTicks + 1f) / 2.0f;      // If cast randomly, consume half of the ticks
                    }
                }
            }else if (Regrowth == null){
                // Case of only Rejuv
                minHeal = 4f * Rejuv.PeriodicTick;
                maxHeal = minHeal;
                rejuvUseChance = 1f;
                if (!character.DruidTalents.GlyphOfSwiftmend)
                {
                    rejuvTicksLost = (Rejuv.PeriodicTicks + 1f)/ 2.0f;      // If cast randomly, consume half of the ticks
                }
            }else{
                // Case of both active

                //TODO: Find a formula to calculate this, instead of "numerical integration"
                float selectedRejuv,selectedRegrowth,lostRejuvTicks,lostRegrowthTicks = 0;

                regrowthUseChance = Rejuv.Duration / Regrowth.Duration * 0.5f;
                rejuvUseChance = (Regrowth.Duration - Rejuv.Duration) / Regrowth.Duration + Rejuv.Duration / Regrowth.Duration * 0.5f;
                lostRejuvTicks = (0.5f * (Rejuv.PeriodicTicks + 1f) * (Regrowth.Duration - Rejuv.Duration) / Regrowth.Duration) + (Rejuv.PeriodicTicks * 10f / 24f) * 0.5f * (Rejuv.Duration / Regrowth.Duration);
                selectedRejuv = rejuvUseChance;
                lostRegrowthTicks = ((Rejuv.PeriodicTicks - 1f)/ 2f);
                selectedRegrowth = 1f;

                minHeal = (6f * regrowthUseChance * Regrowth.PeriodicTick + 4f * rejuvUseChance * Rejuv.PeriodicTick);
                maxHeal = minHeal;

                if (!character.DruidTalents.GlyphOfSwiftmend)
                {
                    rejuvTicksLost = lostRejuvTicks / selectedRejuv;
                    regrowthTicksLost = lostRegrowthTicks / selectedRegrowth;
                }
            }
            #region Nightsong (Tier 8) 2 item set bonus
            minHeal *= (1.0f + stats.SwiftmendBonus);
            maxHeal *= (1.0f + stats.SwiftmendBonus);
            #endregion

            applyHaste();
        }
        public override void applyStats(Stats stats)
        {
            base.applyStats(stats);
            System.Diagnostics.Debug.Assert(false);
        }
    }
}
