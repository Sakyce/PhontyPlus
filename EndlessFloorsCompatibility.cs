using BaldiEndless;

namespace PhontyPlus
{
    internal class EndlessFloorsCompatibility
    {
        public static void Initialize()
        {
            EndlessFloorsPlugin.AddGeneratorAction(Mod.Instance.Info, EndlessGeneratorAction);
        }
        private static void EndlessGeneratorAction(GeneratorData data)
        {
#if DEBUG
            data.npcs.Add(new WeightedNPC() { selection = Mod.assetManager.Get<NPC>("Phonty"), weight = 1000 });
            data.npcs.ForEach(n => Debug.Log($"{n.weight} , {n.selection.name}"));
#endif
            data.npcs.Add(new WeightedNPC() { selection = Mod.assetManager.Get<NPC>("Phonty"), weight = PhontyMenu.guaranteeSpawn.Value ? 10000 : 100 });
        }
    }
}
