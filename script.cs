// This script runs the same load as a normal syringe does.

using UnityEngine;
using System.Collections.Generic;

namespace Mod
{
    public class Mod
    {
        public static void Main()
        {
            ModAPI.Register(new Modification()
            {
                OriginalItem = ModAPI.FindSpawnable("Empty Syringe"),
                NameOverride = "ContentApple's Infinite Empty Syringe",
                DescriptionOverride = "An empty syringe with the enable infinite source option.",
                CategoryOverride = ModAPI.FindCategory("Chemistry"),
                AfterSpawn = (Instance) =>
                {
                    Instance.AddComponent<InfiniteSyringeBehaviour>();
                }
            });
        }
    }

    public class InfiniteSyringeBehaviour : MonoBehaviour
    {
        private SyringeBehaviour syringe;
        private float maxAmount;
        private bool previousFinite = true;
        private readonly Dictionary<Liquid, float> storedMixture = new Dictionary<Liquid, float>();

        private void Start()
        {
            syringe = GetComponent<SyringeBehaviour>();
            if (syringe == null) return;
            maxAmount = syringe.Limits.y;
            syringe.CanToggleInfinite = true;
            previousFinite = syringe.Finite;
        }

        private void FixedUpdate()
        {
            if (syringe == null) return;
            syringe.CanToggleInfinite = true;

            if (syringe.Finite)
            {
                if (syringe.TotalLiquidAmount > 0f)
                {
                    CaptureCurrentMixture();
                }
            }
            else
            {
                if (previousFinite)
                {
                    CaptureCurrentMixture();
                }

                if (storedMixture.Count > 0)
                {
                    RefillStoredMixture();
                }
            }

            previousFinite = syringe.Finite;
        }

        private void CaptureCurrentMixture()
        {
            storedMixture.Clear();
            float percentageSum = 0f;

            foreach (var entry in syringe.LiquidDistribution)
            {
                Liquid liquid = entry.Key;
                float percentage = syringe.GetPercentageOf(liquid);
                if (percentage > 0f)
                {
                    storedMixture[liquid] = percentage;
                    percentageSum += percentage;
                }
            }

            if (percentageSum <= 0f)
            {
                storedMixture.Clear();
                return;
            }

            var keys = new List<Liquid>(storedMixture.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                Liquid key = keys[i];
                storedMixture[key] = storedMixture[key] / percentageSum;
            }
        }

        private void RefillStoredMixture()
        {
            if (syringe.TotalLiquidAmount <= 0f)
            {
                foreach (var entry in storedMixture)
                {
                    syringe.AddLiquid(entry.Key, entry.Value * maxAmount);
                }
                return;
            }

            foreach (var entry in storedMixture)
            {
                Liquid liquid = entry.Key;
                float desiredAmount = entry.Value * maxAmount;
                float currentAmount = syringe.GetAmount(liquid);
                if (currentAmount < desiredAmount)
                {
                    syringe.AddLiquid(liquid, desiredAmount - currentAmount);
                }
            }
        }
    }
}
