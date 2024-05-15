using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

namespace PhontyPlus
{
    public class Phonty : NPC, IClickable<int>
    {
        public static AssetManager sprites = new AssetManager();
        public static List<SoundObject> records = new List<SoundObject>();
        public static AssetManager audios = new AssetManager();

        public static List<Sprite> emergeFrames = new List<Sprite>();
        public static List<Sprite> chaseFrames = new List<Sprite>();

        public CustomSpriteAnimator animator;
        public AudioManager audMan;
        public TextMeshPro counter;
        public GameObject totalDisplay;

        public MapIcon mapIconPre;
        public NoLateIcon mapIcon;

        public bool angry = false;
        public static void LoadAssets()
        {
            var PIXELS_PER_UNIT = 26f;
            sprites.Add("idle_forward", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(Mod.Instance, "idle_forward.png"), PIXELS_PER_UNIT));
            audios.Add("angryIntro", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Mod.Instance, "PhontyIntro.ogg"), "Start remembering what you hear.", SoundType.Voice, Color.yellow));
            audios.Add("angry", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Mod.Instance, "PhontyAngry.ogg"), "* Angry Phonograph *", SoundType.Voice, Color.yellow));
            audios.Add("shockwave", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Mod.Instance, "PhontyShot.ogg"), "", SoundType.Effect, Color.yellow));

            for (int i = 0; i <= 37; i++)
            {
                emergeFrames.Add(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(Mod.Instance, "emerge/frame_" + i + ".png"), PIXELS_PER_UNIT));
            }

            for (int i = 0; i <= 19; i++)
            {
                chaseFrames.Add(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(Mod.Instance, "angry/frame_" + i + ".png"), PIXELS_PER_UNIT));
            }

            var recordsFolder = Directory.GetFiles(Path.Combine(AssetLoader.GetModPath(Mod.Instance), "phonty_records"));
            foreach (var path in recordsFolder)
            {
                try
                {
                    records.Add(ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(path), "* Phonograph Music *", SoundType.Voice, Color.yellow));
                }
                catch (NotImplementedException)
                {
                    var filename = Path.GetFileName(path);
                    WarningScreenPatch.Show($"\n\nOnly audio file types are allowed in <color=blue>phonty_records</color>.\n\nPlease remove <color=red>phonty_records/{filename}</color>.");
                }
            }
        }

        public bool ClickableHidden() => angry;

        public bool ClickableRequiresNormalHeight() => false;

        public void ClickableSighted(int player) { }

        public void ClickableUnsighted(int player) { }

        public void Clicked(int player)
        {
            if (!angry)
            {
                ResetTimer();
            }
        }
        private IEnumerator DeafenPlayer()
        {
            yield return new WaitForSeconds(0.5f);
            AudioListener.volume = 0.01f;
            Mod.assetManager.Get<AudioMixer>("Mixer").SetFloat("EchoWetMix", 1f);
            yield break;
        }
        public void EndGame(Transform player)
        {
            var core = Singleton<CoreGameManager>.Instance;
            if (PhontyMenu.nonLethalConfig.Value == true || CoreGameManager.Instance.currentMode == Mode.Free)
            {
                core.audMan.PlaySingle(audios.Get<SoundObject>("shockwave"));
                behaviorStateMachine.ChangeState(new Phonty_Dead(this));
                StartCoroutine(DeafenPlayer());
                return;
            }
            Time.timeScale = 0f;
            Singleton<MusicManager>.Instance.StopMidi();
            core.disablePause = true;
            core.GetCamera(0).UpdateTargets(transform, 0);
            core.GetCamera(0).offestPos = (player.position - transform.position).normalized * 2f + Vector3.up;
            core.GetCamera(0).controllable = false;
            core.GetCamera(0).matchTargetRotation = false;
            core.audMan.volumeModifier = 0.6f;
            core.audMan.PlaySingle(audios.Get<SoundObject>("shockwave"));
            core.StartCoroutine(core.EndSequence());
            Singleton<InputManager>.Instance.Rumble(1f, 2f);
        }
        public override void Initialize()
        {
            base.Initialize();
            animator.animations.Add("Idle", new CustomAnimation<Sprite>(new Sprite[] { Phonty.sprites.Get<Sprite>("idle_forward") }, 1f));
            animator.animations.Add("Chase", new CustomAnimation<Sprite>(chaseFrames.ToArray(), 0.5f));
            animator.animations.Add("ChaseStatic", new CustomAnimation<Sprite>(new Sprite[] { emergeFrames.Last() }, 1f));
            animator.animations.Add("Emerge", new CustomAnimation<Sprite>(emergeFrames.ToArray(), 1f));
            animator.SetDefaultAnimation("Idle", 1f);

            // Counter on top of Phonty
            var totalBase = GameObject.Instantiate(Mod.assetManager.Get<GameObject>("TotalBase"));
            totalBase.transform.parent = transform;
            totalBase.transform.localPosition = new Vector3(0, 3, 0);
            totalBase.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            totalBase.SetActive(true);
            totalDisplay = totalBase.gameObject.transform.GetChild(0).gameObject;
            totalDisplay.SetActive(true);
            counter = totalDisplay.GetComponent<TextMeshPro>();

            mapIconPre = Mod.assetManager.Get<NoLateIcon>("MapIcon");
            mapIcon = (NoLateIcon)ec.map.AddIcon(mapIconPre, gameObject.transform, Color.white);
            mapIcon.spriteRenderer.sprite = Phonty.sprites.Get<Sprite>("idle_forward");
            mapIcon.gameObject.SetActive(true);

            behaviorStateMachine.ChangeState(new Phonty_PlayingMusic(this));
            navigator.SetSpeed(0f);
            navigator.maxSpeed = 0f;
            navigator.Entity.SetHeight(7f);
            gameObject.layer = LayerMask.NameToLayer("ClickableEntities");

            var position = IntVector2.GetGridPosition(gameObject.transform.position);
            var cell = ec.CellFromPosition(position);
            var startingRoom = cell.room;
            for (int i = 0; i < startingRoom.TileCount; i++)
            {
                this.ec.map.Find(startingRoom.TileAtIndex(i).position.x, startingRoom.TileAtIndex(i).position.z, startingRoom.TileAtIndex(i).ConstBin, startingRoom);
            }
        }

        public void ResetTimer()
        {
            behaviorStateMachine.ChangeState(new Phonty_PlayingMusic(this));
        }

        public void UpdateCounter(int count)
        {
            counter.SetText(string.Join("", count.ToString().Select(ch => "<sprite=" + ch + ">")));
            mapIcon.timeText.SetText(count.ToString());
            mapIcon.UpdatePosition(ec.map);
        }
    }

    public class Phonty_StateBase : NpcState
    {
        public Phonty_StateBase(Phonty phonty) : base(phonty)
        {
            this.phonty = phonty;
        }
        public Phonty phonty;

    }
    public class Phonty_PlayingMusic : Phonty_StateBase
    {
        public Phonty_PlayingMusic(Phonty phonty) : base(phonty) { }
        public override void Enter()
        {
            base.Enter();
            base.ChangeNavigationState(new NavigationState_DoNothing(phonty, 63));
            phonty.audMan.FlushQueue(true);
            phonty.audMan.PlaySingle(Mod.assetManager.Get<SoundObject>("windup"));
            phonty.audMan.QueueRandomAudio(Phonty.records.ToArray());
            phonty.audMan.SetLoop(true);
            phonty.animator.Play("Idle", 1f);
#if DEBUG
            timeLeft = 10;
#endif
        }
        public override void Update()
        {
            base.Update();
            if (timeLeft <= 0)
            {
                phonty.behaviorStateMachine.ChangeState(new Phonty_Chase(phonty));
            }
            else
            {
                timeLeft -= Time.deltaTime * phonty.ec.NpcTimeScale;
            }
            phonty.UpdateCounter((int)timeLeft);
        }
        protected float timeLeft = PhontyMenu.timeLeftUntilMad.Value;
    }
    public class Phonty_Chase : Phonty_StateBase
    {
        protected NavigationState_TargetPlayer targetState;
        protected PlayerManager player;
        public Phonty_Chase(Phonty phonty) : base(phonty)
        {
            player = phonty.ec.Players[0];
            targetState = new NavigationState_TargetPlayer(phonty, 64, player.transform.position);
        }
        public override void Enter()
        {
            base.Enter();
            targetState = new NavigationState_TargetPlayer(phonty, 64, player.transform.position);
            base.ChangeNavigationState(targetState);
            phonty.angry = true;
            phonty.totalDisplay.SetActive(false);
            phonty.mapIcon.gameObject.SetActive(false);

            phonty.audMan.FlushQueue(true);
            phonty.audMan.QueueAudio(Phonty.audios.Get<SoundObject>("angryIntro"), true);

            phonty.StartCoroutine(Emerge());
            phonty.animator.Play("Emerge", 1f);
            phonty.animator.SetDefaultAnimation("ChaseStatic", 1f);

        }
        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            base.ChangeNavigationState(new NavigationState_WanderRandom(phonty, 32));
        }
        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (this.player == player)
            {
                base.ChangeNavigationState(targetState);
                targetState.UpdatePosition(player.transform.position);
            }
        }
        public override void OnStateTriggerEnter(Collider other)
        {
            base.OnStateTriggerEnter(other);
            if (other.CompareTag("Player") && other.GetComponent<PlayerManager>() == player)
            {
                phonty.EndGame(other.transform);
            }
        }

        private IEnumerator Emerge()
        {
            while (phonty.audMan.QueuedAudioIsPlaying)
                yield return null;
            phonty.audMan.QueueAudio(Phonty.audios.Get<SoundObject>("angry"), true);
            phonty.audMan.SetLoop(true);
            phonty.animator.SetDefaultAnimation("Chase", 1f);
            phonty.Navigator.SetSpeed(4f);
            phonty.Navigator.maxSpeed = 20f;
            yield break;
        }
    }
    public class Phonty_Dead : Phonty_StateBase
    {
        public Phonty_Dead(Phonty phonty) : base(phonty)
        {
        }

        public override void Enter()
        {
            base.Enter();
            base.ChangeNavigationState(new NavigationState_Disabled(phonty));
            phonty.angry = true;
            phonty.totalDisplay.SetActive(false);
            phonty.mapIcon.gameObject.SetActive(false);

            phonty.animator.Play("Idle", 1f);
            phonty.animator.SetDefaultAnimation("Idle", 1f);
        }
    }
}
