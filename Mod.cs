using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Registers;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace PhontyPlus
{
    [BepInPlugin("sakyce.baldiplus.phonty", "Phonty", "3.0.5.2")]
    [BepInDependency("mtm101.rulerp.baldiplus.endlessfloors", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    public unsafe class Mod : BaseUnityPlugin
    {
        public static AssetManager assetManager = new AssetManager();

        public string modpath;
        public static Mod Instance;

        public void Awake()
        {
            Harmony harmony = new Harmony("sakyce.baldiplus.phonty");
            harmony.PatchAllConditionals();
            modpath = AssetLoader.GetModPath(this);
            Instance = this;

            EnumExtensions.ExtendEnum<Character>("Phonty");
            LoadingEvents.RegisterOnAssetsLoaded(Info, this.OnAssetsLoaded, false);
            GeneratorManagement.Register(this, GenerationModType.Addend, AddNPCs);
            try { EndlessFloorsCompatibility.Initialize(); } catch (FileNotFoundException) { }
#if DEBUG
            Debug.LogWarning("You're using the DEBUG build of Phonty");
#endif

            CustomOptionsCore.OnMenuInitialize += PhontyMenu.OnMenuInitialize;
            PhontyMenu.Setup();
        }



        private void AddNPCs(string floorName, int floorNumber, LevelObject floorObject)
        {
#if DEBUG
            floorObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = 1000 });
            foreach (var weighted in floorObject.potentialNPCs)
            {
                print($"{weighted.weight} , {weighted.selection.name}");
            }
#endif
            if (floorName.StartsWith("F"))
            {
                floorObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = PhontyMenu.guaranteeSpawn.Value ? 10000 : 75 });
                floorObject.MarkAsNeverUnload();
            }
            else if (floorName == "END") // Endless
            {
                floorObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManager.Get<NPC>("Phonty"), weight = PhontyMenu.guaranteeSpawn.Value ? 10000 : 80 });
                floorObject.MarkAsNeverUnload();
            }
        }
        private void OnAssetsLoaded()
        {
            Phonty.LoadAssets();
            var phonty = new NPCBuilder<Phonty>(Info)
                .AddLooker()
                .SetMinMaxAudioDistance(1, 300)
                .AddSpawnableRoomCategories(RoomCategory.Faculty)
                .SetName("Phonty")
                .SetEnum("Phonty")
                .SetPoster(ObjectCreators.CreateCharacterPoster(AssetLoader.TextureFromMod(this, "poster.png"), "Phonty", "He's a sentient phonograph. Just make sure that whenever he's playing music, it stays playing! OR ELSE! "))
                .Build();
            phonty.audMan = phonty.GetComponent<AudioManager>();
            CustomSpriteAnimator animator = phonty.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.spriteRenderer = phonty.spriteRenderer[0];
            phonty.animator = animator;
            assetManager.Add<Phonty>("Phonty", phonty);

            // phonty timer
            var totalBase = (from x in Resources.FindObjectsOfTypeAll<Transform>()
                             where x.name == "TotalBase"
                             select x).First<Transform>();
            var clone = GameObject.Instantiate(totalBase).gameObject;
            DontDestroyOnLoad(clone);
            assetManager.Add("TotalBase", clone);

            // phonty map icon
            var mapIcon = (from x in Resources.FindObjectsOfTypeAll<NoLateIcon>()
                           where x.name == "MapIcon"
                           select x).First<NoLateIcon>();
            assetManager.Add("MapIcon", mapIcon);

            // clock wind up sound effect
            var clock = (ITM_AlarmClock)ObjectFinders.GetFirstInstance(Items.AlarmClock).item;
            assetManager.Add("windup", clock.audWind);

            var silenceRoom = (from x in Resources.FindObjectsOfTypeAll<SilenceRoomFunction>()
                               where x.name == "LibraryRoomFunction"
                               select x).First();
            assetManager.Add<AudioMixer>("Mixer", silenceRoom.mixer);

        }

    }
}
