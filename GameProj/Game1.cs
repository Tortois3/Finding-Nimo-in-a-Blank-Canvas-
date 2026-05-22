using GameForms;
using GameProj;
using Microsoft.Data.Sqlite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Threading;
using WinForms = System.Windows.Forms;


namespace GameProj
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private const string GameDatabasePath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameDialogue.db";
        private const string SceneAnalyticsTableName = "tblSceneAnalytics";
        private const string ActionAnalyticsTableName = "tblActionAnalytics";
        private const string AttemptTableName = "tblAttempts";
        private const int HiddenWallSceneId = 13;
        private const int HiddenWallRetrySceneId = 14;
        private const int HiddenWallFinalSceneId = 15;
        private const float Scene16FeaturePopupDurationSeconds = 15f;
        private const float Scene16FeaturePopupSlideSeconds = 0.55f;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private KeyboardState keyState;
        private MouseState mouseState;
        private SqliteConnection conn;
        private readonly bool continueFromSave;
        private readonly int? requestedEndingId;
        private bool saveQueuedForExit = false;

        private Camera camera;
        private int currentSceneId;
        private Vector2 playerPosition;

        // scene system
        private SceneManager sceneManager;
        private SceneData currentScene;

        // background + music
        private Texture2D currentBackground;
        private Song currentSong;

        private bool facingRight = true;

        private bool showTeaPrompt = false;
        private bool teaAnimationPlaying = false;

        private readonly float spriteScale = 3f;
        private readonly float groundOffset = 300f;

        private Dictionary<Vector2, int> scene5CollisionLayer = new();
        private Dictionary<Vector2, int> scene6CollisionLayer = new();
        private Dictionary<Vector2, int> scene21CollisionLayer = new();
        private Dictionary<Vector2, int> main = new();
        private Dictionary<Vector2, int> doors = new();
        private Dictionary<Vector2, int> goodbye = new();
        private Dictionary<Vector2, int> scene6InteractLayer = new();
        private Dictionary<Vector2, int> scene21InteractLayer = new();
        private readonly Dictionary<Vector2, string> scene6DoorLabels = new();
        private readonly Dictionary<string, Vector2> scene6DoorLabelCenters = new(StringComparer.OrdinalIgnoreCase);
        // Scene21 label mappings (single declaration)
        private readonly Dictionary<Vector2, string> scene21Labels = new();
        private readonly Dictionary<string, Vector2> scene21LabelCenters = new(StringComparer.OrdinalIgnoreCase);
        private string? currentScene21Label = null;
        private readonly string[] scene6DoorNames =
        {
            "restaurant",
            "inn",
            "internet cafe",
            "alley",
            "center",
            "shop"
        };
        private readonly Dictionary<string, int> scene6DoorSceneIds = new(StringComparer.OrdinalIgnoreCase)
        {
            ["restaurant"] = 7,
            ["inn"] = 8,
            ["internet cafe"] = 9,
            ["alley"] = 10,
            ["center"] = 11,
            ["centers"] = 11,
            ["shop"] = 12
        };
        private readonly Dictionary<int, string> doorSceneNpcKeys = new()
        {
            [7] = "chef",
            [8] = "keeper",
            [9] = "gamer",
            [10] = "addict",
            [11] = "host",
            [12] = "media"
        };
        private readonly Dictionary<int, string> doorSceneItemTokens = new()
        {
            [7] = "ITEM1",
            [8] = "ITEM2",
            [9] = "ITEM3",
            [10] = "ITEM4",
            [11] = "ITEM5",
            [12] = "ITEM6"
        };

        Vector2 position = Vector2.Zero;
        DialogueManager? dialogueManager;

        Texture2D[] idleTextures, walkTextures, jumpTextures, teaTextures, chibiBTextures, chibiFTextures, chibiRTextures, chibiLTextures,
            chefTextures, keeperTextures, gamerTextures, addictTextures, hostTextures, mediaTextures, friendSadTextures, owlFlyTextures, 
            friendHTextures, owlIdleTextures,
            drowningTextures, sailingTextures, sailing2Textures;
        Animation? idleAnimation, walkAnimation, jumpAnimation, teaAnimation, currentAnimation, chibiBAnimation, chibiFAnimation, 
            chibiRAnimation, chibiLAnimation, chefAnimation, keeperAnimation, gamerAnimation, addictAnimation, hostAnimation, mediaAnimation,
            friendSadAnimation, owlFlyAnimation, friendHAnimation, owlIdleAnimation, drowningAnimation, sailingAnimation, sailing2Animation;

        Texture2D? ground, textureAtlas, hitboxTexture, dialogueBoxTexture, dialogueBoxTexture2,
            owlDieTexture,
            item1Texture, item2Texture, item3Texture, item4Texture, item5Texture, item6Texture;
        Texture2D? mcSailTexture;
        SpriteFont? dialogueFont;

        int groundHeight = 50;
        int playerAge;
        float speed = 4.4f;
        float groundY;
        private float scene13GroundAdjustment = 5f; // Ground is 5 pixels higher for scene 13

        bool isMoving = false;
        bool isJumping = false;


        // dialogue data
        List<DialogueEntry> currentSceneDialogues;
        bool showInteractPrompt = false;
        bool showDoorEnterPrompt = false;
        bool showScene5WallPrompt = false;
        bool showHiddenWallStartPrompt = false;
        string? currentDoorLabel = null;
        private List<DialogueEntry> currentDoorIntroDialogues = new();
        private List<DialogueEntry> currentDoorAfterItemDialogues = new();
        private List<DialogueEntry> currentDoorEdgeDialogues = new();
        private string? currentDoorItemToken = null;
        private bool doorSceneWaitingForItemTrigger = false;
        private bool doorSceneWaitingForAfterItemDialogue = false;
        private bool showDoorItemPopup = false;
        private bool doorSceneIntroSequenceComplete = false;
        private bool showDoorNpcItemPrompt = false;
        private bool doorSceneEdgeTriggerArmed = true;
        private bool doorSceneEdgeChoiceActive = false;
        private bool showExitPrompt = false;
        private int selectedExitPromptIndex = 1;
        private bool doorSceneItemChoicePending = false;
        private int doorSceneItemChoiceDialogueId = -1;
        private bool doorSceneItemChoiceActive = false;
        private int doorSceneItemLoopCount = 0;
        private bool isChoiceActive = false;
        private List<ChoiceEntry> activeChoices = new();
        private int selectedChoiceIndex = 0;
        private bool isNarrationActive = false;
        private List<string> activeNarrationPages = new();
        private int activeNarrationIndex = 0;
        private Action? narrationCompletionAction = null;
        private Action? queuedDialogueCompletionAction = null;
        private bool queuedDialogueAwaitingCompletion = false;
        private bool scene13CenterNarrationTriggered = false;
        private bool doorSceneChoicePending = false;
        private int doorSceneChoiceDialogueId = -1;
        private EndingEntry? activeEnding = null;
        // choice menu loop state when opening choices from a DialogueID
        private int? activeChoiceSourceDialogueId = null;
        private bool activeChoiceLoop = false;
        private int? activeChoiceStayNarrationId = null;
        private Action? hiddenWallDialogueCompletionAction = null;
        private bool hiddenWallDialogueAwaitingCompletion = false;
        private Vector2? hiddenWallCarryPosition = null;
        private bool endingTransitionActive = false;
        private float endingTransitionTimer = 0f;
        private const float EndingTransitionDuration = 1.35f;
        private readonly Random endingGlitchRandom = new();
        private Dictionary<string, HashSet<string>> tableColumnCache = new(StringComparer.OrdinalIgnoreCase);
        private List<DialogueEntry> scene17IntroDialogues = new();
        private List<DialogueEntry> scene17CenterDialogues = new();
        private List<DialogueEntry> scene17AfterNamingDialogues = new();
        private bool scene17CenterDialogueTriggered = false;
        private bool scene17NameInputPending = false;
        private bool scene17AfterNamingStarted = false;
        private bool scene17NameInputActive = false;
        private readonly StringBuilder scene17NameInputBuffer = new();
        private Action<string>? scene17NameInputCompletion = null;
        private string savedOwlName = "Owl";
        private const string Scene17NameSavePath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\scene17_name.json";
        private bool scene19TransitionNarrationTriggered = false;
        private bool scene21BottomBranchTriggered = false;
        private bool scene21LeftBranchTriggered = false;
        private bool scene21RightBranchTriggered = false;

        private enum CompanionRoute
        {
            None = 0,
            Left,
            Right
        }

        // Tracks whether the player has chosen the companion route (left/right) from scene21.
        // This is used to enable follower behaviour on scenes 25-28 without enabling camera tracking.
        private CompanionRoute companionRoute = CompanionRoute.None;
        private int scene16LoopCount = 0;
        private bool scene16CenterDialogueTriggered = false;
        private List<DialogueEntry> scene16CenterDialogues = new();
        private bool scene16FeaturePopupActive = false;
        private bool scene16FeaturePopupShown = false;
        private float scene16FeaturePopupElapsed = 0f;
        private List<DialogueEntry> scene20IntroDialogues = new();
        private List<DialogueEntry> scene20RemainingDialogues = new();
        private bool scene20RemainingStarted = false;
        private bool scene20CenterDialogueTriggered = false;
        private Vector2 scene20CharacterPosition = Vector2.Zero;
        private Vector2 scene20FriendPosition = Vector2.Zero;
        private Vector2 scene20OwlPosition = Vector2.Zero;
        private List<DialogueEntry> scene22CenterDialogues = new();
        private bool scene22CenterDialogueTriggered = false;
        private bool scene22TransitionTriggered = false;
        private bool scene22OwlFadeActive = false;
        private bool scene22OwlGone = false;
        private float scene22OwlFadeTimer = 0f;
        private const float Scene22OwlFadeDuration = 1.4f;
        private bool scene33Blackout = false;
        private bool suppressStaticNpcsOnNextTransition = false;
        private List<DialogueEntry> scene23IntroDialogues = new();
        private List<DialogueEntry> scene23RemainingDialogues = new();
        private bool scene23RemainingStarted = false;
        private bool scene23NarrationTriggered = false;
        private bool scene23BridgeImageShown = false;
        private bool showScene23BridgeImage = false;
        private List<DialogueEntry> scene24CenterDialogues = new();
        private List<DialogueEntry> scene24BeforeOwlExitDialogues = new();
        private List<DialogueEntry> scene24AfterOwlExitDialogues = new();
        private bool scene24CenterDialogueTriggered = false;
        private bool scene24OwlFadeActive = false;
        private bool scene24OwlGone = false;
        private float scene24OwlFadeTimer = 0f;
        private const float Scene24OwlFadeDuration = 1.4f;
        private List<DialogueEntry> scene31CenterDialogues = new();
        private List<DialogueEntry> scene31BeforeFriendExitDialogues = new();
        private List<DialogueEntry> scene31AfterFriendExitDialogues = new();
        private bool scene31CenterDialogueTriggered = false;
        private bool scene31DialogueAwaitingCompletion = false;
        private Action? scene31DialogueCompletionAction = null;
        private bool scene31FriendFadeActive = false;
        private bool scene31FriendGone = false;
        private float scene31FriendFadeTimer = 0f;
        private const float Scene31FriendFadeDuration = 1.4f;
        private bool scene33SequenceStarted = false;
        private bool scene33NotePromptReady = false;
        private bool showScene33NoteImage = false;
        private bool scene21NarrationSkippable = false;
        private int scene34LoopCount = 0;
        private bool scene34NarrationStarted = false;
        private bool scene35SequenceStarted = false;
        private bool scene35NarrationStarted = false;
        private bool scene35TimedChoiceActive = false;
        private bool scene35TimeoutDialogueAwaitingCompletion = false;
        private float scene35ChoiceTimer = 0f;
        private const float Scene35ChoiceDuration = 10f;
        private const int Scene35PullChoiceSourceId = -3502;
        private const float Scene35PullChoiceDuration = 2f;
        private int scene34PostLoopTargetSceneId = 35;
        private int scene36LoopCount = 0;
        private int scene36EndingId = 9;
        private bool scene36AnimationStarted = false;
        private bool scene36AnimationFinished = false;
        private List<DialogueEntry> scene37EndingDialogues = new();
        private int scene37EndingDialogueIndex = 0;
        private float scene37EndingDialogueTimer = 0f;
        private const float Scene37EndingDialogueDuration = 2f;
        private int scene37EndingAnimationFrameIndex = 0;
        private float scene37EndingAnimationTimer = 0f;
        private const float Scene37EndingAnimationFrameDuration = 0.9f;
        private bool scene37EndingAnimationLooped = false;
        private bool scene37SequenceStarted = false;
        private bool scene37BlackoutActive = false;
        private bool scene37EndingTriggered = false;
        private float scene37BlackoutTimer = 0f;
        private const float Scene37BlackoutDuration = 10f;
        private List<DialogueEntry> scene37AutoDialogues = new();
        private bool scene37AutoDialogueComplete = false;
        private bool scene38CenterBlackoutTriggered = false;
        private int scene39LoopCount = 0;
        private bool scene39IntroStarted = false;
        private bool scene39CenterEventTriggered = false;
        private List<DialogueEntry> scene39IntroDialogues = new();
        private List<DialogueEntry> scene39CenterDialogues = new();
        private List<DialogueEntry> scene39ActiveAutoDialogues = new();
        private int scene39AutoDialogueIndex = 0;
        private float scene39AutoDialogueTimer = 0f;
        private Action? scene39AutoDialogueCompletion = null;
        private const float Scene39AutoDialogueDuration = 2f;
        private bool scene40SequenceStarted = false;
        private bool scene41SequenceStarted = false;
        private bool scene41TimedChoiceActive = false;
        private bool scene41SuccessBlackoutActive = false;
        private const int Scene41ChoiceSourceId = -4101;
        private const float Scene41ChoiceDuration = 10f;
        private float scene41ChoiceTimer = 0f;
        private Texture2D? activeFullscreenImage = null;
        private string activeFullscreenImageDismissText = "Press ENTER to Exit";
        private Action? activeFullscreenImageCompletion = null;
        private List<DialogueEntry> scene29IntroDialogues = new();
        private List<DialogueEntry> scene29RemainingDialogues = new();
        private bool scene29RemainingStarted = false;
        private bool scene29CenterDialogueTriggered = false;
        private bool scene29FriendFadeActive = false;
        private float scene29FriendAlpha = 1f;
        private float scene29PlayerAlpha = 1f;
        private bool scene29GlitchTransitionActive = false;
        private float scene29GlitchTransitionTimer = 0f;
        private const float Scene29GlitchTransitionDuration = 2f;
        private bool scene30SequenceStarted = false;
        private bool scene30SequenceComplete = false;
        private bool scene30BackViewPauseActive = false;
        private float scene30BackViewPauseTimer = 0f;
        private Action? scene30BackViewPauseCompletion = null;
        private int scene30PendingChoiceStage = 0;
        private const int Scene30IgnoreChoiceOneSourceId = -3001;
        private const int Scene30IgnoreChoiceTwoSourceId = -3002;
        private const int Scene30DoNotIgnoreChoiceSourceId = -3003;
        private const int Scene30ListenChoiceSourceId = -3004;
        private const int Scene30AgeChoiceSourceId = -3005;
        private int scene32SequenceStep = 0;
        private bool scene32Started = false;
        private Action? scene32DialogueCompletionAction = null;
        private bool scene32DialogueAwaitingCompletion = false;
        private int scene32OwlVisualState = 0;
        private bool scene32DialoguesCompleted = false;
        private bool scene32RightExitBlackoutActive = false;
        private float scene32RightExitBlackoutTimer = 0f;
        private const float Scene32RightExitBlackoutDuration = 10f;
        private const float FollowerOwlYOffset = -42f;

        // tutorial overlay
        Texture2D tutorialImage;
        Texture2D bridgeImage;
        Texture2D noteImage;
        Texture2D wavesImage;
        Texture2D waves2Image;
        bool showTutorialPrompt = false;
        bool showTutorialImage = false;

        // camera zoom
        private float cameraZoom = 1.0f;

        private int scene4LoopCount = 0;

        // Add this field at the top of your class if you want to be explicit:
        private bool scene4CameraTracking = true;
        private int currentAttemptNumber = 1;
        private bool analyticsEnabled = true;
        private DateTime sceneAnalyticsStartedAtUtc = DateTime.MinValue;
        private int activePlayerId = 0;
        private string activePlayerName = "Guest";
        private int transientDialogueIdSeed = -10000;
        private int GetTransientDialogueId() => transientDialogueIdSeed--;
        private int sceneDialogueLineCount = 0;
        private int sceneDialogueCharCount = 0;
        private int sceneDialogueAdvanceCount = 0;
        private int sceneInteractCount = 0;
        private int sceneActionCount = 0;
        private bool sceneAnalyticsActive = false;

        public Game1(bool continueFromSave = false, int? requestedEndingId = null)
        {
            this.continueFromSave = continueFromSave;
            this.requestedEndingId = requestedEndingId;
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            camera = new Camera();
            Exiting += OnGameExiting;

        }

        private void BuildScene21Labels()
        {
            scene21Labels.Clear();
            scene21LabelCenters.Clear();

            if (scene21InteractLayer.Count == 0 || scene21CollisionLayer.Count == 0)
                return;

            var triggerTiles = scene21InteractLayer
                .Where(kv => kv.Value == 0)
                .Select(kv => kv.Key)
                .ToHashSet();

            if (triggerTiles.Count == 0)
                return;

            // Group contiguous trigger tiles similarly to scene6, then take centers and topY
            List<List<Vector2>> groups = new();

            while (triggerTiles.Count > 0)
            {
                Vector2 start = triggerTiles.First();
                Queue<Vector2> q = new();
                List<Vector2> group = new();
                q.Enqueue(start);
                triggerTiles.Remove(start);
                while (q.Count > 0)
                {
                    Vector2 cur = q.Dequeue();
                    group.Add(cur);
                    Vector2[] neigh = { new Vector2(cur.X+1, cur.Y), new Vector2(cur.X-1, cur.Y), new Vector2(cur.X, cur.Y+1), new Vector2(cur.X, cur.Y-1) };
                    foreach (var n in neigh)
                    {
                        if (triggerTiles.Remove(n)) q.Enqueue(n);
                    }
                }
                groups.Add(group);
            }

            // Use the main "goodbye" map as the reference coordinate space for drawing and world->tile conversions
            Dictionary<Vector2, int> referenceMap = GetSceneMap(21);
            var ordered = groups.OrderBy(g => g.Average(t => t.X)).ToList();

            // compute map center X in reference map coordinates so we can detect left/right at runtime
            float mapMinX = referenceMap.Keys.Min(k => k.X);
            float mapMaxX = referenceMap.Keys.Max(k => k.X);
            float mapCenterX = (mapMinX + mapMaxX) / 2f;

            int companionIndex = 0;
            for (int i = 0; i < ordered.Count; i++)
            {
                var group = ordered[i];
                float centerX = group.Average(t => t.X);
                float topY = group.Min(t => t.Y);

                // Use a visually-identical base name with a hidden separator so display shows "Companion"
                string labelKey = "Companion\u200B" + (companionIndex++).ToString();

                // Map label tiles into the same coordinate space as the reference map.
                var mappedTiles = new List<Vector2>();
                foreach (var tile in GetScene21InteractTiles(group))
                {
                    Vector2 mappedTile = tile;

                    if (!referenceMap.ContainsKey(mappedTile))
                    {
                        mappedTile = referenceMap.Keys
                            .OrderBy(k => Vector2.DistanceSquared(k, tile))
                            .FirstOrDefault();
                    }

                    if (referenceMap.ContainsKey(mappedTile))
                    {
                        mappedTiles.Add(mappedTile);
                        scene21Labels[mappedTile] = labelKey;
                    }
                }

                // Compute the center/topY using the mapped tiles so drawing aligns with the reference map.
                if (mappedTiles.Count > 0)
                {
                    float mappedCenterX = mappedTiles.Average(t => t.X);
                    float mappedTopY = mappedTiles.Min(t => t.Y);
                    // Lower the label so it sits closer to the trigger marker.
                    scene21LabelCenters[labelKey] = new Vector2(mappedCenterX, mappedTopY + 1.0f);
                }
                else
                {
                    scene21LabelCenters[labelKey] = new Vector2(centerX, topY + 1.0f);
                }
            }
        }

        private IEnumerable<Vector2> GetScene21InteractTiles(List<Vector2> triggerGroup)
        {
            HashSet<Vector2> interactTiles = new(triggerGroup);

            foreach (Vector2 triggerTile in triggerGroup)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        Vector2 candidate = new Vector2(triggerTile.X + dx, triggerTile.Y + dy);
                        if (scene21CollisionLayer.TryGetValue(candidate, out int tileValue) && tileValue == 1)
                            interactTiles.Add(candidate);
                    }
                }
            }

            return interactTiles;
        }

        protected override void Initialize()
        {
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // idle
            idleTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("MC_idle1"),
                Content.Load<Texture2D>("MC_idle2")
            };

            // walk
            walkTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("MC_walk1"),
                Content.Load<Texture2D>("MC_walk2"),
                Content.Load<Texture2D>("MC_walk3"),
                Content.Load<Texture2D>("MC_walk4")
            };

            //jump
            jumpTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("MC_jump1"),
                Content.Load<Texture2D>("MC_jump2"),
                Content.Load<Texture2D>("MC_jump3"),
                Content.Load<Texture2D>("MC_jump4"),
                Content.Load<Texture2D>("MC_jump5")
            };

            //sipping tea
            teaTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("MC_tea1"),
                Content.Load<Texture2D>("MC_tea2"),
                Content.Load<Texture2D>("MC_tea3"),
                Content.Load<Texture2D>("MC_tea4"),
                Content.Load<Texture2D>("MC_tea5"),
            };

            //scene5
            textureAtlas = Content.Load<Texture2D>("New Piskel");
            hitboxTexture = Content.Load<Texture2D>("Colissions");

            //chibi Front
            chibiFTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("MC_chibiF1"),
                Content.Load<Texture2D>("MC_chibiF2"),
            };


            //chibi Back
            chibiBTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("MC_chibiB1"),
                Content.Load<Texture2D>("MC_chibiB2"),
            };

            //chibi Right
            chibiRTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("MC_chibiR1"),
                Content.Load<Texture2D>("MC_chibiR2"),
            };

            //chibi Left
            chibiLTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("MC_chibiL1"),
                Content.Load<Texture2D>("MC_chibiL2"),
            };

            //resturant
            chefTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("Chef1"),
                Content.Load<Texture2D>("Chef2"),
            };

            //inn
            keeperTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("Keeper1"),
                Content.Load<Texture2D>("Keeper2"),
            };

            //gamehub
            gamerTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("Gamer1"),
                Content.Load<Texture2D>("Gamer2"),
            };

            //alleyway
            addictTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("Addict1"),
                Content.Load<Texture2D>("Addict2"),
            };

            //entertainment
            hostTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("Host1"),
                Content.Load<Texture2D>("Host2"),
            };

            //socmed
            mediaTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("media1"),
                Content.Load<Texture2D>("media2"),
            };

            friendSadTextures = new Texture2D[]
            {
                LoadTextureWithFallback("Friend_sad1", "Friend_sad2"),
                LoadTextureWithFallback("Friend_sad2", "Friend_sad1")
            };

            owlFlyTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("Owl_fly1"),
                Content.Load<Texture2D>("Owl_fly2")
            };

            owlIdleTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("Owl_idle1"),
                Content.Load<Texture2D>("Owl_idle2")
            };

            owlDieTexture = Content.Load<Texture2D>("owlDie");

            friendHTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("Friend_joy1"),
                Content.Load<Texture2D>("Friend_joy2")
            };

            drowningTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("drowning1"),
                Content.Load<Texture2D>("drowning2"),
                Content.Load<Texture2D>("drowning3"),
                Content.Load<Texture2D>("drowning4"),
                Content.Load<Texture2D>("drowning5"),
                Content.Load<Texture2D>("drowning6"),
                Content.Load<Texture2D>("drowning7")
            };

            // Use fallback loader for sailing textures so missing assets won't crash the game at startup.
            sailingTextures = new Texture2D[]
            {
                LoadTextureWithFallback("MC_sailing1"),
                LoadTextureWithFallback("MC_sailing2")
            };

            sailing2Textures = new Texture2D[]
            {
                LoadTextureWithFallback("MC_sailing3"),
                LoadTextureWithFallback("MC_sailing4")
            };

            // center sailing sprite for scene41
            mcSailTexture = LoadTextureWithFallback("mc_sail", "MC_idle1");

            item1Texture = Content.Load<Texture2D>("Item1");
            item2Texture = Content.Load<Texture2D>("Item2");
            item3Texture = Content.Load<Texture2D>("Item3");
            item4Texture = Content.Load<Texture2D>("Item4");
            item5Texture = Content.Load<Texture2D>("Item5");
            item6Texture = Content.Load<Texture2D>("Item6");

            idleAnimation = new Animation(idleTextures, 0.8f);
            walkAnimation = new Animation(walkTextures, 0.6f);
            jumpAnimation = new Animation(jumpTextures, 0.4f);
            teaAnimation = new Animation(teaTextures, 0.8f);
            chibiBAnimation = new Animation(chibiBTextures, 0.7f);
            chibiFAnimation = new Animation(chibiFTextures, 0.7f);
            chibiRAnimation = new Animation(chibiRTextures, 0.7f);
            chibiLAnimation = new Animation(chibiLTextures, 0.7f);
            chefAnimation = new Animation(chefTextures, 0.8f);
            keeperAnimation = new Animation(keeperTextures, 0.8f);
            gamerAnimation = new Animation(gamerTextures, 0.8f);
            addictAnimation = new Animation(addictTextures, 0.8f);
            hostAnimation = new Animation(hostTextures, 0.8f);
            mediaAnimation = new Animation(mediaTextures, 0.8f);
            friendSadAnimation = new Animation(friendSadTextures, 0.8f);
            owlFlyAnimation = new Animation(owlFlyTextures, 0.45f);
            owlIdleAnimation = new Animation(owlIdleTextures, 0.65f);
            friendHAnimation = new Animation(friendHTextures, 0.8f);
            drowningAnimation = new Animation(drowningTextures, 0.45f);
            sailingAnimation = new Animation(sailingTextures, 0.75f);
            sailing2Animation = new Animation(sailing2Textures, 0.72f);
            currentAnimation = idleAnimation;

            ground = new Texture2D(GraphicsDevice, 1, 1);
            ground.SetData(new[] { Color.White });

            groundY = GetSceneGroundY(1);
            position = new Vector2(100, groundY);

            dialogueBoxTexture = Content.Load<Texture2D>("DialogueBox");
            dialogueBoxTexture2 = Content.Load<Texture2D>("DialogueBox2");
            dialogueFont = Content.Load<SpriteFont>("DialogueFont");
            dialogueManager = new DialogueManager(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, groundY);

            tutorialImage = Content.Load<Texture2D>("tutorial");
            bridgeImage = Content.Load<Texture2D>("bridge");
            noteImage = LoadTextureWithFallback("Note", "note");
            wavesImage = LoadTextureWithFallback("waVES", "wave1");
            waves2Image = LoadTextureWithFallback("waVES2", "wave2");

            // read active player context
            LoadActivePlayerProfile();
            LoadScene17SavedName();

            // SceneManager initialization
            sceneManager = new SceneManager(Content);

            // Open the database connection
            conn = new SqliteConnection($@"Data Source={GameDatabasePath}");
            conn.Open();
            EnsureSaveDataTable();
            EnsureAchievementTable();
            EnsureAnalyticsTables();

            scene5CollisionLayer = LoadMap(@"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameProj\Data\colissions.csv");
            scene6CollisionLayer = LoadMap(@"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameProj\Data\colissions2.csv");
            scene21CollisionLayer = LoadMap(@"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameProj\Data\colissions3.csv");
            main = LoadMap(@"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameProj\Data\mainpath.csv");
            doors = LoadMap(@"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameProj\Data\doors.csv");
            goodbye = LoadMap(@"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameProj\Data\goodbye.csv");
            scene6InteractLayer = LoadMap(@"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameProj\Data\interact2.csv");
            scene21InteractLayer = LoadMap(@"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameProj\Data\interact1.csv");
            BuildScene6DoorLabels();
            BuildScene21Labels();

            SaveDataEntry? latestSave = requestedEndingId.HasValue ? null : (continueFromSave ? LoadLatestSaveData() : null);
            currentAttemptNumber = ResolveAttemptNumber(latestSave);
            analyticsEnabled = !requestedEndingId.HasValue;
            if (latestSave != null)
            {
                playerAge = latestSave.Age;
                LoadSceneWithDialogues(latestSave.SceneID);
                RestoreSavedProgress(latestSave);
            }
            else
            {
                // Load first scene (this will set position correctly for scene 1)
                LoadSceneWithDialogues(1);
            }

            if (requestedEndingId.HasValue)
            {
                activeEnding = LoadEnding(requestedEndingId.Value);
                endingTransitionActive = false;
                endingTransitionTimer = EndingTransitionDuration;
                MediaPlayer.Stop();
            }
        }

        // In LoadSceneWithDialogues, update the position and camera zoom for scene 5:
        private void LoadSceneWithDialogues(int sceneId)
        {
            FinalizeCurrentSceneAnalytics("SceneTransition");
            int previousSceneId = currentSceneId;
            currentScene = sceneManager.LoadScene(sceneId);
            currentSceneId = sceneId; // track current scene id for camera/logic
            dialogueManager?.Reset();

            // reset scene-local prompts/triggers that should not persist across scenes
            showTutorialPrompt = false;
            showTutorialImage = false;
            showScene23BridgeImage = false;
            showScene33NoteImage = false;
            scene33NotePromptReady = false;
            scene33Blackout = false;
            showTeaPrompt = false;
            teaAnimationPlaying = false;
            showDoorEnterPrompt = false;
            showScene5WallPrompt = false;
            currentDoorLabel = null;
            currentDoorIntroDialogues = new List<DialogueEntry>();
            currentDoorAfterItemDialogues = new List<DialogueEntry>();
            currentDoorEdgeDialogues = new List<DialogueEntry>();
            currentDoorItemToken = null;
            doorSceneWaitingForItemTrigger = false;
            doorSceneWaitingForAfterItemDialogue = false;
            showDoorItemPopup = false;
            doorSceneIntroSequenceComplete = false;
            showDoorNpcItemPrompt = false;
            doorSceneEdgeTriggerArmed = true;
            doorSceneEdgeChoiceActive = false;
            doorSceneChoicePending = false;
            doorSceneChoiceDialogueId = -1;
            isNarrationActive = false;
            activeNarrationPages.Clear();
            activeNarrationIndex = 0;
            narrationCompletionAction = null;
            queuedDialogueCompletionAction = null;
            queuedDialogueAwaitingCompletion = false;
            scene13CenterNarrationTriggered = false;
            scene21NarrationSkippable = false;
            showHiddenWallStartPrompt = false;
            hiddenWallDialogueCompletionAction = null;
            hiddenWallDialogueAwaitingCompletion = false;
            showInteractPrompt = false;
            showDoorEnterPrompt = false;
            showScene5WallPrompt = false;
            showDoorNpcItemPrompt = false;
            showTeaPrompt = false;
            activeChoiceSourceDialogueId = null;
            activeChoiceLoop = false;
            activeChoiceStayNarrationId = null;
            scene17CenterDialogueTriggered = false;
            scene17NameInputPending = false;
            scene17AfterNamingStarted = false;
            scene17NameInputActive = false;
            scene17NameInputBuffer.Clear();
            scene17NameInputCompletion = null;
            scene17IntroDialogues.Clear();
            scene17CenterDialogues.Clear();
            scene17AfterNamingDialogues.Clear();
            scene22TransitionTriggered = false;
            scene21BottomBranchTriggered = false;
            scene21LeftBranchTriggered = false;
            scene21RightBranchTriggered = false;
            scene20IntroDialogues.Clear();
            scene20RemainingDialogues.Clear();
            scene20RemainingStarted = false;
            scene20CenterDialogueTriggered = false;
            scene22CenterDialogues.Clear();
            scene22CenterDialogueTriggered = false;
            scene22OwlFadeActive = false;
            scene22OwlGone = false;
            scene22OwlFadeTimer = 0f;
            scene23IntroDialogues.Clear();
            scene23RemainingDialogues.Clear();
            scene23RemainingStarted = false;
            scene23NarrationTriggered = false;
            scene23BridgeImageShown = false;
            scene24CenterDialogues.Clear();
            scene24BeforeOwlExitDialogues.Clear();
            scene24AfterOwlExitDialogues.Clear();
            scene24CenterDialogueTriggered = false;
            scene24OwlFadeActive = false;
            scene24OwlGone = false;
            scene24OwlFadeTimer = 0f;
            scene31CenterDialogues.Clear();
            scene31BeforeFriendExitDialogues.Clear();
            scene31AfterFriendExitDialogues.Clear();
            scene31CenterDialogueTriggered = false;
            scene31DialogueAwaitingCompletion = false;
            scene31DialogueCompletionAction = null;
            scene31FriendFadeActive = false;
            scene31FriendGone = false;
            scene31FriendFadeTimer = 0f;
            scene33SequenceStarted = false;
            scene29IntroDialogues.Clear();
            scene29RemainingDialogues.Clear();
            scene29RemainingStarted = false;
            scene29CenterDialogueTriggered = false;
            scene29FriendFadeActive = false;
            scene29FriendAlpha = 1f;
            scene29PlayerAlpha = 1f;
            scene29GlitchTransitionActive = false;
            scene29GlitchTransitionTimer = 0f;
            scene30SequenceStarted = false;
            scene30SequenceComplete = false;
            scene30BackViewPauseActive = false;
            scene30BackViewPauseTimer = 0f;
            scene30BackViewPauseCompletion = null;
            scene30PendingChoiceStage = 0;
            scene34NarrationStarted = false;
            scene35SequenceStarted = false;
            scene35NarrationStarted = false;
            scene35TimedChoiceActive = false;
            scene35TimeoutDialogueAwaitingCompletion = false;
            scene35ChoiceTimer = 0f;
            scene36AnimationStarted = false;
            scene36AnimationFinished = false;
            scene37EndingDialogues.Clear();
            scene37EndingDialogueIndex = 0;
            scene37EndingDialogueTimer = 0f;
            scene37EndingAnimationFrameIndex = 0;
            scene37EndingAnimationTimer = 0f;
            scene37EndingAnimationLooped = false;
            scene37SequenceStarted = false;
            scene37BlackoutActive = false;
            scene37EndingTriggered = false;
            scene37BlackoutTimer = 0f;
            scene37AutoDialogues.Clear();
            scene37AutoDialogueComplete = false;
            scene38CenterBlackoutTriggered = false;
            scene39IntroStarted = false;
            scene39CenterEventTriggered = false;
            scene39IntroDialogues.Clear();
            scene39CenterDialogues.Clear();
            scene39ActiveAutoDialogues.Clear();
            scene39AutoDialogueIndex = 0;
            scene39AutoDialogueTimer = 0f;
            scene39AutoDialogueCompletion = null;
            scene40SequenceStarted = false;
            scene41SequenceStarted = false;
            scene41TimedChoiceActive = false;
            scene41ChoiceTimer = 0f;
            scene41SuccessBlackoutActive = false;
            if (sceneId != 39)
                scene39LoopCount = 0;
            activeFullscreenImage = null;
            activeFullscreenImageCompletion = null;
            if (sceneId != 37)
                scene36EndingId = 9;
            scene32SequenceStep = 0;
            scene32Started = false;
            scene32DialogueCompletionAction = null;
            scene32DialogueAwaitingCompletion = false;
            scene32OwlVisualState = 0;
            scene32DialoguesCompleted = false;
            scene32RightExitBlackoutActive = false;
            scene32RightExitBlackoutTimer = 0f;
            scene19TransitionNarrationTriggered = false;
            scene16CenterDialogueTriggered = false;
            scene16CenterDialogues.Clear();
            if (sceneId != 16)
            {
                scene16FeaturePopupActive = false;
                scene16FeaturePopupElapsed = 0f;
            }
            else if (previousSceneId != 16 || !scene16FeaturePopupShown)
            {
                scene16FeaturePopupActive = true;
                scene16FeaturePopupShown = true;
                scene16FeaturePopupElapsed = 0f;
            }

            if (sceneId != 16)
            {
                scene16LoopCount = 0;
                scene16FeaturePopupShown = false;
            }
            else if (currentSceneId != 16)
            {
                scene16LoopCount = 0;
            }

            if (!IsHiddenWallScene(sceneId))
                hiddenWallCarryPosition = null;

            if (sceneId == 3 || sceneId == 4)
            {
                speed = 4.4f;
                currentAnimation = idleAnimation;
                facingRight = true;
                position = new Vector2(speed * 10, GetSceneGroundY(sceneId));
            }
            else if (IsTopDownScene(sceneId))
            {
                speed = 2.5f;
                cameraZoom = 1.0f;
                currentAnimation = chibiFAnimation;

                Dictionary<Vector2, int> currentMap = GetSceneMap(sceneId);
                int displayTileSize = GetTopDownDisplayTileSize(currentMap);

                int minX = currentMap.Keys.Min(k => (int)k.X);
                int minY = currentMap.Keys.Min(k => (int)k.Y);
                int maxX = currentMap.Keys.Max(k => (int)k.X);
                int maxY = currentMap.Keys.Max(k => (int)k.Y);

                int mapWidth = (maxX - minX + 1) * displayTileSize;
                int mapHeight = (maxY - minY + 1) * displayTileSize;

                int offsetX = (_graphics.PreferredBackBufferWidth - mapWidth) / 2;
                int offsetY = (_graphics.PreferredBackBufferHeight - mapHeight) / 2;

                Dictionary<Vector2, int> currentCollisionLayer = GetSceneCollisionLayer(sceneId);
                var walkableTiles = currentCollisionLayer.Where(kv => kv.Value == 1).Select(kv => kv.Key).ToList();

                if (walkableTiles.Count > 0)
                {
                    Vector2 spawnTile;

                    if (sceneId == 6)
                    {
                        float topY = walkableTiles.Min(t => t.Y);
                        var topTiles = walkableTiles.Where(t => t.Y == topY).ToList();
                        float centerX = (topTiles.Min(t => t.X) + topTiles.Max(t => t.X)) / 2f;
                        spawnTile = topTiles
                            .OrderBy(t => Math.Abs(t.X - centerX))
                            .First();
                    }
                    else if (sceneId == 5)
                    {
                        spawnTile = ResolveScene5SpawnTile(walkableTiles);
                    }
                    else if (sceneId == 21)
                    {
                        float topY = walkableTiles.Min(t => t.Y);
                        var topTiles = walkableTiles.Where(t => t.Y == topY).ToList();
                        float centerX = (topTiles.Min(t => t.X) + topTiles.Max(t => t.X)) / 2f;
                        spawnTile = topTiles
                            .OrderBy(t => Math.Abs(t.X - centerX))
                            .First();
                    }
                    else
                    {
                        float bottomY = walkableTiles.Max(t => t.Y);
                        var bottomTiles = walkableTiles.Where(t => t.Y == bottomY).ToList();
                        float centerX = (bottomTiles.Min(t => t.X) + bottomTiles.Max(t => t.X)) / 2f;
                        spawnTile = bottomTiles
                            .OrderBy(t => Math.Abs(t.X - centerX))
                            .First();
                    }

                    position = new Vector2(offsetX + ((spawnTile.X - minX) * displayTileSize) + displayTileSize / 2f,
                        offsetY + ((spawnTile.Y - minY) * displayTileSize) + displayTileSize / 2f);
                }

                else
                {
                    position = new Vector2(
                        _graphics.PreferredBackBufferWidth / 2f,
                        _graphics.PreferredBackBufferHeight / 2f
                    );
                }
            }
                else
                {
                    speed = 4.4f;
                    cameraZoom = 1.0f;
                    facingRight = true;
                    currentAnimation = idleAnimation;
                    position = new Vector2(100, GetSceneGroundY(sceneId));

                    if ((sceneId == HiddenWallRetrySceneId || sceneId == HiddenWallFinalSceneId) && hiddenWallCarryPosition.HasValue)
                        position = hiddenWallCarryPosition.Value;
                    else if ((sceneId == 22 || sceneId == 23 || sceneId == 24 || sceneId == 30 || sceneId == 31 || sceneId == 32) && scene20CharacterPosition != Vector2.Zero)
                        position = scene20CharacterPosition;
                }

            try
            {
                currentSceneDialogues = LoadDialogues(currentScene.SceneID, playerAge);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dialogues: {ex}");
                currentSceneDialogues = new List<DialogueEntry>();
            }

            // fallback dialogues if database returned nothing
            if (currentSceneDialogues.Count == 0)
            {
                currentSceneDialogues = new List<DialogueEntry>
                {};
            }

            if (IsHiddenWallScene(sceneId))
            {
                currentSceneDialogues = new List<DialogueEntry>();
                currentScene.IntroDialoguePlayed = true;
            }

            List<DialogueEntry> analyticsDialogues = new List<DialogueEntry>(currentSceneDialogues);

            if (IsDoorScene(sceneId))
            {
                SplitDoorSceneDialogues(currentSceneDialogues, out currentDoorIntroDialogues, out currentDoorAfterItemDialogues, out currentDoorEdgeDialogues, out currentDoorItemToken);
                if (string.IsNullOrWhiteSpace(currentDoorItemToken) && doorSceneItemTokens.TryGetValue(sceneId, out string fallbackItemToken))
                    currentDoorItemToken = fallbackItemToken;
                doorSceneChoiceDialogueId = ResolveDoorSceneChoiceDialogueId(currentDoorEdgeDialogues);
                currentSceneDialogues = currentDoorIntroDialogues;
            }
            else if (sceneId == 17)
            {
                PrepareScene17Dialogues();
                currentSceneDialogues = scene17IntroDialogues;
            }
            else if (sceneId == 16)
            {
                scene16CenterDialogues = currentSceneDialogues
                    .OrderBy(entry => entry.DialogueID)
                    .ToList();
                currentSceneDialogues = new List<DialogueEntry>();
                currentScene.IntroDialoguePlayed = true;
            }
            else if (sceneId == 37)
            {
                currentSceneDialogues = new List<DialogueEntry>();
                currentScene.IntroDialoguePlayed = true;
            }
            else if (sceneId == 20 || sceneId == 23 || sceneId == 31)
            {
                PrepareStaticPartySceneDialogues(sceneId, currentSceneDialogues,
                    out List<DialogueEntry> introDialogues,
                    out List<DialogueEntry> remainingDialogues);

                if (sceneId == 20)
                {
                    scene20IntroDialogues = introDialogues;
                    scene20RemainingDialogues = remainingDialogues;
                    currentSceneDialogues = scene20IntroDialogues;
                    scene20CharacterPosition = position;
                    scene20FriendPosition = new Vector2(_graphics.PreferredBackBufferWidth * 0.76f, GetSceneGroundY(sceneId));
                    scene20OwlPosition = new Vector2(_graphics.PreferredBackBufferWidth * 0.88f, GetSceneGroundY(sceneId));
                }
                else
                {
                    if (sceneId == 23)
                    {
                        var orderedScene23Dialogues = currentSceneDialogues
                            .OrderBy(entry => entry.DialogueID)
                            .ToList();
                        scene23IntroDialogues = orderedScene23Dialogues.Take(2).ToList();
                        scene23RemainingDialogues = orderedScene23Dialogues.Skip(2).ToList();
                        currentSceneDialogues = new List<DialogueEntry>();
                        currentScene.IntroDialoguePlayed = true;
                    }
                    else if (sceneId == 31)
                    {
                        scene31CenterDialogues = currentSceneDialogues
                            .OrderBy(entry => entry.DialogueID)
                            .ToList();
                        int friendExitIndex = scene31CenterDialogues.FindIndex(entry => entry.DialogueID == 251);
                        if (friendExitIndex >= 0)
                        {
                            scene31BeforeFriendExitDialogues = scene31CenterDialogues.Take(friendExitIndex + 1).ToList();
                            scene31AfterFriendExitDialogues = scene31CenterDialogues.Skip(friendExitIndex + 1).ToList();
                        }
                        else
                        {
                            scene31BeforeFriendExitDialogues = scene31CenterDialogues;
                            scene31AfterFriendExitDialogues = new List<DialogueEntry>();
                        }
                        currentSceneDialogues = new List<DialogueEntry>();
                        currentScene.IntroDialoguePlayed = true;
                    }
                    else
                    {
                        // For other scenes in this group (e.g. 30), just use the intro dialogues directly
                        currentSceneDialogues = introDialogues;
                    }
                }
                // If we suppressed static NPCs for this load, clear positions and reset flag
                if (suppressStaticNpcsOnNextTransition && (sceneId == 22 || sceneId == 23))
                {
                    scene20FriendPosition = Vector2.Zero;
                    scene20OwlPosition = Vector2.Zero;
                    suppressStaticNpcsOnNextTransition = false;
                }
            }
            else if (sceneId == 22)
            {
                scene22CenterDialogues = currentSceneDialogues
                    .OrderBy(entry => entry.DialogueID)
                    .ToList();
                currentSceneDialogues = new List<DialogueEntry>();
                currentScene.IntroDialoguePlayed = true;
            }
            else if (sceneId == 24)
            {
                scene24CenterDialogues = currentSceneDialogues
                    .OrderBy(entry => entry.DialogueID)
                    .ToList();
                scene24BeforeOwlExitDialogues = scene24CenterDialogues
                    .Where(entry => entry.DialogueID <= 211)
                    .ToList();
                scene24AfterOwlExitDialogues = scene24CenterDialogues
                    .Where(entry => entry.DialogueID > 211)
                    .ToList();
                currentSceneDialogues = new List<DialogueEntry>();
                currentScene.IntroDialoguePlayed = true;
            }
            else if (sceneId == 29)
            {
                var orderedScene29Dialogues = currentSceneDialogues
                    .OrderBy(entry => entry.DialogueID)
                    .ToList();
                scene29IntroDialogues = orderedScene29Dialogues.Take(1).ToList();
                scene29RemainingDialogues = orderedScene29Dialogues.Skip(1).ToList();
                scene29CenterDialogueTriggered = false;
                scene29RemainingStarted = false;
                currentSceneDialogues = new List<DialogueEntry>();
                currentScene.IntroDialoguePlayed = true;
            }
            else if (sceneId == 32)
            {
                currentSceneDialogues = new List<DialogueEntry>();
                currentScene.IntroDialoguePlayed = true;
            }
            else if (sceneId == 34)
            {
                scene34LoopCount++;
                if (scene34LoopCount == 1)
                {
                    currentSceneDialogues = currentSceneDialogues
                        .OrderBy(entry => entry.DialogueID)
                        .ToList();
                }
                else
                {
                    currentSceneDialogues = new List<DialogueEntry>();
                    currentScene.IntroDialoguePlayed = true;
                }
            }
            else if (sceneId == 35 || sceneId == 36 || sceneId == 37 || sceneId == 38)
            {
                currentSceneDialogues = sceneId == 35
                    ? currentSceneDialogues
                        .Where(entry => entry.DialogueID >= 287 && entry.DialogueID <= 298)
                        .OrderBy(entry => entry.DialogueID)
                        .ToList()
                    : new List<DialogueEntry>();
                if (sceneId == 36 || sceneId == 37 || sceneId == 38)
                    currentScene.IntroDialoguePlayed = true;

                if (sceneId == 36)
                    scene36LoopCount++;
            }
            else if (sceneId == 39)
            {
                scene39LoopCount++;
                var orderedScene39Dialogues = currentSceneDialogues
                    .OrderBy(entry => entry.DialogueID)
                    .ToList();

                scene39IntroDialogues = orderedScene39Dialogues.Take(3).ToList();
                if (scene39IntroDialogues.Count == 0)
                {
                    scene39IntroDialogues = BuildTransientSceneDialogues(39,
                        "It may be a small boat, but this is quite enough for me. I have no prior sailing experience yet, but I could learn a lot from this.",
                        "The waves are still calm in this area too. I hope this peace will continue, though.",
                        "I might as well treat this as a vacation, a quick side quest-a dangerous one too.");
                }

                scene39CenterDialogues = orderedScene39Dialogues.Skip(3).Take(1).ToList();
                if (scene39CenterDialogues.Count == 0)
                    scene39CenterDialogues = BuildTransientSceneDialogues(39, "Hu-huhh?!");

                currentSceneDialogues = new List<DialogueEntry>();
                currentScene.IntroDialoguePlayed = true;
            }
            else if (sceneId == 30)
            {
                currentSceneDialogues = new List<DialogueEntry>();
                currentScene.IntroDialoguePlayed = true;
                scene30SequenceStarted = false;
                scene30SequenceComplete = false;
                scene30BackViewPauseActive = false;
                scene30BackViewPauseCompletion = null;
                scene30PendingChoiceStage = 0;
            }
            else if (sceneId == 40 || sceneId == 41)
            {
                currentSceneDialogues = new List<DialogueEntry>();
                currentScene.IntroDialoguePlayed = true;
            }

            // auto-trigger dialogues (will start the regular dialogue box)
            if (currentSceneDialogues.Count > 0 && !currentScene.IntroDialoguePlayed)
            {
                bool startImmediately = sceneId == 17 || sceneId == 20 || sceneId == 29 || sceneId == 30 || sceneId == 34 || sceneId == 35 || IsDoorScene(sceneId);
                bool requireInteractBetween = !IsDoorScene(sceneId);
                dialogueManager.StartDialogue(currentSceneDialogues, dialogueBoxTexture, dialogueBoxTexture2, requireInteractBetween, startImmediately);
                currentScene.IntroDialoguePlayed = true;
            }
            else if (IsDoorScene(sceneId) && currentDoorItemToken != null)
            {
                doorSceneWaitingForItemTrigger = true;
            }

            InitializeHiddenWallScene(sceneId);

            // background + music
            if (!string.IsNullOrEmpty(currentScene.BackgroundAssetName))
            {
                try
                {
                    string backgroundAssetName = ResolveBackgroundAsset(currentScene.BackgroundAssetName);
                    currentBackground = Content.Load<Texture2D>(backgroundAssetName);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Missing background '{currentScene.BackgroundAssetName}': {ex.Message}");
                    currentBackground = Content.Load<Texture2D>("black");
                }
            }

            if (!string.IsNullOrEmpty(currentScene.MusicTrack))
            {
                try
                {
                    string trackName = ResolveMusicTrack(currentScene.MusicTrack);
                    currentSong = Content.Load<Song>($"Music/{trackName}");
                    MediaPlayer.Play(currentSong);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Missing music '{currentScene.MusicTrack}': {ex.Message}");
                }
            }

            // Set camera zoom for scene 3 and 4
            if (sceneId == 3 || sceneId == 4 || (sceneId == 16 && scene16LoopCount == 0) || IsFollowerPartyScene(sceneId))
                cameraZoom = 3f;
            else if (!IsTopDownScene(sceneId))
                cameraZoom = 1.0f;

            if (sceneId == 4)
                scene4LoopCount = 0;

            if (sceneId != 34 && sceneId != 35)
                scene34LoopCount = 0;
            if (sceneId != 36)
                scene36LoopCount = 0;

            StartSceneAnalytics(sceneId, analyticsDialogues);
            LogActionAnalytics("SceneEnter", GetSceneAnalyticsName(sceneId), $"Attempt {currentAttemptNumber}", null);

            // Companion route lifecycle: keep the chosen companion only through scenes 25-28.
            if (sceneId == 29 || sceneId == 30 || sceneId == 32 || sceneId == 23)
            {
                companionRoute = CompanionRoute.None;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState previousKeyState = keyState;
            MouseState previousMouseState = mouseState;
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();

            UpdateScene16FeaturePopup(gameTime);

            if (keyState.IsKeyDown(Keys.F2) && previousKeyState.IsKeyUp(Keys.F2))
            {
                if (currentSceneId >= 16 && currentSceneId <= 41)
                {
                    TryOpenMemoirFromGame();
                    base.Update(gameTime);
                    return;
                }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                showExitPrompt = true;
                selectedExitPromptIndex = 1;
                base.Update(gameTime);
                return;
            }

            if (keyState.IsKeyDown(Keys.Escape) && previousKeyState.IsKeyUp(Keys.Escape))
            {
                showExitPrompt = !showExitPrompt;
                selectedExitPromptIndex = 1;
                base.Update(gameTime);
                return;
            }

            if (showExitPrompt)
            {
                UpdateExitPrompt(previousKeyState, previousMouseState);
                base.Update(gameTime);
                return;
            }

            if (scene17NameInputActive)
            {
                UpdateScene17NameInput(previousKeyState);
                base.Update(gameTime);
                return;
            }

            if (endingTransitionActive)
            {
                UpdateEndingTransition(gameTime);
                base.Update(gameTime);
                return;
            }

            if (activeEnding != null)
            {
                base.Update(gameTime);
                return;
            }

            if (isNarrationActive)
            {
                UpdateNarrationSequence(previousKeyState);
                base.Update(gameTime);
                return;
            }

            if (isChoiceActive)
            {
                UpdateChoiceSelection(previousKeyState, previousMouseState, gameTime);
                base.Update(gameTime);
                return;
            }

            if (showDoorItemPopup)
            {
                if (keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter))
                {
                    showDoorItemPopup = false;
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    activeChoiceLoop = false;
                    activeChoiceStayNarrationId = null;
                    doorSceneChoicePending = false;
                    doorSceneEdgeChoiceActive = false;
                    doorSceneItemChoicePending = false;
                    doorSceneItemChoiceActive = true;
                    doorSceneItemLoopCount = 0;
                    OpenDoorSceneItemChoice();
                }

                base.Update(gameTime);
                return;
            }

            // Handle door scene item choice when the choice menu is active
            if (doorSceneItemChoiceActive)
            {
                if (isChoiceActive)
                {
                    base.Update(gameTime);
                    return;
                }
            }

            // Block input if tutorial image is showing
            if (showTutorialImage)
            {
                if (keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter))
                {
                    showTutorialImage = false;
                    currentScene.SecondTriggerPlayed = true; // ? scene-local flag
                }
                base.Update(gameTime);
                return;
            }

            if (showScene23BridgeImage)
            {
                if (keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter))
                {
                    showScene23BridgeImage = false;
                    scene23RemainingStarted = true;
                    if (scene23RemainingDialogues.Count > 0)
                        dialogueManager.StartDialogue(scene23RemainingDialogues, dialogueBoxTexture, dialogueBoxTexture2, false, true);
                }

                base.Update(gameTime);
                return;
            }

            if (showScene33NoteImage)
            {
                if (keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter))
                {
                    showScene33NoteImage = false;
                    scene33NotePromptReady = false;
                    scene33Blackout = true;
                    OpenChoiceMenuFromDialogue(282);
                }

                base.Update(gameTime);
                return;
            }

            if (activeFullscreenImage != null)
            {
                if (keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter))
                {
                    Action? onComplete = activeFullscreenImageCompletion;
                    activeFullscreenImage = null;
                    activeFullscreenImageCompletion = null;
                    onComplete?.Invoke();
                }

                base.Update(gameTime);
                return;
            }

            DialogueEntry? dialogueBeforeUpdate = dialogueManager.CurrentEntry;
            dialogueManager.Update(keyState, previousKeyState);
            if (dialogueBeforeUpdate != null && dialogueManager.CurrentEntry != null &&
                dialogueManager.CurrentEntry.DialogueID != dialogueBeforeUpdate.DialogueID)
            {
                sceneDialogueAdvanceCount++;
                LogActionAnalytics("DialogueAdvance", dialogueManager.CurrentEntry.Speaker, dialogueManager.CurrentEntry.DialogueID.ToString(CultureInfo.InvariantCulture));
            }
            UpdateDoorNpcAnimation(gameTime);
            UpdateScene17NpcAnimation(gameTime);
            UpdateFollowerPartyAnimations(gameTime);
            UpdateCompanionFollowerAnimations(gameTime);
            if (currentSceneId == 30 && !scene30SequenceComplete)
            {
                if (scene30BackViewPauseActive)
                {
                    chibiBAnimation?.Update(gameTime);
                    currentAnimation = chibiBAnimation ?? currentAnimation;
                }
                else
                {
                    idleAnimation?.Update(gameTime);
                    currentAnimation = idleAnimation ?? currentAnimation;
                }
            }

            if (doorSceneChoicePending &&
                !dialogueManager.IsActive &&
                !dialogueManager.IsWaitingForInteract &&
                !isChoiceActive &&
                !showDoorItemPopup &&
                !doorSceneItemChoiceActive &&
                !isNarrationActive)
            {
                OpenDoorSceneChoiceMenu();
                base.Update(gameTime);
                return;
            }

            if (hiddenWallDialogueAwaitingCompletion &&
                !dialogueManager.IsActive &&
                !dialogueManager.IsWaitingForInteract &&
                dialogueManager.IsFinished)
            {
                hiddenWallDialogueAwaitingCompletion = false;
                Action? onComplete = hiddenWallDialogueCompletionAction;
                hiddenWallDialogueCompletionAction = null;
                onComplete?.Invoke();
                base.Update(gameTime);
                return;
            }

            if (dialogueManager.IsActive)
            {
                base.Update(gameTime);
                return;
            }

            if (queuedDialogueAwaitingCompletion &&
                !dialogueManager.IsActive &&
                !dialogueManager.IsWaitingForInteract &&
                dialogueManager.IsFinished)
            {
                queuedDialogueAwaitingCompletion = false;
                Action? onComplete = queuedDialogueCompletionAction;
                queuedDialogueCompletionAction = null;
                onComplete?.Invoke();
                base.Update(gameTime);
                return;
            }

            if (scene32DialogueAwaitingCompletion &&
                !dialogueManager.IsActive &&
                !dialogueManager.IsWaitingForInteract &&
                dialogueManager.IsFinished)
            {
                scene32DialogueAwaitingCompletion = false;
                Action? onComplete = scene32DialogueCompletionAction;
                scene32DialogueCompletionAction = null;
                onComplete?.Invoke();
                base.Update(gameTime);
                return;
            }

            if (scene31DialogueAwaitingCompletion &&
                !dialogueManager.IsActive &&
                !dialogueManager.IsWaitingForInteract &&
                dialogueManager.IsFinished)
            {
                scene31DialogueAwaitingCompletion = false;
                Action? onComplete = scene31DialogueCompletionAction;
                scene31DialogueCompletionAction = null;
                onComplete?.Invoke();
                base.Update(gameTime);
                return;
            }

            if (scene22OwlFadeActive)
            {
                UpdateScene22OwlFade(gameTime);
                base.Update(gameTime);
                return;
            }

            if (scene24OwlFadeActive)
            {
                UpdateScene24OwlFade(gameTime);
                base.Update(gameTime);
                return;
            }

            if (scene31FriendFadeActive)
            {
                UpdateScene31FriendFade(gameTime);
                base.Update(gameTime);
                return;
            }

            if (scene35TimeoutDialogueAwaitingCompletion &&
                !dialogueManager.IsActive &&
                !dialogueManager.IsWaitingForInteract &&
                dialogueManager.IsFinished)
            {
                scene35TimeoutDialogueAwaitingCompletion = false;
                scene36EndingId = 9;
                StartNarrationSequence(LoadNarrationPagesByIds(61, 62), () => LoadSceneWithDialogues(37));
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 17)
            {
                TryTriggerScene17FollowupDialogue();
                if (dialogueManager.IsActive || scene17NameInputActive)
                {
                    base.Update(gameTime);
                    return;
                }
            }

            if (currentSceneId == 32)
            {
                UpdateScene32Sequence();
                if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isNarrationActive || activeEnding != null || endingTransitionActive)
                {
                    base.Update(gameTime);
                    return;
                }
            }

            // If scene32's right-exit blackout is active, progress timer and finish transition when elapsed
            if (currentSceneId == 32 && scene32RightExitBlackoutActive)
            {
                scene32RightExitBlackoutTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                isMoving = false;
                currentAnimation = idleAnimation;

                if (scene32RightExitBlackoutTimer >= Scene32RightExitBlackoutDuration)
                {
                    scene32RightExitBlackoutActive = false;
                    scene32RightExitBlackoutTimer = 0f;
                    // After blackout, load scene 23
                    LoadSceneWithDialogues(23);
                    // place player at left of new scene
                    position = new Vector2(0, GetSceneGroundY(23));
                }

                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 33 && UpdateScene33Sequence(previousKeyState))
            {
                base.Update(gameTime);
                return;
            }

            // Allow scene32 player to exit to the right only after dialogues complete
            if (currentSceneId == 32 && scene32DialoguesCompleted)
            {
                // If player moves past the right edge, go to scene23
                if (position.X > _graphics.PreferredBackBufferWidth)
                {
                    // begin 5-second pure black screen before transitioning
                    scene32RightExitBlackoutActive = true;
                    scene32RightExitBlackoutTimer = 0f;
                    // clamp position to right edge during blackout
                    position.X = _graphics.PreferredBackBufferWidth;
                    isMoving = false;
                    currentAnimation = idleAnimation;
                    base.Update(gameTime);
                    return;
                }

                // Prevent leaving to the left
                if (position.X < 0)
                {
                    position.X = 0;
                }
            }

            // scene33 blackout handling: when dialogue 282 triggers, enter blackout mode (handled elsewhere)

            if (currentSceneId == 34 && UpdateScene34Sequence())
            {
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 35 && UpdateScene35Sequence())
            {
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 29 && UpdateScene29GlitchTransition(gameTime))
            {
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 30 && UpdateScene30Sequence(gameTime))
            {
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 39 && UpdateScene39Sequence(gameTime))
            {
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 40 && UpdateScene40Sequence(gameTime))
            {
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 41 && UpdateScene41Sequence(gameTime))
            {
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 37)
            {
                UpdateScene37DrowningSequence(gameTime);
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 38 && UpdateScene37Sequence(gameTime))
            {
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 16)
            {
                TryTriggerScene16CenterDialogue();
                if (dialogueManager.IsActive)
                {
                    base.Update(gameTime);
                    return;
                }
            }

            if (currentSceneId == 23 && TryTriggerScene23Narration())
            {
                base.Update(gameTime);
                return;
            }

            if (currentSceneId == 20 || currentSceneId == 23 || currentSceneId == 29)
            {
                TryTriggerStaticPartySceneRemainder();
                if (TryAdvanceStaticPartySceneAfterDialogues())
                {
                    base.Update(gameTime);
                    return;
                }
                if (dialogueManager.IsActive)
                {
                    base.Update(gameTime);
                    return;
                }
            }

            if (currentSceneId == 22)
            {
                // Trigger center dialogue; completion handles owl fade and the next scene.
                TryTriggerScene22CenterDialogue();
            }

            if (currentSceneId == 24)
            {
                TryTriggerScene24CenterDialogue();
                if (dialogueManager.IsActive)
                {
                    base.Update(gameTime);
                    return;
                }
            }

            if (currentSceneId == 31)
            {
                TryTriggerScene31CenterDialogue();
                if (dialogueManager.IsActive)
                {
                    base.Update(gameTime);
                    return;
                }
            }

            if (TryTriggerScene19CenterNarration())
            {
                base.Update(gameTime);
                return;
            }

            if (IsDoorScene(currentSceneId))
            {
                if (!doorSceneIntroSequenceComplete && currentScene.IntroDialoguePlayed)
                {
                    doorSceneIntroSequenceComplete = true;
                }
            }

            // movement for scenes 1-4 only
            isMoving = false;
            if (!IsTopDownScene(currentSceneId))
            {
                position.Y = GetSceneGroundY(currentSceneId);

                if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                {
                    facingRight = true;
                    position.X += speed;
                    isMoving = true;
                }
                else if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                {
                    facingRight = false;
                    position.X -= speed;
                    isMoving = true;
                }
            }

            if (keyState.IsKeyDown(Keys.Space) && !isJumping && !IsSailingScene(currentSceneId))
            {
                isJumping = true;
                jumpAnimation.Reset();
                currentAnimation = jumpAnimation;
            }

            if (isJumping)
            {
                bool finished = jumpAnimation.UpdateOneShot(gameTime);
                if (finished)
                {
                    isJumping = false;
                    currentAnimation = idleAnimation;
                }
            }
            else
            {
                // 4. In the animation update section, after jump logic, add this block:
                if (teaAnimationPlaying)
                {
                    bool finished = teaAnimation.UpdateOneShot(gameTime);
                    if (finished)
                    {
                        teaAnimationPlaying = false;
                        currentAnimation = idleAnimation;
                    }
                }
                else
                {
                    // existing walk/idle animation update logic...
                    if (isMoving && currentAnimation != walkAnimation)
                    {
                        walkAnimation.Reset();
                        currentAnimation = walkAnimation;
                    }
                    else if (!isMoving && currentAnimation != idleAnimation)
                    {
                        idleAnimation.Reset();
                        currentAnimation = idleAnimation;
                    }
                    currentAnimation?.Update(gameTime);
                }
            }

            // show interact prompt when waiting for F, but never in scene 3
            showInteractPrompt = !dialogueManager.IsActive
                && dialogueManager.IsWaitingForInteract
                && currentScene.SceneID != 3
                && currentScene.SceneID != 4
                && currentScene.SceneID != 21
                && !IsHiddenWallScene(currentSceneId);
            currentDoorLabel = currentSceneId == 6 && !dialogueManager.IsActive ? GetDoorLabelAtPosition(position) : null;
            currentScene21Label = currentSceneId == 21 && !dialogueManager.IsActive ? GetScene21LabelAtPosition(position) : null;
            showDoorEnterPrompt = currentDoorLabel != null;
            showScene5WallPrompt = currentSceneId == 5 && !dialogueManager.IsActive && IsScene5HiddenWallTriggerPosition(position);
            showDoorNpcItemPrompt = IsDoorScene(currentSceneId)
                && !dialogueManager.IsActive
                && !showDoorItemPopup
                && !isChoiceActive
                && currentDoorItemToken != null
                && IsPlayerNearDoorSceneNpc();

            showHiddenWallStartPrompt = currentSceneId == HiddenWallSceneId
                && !scene13CenterNarrationTriggered
                && !dialogueManager.IsActive
                && !dialogueManager.IsWaitingForInteract
                && !isNarrationActive
                && !isChoiceActive
                && position.X >= (_graphics.PreferredBackBufferWidth * 0.5f);

            if (TryTriggerScene13CenterNarration(previousKeyState))
            {
                base.Update(gameTime);
                return;
            }

            if (showInteractPrompt && keyState.IsKeyDown(Keys.F) && previousKeyState.IsKeyUp(Keys.F))
            {
                sceneInteractCount++;
                LogActionAnalytics("DialogueInteract", dialogueManager.CurrentEntry?.Speaker, dialogueManager.CurrentEntry?.DialogueID.ToString(CultureInfo.InvariantCulture));
                dialogueManager.ShowNext();
                sceneDialogueAdvanceCount++;
            }

            if (showDoorEnterPrompt && keyState.IsKeyDown(Keys.F) && previousKeyState.IsKeyUp(Keys.F))
            {
                System.Diagnostics.Debug.WriteLine($"Scene 6 door trigger activated: {currentDoorLabel}");
                int destinationSceneId = GetDoorSceneId(currentDoorLabel);
                LogActionAnalytics("DoorEnter", currentDoorLabel, destinationSceneId > 0 ? $"Scene {destinationSceneId}" : null);
                QuickLogHistory($"Entered the {currentDoorLabel}.");
                if (destinationSceneId > 0)
                    LoadSceneWithDialogues(destinationSceneId);
            }

            if (currentSceneId == 21 && currentScene21Label != null && keyState.IsKeyDown(Keys.F) && previousKeyState.IsKeyUp(Keys.F))
            {
                System.Diagnostics.Debug.WriteLine($"Scene 21 companion trigger activated: {currentScene21Label}");

                // Determine side by comparing the stored center X against the reference map center.
                var referenceMap = GetSceneMap(21);
                float mapCenterX = (referenceMap.Keys.Min(k => k.X) + referenceMap.Keys.Max(k => k.X)) / 2f;
                Vector2 labelCenter = scene21LabelCenters.ContainsKey(currentScene21Label)
                    ? scene21LabelCenters[currentScene21Label]
                    : Vector2.Zero;

                bool onLeft = labelCenter.X < mapCenterX;
                int overrideTarget = onLeft ? 22 : 31;

                // Use narration pages 24/25 for the companion confirmation overlay (black screen)
                var pages = LoadNarrationPagesByIds(24, 25);
                if (pages.Count == 0)
                    pages = new List<string> { "Are you sure? Your decision will be permanent until the end of your journey.\nYour journey will also slightly differ based on your circumstances." };

                StartNarrationSequence(pages, () =>
                {
                    companionRoute = onLeft ? CompanionRoute.Left : CompanionRoute.Right;
                    OpenChoiceMenuFromDialogue(191, overrideYesTargetSceneId: overrideTarget);
                });
                base.Update(gameTime);
                return;
            }

            if (showScene5WallPrompt && keyState.IsKeyDown(Keys.F) && previousKeyState.IsKeyUp(Keys.F))
            {
                sceneInteractCount++;
                LogActionAnalytics("HiddenPathInspect", "Scene5Wall", $"Scene {HiddenWallSceneId}");
                QuickLogHistory("Inspected the hidden path.");
                LoadSceneWithDialogues(HiddenWallSceneId);
                base.Update(gameTime);
                return;
            }

            if (showDoorNpcItemPrompt && keyState.IsKeyDown(Keys.F) && previousKeyState.IsKeyUp(Keys.F))
            {
                doorSceneChoicePending = false;
                doorSceneEdgeChoiceActive = false;
                showDoorItemPopup = true;
                sceneInteractCount++;
                LogActionAnalytics("NpcItemView", currentDoorItemToken, doorSceneNpcKeys.TryGetValue(currentSceneId, out string? npcKey) ? npcKey : null);
                QuickLogHistory("Viewed the nearby item.");
                base.Update(gameTime);
                return;
            }

            if (TryTriggerDoorSceneEdgeDialogue())
            {
                base.Update(gameTime);
                return;
            }

            if (IsHiddenWallScene(currentSceneId))
            {
                float wallLimitX = GetHiddenWallRightLimit();
                if (position.X > wallLimitX)
                    position.X = wallLimitX;
            }

            // 2nd trigger: show tutorial prompt when player reaches right side after dialogues finish
            float triggerStartX = _graphics.PreferredBackBufferWidth / 2f;
            float triggerEndX = _graphics.PreferredBackBufferWidth - ((currentAnimation?.CurrentFrame?.Width * spriteScale * 0.5f) ?? 0f);

            if (currentScene.SceneID == 1)
            {
                if (!dialogueManager.IsActive && !dialogueManager.IsWaitingForInteract
                    && dialogueManager.IsFinished && !currentScene.SecondTriggerPlayed
                    && position.X >= triggerStartX && position.X <= triggerEndX)
                {
                    showTutorialPrompt = true;
                }

                if (showTutorialPrompt && !currentScene.SecondTriggerPlayed
                    && keyState.IsKeyDown(Keys.F) && previousKeyState.IsKeyUp(Keys.F))
                {
                    showTutorialPrompt = false;
                    showTutorialImage = true;
                    sceneInteractCount++;
                    LogActionAnalytics("TutorialOpened", "SceneTutorial", "OverlayShown");
                    QuickLogHistory("Opened the tutorial.");
                }
            }
            else if (currentScene.SceneID == 4)
            {
                showTutorialPrompt = false;
                showTutorialImage = false;
            }
            else
            {
                showTutorialPrompt = false;
                showTutorialImage = false;
            }

            // Tea trigger only for scene 2
            bool canShowTeaPrompt = currentSceneId == 2
                && !IsHiddenWallScene(currentSceneId)
                && !IsDoorScene(currentSceneId)
                && dialogueManager.IsFinished
                && !showTeaPrompt
                && !teaAnimationPlaying;

            if (canShowTeaPrompt)
            {
                showTeaPrompt = true;
            }
            else
            {
                showTeaPrompt = false;
                teaAnimationPlaying = false;
            }

            if (currentScene.SceneID == 4)
            {
                showTeaPrompt = false;
                teaAnimationPlaying = false;
            }

            if (showTeaPrompt && keyState.IsKeyDown(Keys.F) && previousKeyState.IsKeyUp(Keys.F))
            {
                showTeaPrompt = false;
                teaAnimationPlaying = true;
                teaAnimation.Reset();
                currentAnimation = teaAnimation;
                sceneInteractCount++;
                LogActionAnalytics("TeaInteract", "TeaPrompt", "TeaAnimationStarted");
                QuickLogHistory("Took a sip of tea.");
            }

            // Scene transition: when character passes through the right side of the screen
            if (position.X > _graphics.PreferredBackBufferWidth)
            {
                if (currentSceneId == 19)
                {
                    position = new Vector2(_graphics.PreferredBackBufferWidth, position.Y);
                }
                else if (currentSceneId == 16)
                {
                    if (scene16LoopCount == 0)
                    {
                        scene16LoopCount = 1;
                        LoadSceneWithDialogues(16);
                        position = new Vector2(0, position.Y);
                    }
                    else
                    {
                        LoadSceneWithDialogues(17);
                        position = new Vector2(100, GetSceneGroundY(17));
                        base.Update(gameTime);
                        return;
                    }
                }
                else if (currentSceneId == 22)
                {
                    position = new Vector2(_graphics.PreferredBackBufferWidth - 2f, position.Y);
                    TryTriggerScene22CenterDialogue();
                }
                else if (currentSceneId == 31)
                {
                    LoadSceneWithDialogues(25);
                    position = new Vector2(0, GetSceneGroundY(25));
                }
                else if (currentSceneId == 23)
                {
                    LoadSceneWithDialogues(33);
                    position = new Vector2(100, GetSceneGroundY(33));
                }
                else if (currentSceneId == 34)
                {
                    if (scene34LoopCount < 3)
                    {
                        LoadSceneWithDialogues(34);
                        position = new Vector2(0, GetSceneGroundY(34));
                    }
                    else
                    {
                        position = new Vector2(_graphics.PreferredBackBufferWidth, position.Y);
                    }
                }
                else if (currentSceneId == 36)
                {
                    if (scene36LoopCount < 3)
                    {
                        LoadSceneWithDialogues(36);
                        position = new Vector2(0, GetSceneGroundY(36));
                    }
                    else
                    {
                        LoadSceneWithDialogues(38);
                        position = new Vector2(100, GetSceneGroundY(38));
                    }
                }
                else if (currentSceneId == 39)
                {
                    if (scene39LoopCount < 3)
                    {
                        LoadSceneWithDialogues(39);
                        position = new Vector2(0, GetSceneGroundY(39));
                    }
                    else
                    {
                        position = new Vector2(_graphics.PreferredBackBufferWidth, GetSceneGroundY(39));
                    }
                }
                else if (currentSceneId == 38)
                {
                    position = new Vector2(_graphics.PreferredBackBufferWidth, GetSceneGroundY(38));
                }
                else if (currentSceneId == 28)
                {
                    int nextSceneId = companionRoute == CompanionRoute.Right ? 32 : 29;
                    LoadSceneWithDialogues(nextSceneId);
                    position = new Vector2(100, GetSceneGroundY(nextSceneId));
                }
                else if (currentSceneId == 29)
                {
                    LoadSceneWithDialogues(30);
                    position = new Vector2(0, GetSceneGroundY(30));
                }
                else if (currentSceneId == 30)
                {
                    LoadSceneWithDialogues(23);
                    position = new Vector2(0, GetSceneGroundY(23));
                }
                else if (IsDoorScene(currentSceneId) || IsHiddenWallScene(currentSceneId))
                {
                    position = new Vector2(_graphics.PreferredBackBufferWidth, position.Y);
                }
                else if (currentScene.SceneID == 4)
                {
                    if (scene4LoopCount < 2)
                    {
                        scene4LoopCount++;
                        position = new Vector2(0, position.Y); // wrap to left edge, stay in scene 4
                    }
                    else
                    {
                        int nextSceneId = currentScene.SceneID + 1;
                        LoadSceneWithDialogues(nextSceneId);
                        position = new Vector2(0, position.Y);
                    }
                }
                else
                {
                    int nextSceneId = currentScene.SceneID + 1;
                    LoadSceneWithDialogues(nextSceneId);

                    // ONLY wrap to the left if we are NOT in the new Scene 5 map
                    if (!IsTopDownScene(nextSceneId))
                    {
                        position = new Vector2(0, position.Y);
                    }
                }
            }
            else if (position.X < 0)
            {
                if (currentSceneId == 38)
                {
                    position = new Vector2(0, GetSceneGroundY(38));
                }
                else if (currentSceneId == 30)
                {
                    position = new Vector2(0, GetSceneGroundY(30));
                }
                else if (currentSceneId == 39 || currentSceneId == 40 || currentSceneId == 41)
                {
                    position = new Vector2(0, GetSceneGroundY(currentSceneId));
                }
                else if (IsDoorScene(currentSceneId))
                {
                    position = new Vector2(0, position.Y);
                }
                else if (IsHiddenWallScene(currentSceneId))
                {
                    LoadSceneWithDialogues(5);
                }
                else
                {
                    int prevSceneId = currentScene.SceneID - 1;
                    if (prevSceneId > 0)
                    {
                        LoadSceneWithDialogues(prevSceneId); // ? reload dialogues + triggers
                        position = new Vector2(_graphics.PreferredBackBufferWidth, position.Y);
                    }
                }
            }

            // Update playerPosition used by camera
            playerPosition = position;

            if (currentSceneId == 3 || (currentSceneId == 16 && scene16LoopCount == 0) || IsFollowerPartyScene(currentSceneId) || (currentSceneId == 4 && scene4CameraTracking))
            {
                camera.Follow(playerPosition,
                              _graphics.PreferredBackBufferWidth,
                              _graphics.PreferredBackBufferHeight,
                              cameraZoom,
                              0f,
                              160f);
            }
            else
            {
                camera.Follow(playerPosition,
                              _graphics.PreferredBackBufferWidth,
                              _graphics.PreferredBackBufferHeight,
                              1.0f);
            }

            // For testing: loop scene 4 when 'L' is pressed
            if (keyState.IsKeyDown(Keys.L) && previousKeyState.IsKeyUp(Keys.L))
            {
                scene4LoopCount++;
                if (scene4LoopCount <= 1) // limited loops for testing
                {
                    LoadSceneWithDialogues(4);
                    position = new Vector2(100, groundY); // reset position
                }
                else
                {
                    scene4LoopCount = 0;
                }
            }

            if (currentSceneId == 4)
            {
                // Odd loops (1, 3, 5...) are camera tracking, even loops (2, 4...) are normal
                scene4CameraTracking = (scene4LoopCount % 2 == 1);
            }
            else
            {
                scene4CameraTracking = false;
            }

            if (IsTopDownScene(currentSceneId))
            {
                if (!IsWalkable(position))
                    position = ClampToNearestWalkablePosition(position, currentSceneId);

                isMoving = false;
                Vector2 newPosition = position;

                if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                {
                    newPosition.X += speed;
                    currentAnimation = chibiRAnimation;
                    isMoving = true;
                }
                else if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                {
                    newPosition.X -= speed;
                    currentAnimation = chibiLAnimation;
                    isMoving = true;
                }
                else if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                {
                    newPosition.Y -= speed;
                    currentAnimation = chibiBAnimation;
                    isMoving = true;
                }
                else if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                {
                    newPosition.Y += speed;
                    currentAnimation = chibiFAnimation;
                    isMoving = true;
                }

                if (isMoving)
                {
                    float tileSpeed = GetTileSpeed(newPosition);

                    if (tileSpeed > 0f || !IsWalkable(position))
                    {
                        // Apply adjusted speed
                        Vector2 adjustedPosition = position;

                        if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                            adjustedPosition.X += tileSpeed;
                        else if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                            adjustedPosition.X -= tileSpeed;
                        else if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                            adjustedPosition.Y -= tileSpeed;
                        else if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                            adjustedPosition.Y += tileSpeed;

                        position = adjustedPosition;
                        currentAnimation?.Update(gameTime);

                        if (TryHandleTopDownSceneTransition())
                        {
                            base.Update(gameTime);
                            return;
                        }
                    }
                }

                if (currentSceneId == 21 && TryTriggerScene21Branch())
                {
                    base.Update(gameTime);
                    return;
                }

                // Scene32 movement restrictions: before dialogues completed, block movement left/right and passage.
                if (currentSceneId == 32 && !scene32DialoguesCompleted)
                {
                    // Prevent movement until the center trigger/dialogues start/finish
                    // Player may walk into the center to start the sequence; once started, freeze movement.
                    if (!scene32Started)
                    {
                        // Allow moving right to reach center, but prevent moving left past left edge
                        if (position.X < 0)
                            position.X = 0;
                    }
                    else
                    {
                        // Sequence running - keep player centered
                        position.X = _graphics.PreferredBackBufferWidth * 0.5f;
                        position.Y = GetSceneGroundY(32);
                    }
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            int screenWidth = _graphics.PreferredBackBufferWidth;
            int screenHeight = _graphics.PreferredBackBufferHeight;

            if (endingTransitionActive)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawEndingTransition(_spriteBatch, screenWidth, screenHeight);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            if (activeEnding != null)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawEndingScreen(_spriteBatch, screenWidth, screenHeight);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            // Scene32 right-exit blackout: draw pure black for the duration before transitioning
            if (currentSceneId == 32 && scene32RightExitBlackoutActive)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            if (isNarrationActive)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawNarrationScreen(_spriteBatch, screenWidth, screenHeight);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            if (isChoiceActive)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawChoiceScreen(_spriteBatch, screenWidth, screenHeight);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            if (activeFullscreenImage != null)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawFullscreenImageOverlay(_spriteBatch, activeFullscreenImage, screenWidth, screenHeight, activeFullscreenImageDismissText);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            if (scene17NameInputActive)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawScene17NameInput(_spriteBatch, screenWidth, screenHeight);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            if (currentSceneId == 38 && scene37BlackoutActive && activeEnding == null && !endingTransitionActive)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            if (currentSceneId == 37 && activeEnding == null && !endingTransitionActive)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawScene37EndingCinematic(_spriteBatch, screenWidth, screenHeight);
                dialogueManager.UseTopPlacement = false;
                dialogueManager.Draw(_spriteBatch, dialogueFont);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            if (currentSceneId == 38 && !scene37BlackoutActive && activeEnding == null && !endingTransitionActive)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawScene37Cinematic(_spriteBatch, screenWidth, screenHeight);
                dialogueManager.Draw(_spriteBatch, dialogueFont);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            if (IsSailingScene(currentSceneId) && activeEnding == null && !endingTransitionActive)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawSailingScene(_spriteBatch, screenWidth, screenHeight);
                dialogueManager.UseTopPlacement = ShouldUseTopDialoguePlacement();
                dialogueManager.Draw(_spriteBatch, dialogueFont);
                if (showExitPrompt)
                    DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            // Begin sprite batch (camera transform only for scene 3/4 tracking)
            _spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: (currentSceneId == 3 || (currentSceneId == 16 && scene16LoopCount == 0) || IsFollowerPartyScene(currentSceneId) || (currentSceneId == 4 && scene4CameraTracking)) ? camera.Transform : Matrix.Identity
            );

            // Side scenes use the original full-screen background stretch.
            // Top-down scenes keep their current background fill behavior.
            if (IsTopDownScene(currentSceneId))
            {
                DrawBackgroundFill(_spriteBatch, currentBackground ?? ground, screenWidth, screenHeight);
            }
            else
            {
                // If scene33 blackout is active, draw a plain black background instead of the scene background
                if (scene33Blackout)
                {
                    _spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);
                }
                else
                {
                    bool useBlackCinematicBackground = scene33Blackout ||
                        (currentSceneId == 30 && scene30BackViewPauseActive && !scene30SequenceComplete);

                    Texture2D backgroundToDraw = currentBackground ?? ground;
                    if (useBlackCinematicBackground)
                        _spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);
                    else
                        _spriteBatch.Draw(backgroundToDraw, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                }

                if (currentSceneId == 38)
                    DrawScene37GlitchOverlay(_spriteBatch, screenWidth, screenHeight);

                bool hideSideSceneGround = currentSceneId == 30 && scene30BackViewPauseActive && !scene30SequenceComplete;
                if (!hideSideSceneGround)
                    DrawSideSceneGround(_spriteBatch, screenWidth, screenHeight);

                DrawDoorSceneNpc(_spriteBatch);
                DrawScene17Npcs(_spriteBatch);
                DrawStaticPartySceneNpcs(_spriteBatch);
                DrawFollowerPartyNpcs(_spriteBatch);
                DrawCompanionFollowerNpc(_spriteBatch);

                // --- Main character draw ---
                Texture2D frameToDraw = currentAnimation?.CurrentFrame ?? idleTextures[0];
                Vector2 origin = new Vector2(frameToDraw.Width / 2f, frameToDraw.Height);
                SpriteEffects flip = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                float playerAlpha = currentSceneId == 29 ? MathHelper.Clamp(scene29PlayerAlpha, 0f, 1f) : 1f;
                _spriteBatch.Draw(frameToDraw, position, null, Color.White * playerAlpha, 0f, origin, spriteScale, flip, 0f);

                if (currentSceneId == 29)
                    DrawScene29GlitchOverlay(_spriteBatch, screenWidth, screenHeight);

                if (currentSceneId == 30 && !scene30SequenceComplete)
                    DrawScene30GlitchOverlay(_spriteBatch, screenWidth, screenHeight);
            }

            // Dialogue + prompts (skip in Scene5)
            if (!IsTopDownScene(currentSceneId))
            {
                if (showInteractPrompt && !dialogueManager.IsActive)
                {
                    string promptText = "Press F to Interact";
                    Vector2 promptSize = dialogueFont.MeasureString(promptText);
                    float promptX = Math.Min(position.X - promptSize.X / 2f, screenWidth - promptSize.X - 10);
                    _spriteBatch.DrawString(dialogueFont, promptText,
                        new Vector2(promptX, position.Y - 200), Color.Yellow);
                }

                if (showDoorNpcItemPrompt)
                {
                    string itemPromptText = "Press F to Interact";
                    Vector2 promptSize = dialogueFont.MeasureString(itemPromptText);
                    float npcX = GetDoorSceneNpcX();
                    float promptX = Math.Max(10f, Math.Min(npcX - promptSize.X / 2f, screenWidth - promptSize.X - 10f));
                    // Adjust prompt Y position for scene 13
                    float groundAdjustment = (currentSceneId == 13) ? scene13GroundAdjustment : 0f;
                    float promptY = GetSceneGroundY(currentSceneId) - 180f - groundAdjustment;
                    _spriteBatch.DrawString(dialogueFont, itemPromptText, new Vector2(promptX, promptY), Color.Yellow);
                }

                if (showTutorialPrompt && !currentScene.SecondTriggerPlayed)
                {
                    string tutorialText = "TUTORIAL MODE\nPress F to View";
                    string[] tutorialLines = tutorialText.Split('\n');
                    float tutorialY = position.Y - 220;
                    foreach (string line in tutorialLines)
                    {
                        Vector2 lineSize = dialogueFont.MeasureString(line);
                        float lineX = position.X - lineSize.X / 2f;
                        lineX = Math.Max(10, Math.Min(lineX, screenWidth - lineSize.X - 10));
                        _spriteBatch.DrawString(dialogueFont, line, new Vector2(lineX, tutorialY), Color.Cyan);
                        tutorialY += lineSize.Y;
                    }
                }

                dialogueManager.UseTopPlacement = ShouldUseTopDialoguePlacement();
                dialogueManager.Draw(_spriteBatch, dialogueFont);
                DrawDoorSceneItem(_spriteBatch, screenWidth, screenHeight);

                if (showTutorialImage)
                {
                    _spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.7f);

                    int margin = 70;
                    int imgWidth = screenWidth - margin * 2;
                    int imgHeight = screenHeight - margin * 2;
                    int imgX = margin;
                    int imgY = margin;
                    _spriteBatch.Draw(tutorialImage, new Rectangle(imgX, imgY, imgWidth, imgHeight), Color.White);

                    string dismissText = "Press ENTER to close";
                    Vector2 dismissSize = dialogueFont.MeasureString(dismissText);
                    float dismissX = (screenWidth - dismissSize.X) / 2f;
                    float dismissY = screenHeight - margin / 2f - dismissSize.Y / 2f;
                    _spriteBatch.DrawString(dialogueFont, dismissText, new Vector2(dismissX, dismissY), Color.Yellow);
                }

                if (showScene23BridgeImage)
                {
                    DrawFullscreenImageOverlay(_spriteBatch, bridgeImage, screenWidth, screenHeight, "Press ENTER to continue");
                }

                if (showScene33NoteImage)
                {
                    // For scene33 dialogue 282, we want a blackout with normal dialogue box. If scene33Blackout is true,
                    // the background will already be black. Show the note overlay only when explicitly requested.
                    DrawFullscreenImageOverlay(_spriteBatch, noteImage, screenWidth, screenHeight, "Press ENTER to close");
                }

                if (showTeaPrompt)
                {
                    string teaText = "Take a sip (Press F)";
                    Vector2 teaSize = dialogueFont.MeasureString(teaText);
                    float teaX = Math.Min(position.X - teaSize.X / 2f, screenWidth - teaSize.X - 10);
                    _spriteBatch.DrawString(dialogueFont, teaText, new Vector2(teaX, position.Y - 240), Color.LightGreen);
                }

                if (showHiddenWallStartPrompt)
                {
                    string hiddenWallPromptText = "Press F to Inspect";
                    Vector2 hiddenWallPromptSize = dialogueFont.MeasureString(hiddenWallPromptText);
                    float hiddenWallPromptX = Math.Max(10f, Math.Min(position.X - hiddenWallPromptSize.X / 2f, screenWidth - hiddenWallPromptSize.X - 10f));
                    float hiddenWallPromptY = Math.Max(40f, position.Y - 220f);
                    _spriteBatch.DrawString(dialogueFont, hiddenWallPromptText, new Vector2(hiddenWallPromptX, hiddenWallPromptY), Color.Yellow);
                }

                if (currentSceneId == 33 && scene33NotePromptReady && !showScene33NoteImage)
                {
                    string notePromptText = "Press F to Check Note";
                    Vector2 notePromptSize = dialogueFont.MeasureString(notePromptText);
                    float notePromptX = Math.Max(10f, Math.Min(position.X - notePromptSize.X / 2f, screenWidth - notePromptSize.X - 10f));
                    float notePromptY = Math.Max(40f, position.Y - 220f);
                    _spriteBatch.DrawString(dialogueFont, notePromptText, new Vector2(notePromptX, notePromptY), Color.Yellow);
                }

            }
            else
            {
            }

            // --- Scene5 / Scene6 special draw ---
            if (IsTopDownScene(currentSceneId))
            {
                Dictionary<Vector2, int> currentMap = GetSceneMap(currentSceneId);
                Texture2D currentAtlas = GetTopDownTextureAtlas(currentSceneId);

                // Draw Tiled map layer (mainpath only)
                int display_tilesize = GetTopDownDisplayTileSize(currentMap);
                int pixel_tilesize = 8;
                int num_tiles_per_row = currentAtlas.Width / pixel_tilesize;

                int minX = currentMap.Keys.Min(k => (int)k.X);
                int minY = currentMap.Keys.Min(k => (int)k.Y);
                int maxX = currentMap.Keys.Max(k => (int)k.X);
                int maxY = currentMap.Keys.Max(k => (int)k.Y);

                int mapWidth = (maxX - minX + 1) * display_tilesize;
                int mapHeight = (maxY - minY + 1) * display_tilesize;

                int offsetX = (screenWidth - mapWidth) / 2;
                int offsetY = (screenHeight - mapHeight) / 2;

                foreach (var item in currentMap)
                {
                    Rectangle drect = new Rectangle(
                        offsetX + ((int)item.Key.X - minX) * display_tilesize,
                        offsetY + ((int)item.Key.Y - minY) * display_tilesize,
                        display_tilesize,
                        display_tilesize);

                    int x = item.Value % num_tiles_per_row;
                    int y = item.Value / num_tiles_per_row;

                    Rectangle scr = new Rectangle(x * pixel_tilesize, y * pixel_tilesize, pixel_tilesize, pixel_tilesize);
                    _spriteBatch.Draw(currentAtlas, drect, scr, Color.White);
                }

                // Draw chibi character
                Texture2D chibiFrame = currentAnimation?.CurrentFrame ?? chibiFTextures[0];
                Vector2 chibiOrigin = new Vector2(chibiFrame.Width / 2f, chibiFrame.Height / 2f);

                _spriteBatch.Draw(chibiFrame, position, null, Color.White, 0f, chibiOrigin, 1.0f, SpriteEffects.None, 0f);

                if (currentSceneId == 5 && showScene5WallPrompt)
                {
                    string promptText = "Press F to see";
                    Vector2 promptSize = dialogueFont.MeasureString(promptText);
                    float promptX = Math.Max(10f, Math.Min(position.X - (promptSize.X / 2f), screenWidth - promptSize.X - 10f));
                    float promptY = Math.Max(10f, position.Y - 90f);
                    _spriteBatch.DrawString(dialogueFont, promptText, new Vector2(promptX, promptY), Color.Yellow);
                }

                if (currentSceneId == 6)
                    DrawScene6DoorLabels(_spriteBatch, screenWidth, screenHeight);
                if (currentSceneId == 21)
                    DrawScene21Labels(_spriteBatch, screenWidth, screenHeight);
            }
            _spriteBatch.End();

            if (scene16FeaturePopupActive && currentSceneId == 16)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawScene16FeaturePopup(_spriteBatch, screenWidth);
                _spriteBatch.End();
            }

            if (showExitPrompt)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawExitPrompt(_spriteBatch, screenWidth, screenHeight);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void DrawFullscreenImageOverlay(SpriteBatch spriteBatch, Texture2D image, int screenWidth, int screenHeight, string dismissText)
        {
            spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.8f);

            float scale = Math.Min(screenWidth / (float)image.Width, screenHeight / (float)image.Height);
            int imageWidth = (int)(image.Width * scale);
            int imageHeight = (int)(image.Height * scale);
            int imageX = (screenWidth - imageWidth) / 2;
            int imageY = (screenHeight - imageHeight) / 2;
            spriteBatch.Draw(image, new Rectangle(imageX, imageY, imageWidth, imageHeight), Color.White);

            Vector2 dismissSize = dialogueFont.MeasureString(dismissText);
            float dismissX = (screenWidth - dismissSize.X) / 2f;
            float dismissY = screenHeight - dismissSize.Y - 28f;
            spriteBatch.Draw(ground, new Rectangle(0, (int)dismissY - 12, screenWidth, (int)dismissSize.Y + 24), Color.Black * 0.75f);
            spriteBatch.DrawString(dialogueFont, dismissText, new Vector2(dismissX, dismissY), Color.Yellow);
        }

        private void DrawScene16FeaturePopup(SpriteBatch spriteBatch, int screenWidth)
        {
            if (dialogueFont == null || ground == null)
                return;

            string title = "YOU HAVE UNLOCKED A FEATURE";
            string message = "Return to Home or press F2 anytime.";
            float elapsed = scene16FeaturePopupElapsed;
            float targetX = 24f;
            float startX = -520f;
            float popupY = 28f;

            float x;
            if (elapsed < Scene16FeaturePopupSlideSeconds)
            {
                float progress = MathHelper.Clamp(elapsed / Scene16FeaturePopupSlideSeconds, 0f, 1f);
                x = MathHelper.Lerp(startX, targetX, progress);
            }
            else if (elapsed > Scene16FeaturePopupDurationSeconds - Scene16FeaturePopupSlideSeconds)
            {
                float progress = MathHelper.Clamp((elapsed - (Scene16FeaturePopupDurationSeconds - Scene16FeaturePopupSlideSeconds)) / Scene16FeaturePopupSlideSeconds, 0f, 1f);
                x = MathHelper.Lerp(targetX, startX, progress);
            }
            else
            {
                x = targetX;
            }

            x = Math.Min(x, screenWidth - 20f);

            Vector2 titleSize = dialogueFont.MeasureString(title);
            Vector2 messageSize = dialogueFont.MeasureString(message);
            float width = Math.Max(titleSize.X, messageSize.X) + 40f;
            float height = titleSize.Y + messageSize.Y + 34f;
            Rectangle popupRect = new Rectangle((int)x, (int)popupY, (int)width, (int)height);

            spriteBatch.Draw(ground, popupRect, Color.Black * 0.88f);
            spriteBatch.Draw(ground, new Rectangle(popupRect.X, popupRect.Y, 6, popupRect.Height), Color.YellowGreen);

            spriteBatch.DrawString(dialogueFont, title, new Vector2(popupRect.X + 20f, popupRect.Y + 10f), Color.White);
            spriteBatch.DrawString(dialogueFont, message, new Vector2(popupRect.X + 20f, popupRect.Y + 18f + titleSize.Y), Color.LightGray);
        }

        private bool ShouldUseTopDialoguePlacement()
        {
            if (currentSceneId == 35 || currentSceneId == 36)
                return true;

            if (currentSceneId == 41)
                return true;

            return false;
        }

        private void DrawSailingScene(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (currentSceneId == 41 && scene41SuccessBlackoutActive)
            {
                spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);
            }
            else
            {
                Texture2D backgroundToDraw = currentBackground ?? ground;
                spriteBatch.Draw(backgroundToDraw, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
            }

            Animation? animation = currentSceneId == 39 ? sailingAnimation : sailing2Animation;
            Texture2D? frameToDraw = animation?.CurrentFrame ?? idleTextures[0];
            if (frameToDraw == null)
                return;

            // Scene 39: use sailingAnimation and draw at the player's position (boat follows player)
            // Scene 40: use sailing2Animation and draw centered on screen
            // Scene 41: draw the mcSailTexture centered on screen (static image)

            if (currentSceneId == 39)
            {
                Vector2 origin = new Vector2(frameToDraw.Width / 2f, frameToDraw.Height);
                SpriteEffects flip = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                float scale = spriteScale;
                spriteBatch.Draw(frameToDraw, position, null, Color.White, 0f, origin, scale, flip, 0f);
                return;
            }

            if (currentSceneId == 40)
            {
                Vector2 origin = new Vector2(frameToDraw.Width / 2f, frameToDraw.Height);
                SpriteEffects flip = SpriteEffects.None;
                float scale = spriteScale;
                Vector2 centerPos = new Vector2(screenWidth / 2f, GetSceneGroundY(40));
                spriteBatch.Draw(frameToDraw, centerPos, null, Color.White, 0f, origin, scale, flip, 0f);
                return;
            }

            if (currentSceneId == 41)
            {
                Texture2D? tex = mcSailTexture ?? frameToDraw;
                if (tex == null)
                    return;
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height);
                Vector2 centerPos = new Vector2(screenWidth / 2f, GetSceneGroundY(41));
                spriteBatch.Draw(tex, centerPos, null, Color.White, 0f, origin, spriteScale, SpriteEffects.None, 0f);
                return;
            }

            // Fallback: draw at position
            Vector2 fallbackOrigin = new Vector2(frameToDraw.Width / 2f, frameToDraw.Height);
            spriteBatch.Draw(frameToDraw, position, null, Color.White, 0f, fallbackOrigin, spriteScale, SpriteEffects.None, 0f);
        }

        private void DrawScene39AutoDialogue(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (dialogueFont == null ||
                scene39ActiveAutoDialogues.Count == 0 ||
                scene39AutoDialogueIndex < 0 ||
                scene39AutoDialogueIndex >= scene39ActiveAutoDialogues.Count)
                return;

            DialogueEntry entry = scene39ActiveAutoDialogues[scene39AutoDialogueIndex];
            int boxWidth = Math.Min(1040, screenWidth - 160);
            int boxHeight = 118;
            int boxX = (screenWidth - boxWidth) / 2;
            int boxY = screenHeight - boxHeight - 42;
            Texture2D boxTexture = dialogueBoxTexture ?? ground;
            spriteBatch.Draw(boxTexture, new Rectangle(boxX, boxY, boxWidth, boxHeight), Color.White * 0.95f);

            string speaker = NormalizeTextForSpriteFont(entry.Speaker ?? "Player");
            string dialogue = WrapChoiceText(NormalizeTextForSpriteFont(entry.Dialogue), boxWidth - 96f);
            spriteBatch.DrawString(dialogueFont, speaker, new Vector2(boxX + 44, boxY + 18), Color.Yellow);

            string[] lines = dialogue.Split('\n');
            float textY = boxY + 50;
            foreach (string line in lines.Take(2))
            {
                spriteBatch.DrawString(dialogueFont, line, new Vector2(boxX + 44, textY), Color.White);
                textY += dialogueFont.LineSpacing;
            }
        }

        private void DrawScene37EndingCinematic(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (!scene37AutoDialogueComplete)
            {
                spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);
                return;
            }

            Texture2D backgroundToDraw = currentBackground ?? ground;
            spriteBatch.Draw(backgroundToDraw, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);

            Texture2D? frame = drowningTextures.Length > 0
                ? drowningTextures[Math.Clamp(scene37EndingAnimationFrameIndex, 0, drowningTextures.Length - 1)]
                : null;
            if (frame != null)
            {
                float scale = screenHeight / (float)frame.Height;
                int drawWidth = (int)(frame.Width * scale);
                int drawHeight = screenHeight;
                int drawX = (screenWidth - drawWidth) / 2;
                int drawY = 0;
                spriteBatch.Draw(frame, new Rectangle(drawX, drawY, drawWidth, drawHeight), Color.White);
            }
        }

        private void DrawScene37EndingAutoDialogue(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (scene37EndingDialogues.Count == 0 ||
                scene37EndingDialogueIndex < 0 ||
                scene37EndingDialogueIndex >= scene37EndingDialogues.Count ||
                dialogueFont == null)
                return;

            DialogueEntry entry = scene37EndingDialogues[scene37EndingDialogueIndex];
            int boxWidth = Math.Min(1040, screenWidth - 180);
            int boxHeight = 112;
            int boxX = (screenWidth - boxWidth) / 2;
            int boxY = Math.Min(screenHeight - boxHeight - 42, (int)(screenHeight * 0.72f));

            Texture2D boxTexture = dialogueBoxTexture ?? ground;
            spriteBatch.Draw(boxTexture, new Rectangle(boxX, boxY, boxWidth, boxHeight), Color.White * 0.95f);

            string speaker = NormalizeTextForSpriteFont(entry.Speaker ?? "Player");
            string dialogue = WrapChoiceText(NormalizeTextForSpriteFont(entry.Dialogue), boxWidth - 96f);

            spriteBatch.DrawString(dialogueFont, speaker, new Vector2(boxX + 44, boxY + 20), Color.Yellow);

            string[] lines = dialogue.Split('\n');
            float textY = boxY + 50;
            foreach (string line in lines.Take(2))
            {
                spriteBatch.DrawString(dialogueFont, line, new Vector2(boxX + 44, textY), Color.White);
                textY += dialogueFont.LineSpacing;
            }
        }

        private void DrawScene37Cinematic(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            Texture2D backgroundToDraw = currentBackground ?? ground;
            spriteBatch.Draw(backgroundToDraw, new Rectangle(0, 0, screenWidth, screenHeight), Color.White * 0.86f);
            spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.18f);
            DrawScene38UnsettlingOverlay(spriteBatch, screenWidth, screenHeight);

            Texture2D? frame = currentAnimation?.CurrentFrame ?? idleTextures[0];
            if (frame != null)
            {
                Vector2 origin = new Vector2(frame.Width / 2f, frame.Height);
                SpriteEffects flip = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                spriteBatch.Draw(frame, position, null, Color.White, 0f, origin, spriteScale, flip, 0f);
            }
        }

        private void DrawScene38UnsettlingOverlay(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (ground == null)
                return;

            int bandCount = 4;
            for (int i = 0; i < bandCount; i++)
            {
                int bandHeight = endingGlitchRandom.Next(2, 6);
                int y = endingGlitchRandom.Next(0, Math.Max(1, screenHeight - bandHeight));
                int offset = endingGlitchRandom.Next(-8, 9);
                Color bandColor = i % 2 == 0 ? Color.Cyan * 0.035f : Color.Red * 0.035f;
                spriteBatch.Draw(ground, new Rectangle(offset, y, screenWidth, bandHeight), bandColor);
            }

            for (int i = 0; i < 7; i++)
            {
                int width = endingGlitchRandom.Next(18, 90);
                int height = endingGlitchRandom.Next(2, 5);
                int x = endingGlitchRandom.Next(0, Math.Max(1, screenWidth - width));
                int y = endingGlitchRandom.Next((int)(screenHeight * 0.08f), Math.Max((int)(screenHeight * 0.72f), 1));
                spriteBatch.Draw(ground, new Rectangle(x, y, width, height), Color.White * 0.045f);
            }

            Point center = new Point(screenWidth / 2, (int)(screenHeight * 0.24f));
            Color crackColor = Color.White * 0.23f;
            DrawGlitchCrack(spriteBatch, center, new Point(center.X - 120, center.Y - 46), crackColor);
            DrawGlitchCrack(spriteBatch, center, new Point(center.X + 132, center.Y - 34), crackColor);
            DrawGlitchCrack(spriteBatch, center, new Point(center.X - 74, center.Y + 80), crackColor * 0.7f);
            DrawGlitchCrack(spriteBatch, center, new Point(center.X + 58, center.Y + 94), crackColor * 0.7f);
        }

        private void DrawGlitchCrack(SpriteBatch spriteBatch, Point start, Point end, Color color)
        {
            int segments = 5;
            Point previous = start;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                int x = (int)MathHelper.Lerp(start.X, end.X, t) + endingGlitchRandom.Next(-10, 11);
                int y = (int)MathHelper.Lerp(start.Y, end.Y, t) + endingGlitchRandom.Next(-8, 9);
                Point next = new Point(x, y);
                DrawLine(spriteBatch, previous, next, color, 2);
                previous = next;
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Point start, Point end, Color color, int thickness)
        {
            Vector2 startVector = new Vector2(start.X, start.Y);
            Vector2 endVector = new Vector2(end.X, end.Y);
            Vector2 edge = endVector - startVector;
            float angle = MathF.Atan2(edge.Y, edge.X);
            spriteBatch.Draw(ground, new Rectangle(start.X, start.Y, (int)edge.Length(), thickness), null, color, angle, Vector2.Zero, SpriteEffects.None, 0f);
        }

        private void DrawChoiceScreen(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);
            if (IsScene30ChoiceSource(activeChoiceSourceDialogueId))
                DrawScene30GlitchOverlay(spriteBatch, screenWidth, screenHeight);

            List<Rectangle> optionRects = GetChoiceOptionRectangles();
            for (int i = 0; i < activeChoices.Count; i++)
            {
                ChoiceEntry choice = activeChoices[i];
                Rectangle rect = optionRects[i];
                bool isSelected = i == selectedChoiceIndex;

                string label = isSelected ? $"> {choice.ChoiceText} <" : choice.ChoiceText;
                Vector2 textSize = dialogueFont.MeasureString(label);
                float textX = (screenWidth - textSize.X) / 2f;
                float textY = rect.Y + (rect.Height - textSize.Y) / 2f;
                spriteBatch.DrawString(dialogueFont, label, new Vector2(textX, textY), isSelected ? Color.Yellow : Color.White);
            }

            if (scene35TimedChoiceActive && (activeChoiceSourceDialogueId == 298 || activeChoiceSourceDialogueId == Scene35PullChoiceSourceId))
            {
                string timerText = $"TIME: {Math.Max(0, (int)Math.Ceiling(scene35ChoiceTimer))}";
                Vector2 timerSize = dialogueFont.MeasureString(timerText);
                spriteBatch.DrawString(
                    dialogueFont,
                    timerText,
                    new Vector2((screenWidth - timerSize.X) / 2f, Math.Max(36f, screenHeight * 0.18f)),
                    scene35ChoiceTimer <= 3f ? Color.Red : Color.Yellow);
            }

            if (scene41TimedChoiceActive && activeChoiceSourceDialogueId == Scene41ChoiceSourceId)
            {
                string timerText = $"TIME: {Math.Max(0, (int)Math.Ceiling(scene41ChoiceTimer))}";
                Vector2 timerSize = dialogueFont.MeasureString(timerText);
                spriteBatch.DrawString(
                    dialogueFont,
                    timerText,
                    new Vector2((screenWidth - timerSize.X) / 2f, Math.Max(36f, screenHeight * 0.18f)),
                    scene41ChoiceTimer <= 3f ? Color.Red : Color.Yellow);
            }
        }

        private void DrawNarrationScreen(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (dialogueFont == null || activeNarrationPages == null || activeNarrationPages.Count == 0)
                return;

            string rawPage = activeNarrationPages[Math.Clamp(activeNarrationIndex, 0, activeNarrationPages.Count - 1)];
            string safePage = NormalizeTextForSpriteFont(rawPage);
            string wrappedPage = WrapChoiceText(safePage, screenWidth * 0.68f);
            string[] lines = wrappedPage.Split('\n');

            float lineHeight = dialogueFont.LineSpacing;
            float totalHeight = 0f;
            foreach (var l in lines)
                totalHeight += string.IsNullOrEmpty(l) ? lineHeight : dialogueFont.MeasureString(l).Y;

            float startY = (screenHeight - totalHeight) / 2f;
            float y = startY;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    y += lineHeight;
                    continue;
                }

                Vector2 lineSize = dialogueFont.MeasureString(line);
                float x = (screenWidth - lineSize.X) / 2f;

                Color narrationColor = ((currentSceneId == 35 && activeNarrationIndex == activeNarrationPages.Count - 1) ||
                                        (currentSceneId == 41 && activeNarrationIndex == 0))
                                        ? Color.Red
                                        : Color.White;

                // Draw text without any random jitter/glitch offsets
                spriteBatch.DrawString(dialogueFont, line, new Vector2(x, y), narrationColor);
                y += lineSize.Y;
            }

            string hintText = activeNarrationIndex >= activeNarrationPages.Count - 1
                ? "Press ENTER to continue"
                : "Press ENTER for next";
            Vector2 hintSize = dialogueFont.MeasureString(hintText);
            spriteBatch.DrawString(dialogueFont, hintText, new Vector2((screenWidth - hintSize.X) / 2f, screenHeight - hintSize.Y - 36f), Color.Yellow);
        }
        private void DrawExitPrompt(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.55f);

            int boxWidth = Math.Min(screenWidth - 100, 1280);
            int boxHeight = Math.Min((int)(screenHeight * 0.24f), 280);
            int boxX = (screenWidth - boxWidth) / 2;
            int boxY = (screenHeight - boxHeight) / 2;
            spriteBatch.Draw(ground, new Rectangle(boxX, boxY, boxWidth, boxHeight), Color.Black * 0.92f);
            spriteBatch.Draw(ground, new Rectangle(boxX, boxY, boxWidth, 3), Color.White);
            spriteBatch.Draw(ground, new Rectangle(boxX, boxY + boxHeight - 3, boxWidth, 3), Color.White);
            spriteBatch.Draw(ground, new Rectangle(boxX, boxY, 3, boxHeight), Color.White);
            spriteBatch.Draw(ground, new Rectangle(boxX + boxWidth - 3, boxY, 3, boxHeight), Color.White);

            string message = "Saving progress... I'm sad you're leaving, please do come again soon!";
            string wrappedMessage = WrapChoiceText(message, boxWidth - 80f);
            string safeMessage = NormalizeTextForSpriteFont(wrappedMessage);
            string[] lines = safeMessage.Split('\n');
            float totalTextHeight = lines.Length * dialogueFont.LineSpacing;
            float textY = boxY + ((boxHeight - totalTextHeight) / 2f) - 20f;

            foreach (string line in lines)
            {
                Vector2 lineSize = dialogueFont.MeasureString(line);
                float lineX = boxX + ((boxWidth - lineSize.X) / 2f);
                spriteBatch.DrawString(dialogueFont, line, new Vector2(lineX, textY), Color.White);
                textY += dialogueFont.LineSpacing;
            }

            string[] options = { "OK", "Cancel" };
            int optionY = boxY + boxHeight - 72;
            int spacing = 180;
            int centerX = screenWidth / 2;

            for (int i = 0; i < options.Length; i++)
            {
                string label = selectedExitPromptIndex == i ? $"> {options[i]} <" : options[i];
                Vector2 labelSize = dialogueFont.MeasureString(label);
                float labelX = centerX + ((i == 0 ? -1 : 1) * (spacing / 2f)) - (labelSize.X / 2f);
                spriteBatch.DrawString(dialogueFont, label, new Vector2(labelX, optionY), selectedExitPromptIndex == i ? Color.Yellow : Color.White);
            }
        }

        private void DrawEndingScreen(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);

            string author = string.IsNullOrWhiteSpace(activeEnding.Author) ? string.Empty : $"- {activeEnding.Author}";
            author = NormalizeTextForSpriteFont(author);
            string quote = NormalizeTextForSpriteFont(activeEnding.Quote);
            string wrappedQuote = WrapChoiceText(quote, screenWidth * 0.65f);
            string[] quoteLines = wrappedQuote.Split('\n');
            float lineHeight = dialogueFont.LineSpacing;
            float quoteBlockHeight = quoteLines.Length * lineHeight;
            Vector2 authorSize = string.IsNullOrEmpty(author) ? Vector2.Zero : dialogueFont.MeasureString(author);
            float blockHeight = quoteBlockHeight + (authorSize == Vector2.Zero ? 0f : authorSize.Y + 28f);
            float quoteY = (screenHeight - blockHeight) / 2f;

            foreach (string line in quoteLines)
            {
                Vector2 lineSize = dialogueFont.MeasureString(line);
                float lineX = (screenWidth - lineSize.X) / 2f;
                spriteBatch.DrawString(dialogueFont, line, new Vector2(lineX, quoteY), Color.White);
                quoteY += lineHeight;
            }

            if (!string.IsNullOrEmpty(author))
            {
                float authorX = (screenWidth - authorSize.X) / 2f;
                float authorY = quoteY + 28f;
                spriteBatch.DrawString(dialogueFont, author, new Vector2(authorX, authorY), Color.Gray);
            }

            Texture2D chibiFrame = chibiFTextures[0];
            float endingScale = Math.Min(7f, screenWidth / (float)chibiFrame.Width / 5f);
            Vector2 chibiOrigin = new Vector2(chibiFrame.Width / 2f, chibiFrame.Height);
            Vector2 chibiPosition = new Vector2(screenWidth / 2f, screenHeight - 30f);
            spriteBatch.Draw(chibiFrame, chibiPosition, null, Color.White, 0f, chibiOrigin, endingScale, SpriteEffects.None, 0f);

            string exitText = "ESC to exit";
            Vector2 exitSize = dialogueFont.MeasureString(exitText);
            float exitX = (screenWidth - exitSize.X) / 2f;
            float exitY = screenHeight - exitSize.Y - 24f;
            spriteBatch.DrawString(dialogueFont, exitText, new Vector2(exitX, exitY), Color.Gray);
        }

        private bool IsWalkable(Vector2 newPosition)
        {
            Dictionary<Vector2, int> currentMap = GetSceneMap(currentSceneId);
            Dictionary<Vector2, int> currentCollisionLayer = GetSceneCollisionLayer(currentSceneId);
            int displayTileSize = GetTopDownDisplayTileSize(currentMap);

            int minX = currentMap.Keys.Min(k => (int)k.X);
            int minY = currentMap.Keys.Min(k => (int)k.Y);
            int maxX = currentMap.Keys.Max(k => (int)k.X);
            int maxY = currentMap.Keys.Max(k => (int)k.Y);

            int offsetX = (_graphics.PreferredBackBufferWidth - ((maxX - minX + 1) * displayTileSize)) / 2;
            int offsetY = (_graphics.PreferredBackBufferHeight - ((maxY - minY + 1) * displayTileSize)) / 2;

            int tileX = minX + (int)((newPosition.X - offsetX) / displayTileSize);
            int tileY = minY + (int)((newPosition.Y - offsetY) / displayTileSize);

            Vector2 tilePos = new Vector2(tileX, tileY);

            return currentCollisionLayer.TryGetValue(tilePos, out int tileValue) && tileValue == 1;
        }

        private float GetTileSpeed(Vector2 newPosition)
        {
            Dictionary<Vector2, int> currentMap = GetSceneMap(currentSceneId);
            Dictionary<Vector2, int> currentCollisionLayer = GetSceneCollisionLayer(currentSceneId);
            int displayTileSize = GetTopDownDisplayTileSize(currentMap);

            int minX = currentMap.Keys.Min(k => (int)k.X);
            int minY = currentMap.Keys.Min(k => (int)k.Y);
            int maxX = currentMap.Keys.Max(k => (int)k.X);
            int maxY = currentMap.Keys.Max(k => (int)k.Y);

            int offsetX = (_graphics.PreferredBackBufferWidth - ((maxX - minX + 1) * displayTileSize)) / 2;
            int offsetY = (_graphics.PreferredBackBufferHeight - ((maxY - minY + 1) * displayTileSize)) / 2;

            int tileX = minX + (int)((newPosition.X - offsetX) / displayTileSize);
            int tileY = minY + (int)((newPosition.Y - offsetY) / displayTileSize);

            Vector2 tilePos = new Vector2(tileX, tileY);

            if (currentCollisionLayer.TryGetValue(tilePos, out int tileValue) && tileValue == 1)
                return speed;

            return 0f; // outside map / invalid tile
        }

        private bool TryHandleTopDownSceneTransition()
        {
            if (currentSceneId != 5)
                return false;

            Vector2 tilePos = WorldToTilePosition(position, GetSceneMap(currentSceneId));

            bool movingRight = keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D);
            if (movingRight && IsScene5EndingTrigger(tilePos))
            {
                LoadSceneWithDialogues(13);
                return true;
            }

            bool movingDown = keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S);
            if (!movingDown)
                return false;

            Dictionary<Vector2, int> currentCollisionLayer = GetSceneCollisionLayer(currentSceneId);
            var walkableTiles = currentCollisionLayer
                .Where(kv => kv.Value == 1)
                .Select(kv => kv.Key)
                .ToList();

            if (walkableTiles.Count == 0)
                return false;

            float bottomWalkableY = walkableTiles.Max(tile => tile.Y);
            if (tilePos.Y < bottomWalkableY)
                return false;

            LoadSceneWithDialogues(6);
            return true;
        }


        // Read player age from userinfo.json
        private void OnGameExiting(object? sender, EventArgs args)
        {
            FinalizeCurrentSceneAnalytics("GameExit");
            SaveProgressToSqlite();
        }

        private void EnsureSaveDataTable()
        {
            const string createTableSql = @"
                CREATE TABLE IF NOT EXISTS tblSaveData (
                    SaveID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerID INT NOT NULL DEFAULT 0,
                    PlayerName TEXT,
                    AttemptNumber INT NOT NULL DEFAULT 1,
                    Age INT NOT NULL,
                    SceneID INT NOT NULL,
                    PositionX REAL NOT NULL,
                    PositionY REAL NOT NULL,
                    SavedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";

            using var cmd = new SqliteCommand(createTableSql, conn);
            cmd.ExecuteNonQuery();
            TryEnsureColumnExists("tblSaveData", "PlayerID", "ALTER TABLE tblSaveData ADD COLUMN PlayerID INT NOT NULL DEFAULT 0;");
            TryEnsureColumnExists("tblSaveData", "PlayerName", "ALTER TABLE tblSaveData ADD COLUMN PlayerName TEXT;");
            TryEnsureColumnExists("tblSaveData", "AttemptNumber", "ALTER TABLE tblSaveData ADD COLUMN AttemptNumber INT NOT NULL DEFAULT 1;");
            TryEnsureColumnExists("tblSaveData", "SavedAt", "ALTER TABLE tblSaveData ADD COLUMN SavedAt TEXT;");
        }

        private void EnsureAnalyticsTables()
        {
            string createAttemptsSql = $@"
                CREATE TABLE IF NOT EXISTS {AttemptTableName} (
                    AttemptNumber INTEGER PRIMARY KEY,
                    PlayerID INT NOT NULL DEFAULT 0,
                    PlayerName TEXT,
                    StartedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    StartedFrom TEXT
                );";

            string createSceneAnalyticsSql = $@"
                CREATE TABLE IF NOT EXISTS {SceneAnalyticsTableName} (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerID INT NOT NULL DEFAULT 0,
                    PlayerName TEXT,
                    AttemptNumber INT NOT NULL,
                    SceneID INT NOT NULL,
                    SceneName TEXT NOT NULL,
                    DurationSeconds REAL NOT NULL,
                    DialogueLineCount INT NOT NULL DEFAULT 0,
                    DialogueAdvanceCount INT NOT NULL DEFAULT 0,
                    InteractCount INT NOT NULL DEFAULT 0,
                    ActionCount INT NOT NULL DEFAULT 0,
                    Classification TEXT,
                    LoggedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";

            string createActionAnalyticsSql = $@"
                CREATE TABLE IF NOT EXISTS {ActionAnalyticsTableName} (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerID INT NOT NULL DEFAULT 0,
                    PlayerName TEXT,
                    AttemptNumber INT NOT NULL,
                    SceneID INT NOT NULL,
                    SceneName TEXT NOT NULL,
                    ActionType TEXT NOT NULL,
                    ActionValue TEXT,
                    Outcome TEXT,
                    LoggedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";

            using (var cmd = new SqliteCommand(createAttemptsSql, conn))
                cmd.ExecuteNonQuery();

            using (var cmd = new SqliteCommand(createSceneAnalyticsSql, conn))
                cmd.ExecuteNonQuery();

            using (var cmd = new SqliteCommand(createActionAnalyticsSql, conn))
                cmd.ExecuteNonQuery();

            TryEnsureColumnExists(AttemptTableName, "PlayerID", $"ALTER TABLE {AttemptTableName} ADD COLUMN PlayerID INT NOT NULL DEFAULT 0;");
            TryEnsureColumnExists(AttemptTableName, "PlayerName", $"ALTER TABLE {AttemptTableName} ADD COLUMN PlayerName TEXT;");
            TryEnsureColumnExists(SceneAnalyticsTableName, "PlayerID", $"ALTER TABLE {SceneAnalyticsTableName} ADD COLUMN PlayerID INT NOT NULL DEFAULT 0;");
            TryEnsureColumnExists(SceneAnalyticsTableName, "PlayerName", $"ALTER TABLE {SceneAnalyticsTableName} ADD COLUMN PlayerName TEXT;");
            TryEnsureColumnExists(ActionAnalyticsTableName, "PlayerID", $"ALTER TABLE {ActionAnalyticsTableName} ADD COLUMN PlayerID INT NOT NULL DEFAULT 0;");
            TryEnsureColumnExists(ActionAnalyticsTableName, "PlayerName", $"ALTER TABLE {ActionAnalyticsTableName} ADD COLUMN PlayerName TEXT;");
        }

        private void TryEnsureColumnExists(string tableName, string columnName, string alterSql)
        {
            using var pragma = new SqliteCommand($"PRAGMA table_info({tableName});", conn);
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            using var alter = new SqliteCommand(alterSql, conn);
            alter.ExecuteNonQuery();
        }

        private int ResolveAttemptNumber(SaveDataEntry? latestSave)
        {
            if (requestedEndingId.HasValue)
                return Math.Max(1, GetLatestAttemptNumber());

            if (latestSave != null && latestSave.AttemptNumber > 0)
                return latestSave.AttemptNumber;

            return CreateNewAttempt("NewGame");
        }

        private int GetLatestAttemptNumber()
        {
            using var cmd = new SqliteCommand($"SELECT COALESCE(MAX(AttemptNumber), 0) FROM {AttemptTableName};", conn);
            object? result = cmd.ExecuteScalar();
            return Convert.ToInt32(result ?? 0, CultureInfo.InvariantCulture);
        }

        private int CreateNewAttempt(string startedFrom)
        {
            int nextAttemptNumber = GetLatestAttemptNumber() + 1;
            using var cmd = new SqliteCommand($@"
                INSERT INTO {AttemptTableName} (AttemptNumber, PlayerID, PlayerName, StartedFrom)
                VALUES (@attemptNumber, @playerId, @playerName, @startedFrom);", conn);
            cmd.Parameters.AddWithValue("@attemptNumber", nextAttemptNumber);
            cmd.Parameters.AddWithValue("@playerId", activePlayerId);
            cmd.Parameters.AddWithValue("@playerName", activePlayerName);
            cmd.Parameters.AddWithValue("@startedFrom", startedFrom);
            cmd.ExecuteNonQuery();
            return nextAttemptNumber;
        }

        private void StartSceneAnalytics(int sceneId, List<DialogueEntry> analyticsDialogues)
        {
            if (!analyticsEnabled || sceneId <= 0)
                return;

            sceneAnalyticsStartedAtUtc = DateTime.UtcNow;
            sceneDialogueLineCount = analyticsDialogues.Count;
            sceneDialogueCharCount = analyticsDialogues.Sum(entry => entry.Dialogue?.Length ?? 0);
            sceneDialogueAdvanceCount = 0;
            sceneInteractCount = 0;
            sceneActionCount = 0;
            sceneAnalyticsActive = true;
        }

        private void FinalizeCurrentSceneAnalytics(string outcome)
        {
            if (!analyticsEnabled || !sceneAnalyticsActive || currentSceneId <= 0)
                return;

            double durationSeconds = Math.Max(1d, (DateTime.UtcNow - sceneAnalyticsStartedAtUtc).TotalSeconds);
            string sceneName = GetSceneAnalyticsName(currentSceneId);
            string classification = ClassifySceneEngagement(durationSeconds);

            using var cmd = new SqliteCommand($@"
                INSERT INTO {SceneAnalyticsTableName}
                    (PlayerID, PlayerName, AttemptNumber, SceneID, SceneName, DurationSeconds, DialogueLineCount, DialogueAdvanceCount, InteractCount, ActionCount, Classification)
                VALUES
                    (@playerId, @playerName, @attemptNumber, @sceneId, @sceneName, @durationSeconds, @dialogueLineCount, @dialogueAdvanceCount, @interactCount, @actionCount, @classification);", conn);
            cmd.Parameters.AddWithValue("@playerId", activePlayerId);
            cmd.Parameters.AddWithValue("@playerName", activePlayerName);
            cmd.Parameters.AddWithValue("@attemptNumber", currentAttemptNumber);
            cmd.Parameters.AddWithValue("@sceneId", currentSceneId);
            cmd.Parameters.AddWithValue("@sceneName", sceneName);
            cmd.Parameters.AddWithValue("@durationSeconds", durationSeconds);
            cmd.Parameters.AddWithValue("@dialogueLineCount", sceneDialogueLineCount);
            cmd.Parameters.AddWithValue("@dialogueAdvanceCount", sceneDialogueAdvanceCount);
            cmd.Parameters.AddWithValue("@interactCount", sceneInteractCount);
            cmd.Parameters.AddWithValue("@actionCount", sceneActionCount);
            cmd.Parameters.AddWithValue("@classification", classification);
            cmd.ExecuteNonQuery();

            LogActionAnalytics("SceneExit", sceneName, string.IsNullOrWhiteSpace(outcome) ? classification : $"{classification} | {outcome}", currentSceneId);
            sceneAnalyticsActive = false;
        }

        private string ClassifySceneEngagement(double durationSeconds)
        {
            if (sceneDialogueLineCount <= 0)
                return sceneActionCount > 0 || sceneInteractCount > 0 ? "Explored" : "Idle";

            double expectedSeconds = Math.Max(sceneDialogueLineCount * 2.75d, sceneDialogueCharCount / 14d);

            if (sceneDialogueAdvanceCount == 0 && sceneInteractCount == 0 && durationSeconds < 5d)
                return "Ignored";

            if (durationSeconds < expectedSeconds * 0.5d || sceneDialogueAdvanceCount < Math.Max(1, sceneDialogueLineCount / 3))
                return "Skimmed";

            return "Read";
        }

        private string GetSceneAnalyticsName(int sceneId)
        {
            return $"Scene {sceneId}";
        }

        private void LogActionAnalytics(string actionType, string? actionValue = null, string? outcome = null, int? sceneId = null)
        {
            if (!analyticsEnabled || conn == null || currentAttemptNumber <= 0)
                return;

            int effectiveSceneId = sceneId ?? currentSceneId;
            if (effectiveSceneId <= 0)
                return;

            if (conn.State != ConnectionState.Open)
                conn.Open();

            sceneActionCount++;

            using var cmd = new SqliteCommand($@"
                INSERT INTO {ActionAnalyticsTableName}
                    (PlayerID, PlayerName, AttemptNumber, SceneID, SceneName, ActionType, ActionValue, Outcome)
                VALUES
                    (@playerId, @playerName, @attemptNumber, @sceneId, @sceneName, @actionType, @actionValue, @outcome);", conn);
            cmd.Parameters.AddWithValue("@playerId", activePlayerId);
            cmd.Parameters.AddWithValue("@playerName", activePlayerName);
            cmd.Parameters.AddWithValue("@attemptNumber", currentAttemptNumber);
            cmd.Parameters.AddWithValue("@sceneId", effectiveSceneId);
            cmd.Parameters.AddWithValue("@sceneName", GetSceneAnalyticsName(effectiveSceneId));
            cmd.Parameters.AddWithValue("@actionType", actionType);
            cmd.Parameters.AddWithValue("@actionValue", (object?)actionValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@outcome", (object?)outcome ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        private void SaveProgressToSqlite()
        {
            if (saveQueuedForExit)
            {
                saveQueuedForExit = false;
                return;
            }

            if (conn == null)
                return;

            if (currentSceneId <= 0)
                return;

            if (conn.State != ConnectionState.Open)
                conn.Open();

            const string insertSql = @"
                INSERT INTO tblSaveData (PlayerID, PlayerName, AttemptNumber, Age, SceneID, PositionX, PositionY)
                VALUES (@playerId, @playerName, @attemptNumber, @age, @sceneId, @positionX, @positionY);";

            using var cmd = new SqliteCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("@playerId", activePlayerId);
            cmd.Parameters.AddWithValue("@playerName", activePlayerName);
            cmd.Parameters.AddWithValue("@attemptNumber", currentAttemptNumber);
            cmd.Parameters.AddWithValue("@age", playerAge);
            cmd.Parameters.AddWithValue("@sceneId", currentSceneId);
            cmd.Parameters.AddWithValue("@positionX", position.X);
            cmd.Parameters.AddWithValue("@positionY", position.Y);
            cmd.ExecuteNonQuery();
        }

        private void RestoreSavedProgress(SaveDataEntry latestSave)
        {
            position = latestSave.Position;
            currentSceneId = latestSave.SceneID;

            if (IsTopDownScene(latestSave.SceneID))
                position = ClampToNearestWalkablePosition(position, latestSave.SceneID);

            playerPosition = position;
        }

        private Vector2 ClampToNearestWalkablePosition(Vector2 worldPosition, int sceneId)
        {
            Dictionary<Vector2, int> currentMap = GetSceneMap(sceneId);
            Dictionary<Vector2, int> currentCollisionLayer = GetSceneCollisionLayer(sceneId);

            var walkableTiles = currentCollisionLayer
                .Where(kv => kv.Value == 1)
                .Select(kv => kv.Key)
                .ToList();

            if (walkableTiles.Count == 0)
                return worldPosition;

            Vector2 currentTile = WorldToTilePosition(worldPosition, currentMap);
            Vector2 nearestTile = walkableTiles
                .OrderBy(tile => Vector2.DistanceSquared(tile, currentTile))
                .First();

            return TileToWorldPosition(nearestTile, currentMap);
        }

        private void SaveAndExit()
        {
            if (!saveQueuedForExit)
            {
                SaveProgressToSqlite();
                saveQueuedForExit = true;
            }

            Exit();
        }

        private void TryOpenMemoirFromGame()
        {
            WinForms.DialogResult result = WinForms.MessageBox.Show(
                "Directing to another page will exit the game.",
                "Open Memoir",
                WinForms.MessageBoxButtons.OKCancel,
                WinForms.MessageBoxIcon.Information);

            if (result != WinForms.DialogResult.OK)
                return;

            if (!TryLaunchGameFormsMemoir(out string? errorMessage))
            {
                WinForms.MessageBox.Show(
                    errorMessage ?? "Unable to open the memoir page right now.",
                    "Memoir Launch Failed",
                    WinForms.MessageBoxButtons.OK,
                    WinForms.MessageBoxIcon.Error);
                return;
            }

            QuickLogHistory("Opened Memoir from the game.");
            SaveAndExit();
        }

        private bool TryLaunchGameFormsMemoir(out string? errorMessage)
        {
            errorMessage = null;

            string[] candidatePaths =
            {
                @"C:\Users\Tiffany Mae\AppData\Local\FindingNimo\GameForms\bin\Debug\net8.0-windows\GameForms.exe",
                @"C:\Users\Tiffany Mae\AppData\Local\FindingNimo\GameForms\bin\Release\net8.0-windows\GameForms.exe",
                @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameForms\bin\Debug\net8.0-windows\GameForms.exe",
                @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameForms\bin\Release\net8.0-windows\GameForms.exe"
            };

            string? executablePath = candidatePaths.FirstOrDefault(File.Exists);
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                errorMessage = "GameForms.exe could not be found. Build the GameForms project first.";
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = activePlayerId > 0
                        ? $"--memoir --playerId={activePlayerId}"
                        : "--memoir",
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(executablePath) ?? AppContext.BaseDirectory
                });

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private void UpdateScene16FeaturePopup(GameTime gameTime)
        {
            if (!scene16FeaturePopupActive)
                return;

            if (currentSceneId != 16)
            {
                scene16FeaturePopupActive = false;
                scene16FeaturePopupElapsed = 0f;
                return;
            }

            scene16FeaturePopupElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (scene16FeaturePopupElapsed >= Scene16FeaturePopupDurationSeconds)
            {
                scene16FeaturePopupElapsed = Scene16FeaturePopupDurationSeconds;
                scene16FeaturePopupActive = false;
            }
        }

        private SaveDataEntry? LoadLatestSaveData()
        {
            if (conn == null)
                return null;

            if (conn.State != ConnectionState.Open)
                conn.Open();

            const string query = @"
                SELECT SaveID, AttemptNumber, Age, SceneID, PositionX, PositionY, SavedAt
                FROM tblSaveData
                WHERE PlayerID = @playerId
                ORDER BY COALESCE(SavedAt, CURRENT_TIMESTAMP) DESC, SaveID DESC
                LIMIT 1;";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerId", activePlayerId);
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new SaveDataEntry
            {
                SaveID = reader.GetInt32(0),
                AttemptNumber = reader.IsDBNull(1) ? 1 : reader.GetInt32(1),
                Age = reader.GetInt32(2),
                SceneID = reader.GetInt32(3),
                Position = new Vector2(
                    Convert.ToSingle(reader.GetValue(4), CultureInfo.InvariantCulture),
                    Convert.ToSingle(reader.GetValue(5), CultureInfo.InvariantCulture)),
                SavedAt = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture)
            };
        }

        private void LoadActivePlayerProfile()
        {
            try
            {
                using var playerConn = new SqliteConnection($@"Data Source={GameDatabasePath}");
                playerConn.Open();

                const string playerQuery = @"
                    SELECT PlayerID, Name, Age
                    FROM tblPlayer
                    ORDER BY
                        CASE WHEN LastLoginAt IS NULL THEN 1 ELSE 0 END,
                        LastLoginAt DESC,
                        PlayerID DESC
                    LIMIT 1;";

                using var playerCmd = new SqliteCommand(playerQuery, playerConn);
                using var reader = playerCmd.ExecuteReader();
                if (reader.Read())
                {
                    activePlayerId = reader.GetInt32(0);
                    activePlayerName = reader.IsDBNull(1) ? "Guest" : reader.GetString(1);
                    playerAge = reader.GetInt32(2);
                    System.Diagnostics.Debug.WriteLine($"Loaded active player {activePlayerName} ({activePlayerId}) age {playerAge} from tblPlayer");
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to read age from tblPlayer: {ex.Message}");
            }

            activePlayerId = 0;
            activePlayerName = "Guest";
            playerAge = LoadPlayerAgeFallback();
        }

        private int LoadPlayerAgeFallback()
        {
            string[] possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userinfo.json"),
                @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameForms\bin\Debug\net8.0-windows\userinfo.json",
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        string json = File.ReadAllText(path);
                        var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("Age", out JsonElement ageProp))
                        {
                            int age = ageProp.GetInt32();
                            System.Diagnostics.Debug.WriteLine($"Loaded age {age} from {path}");
                            return age;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to read age from {path}: {ex.Message}");
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("userinfo.json not found, defaulting age to 10");
            return 10;
        }

        private string GetActivePlayerHistoryLogPath()
        {
            string safePlayerName = string.IsNullOrWhiteSpace(activePlayerName) ? "guest" : activePlayerName;
            foreach (char invalid in Path.GetInvalidFileNameChars())
                safePlayerName = safePlayerName.Replace(invalid, '_');

            string root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FindingNimo",
                "Players",
                $"{activePlayerId}_{safePlayerName.Trim()}");

            Directory.CreateDirectory(root);
            return Path.Combine(root, "history.log");
        }

        private void QuickLogHistory(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
                return;

            string logPath = GetActivePlayerHistoryLogPath();
            string date = DateTime.Now.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            string time = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            using var fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var sw = new StreamWriter(fs);
            sw.WriteLine($"[ {date} | {time} ]");
            sw.WriteLine($"   <PLAYER> --- {action}");
            sw.WriteLine();
        }

        private static string FormatChoiceLogText(string choiceText)
        {
            string normalized = choiceText.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                return "Made a choice.";

            return $"Chose '{normalized}'.";
        }

        // Load dialogues from SQLite, filtering with prepared statements
        public List<DialogueEntry> LoadDialogues(int sceneId, int playerAge)
        {
            List<DialogueEntry> dialogues = new List<DialogueEntry>();

            if (conn.State != ConnectionState.Open)
                conn.Open();

            HashSet<string> dialogueColumns = GetTableColumns("tblDialogues");
            bool hasDialogueColumn = dialogueColumns.Contains("Dialogue");
            bool hasLineTextColumn = dialogueColumns.Contains("LineText");

            string dialogueTextExpression = hasDialogueColumn && hasLineTextColumn
                ? "COALESCE(NULLIF(d.Dialogue, ''), d.LineText)"
                : hasDialogueColumn
                    ? "d.Dialogue"
                    : hasLineTextColumn
                        ? "d.LineText"
                        : "''";

            string query = $@"
        SELECT d.DialogueID,
               d.Speaker,
               {dialogueTextExpression} AS DialogueText,
               d.MinAge,
               d.MaxAge
        FROM tblDialogues d
        INNER JOIN tblScene s ON d.SceneID = s.SceneID
        WHERE d.SceneID = @id
        ORDER BY d.DialogueID";

            using (var cmd = new SqliteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", sceneId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int dialogueId = Convert.ToInt32(reader["DialogueID"]);
                        string speaker = reader["Speaker"].ToString();
                        string dialogue = ApplyDynamicDialogueTokens(reader["DialogueText"].ToString());
                        int? minAge = reader["MinAge"] != DBNull.Value ? Convert.ToInt32(reader["MinAge"]) : (int?)null;
                        int? maxAge = reader["MaxAge"] != DBNull.Value ? Convert.ToInt32(reader["MaxAge"]) : (int?)null;

                        if ((minAge == null || playerAge >= minAge) && (maxAge == null || playerAge <= maxAge))
                        {
                            dialogues.Add(new DialogueEntry(dialogueId, sceneId, speaker, dialogue, minAge, maxAge));
                        }
                    }
                }
            }

            return dialogues;
        }

        private HashSet<string> GetTableColumns(string tableName)
        {
            if (tableColumnCache.TryGetValue(tableName, out HashSet<string>? cachedColumns))
                return cachedColumns;

            HashSet<string> columns = new(StringComparer.OrdinalIgnoreCase);

            if (conn.State != ConnectionState.Open)
                conn.Open();

            using var cmd = new SqliteCommand($"PRAGMA table_info({tableName});", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                if (reader["name"] != DBNull.Value)
                    columns.Add(reader["name"].ToString() ?? string.Empty);
            }

            tableColumnCache[tableName] = columns;
            return columns;
        }

        private string ApplyDynamicDialogueTokens(string? rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return string.Empty;

            return rawText
                .Replace("{PLAYER}", activePlayerName, StringComparison.OrdinalIgnoreCase)
                .Replace("{Player}", activePlayerName, StringComparison.OrdinalIgnoreCase)
                .Replace("{Owl}", savedOwlName, StringComparison.OrdinalIgnoreCase)
                .Replace("{OWL}", savedOwlName, StringComparison.OrdinalIgnoreCase);
        }

        public List<ChoiceEntry> LoadChoices(int dialogueId)
        {
            List<ChoiceEntry> choices = new List<ChoiceEntry>();

            if (conn.State != ConnectionState.Open)
                conn.Open();

            string query = @"
                SELECT DialogueID, ChoiceOrder, ChoiceText, TargetEndingID, TargetSceneID
                FROM tblChoice
                WHERE DialogueID = @dialogueId
                ORDER BY ChoiceOrder";

            using (var cmd = new SqliteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@dialogueId", dialogueId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int sourceDialogueId = reader.GetInt32(0);
                        int choiceOrder = reader.GetInt32(1);
                        string choiceText = reader.GetString(2);
                        int? targetEndingId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
                        int? targetSceneId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4);
                        choices.Add(new ChoiceEntry(sourceDialogueId, choiceOrder, choiceText, targetEndingId, targetSceneId));
                    }
                }
            }

            return choices;
        }

        public EndingEntry LoadEnding(int endingId)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            string query = @"
                SELECT EndingID, SceneID, Author, Quote
                FROM tblEnding
                WHERE EndingID = @endingId";

            using (var cmd = new SqliteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@endingId", endingId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int? sceneId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);
                        string author = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                        string quote = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                        return new EndingEntry(id, sceneId, author, quote);
                    }
                }
            }

            return new EndingEntry(endingId, null, "Ending", "No ending text found.");
        }

        private void SaveUnlockedEnding(int endingId)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            EnsureAchievementTable();
            EnsureUnlockedEndingTable();

            const string upsertSql = @"
                INSERT INTO tblUnlockedEnding (PlayerID, PlayerName, EndingID, UnlockedAt)
                VALUES (@playerId, @playerName, @endingId, CURRENT_TIMESTAMP)
                ON CONFLICT(PlayerID, EndingID) DO UPDATE SET
                    PlayerName = excluded.PlayerName,
                    UnlockedAt = CURRENT_TIMESTAMP;";

            using (var upsertCmd = new SqliteCommand(upsertSql, conn))
            {
                upsertCmd.Parameters.AddWithValue("@playerId", activePlayerId);
                upsertCmd.Parameters.AddWithValue("@playerName", activePlayerName);
                upsertCmd.Parameters.AddWithValue("@endingId", endingId);
                upsertCmd.ExecuteNonQuery();
            }

            const string updateAchievementSql = @"
                UPDATE tblAchievements
                SET TimesUnlocked = COALESCE(TimesUnlocked, 0) + 1,
                    LastUnlockedAt = CURRENT_TIMESTAMP
                WHERE EndingID = @endingId
                  AND PlayerID = @playerId;";

            using (var updateCmd = new SqliteCommand(updateAchievementSql, conn))
            {
                updateCmd.Parameters.AddWithValue("@endingId", endingId);
                updateCmd.Parameters.AddWithValue("@playerId", activePlayerId);
                int rowsAffected = updateCmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    string fallbackAchievementName = GetAchievementNameForEnding(endingId);

                    const string insertAchievementSql = @"
                        INSERT INTO tblAchievements (SaveID, PlayerID, PlayerName, EndingID, AchievementName, TimesUnlocked, LastUnlockedAt)
                        VALUES (
                            (SELECT SaveID FROM tblSaveData WHERE PlayerID = @playerId ORDER BY SaveID DESC LIMIT 1),
                            @playerId,
                            @playerName,
                            @endingId,
                            @achievementName,
                            1,
                            CURRENT_TIMESTAMP
                        );";

                    using var insertCmd = new SqliteCommand(insertAchievementSql, conn);
                    insertCmd.Parameters.AddWithValue("@playerId", activePlayerId);
                    insertCmd.Parameters.AddWithValue("@playerName", activePlayerName);
                    insertCmd.Parameters.AddWithValue("@endingId", endingId);
                    insertCmd.Parameters.AddWithValue("@achievementName", fallbackAchievementName);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private void EnsureAchievementTable()
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            const string createTableSql = @"
                CREATE TABLE IF NOT EXISTS tblAchievements (
                    AchievementID INTEGER PRIMARY KEY AUTOINCREMENT,
                    SaveID INT,
                    PlayerID INT NOT NULL DEFAULT 0,
                    PlayerName TEXT,
                    EndingID INT,
                    AchievementName TEXT,
                    TimesUnlocked INT DEFAULT 0,
                    LastUnlockedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

            using (var createCmd = new SqliteCommand(createTableSql, conn))
            {
                createCmd.ExecuteNonQuery();
            }

            bool hasEndingIdColumn = false;
            using (var pragmaCmd = new SqliteCommand("PRAGMA table_info(tblAchievements);", conn))
            using (var reader = pragmaCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (string.Equals(reader["name"]?.ToString(), "EndingID", StringComparison.OrdinalIgnoreCase))
                    {
                        hasEndingIdColumn = true;
                        break;
                    }
                }
            }

            if (!hasEndingIdColumn)
            {
                using var alterCmd = new SqliteCommand("ALTER TABLE tblAchievements ADD COLUMN EndingID INT;", conn);
                alterCmd.ExecuteNonQuery();
            }

            TryEnsureColumnExists("tblAchievements", "PlayerID", "ALTER TABLE tblAchievements ADD COLUMN PlayerID INT NOT NULL DEFAULT 0;");
            TryEnsureColumnExists("tblAchievements", "PlayerName", "ALTER TABLE tblAchievements ADD COLUMN PlayerName TEXT;");

            const string backfillSql = @"
                UPDATE tblAchievements
                SET EndingID = AchievementID
                WHERE EndingID IS NULL
                  AND AchievementID BETWEEN 1 AND 6;";

            using (var backfillCmd = new SqliteCommand(backfillSql, conn))
            {
                backfillCmd.ExecuteNonQuery();
            }
        }

        private void EnsureUnlockedEndingTable()
        {
            const string createTableSql = @"
                CREATE TABLE IF NOT EXISTS tblUnlockedEnding (
                    UnlockID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerID INT NOT NULL DEFAULT 0,
                    PlayerName TEXT,
                    EndingID INT NOT NULL,
                    UnlockedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";

            using (var createCmd = new SqliteCommand(createTableSql, conn))
            {
                createCmd.ExecuteNonQuery();
            }

            bool requiresMigration = false;
            using (var pragmaCmd = new SqliteCommand("PRAGMA table_info(tblUnlockedEnding);", conn))
            using (var reader = pragmaCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string columnName = reader.GetString(1);
                    int primaryKeyOrdinal = reader.GetInt32(5);
                    if (string.Equals(columnName, "EndingID", StringComparison.OrdinalIgnoreCase) && primaryKeyOrdinal > 0)
                    {
                        requiresMigration = true;
                        break;
                    }
                }
            }

            if (requiresMigration)
            {
                const string migrateSql = @"
                    CREATE TABLE IF NOT EXISTS tblUnlockedEnding_New (
                        UnlockID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PlayerID INT NOT NULL DEFAULT 0,
                        PlayerName TEXT,
                        EndingID INT NOT NULL,
                        UnlockedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                    );

                    INSERT INTO tblUnlockedEnding_New (PlayerID, PlayerName, EndingID, UnlockedAt)
                    SELECT 0, 'Guest', EndingID, UnlockedAt
                    FROM tblUnlockedEnding;

                    DROP TABLE tblUnlockedEnding;
                    ALTER TABLE tblUnlockedEnding_New RENAME TO tblUnlockedEnding;";

                using var migrateCmd = new SqliteCommand(migrateSql, conn);
                migrateCmd.ExecuteNonQuery();
            }

            TryEnsureColumnExists("tblUnlockedEnding", "PlayerID", "ALTER TABLE tblUnlockedEnding ADD COLUMN PlayerID INT NOT NULL DEFAULT 0;");
            TryEnsureColumnExists("tblUnlockedEnding", "PlayerName", "ALTER TABLE tblUnlockedEnding ADD COLUMN PlayerName TEXT;");

            using var indexCmd = new SqliteCommand("CREATE UNIQUE INDEX IF NOT EXISTS IX_tblUnlockedEnding_Player_Ending ON tblUnlockedEnding(PlayerID, EndingID);", conn);
            indexCmd.ExecuteNonQuery();
        }

        private string GetAchievementNameForEnding(int endingId)
        {
            return endingId switch
            {
                1 => "That time I got trapped by a mad chef in their restaurant",
                2 => "I guess the owner wants me to sleep forever?",
                3 => "Is playing games forever a good thing or not?",
                4 => "This mad rat wants me mad",
                5 => "I guess I'm trapped in a paradise",
                6 => "Does that even change my current lifestyle...",
                _ => $"Ending {endingId}"
            };
        }

        private Dictionary<Vector2, int> LoadMap(string filepath)
        {
            Dictionary<Vector2, int> result = new();

            StreamReader reader = new(filepath);

            int y = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(',');
                for (int x = 0; x < items.Length; x++)
                {
                    if (int.TryParse(items[x], out int value))
                    {
                        if (value > -1)
                        result[new Vector2(x, y)] = value;
                    }
                }
                y++;
            }
            return result;
        }

        private bool IsTopDownScene(int sceneId)
        {
            return sceneId == 5 || sceneId == 6 || sceneId == 21;
        }

        private bool IsFollowerPartyScene(int sceneId)
        {
            return sceneId == 18 || sceneId == 19;
        }

        private bool IsDoorScene(int sceneId)
        {
            return sceneId >= 7 && sceneId <= 12;
        }

        private bool IsHiddenWallScene(int sceneId)
        {
            return sceneId >= HiddenWallSceneId && sceneId <= HiddenWallFinalSceneId;
        }

        private Dictionary<Vector2, int> GetSceneMap(int sceneId)
        {
            return sceneId switch
            {
                6 => doors,
                21 => goodbye,
                _ => main
            };
        }

        private Texture2D GetTopDownTextureAtlas(int sceneId)
        {
            return textureAtlas ?? throw new InvalidOperationException("Top-down texture atlas is not loaded.");
        }

        private Dictionary<Vector2, int> GetSceneCollisionLayer(int sceneId)
        {
            return sceneId switch
            {
                6 => scene6CollisionLayer,
                21 => scene21CollisionLayer,
                _ => scene5CollisionLayer
            };
        }

        private bool IsOnDoorTrigger(Vector2 worldPosition)
        {
            return GetDoorLabelAtPosition(worldPosition) != null;
        }

        private int GetDoorSceneId(string? doorLabel)
        {
            if (string.IsNullOrWhiteSpace(doorLabel))
                return -1;

            return scene6DoorSceneIds.TryGetValue(doorLabel, out int sceneId) ? sceneId : -1;
        }

        private string? GetDoorLabelAtPosition(Vector2 worldPosition)
        {
            if (currentSceneId != 6 || scene6InteractLayer.Count == 0)
                return null;

            Vector2 tilePos = WorldToTilePosition(worldPosition, GetSceneMap(currentSceneId));
            return scene6DoorLabels.TryGetValue(tilePos, out string label) ? label : null;
        }

        private string? GetScene21LabelAtPosition(Vector2 worldPosition)
        {
            if (currentSceneId != 21 || scene21LabelCenters.Count == 0)
                return null;

            Vector2 tilePos = WorldToTilePosition(worldPosition, GetSceneMap(currentSceneId));
            return scene21Labels.TryGetValue(tilePos, out string label) ? label : null;
        }

        private Vector2 WorldToTilePosition(Vector2 worldPosition, Dictionary<Vector2, int> currentMap)
        {
            int displayTileSize = GetTopDownDisplayTileSize(currentMap);

            int minX = currentMap.Keys.Min(k => (int)k.X);
            int minY = currentMap.Keys.Min(k => (int)k.Y);
            int maxX = currentMap.Keys.Max(k => (int)k.X);
            int maxY = currentMap.Keys.Max(k => (int)k.Y);

            int offsetX = (_graphics.PreferredBackBufferWidth - ((maxX - minX + 1) * displayTileSize)) / 2;
            int offsetY = (_graphics.PreferredBackBufferHeight - ((maxY - minY + 1) * displayTileSize)) / 2;

            int tileX = minX + (int)((worldPosition.X - offsetX) / displayTileSize);
            int tileY = minY + (int)((worldPosition.Y - offsetY) / displayTileSize);
            return new Vector2(tileX, tileY);
        }

        private Vector2 TileToWorldPosition(Vector2 tilePosition, Dictionary<Vector2, int> currentMap)
        {
            int displayTileSize = GetTopDownDisplayTileSize(currentMap);

            int minX = currentMap.Keys.Min(k => (int)k.X);
            int minY = currentMap.Keys.Min(k => (int)k.Y);
            int maxX = currentMap.Keys.Max(k => (int)k.X);
            int maxY = currentMap.Keys.Max(k => (int)k.Y);

            int offsetX = (_graphics.PreferredBackBufferWidth - ((maxX - minX + 1) * displayTileSize)) / 2;
            int offsetY = (_graphics.PreferredBackBufferHeight - ((maxY - minY + 1) * displayTileSize)) / 2;

            return new Vector2(
                offsetX + ((tilePosition.X - minX) * displayTileSize) + displayTileSize / 2f,
                offsetY + ((tilePosition.Y - minY) * displayTileSize) + displayTileSize / 2f);
        }

        private void BuildScene6DoorLabels()
        {
            scene6DoorLabels.Clear();
            scene6DoorLabelCenters.Clear();

            var triggerTiles = scene6InteractLayer
                .Where(kv => kv.Value == 0)
                .Select(kv => kv.Key)
                .ToHashSet();

            if (triggerTiles.Count == 0)
                return;

            List<List<Vector2>> doorGroups = new();

            while (triggerTiles.Count > 0)
            {
                Vector2 start = triggerTiles.First();
                Queue<Vector2> queue = new();
                List<Vector2> group = new();

                queue.Enqueue(start);
                triggerTiles.Remove(start);

                while (queue.Count > 0)
                {
                    Vector2 current = queue.Dequeue();
                    group.Add(current);

                    Vector2[] neighbors =
                    {
                        new Vector2(current.X + 1, current.Y),
                        new Vector2(current.X - 1, current.Y),
                        new Vector2(current.X, current.Y + 1),
                        new Vector2(current.X, current.Y - 1)
                    };

                    foreach (Vector2 neighbor in neighbors)
                    {
                        if (triggerTiles.Remove(neighbor))
                            queue.Enqueue(neighbor);
                    }
                }

                doorGroups.Add(group);
            }

            var orderedGroups = doorGroups
                .OrderBy(group => group.Average(tile => tile.X))
                .ToList();

            for (int i = 0; i < orderedGroups.Count && i < scene6DoorNames.Length; i++)
            {
                string label = scene6DoorNames[i];
                float centerX = (float)orderedGroups[i].Average(tile => tile.X);
                float topY = orderedGroups[i].Min(tile => tile.Y);
                scene6DoorLabelCenters[label] = new Vector2(centerX, topY);

                foreach (Vector2 tile in orderedGroups[i])
                    scene6DoorLabels[tile] = label;
            }
        }

        private void DrawScene6DoorLabels(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (currentSceneId != 6 || dialogueFont == null || scene6DoorLabelCenters.Count == 0)
                return;

            Dictionary<Vector2, int> currentMap = GetSceneMap(currentSceneId);
            int displayTileSize = GetTopDownDisplayTileSize(currentMap);

            int minX = currentMap.Keys.Min(k => (int)k.X);
            int minY = currentMap.Keys.Min(k => (int)k.Y);
            int maxX = currentMap.Keys.Max(k => (int)k.X);
            int maxY = currentMap.Keys.Max(k => (int)k.Y);

            int mapWidth = (maxX - minX + 1) * displayTileSize;
            int mapHeight = (maxY - minY + 1) * displayTileSize;
            int offsetX = (screenWidth - mapWidth) / 2;
            int offsetY = (screenHeight - mapHeight) / 2;

            foreach (var entry in scene6DoorLabelCenters)
            {
                string promptText = string.Equals(entry.Key, currentDoorLabel, StringComparison.OrdinalIgnoreCase)
                    ? $"F: {entry.Key}"
                    : entry.Key;

                Vector2 promptSize = dialogueFont.MeasureString(promptText);
                float worldX = offsetX + ((entry.Value.X - minX) * displayTileSize) + displayTileSize / 2f;
                float worldY = offsetY + ((entry.Value.Y - minY) * displayTileSize) - promptSize.Y - 18f;

                float promptX = Math.Max(10f, Math.Min(worldX - (promptSize.X / 2f), screenWidth - promptSize.X - 10f));
                float promptY = Math.Max(10f, worldY);
                Color promptColor = string.Equals(entry.Key, currentDoorLabel, StringComparison.OrdinalIgnoreCase)
                    ? Color.Yellow
                    : Color.White;

                spriteBatch.DrawString(dialogueFont, promptText, new Vector2(promptX, promptY), promptColor);
            }
            return;
        }

        private void DrawScene21Labels(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (currentSceneId != 21 || dialogueFont == null || scene21LabelCenters.Count == 0)
                return;

            Dictionary<Vector2, int> currentMap = GetSceneMap(currentSceneId);
            int displayTileSize = GetTopDownDisplayTileSize(currentMap);

            int minX = currentMap.Keys.Min(k => (int)k.X);
            int minY = currentMap.Keys.Min(k => (int)k.Y);
            int maxX = currentMap.Keys.Max(k => (int)k.X);
            int maxY = currentMap.Keys.Max(k => (int)k.Y);

            int mapWidth = (maxX - minX + 1) * displayTileSize;
            int mapHeight = (maxY - minY + 1) * displayTileSize;
            int offsetX = (screenWidth - mapWidth) / 2;
            int offsetY = (screenHeight - mapHeight) / 2;

            foreach (var entry in scene21LabelCenters)
            {
                // label key ends with index; trim the numeric suffix for display
                string tileKey = entry.Key;
                Vector2 worldCenter = entry.Value;
                string baseLabel = tileKey.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                if (string.IsNullOrWhiteSpace(baseLabel))
                    baseLabel = "Companion";

                // Remove non-printable / zero-width characters so SpriteFont.MeasureString won't throw
                string displayLabel = new string(baseLabel.Where(ch => !char.IsControl(ch) && ch != '\u200B').ToArray());
                if (string.IsNullOrWhiteSpace(displayLabel))
                    displayLabel = "Companion";

                string promptText = string.Equals(tileKey, currentScene21Label, StringComparison.OrdinalIgnoreCase) ? $"F: {displayLabel}" : displayLabel;

                Vector2 promptSize = dialogueFont.MeasureString(promptText);
                float worldX = offsetX + ((worldCenter.X - minX) * displayTileSize) + displayTileSize / 2f;
                float worldY = offsetY + ((worldCenter.Y - minY) * displayTileSize) - promptSize.Y - 4f;

                float promptX = Math.Max(10f, Math.Min(worldX - (promptSize.X / 2f), screenWidth - promptSize.X - 10f));
                float promptY = Math.Max(10f, worldY);
                Color promptColor = string.Equals(tileKey, currentScene21Label, StringComparison.OrdinalIgnoreCase) ? Color.Yellow : Color.White;

                spriteBatch.DrawString(dialogueFont, promptText, new Vector2(promptX, promptY), promptColor);
            }

            if (goodbye.Count > 0)
            {
                int branchMinX = goodbye.Keys.Min(k => (int)k.X);
                int branchMaxX = goodbye.Keys.Max(k => (int)k.X);
                int branchMaxY = goodbye.Keys.Max(k => (int)k.Y);
                Vector2 bottomCenter = TileToWorldPosition(new Vector2((branchMinX + branchMaxX) / 2f, branchMaxY), goodbye);
                string bottomLabel = "Alone";
                Vector2 bottomLabelSize = dialogueFont.MeasureString(bottomLabel);
                float bottomX = Math.Max(10f, Math.Min(bottomCenter.X - (bottomLabelSize.X / 2f), screenWidth - bottomLabelSize.X - 10f));
                float bottomY = Math.Min(screenHeight - bottomLabelSize.Y - 18f, bottomCenter.Y + 22f);
                spriteBatch.DrawString(dialogueFont, bottomLabel, new Vector2(bottomX, bottomY), Color.White);
            }
        }

private void DrawBackgroundFill(SpriteBatch spriteBatch, Texture2D backgroundTexture, int screenWidth, int screenHeight)
        {
            float scaleX = screenWidth / (float)backgroundTexture.Width;
            float scaleY = screenHeight / (float)backgroundTexture.Height;
            float scale = Math.Max(scaleX, scaleY);

            int destWidth = (int)Math.Ceiling(backgroundTexture.Width * scale);
            int destHeight = (int)Math.Ceiling(backgroundTexture.Height * scale);
            int destX = (screenWidth - destWidth) / 2;
            int destY = (screenHeight - destHeight) / 2;

            spriteBatch.Draw(backgroundTexture, new Rectangle(destX, destY, destWidth, destHeight), Color.White);
        }

        private void DrawSideSceneGround(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            // Do not draw the ground overlay for cinematic scenes where a full-background
            // image is preferred (remove the visible ground marking for these scenes).
            if (currentSceneId == 33 || currentSceneId == 34 || currentSceneId == 35 || currentSceneId == 36 || currentSceneId == 38 ||
                currentSceneId == 39 || currentSceneId == 40 || currentSceneId == 41)
                return;

            // Adjust ground position for scene 13 (move it up 5 pixels)
            float groundAdjustment = (currentSceneId == 13) ? scene13GroundAdjustment : 0f;
            int topY = (int)(GetSceneGroundY(currentSceneId) - groundAdjustment);
            int groundHeightPixels = Math.Max(0, screenHeight - topY);

            if (groundHeightPixels <= 0)
                return;

            Color groundColor = Color.Black;
            Color lipColor = new Color(38, 38, 38);
            Color shadowColor = Color.Black * 0.4f;

            spriteBatch.Draw(ground, new Rectangle(0, topY, screenWidth, groundHeightPixels), groundColor);
            spriteBatch.Draw(ground, new Rectangle(0, topY, screenWidth, 8), lipColor);
            spriteBatch.Draw(ground, new Rectangle(0, topY + 8, screenWidth, 24), shadowColor);
        }

        private float GetSceneGroundY(int sceneId)
        {
            int screenHeight = _graphics.PreferredBackBufferHeight;

            if (sceneId == 4 || sceneId == 18 || sceneId == 19)
                return screenHeight * 0.78f;

            float sceneGroundY = screenHeight * 0.60f;

            if (sceneId == 33)
                return sceneGroundY + 265f;

            if (sceneId == 35)
                return sceneGroundY + 205f;

            if (sceneId == 34 || sceneId == 36)
                return sceneGroundY + 155f;

            if (sceneId == 38)
                return sceneGroundY + 40f;

            if (sceneId == 41)
                return sceneGroundY + 245f;

            return sceneGroundY;
        }

        private Vector2 ResolveScene5SpawnTile(List<Vector2> walkableTiles)
        {
            Vector2 preferred = new Vector2(0, 5);
            if (walkableTiles.Contains(preferred))
                return preferred;

            float leftMostX = walkableTiles.Min(tile => tile.X);
            var leftTiles = walkableTiles
                .Where(tile => tile.X == leftMostX)
                .OrderBy(tile => Math.Abs(tile.Y - preferred.Y))
                .ThenBy(tile => tile.Y)
                .ToList();

            if (leftTiles.Count > 0)
                return leftTiles[0];

            return walkableTiles[0];
        }

        private bool IsScene5EndingTrigger(Vector2 tilePos)
        {
            Dictionary<Vector2, int> collisionLayer = GetSceneCollisionLayer(5);
            var walkableTiles = collisionLayer
                .Where(kv => kv.Value == 1)
                .Select(kv => kv.Key)
                .ToList();

            if (walkableTiles.Count == 0)
                return false;

            float rightMostX = walkableTiles.Max(tile => tile.X);
            var rightExitBand = walkableTiles
                .Where(tile => tile.X >= rightMostX - 1f)
                .ToList();

            if (rightExitBand.Count == 0)
                return false;

            float upperBandMaxY = rightExitBand.Max(tile => tile.Y);
            return tilePos.X >= rightMostX - 1f && tilePos.Y <= upperBandMaxY;
        }

        private bool IsScene5HiddenWallTriggerPosition(Vector2 worldPosition)
        {
            if (currentSceneId != 5 || main.Count == 0)
                return false;

            Vector2 tilePos = WorldToTilePosition(worldPosition, main);
            int maxX = main.Keys.Max(k => (int)k.X);
            int minY = main.Keys.Min(k => (int)k.Y);

            return tilePos.X >= maxX - 1 && tilePos.Y <= minY + 6;
        }

        private bool IsScene21BottomExitPosition(Vector2 worldPosition)
        {
            if (currentSceneId != 21 || goodbye.Count == 0 || scene21InteractLayer.Count == 0)
                return false;

            Vector2 tilePos = WorldToTilePosition(worldPosition, goodbye);
            if (!scene21InteractLayer.TryGetValue(tilePos, out int tileValue) || tileValue != 0)
                return false;

            int maxY = goodbye.Keys.Max(k => (int)k.Y);
            int minX = goodbye.Keys.Min(k => (int)k.X);
            int maxX = goodbye.Keys.Max(k => (int)k.X);
            float centerX = (minX + maxX) / 2f;
            float centerBand = Math.Max(2f, (maxX - minX) * 0.25f);
            return tilePos.Y >= maxY - 1 && Math.Abs(tilePos.X - centerX) <= centerBand;
        }

        private bool IsScene21LeftExitPosition(Vector2 worldPosition)
        {
            if (currentSceneId != 21 || goodbye.Count == 0)
                return false;

            Vector2 tilePos = WorldToTilePosition(worldPosition, goodbye);
            int minX = goodbye.Keys.Min(k => (int)k.X);
            return tilePos.X <= minX + 1;
        }

        private bool IsScene21RightExitPosition(Vector2 worldPosition)
        {
            if (currentSceneId != 21 || goodbye.Count == 0)
                return false;

            Vector2 tilePos = WorldToTilePosition(worldPosition, goodbye);
            int maxX = goodbye.Keys.Max(k => (int)k.X);
            return tilePos.X >= maxX - 1;
        }

        private bool TryTriggerScene21Branch()
        {
            if (currentSceneId != 21 || dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isChoiceActive || isNarrationActive)
                return false;

            // Trigger bottom/center path: either map-defined bottom exit tile OR player walks to the center-bottom area of the screen
            if (!scene21BottomBranchTriggered && (IsScene21BottomExitPosition(position) ||
                (position.Y >= (_graphics.PreferredBackBufferHeight * 0.74f) && Math.Abs(position.X - (_graphics.PreferredBackBufferWidth / 2f)) <= (_graphics.PreferredBackBufferWidth * 0.25f))))
            {
                scene21BottomBranchTriggered = true;
                var pages = LoadNarrationPagesByIds(24, 25);
                if (pages.Count == 0)
                    pages = new List<string> { "Are you sure? Your decision will be permanent until the end of your journey.\nYour journey will also slightly differ based on your circumstances." };
                System.Diagnostics.Debug.WriteLine($"Scene21 bottom branch triggered - narration pages: {pages.Count}");
                StartNarrationSequence(pages, () =>
                {
                    System.Diagnostics.Debug.WriteLine("Starting choice menu for Scene21 bottom branch using DialogueID 191");
                    companionRoute = CompanionRoute.None;
                    // Present only one Yes -> scene22 and one Go Back -> scene21
                    OpenChoiceMenuFromDialogue(191, overrideYesTargetSceneId: 22);
                });
                return true;
            }

            return false;
        }

        private string ResolveMusicTrack(string musicTrack)
        {
            if (string.Equals(musicTrack, "doors", StringComparison.OrdinalIgnoreCase))
                return "door";
            if (string.Equals(musicTrack, "end", StringComparison.OrdinalIgnoreCase))
                return "ending";
            return musicTrack;
        }

        private string ResolveBackgroundAsset(string backgroundAssetName)
        {
            return string.Equals(backgroundAssetName, "END", StringComparison.OrdinalIgnoreCase)
                ? "end"
                : backgroundAssetName;
        }

        // Allow callers to provide only the primary asset name. If the primary asset
        // is missing, attempt the fallback. As a final safety, return a 1x1 white
        // texture to avoid null-reference exceptions in rendering code.
        private Texture2D LoadTextureWithFallback(string primaryAssetName, string fallbackAssetName = "MC_idle1")
        {
            try
            {
                return Content.Load<Texture2D>(primaryAssetName);
            }
            catch (Exception)
            {
                try
                {
                    return Content.Load<Texture2D>(fallbackAssetName);
                }
                catch (Exception)
                {
                    var tex = new Texture2D(GraphicsDevice, 1, 1);
                    tex.SetData(new[] { Color.White });
                    return tex;
                }
            }
        }

        private List<Microsoft.Xna.Framework.Rectangle> GetChoiceOptionRectangles()
        {
            List<Microsoft.Xna.Framework.Rectangle> rects = new List<Microsoft.Xna.Framework.Rectangle>();
            int optionWidth = 420;
            int optionHeight = 64;
            int gap = 16;
            int totalHeight = (activeChoices.Count * optionHeight) + ((activeChoices.Count - 1) * gap);
            int startY = (int)Math.Max(groundY + 30f, _graphics.PreferredBackBufferHeight - totalHeight - 120);
            int startX = (_graphics.PreferredBackBufferWidth - optionWidth) / 2;

            for (int i = 0; i < activeChoices.Count; i++)
            {
                rects.Add(new Microsoft.Xna.Framework.Rectangle(startX, startY + (i * (optionHeight + gap)), optionWidth, optionHeight));
            }

            return rects;
        }

        private string WrapChoiceText(string text, float maxWidth)
        {
            string[] words = text.Split(' ');
            string line = string.Empty;
            string result = string.Empty;

            foreach (string word in words)
            {
                string testLine = string.IsNullOrEmpty(line) ? word : line + " " + word;
                if (dialogueFont.MeasureString(testLine).X > maxWidth)
                {
                    result += line + "\n";
                    line = word;
                }
                else
                {
                    line = testLine;
                }
            }

            return result + line;
        }

        private string NormalizeTextForSpriteFont(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            string normalized = text
                .Replace('\u2018', '\'')
                .Replace('\u2019', '\'')
                .Replace('\u201C', '"')
                .Replace('\u201D', '"')
                .Replace('\u2013', '-')
                .Replace('\u2014', '-')
                .Replace('\u2610', '\u25A0')
                .Replace("\u2026", "...");

            string decomposed = normalized.Normalize(NormalizationForm.FormD);
            StringBuilder builder = new StringBuilder(decomposed.Length);

            foreach (char c in decomposed)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.NonSpacingMark)
                    continue;

                if (category == UnicodeCategory.SpaceSeparator ||
                    category == UnicodeCategory.LineSeparator ||
                    category == UnicodeCategory.ParagraphSeparator)
                {
                    builder.Append(' ');
                    continue;
                }

                if (c == '\n' || c == '\r' || c == '\t')
                {
                    builder.Append(c);
                    continue;
                }

                if (c == '\u266A' || c == '\u266B')
                {
                    builder.Append(c);
                    continue;
                }

                if (c == '\u25A0')
                {
                    builder.Append(c);
                    continue;
                }

                if (c >= 32 && c <= 126)
                    builder.Append(c);
                else
                    builder.Append('?');
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private void UpdateExitPrompt(KeyboardState previousKeyState, MouseState previousMouseState)
        {
            if ((keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W)) &&
                previousKeyState.IsKeyUp(Keys.Left) && previousKeyState.IsKeyUp(Keys.A) &&
                previousKeyState.IsKeyUp(Keys.Up) && previousKeyState.IsKeyUp(Keys.W))
            {
                selectedExitPromptIndex = 0;
            }
            else if ((keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D) || keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Tab)) &&
                     previousKeyState.IsKeyUp(Keys.Right) && previousKeyState.IsKeyUp(Keys.D) &&
                     previousKeyState.IsKeyUp(Keys.Down) && previousKeyState.IsKeyUp(Keys.S) && previousKeyState.IsKeyUp(Keys.Tab))
            {
                selectedExitPromptIndex = 1;
            }

            Rectangle[] optionRects = GetExitPromptOptionRectangles();
            Point mousePoint = new Point(mouseState.X, mouseState.Y);
            for (int i = 0; i < optionRects.Length; i++)
            {
                if (optionRects[i].Contains(mousePoint))
                {
                    selectedExitPromptIndex = i;
                    break;
                }
            }

            bool confirm = keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter);
            bool cancel = keyState.IsKeyDown(Keys.Escape) && previousKeyState.IsKeyUp(Keys.Escape);
            bool click = mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;

            if (click)
            {
                if (optionRects[0].Contains(mousePoint))
                    confirm = true;
                else if (optionRects[1].Contains(mousePoint))
                    cancel = true;
            }

            if (confirm)
            {
                if (selectedExitPromptIndex == 0)
                    SaveAndExit();
                else
                    showExitPrompt = false;
            }
            else if (cancel)
            {
                showExitPrompt = false;
            }
        }

        private Microsoft.Xna.Framework.Rectangle[] GetExitPromptOptionRectangles()
        {
            int screenWidth = _graphics.PreferredBackBufferWidth;
            int screenHeight = _graphics.PreferredBackBufferHeight;
            int boxWidth = 860;
            int boxHeight = 220;
            int boxX = (screenWidth - boxWidth) / 2;
            int boxY = (screenHeight - boxHeight) / 2;
            int optionY = boxY + boxHeight - 82;
            int optionWidth = 140;
            int optionHeight = 52;

            return new[]
            {
                new Microsoft.Xna.Framework.Rectangle((screenWidth / 2) - 150, optionY, optionWidth, optionHeight),
                new Microsoft.Xna.Framework.Rectangle((screenWidth / 2) + 10, optionY, optionWidth, optionHeight)
            };
        }

        private void SplitDoorSceneDialogues(
            List<DialogueEntry> sourceDialogues,
            out List<DialogueEntry> introDialogues,
            out List<DialogueEntry> afterItemDialogues,
            out List<DialogueEntry> edgeDialogues,
            out string? itemToken)
        {
            introDialogues = new List<DialogueEntry>();
            afterItemDialogues = new List<DialogueEntry>();
            edgeDialogues = new List<DialogueEntry>();
            itemToken = null;

            bool itemSeen = false;
            bool secondPartStarted = false;
            foreach (DialogueEntry entry in sourceDialogues)
            {
                if (!secondPartStarted && IsLeaveDialogueMarker(entry.Dialogue))
                {
                    secondPartStarted = true;
                }

                if (secondPartStarted)
                {
                    edgeDialogues.Add(entry);
                }
                else if (!itemSeen && TryExtractItemToken(entry.Dialogue, out string extractedItemToken))
                {
                    itemSeen = true;
                    itemToken = extractedItemToken;
                }
                else if (!itemSeen)
                {
                    introDialogues.Add(entry);
                }
                else
                {
                    afterItemDialogues.Add(entry);
                }
            }
        }

        private int ResolveDoorSceneChoiceDialogueId(List<DialogueEntry> edgeDialogues)
        {
            int choiceDialogueId = -1;

            foreach (DialogueEntry entry in edgeDialogues)
            {
                List<ChoiceEntry> choices = LoadChoices(entry.DialogueID);
                bool hasStayLeaveChoices = choices.Any(choice =>
                    string.Equals(choice.ChoiceText?.Trim(), "Stay", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(choice.ChoiceText?.Trim(), "Leave", StringComparison.OrdinalIgnoreCase));

                if (hasStayLeaveChoices)
                    choiceDialogueId = entry.DialogueID;
            }

            return choiceDialogueId;
        }

        private bool IsItemDialogueMarker(string dialogueText)
        {
            return TryExtractItemToken(dialogueText, out _);
        }

        private bool TryExtractItemToken(string dialogueText, out string itemToken)
        {
            itemToken = string.Empty;

            if (string.IsNullOrWhiteSpace(dialogueText))
                return false;

            string normalized = dialogueText.Trim().ToUpperInvariant();
            string[] knownTokens = { "ITEM1", "ITEM2", "ITEM3", "ITEM4", "ITEM5", "ITEM6" };

            foreach (string token in knownTokens)
            {
                if (normalized.Contains(token, StringComparison.Ordinal))
                {
                    itemToken = token;
                    return true;
                }
            }

            return false;
        }

        private bool IsLeaveDialogueMarker(string dialogueText)
        {
            string normalized = dialogueText.Trim();
            return normalized.Equals("Leave", StringComparison.OrdinalIgnoreCase) ||
                   normalized.StartsWith("MY APOLOGIES", StringComparison.OrdinalIgnoreCase);
        }

        private void InitializeHiddenWallScene(int sceneId)
        {
            if (!IsHiddenWallScene(sceneId))
                return;

            if (sceneId == HiddenWallRetrySceneId)
            {
                StartHiddenWallDialogueSequence(
                    BuildHiddenWallDialogueEntries(sceneId,
                        (GetHiddenWallPlayerSpeaker(), "Looks perfect! The structure may be fragile, but if I heed it with care, I'm confident I can move up there!"),
                        (GetHiddenWallPlayerSpeaker(), "I wonder what the other side looks like. Is it gloomy like the places I've passed."),
                        (GetHiddenWallPlayerSpeaker(), "Will I encounter people like me there too?")),
                    () => OpenChoiceMenuFromDialogue(122));
            }
            else if (sceneId == HiddenWallFinalSceneId)
            {
                StartNarrationSequence(
                    LoadNarrationPagesByIds(2),
                    () => StartHiddenWallDialogueSequence(
                        BuildHiddenWallDialogueEntries(sceneId,
                            (GetHiddenWallPlayerSpeaker(), "..."),
                            (GetHiddenWallPlayerSpeaker(), "Man, why am I doing this..?")),
                        () => OpenChoiceMenuFromDialogue(133)));
            }
        }

        private bool TryTriggerScene13CenterNarration(KeyboardState previousKeyState)
        {
            if (currentSceneId != HiddenWallSceneId || scene13CenterNarrationTriggered)
                return false;

            if (dialogueManager.IsActive || showDoorItemPopup || isChoiceActive || isNarrationActive || activeEnding != null || endingTransitionActive)
                return false;

            if (position.X < _graphics.PreferredBackBufferWidth * 0.5f)
                return false;

            if (!(keyState.IsKeyDown(Keys.F) && previousKeyState.IsKeyUp(Keys.F)))
                return false;

            scene13CenterNarrationTriggered = true;
            StartHiddenWallDialogueSequence(
                BuildHiddenWallDialogueEntries(HiddenWallSceneId,
                    (GetHiddenWallPlayerSpeaker(), "A wall..."),
                    (GetHiddenWallPlayerSpeaker(), "peeks on the holes"),
                    (GetHiddenWallPlayerSpeaker(), "There's a path ahead! Then why is there a wall in here?"),
                    ("System", "Is someone hiding something..? Or do they want me to end the journey here?"),
                    (GetHiddenWallPlayerSpeaker(), "Hmp! They shouldn't have underestimated me."),
                    (GetHiddenWallPlayerSpeaker(), "If I try to make a makeshift ladder or stair out of this trash, I'm certain I can cross the wall!")),
                () => OpenChoiceMenuFromDialogue(118));
            return true;
        }

        private void StartNarrationSequence(IEnumerable<string> pages, Action? onComplete = null)
        {
            // Preserve pages exactly as stored (including intentional empty pages used to create spacing).
            // Null pages are converted to empty strings. Do not filter out whitespace-only pages because
            // they may be used deliberately as blank lines / page separators in poem blocks.
            activeNarrationPages = pages
                .Select(page => page ?? string.Empty)
                .ToList();

            if (activeNarrationPages.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            isNarrationActive = true;
            activeNarrationIndex = 0;
            narrationCompletionAction = onComplete;
            isMoving = false;
            currentAnimation = idleAnimation;
        }

        private void UpdateNarrationSequence(KeyboardState previousKeyState)
        {
            if (activeNarrationPages.Count == 0)
            {
                isNarrationActive = false;
                narrationCompletionAction?.Invoke();
                narrationCompletionAction = null;
                return;
            }

            if (keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter))
            {
                // Advance to next narration page. Allow a special case where Scene21 narration
                // can be flagged as skippable: if skippable, pressing Enter on the last page
                // will immediately invoke completion. Otherwise preserve normal flow.
                activeNarrationIndex++;
                if (activeNarrationIndex >= activeNarrationPages.Count)
                {
                    isNarrationActive = false;
                    activeNarrationIndex = 0;
                    Action? onComplete = narrationCompletionAction;
                    narrationCompletionAction = null;
                    // Clear pages only after completion to preserve potential re-use when debugging.
                    activeNarrationPages.Clear();
                    onComplete?.Invoke();
                }
            }
        }

        private void StartHiddenWallDialogueSequence(List<DialogueEntry> dialogues, Action? onComplete = null)
        {
            if (dialogues.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            hiddenWallDialogueCompletionAction = onComplete;
            hiddenWallDialogueAwaitingCompletion = onComplete != null;
            dialogueManager.StartDialogue(dialogues, dialogueBoxTexture, dialogueBoxTexture2, false);
        }

        private List<DialogueEntry> BuildHiddenWallDialogueEntries(int sceneId, params (string Speaker, string Dialogue)[] lines)
        {
            List<DialogueEntry> entries = new();
            foreach (var line in lines)
            {
                entries.Add(new DialogueEntry(transientDialogueIdSeed--, sceneId, line.Speaker, line.Dialogue));
            }

            return entries;
        }

        private string GetHiddenWallPlayerSpeaker()
        {
            return "Player";
        }

        private float GetHiddenWallRightLimit()
        {
            return _graphics.PreferredBackBufferWidth * 0.82f;
        }

        private List<string> LoadNarrationPagesByIds(params int[] narrationIds)
        {
            List<string> pages = new();
            foreach (int narrationId in narrationIds)
            {
                string? page = LoadNarrationTextById(narrationId);
                // Preserve pages exactly as stored (including leading/trailing whitespace).
                // Do NOT Trim here because poem lines may rely on leading spaces for formatting
                // and blank pages should remain blank. Convert null to empty string so callers
                // always receive a string entry per requested id.
                pages.Add(page ?? string.Empty);
            }

            return pages;
        }

        private string? LoadNarrationTextById(int narrationId)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using var cmd = new SqliteCommand(
                    "SELECT Narration FROM tblNarration WHERE NarrationID = @narrationId LIMIT 1;",
                    conn);
                cmd.Parameters.AddWithValue("@narrationId", narrationId);
                object? value = cmd.ExecuteScalar();
                if (value != null && value != DBNull.Value)
                    return value.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Narration lookup failed for {narrationId}: {ex.Message}");
            }

            return narrationId switch
            {
                1 => "You attempted to climb up there, but because of an idiotic mistake, you didn't notice you tried to walk... in the air.",
                2 => "Wow, congratulations, you just destroyed your makeshift ladder.",
                3 => "This time, you are a more experienced person. With great courage, you stepped into the material that could give out any time. With trust, you stretched out your arms with the hope your strength won't fall short. With focus, you are able to make it to the top with no accidents!",
                4 => "Congratulations for passing the first obstacle!",
                5 => "The journey ahead will be different from what you have encountered in the past. This time, it won't be because of your choice but of circumstances... I pray for the success of your travels.",
                6 => "Are you serious? There's nothing much to do here...",
                7 => "A quiet darkness settled over the path ahead.",
                8 => "If a road forgets your name, keep walking.",
                9 => "",
                10 => "There is nothing like a good friend",
                11 => "To tell your troubles to.",
                12 => "Someone to open up with,",
                13 => "Someone to talk you through,",
                14 => "The highs; the lows,",
                15 => "The everyday struggles,",
                16 => "Someone who knows,",
                17 => "Your every quirk and flaw,",
                18 => "Someone who understands your ways,",
                19 => " And loves you after all.",
                20 => "",
                21 => "-Mobi world-",
                // Scene21 companion confirmation pages moved to ids 24/25 (see caller changes)
                24 => "Are you sure? Your decision will be permanent until the end of your journey.",
                25 => "Your journey will also slightly differ based on your circumstances.",
                _ => null
            };
        }

        // Open a choice menu by loading choices from the database for a specific DialogueID.
        // If loopOnStay is true and the Stay option is selected, the narration with id stayNarrationId
        // will play and the choice menu will reopen (loop) until the player selects the other option.
        private void OpenChoiceMenuFromDialogue(int dialogueId, bool loopOnStay = false, int? stayNarrationId = null, int? overrideYesTargetSceneId = null)
        {
            List<ChoiceEntry> choices = LoadChoices(dialogueId);
            // For DialogueID 191 (scene21 branch), restrict choices to exactly Yes / Go Back
            // If an overrideYesTargetSceneId is provided, present only a single Yes->override and Go Back->21
            if (dialogueId == 191)
            {
                if (overrideYesTargetSceneId.HasValue)
                {
                    ChoiceEntry yesChoice = choices
                        .Where(c => string.Equals(c.ChoiceText?.Trim(), "Yes", StringComparison.OrdinalIgnoreCase)
                                    && c.TargetSceneID == overrideYesTargetSceneId.Value)
                        .OrderBy(c => c.ChoiceOrder)
                        .FirstOrDefault()
                        ?? new ChoiceEntry(191, 1, "Yes", null, overrideYesTargetSceneId.Value);

                    ChoiceEntry goBackChoice = choices
                        .Where(c => string.Equals(c.ChoiceText?.Trim(), "Go Back", StringComparison.OrdinalIgnoreCase)
                                    && (!c.TargetSceneID.HasValue || c.TargetSceneID == 21))
                        .OrderBy(c => c.ChoiceOrder)
                        .FirstOrDefault()
                        ?? new ChoiceEntry(191, 2, "Go Back", null, 21);

                    choices = new List<ChoiceEntry>
                    {
                        yesChoice,
                        goBackChoice.TargetSceneID == 21 ? goBackChoice : new ChoiceEntry(191, goBackChoice.ChoiceOrder, goBackChoice.ChoiceText, goBackChoice.TargetEndingID, 21)
                    };
                }
                else
                {
                    if (choices == null || choices.Count == 0)
                    {
                        choices = new List<ChoiceEntry>
                        {
                            new ChoiceEntry(191, 1, "Yes", null, 22),
                            new ChoiceEntry(191, 2, "Go Back", null, 21)
                        };
                    }
                    else
                    {
                        // Filter to only Yes/Go Back, preserve targets if present, otherwise force to 22/21
                        var filtered = choices
                            .Where(c => string.Equals(c.ChoiceText?.Trim(), "Yes", StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(c.ChoiceText?.Trim(), "Go Back", StringComparison.OrdinalIgnoreCase))
                            .OrderBy(c => c.ChoiceOrder)
                            .ToList();

                        if (filtered.Count == 0)
                        {
                            filtered = new List<ChoiceEntry>
                            {
                                new ChoiceEntry(191, 1, "Yes", null, 22),
                                new ChoiceEntry(191, 2, "Go Back", null, 21)
                            };
                        }
                        else
                        {
                            // Ensure the Yes/Go Back targets are correct if DB omitted them
                            for (int i = 0; i < filtered.Count; i++)
                            {
                                var c = filtered[i];
                                int src = c.DialogueID;
                                if (string.Equals(c.ChoiceText?.Trim(), "Yes", StringComparison.OrdinalIgnoreCase) && !c.TargetSceneID.HasValue)
                                    filtered[i] = new ChoiceEntry(src, c.ChoiceOrder, c.ChoiceText, c.TargetEndingID, 22);
                                if (string.Equals(c.ChoiceText?.Trim(), "Go Back", StringComparison.OrdinalIgnoreCase) && !c.TargetSceneID.HasValue)
                                    filtered[i] = new ChoiceEntry(src, c.ChoiceOrder, c.ChoiceText, c.TargetEndingID, 21);
                            }
                        }

                        choices = filtered;
                    }
                }
            }
            if ((choices == null || choices.Count == 0) && dialogueId == 139)
            {
                choices = new List<ChoiceEntry>
                {
                    new ChoiceEntry(139, 1, "Climb Down", null, 16),
                    new ChoiceEntry(139, 2, "Stay", null, null)
                };
            }

            if (choices == null || choices.Count == 0)
                return;

            activeChoices = choices;
            selectedChoiceIndex = 0;
            isChoiceActive = true;
            activeChoiceSourceDialogueId = dialogueId;
            activeChoiceLoop = loopOnStay;
            activeChoiceStayNarrationId = stayNarrationId;
            doorSceneChoicePending = false;
            LogActionAnalytics("ChoiceMenuOpened", $"Dialogue:{dialogueId}", $"Choices:{choices.Count}");
        }

        private void OpenBinarySceneChoice(string positiveText, int positiveSceneId, string negativeText, int negativeSceneId)
        {
            activeChoices = new List<ChoiceEntry>
            {
                new ChoiceEntry(-1, 1, positiveText, null, positiveSceneId),
                new ChoiceEntry(-1, 2, negativeText, null, negativeSceneId)
            };
            selectedChoiceIndex = 0;
            isChoiceActive = true;
            activeChoiceSourceDialogueId = null;
            activeChoiceLoop = false;
            activeChoiceStayNarrationId = null;
            doorSceneChoicePending = false;
        }

        private bool TryTriggerDoorSceneEdgeDialogue()
        {
            if (!IsDoorScene(currentSceneId) || currentDoorEdgeDialogues.Count == 0)
                return false;

            // Original logic prevented edge triggers while waiting for the item trigger.
            // For scenes 7-12 we want the edge dialogue/corner logic to fire even if the item
            // trigger flag is set. Remove doorSceneWaitingForItemTrigger from the blocking checks.
            if ((!doorSceneIntroSequenceComplete && !(currentSceneId >= 7 && currentSceneId <= 12))
                || dialogueManager.IsActive || showDoorItemPopup || doorSceneWaitingForAfterItemDialogue)
                return false;

            float leftLimit = 24f;
            float rightLimit = _graphics.PreferredBackBufferWidth - 24f;

            bool tryingLeftEdge = position.X <= leftLimit;
            bool tryingRightEdge = position.X >= rightLimit;

            if (!tryingLeftEdge && !tryingRightEdge)
            {
                doorSceneEdgeTriggerArmed = true;
                return false;
            }

            if (!doorSceneEdgeTriggerArmed)
                return false;

            position.X = tryingLeftEdge ? leftLimit : rightLimit;
            isMoving = false;
            currentAnimation = idleAnimation;

            // Start the edge dialogues automatically (no "Press F" required) and when they finish
            // we set doorSceneChoicePending based on whether the edge dialogues contained a stay/leave marker.
            StartAutomaticDialogueSequence(currentDoorEdgeDialogues, () =>
            {
                // If the DB doesn't provide a suitable stay/leave choice id we still want the menu to present
                // only Stay/Leave. ResolveDoorSceneChoiceDialogueId may be -1; the OpenDoorSceneChoiceMenu below
                // will ensure fallback choices are created.
                doorSceneChoicePending = doorSceneChoiceDialogueId > 0 || true;
            });

            doorSceneEdgeTriggerArmed = false;
            sceneInteractCount++;
            LogActionAnalytics("EdgeDialogueTriggered", tryingLeftEdge ? "LeftEdge" : "RightEdge", doorSceneChoiceDialogueId > 0 ? "ChoicePending" : "DialogueOnly");
            return true;
        }

        private bool IsPlayerNearDoorSceneNpc()
        {
            if (!IsDoorScene(currentSceneId))
                return false;

            float npcX = GetDoorSceneNpcX();
            float triggerDistance = 140f;
            return Math.Abs(position.X - npcX) <= triggerDistance;
        }

        private void OpenDoorSceneChoiceMenu()
        {
            List<ChoiceEntry> choices = BuildDoorSceneEdgeChoices();

            activeChoices = choices;
            selectedChoiceIndex = 0;
            isChoiceActive = true;
            activeChoiceSourceDialogueId = null;
            activeChoiceLoop = false;
            activeChoiceStayNarrationId = null;
            doorSceneItemChoiceActive = false;
            doorSceneEdgeChoiceActive = true;
            doorSceneItemChoicePending = false;
            doorSceneChoicePending = false;
            LogActionAnalytics("ChoiceMenuOpened", "StayOrLeave", $"Choices:{choices.Count}");
        }

        private void OpenDoorSceneItemChoice()
        {
            List<ChoiceEntry> itemChoices = BuildDoorSceneItemChoices(currentSceneId);

            if (itemChoices == null || itemChoices.Count == 0)
            {
                doorSceneItemChoiceActive = false;
                return;
            }

            activeChoices.Clear();
            activeChoices = itemChoices;
            selectedChoiceIndex = 0;
            isChoiceActive = true;
            activeChoiceSourceDialogueId = GetDoorSceneItemChoiceDialogueId(currentSceneId);
            activeChoiceLoop = false;
            activeChoiceStayNarrationId = null;
            doorSceneItemChoiceActive = true;
            doorSceneEdgeChoiceActive = false;
            doorSceneItemChoicePending = false;
            doorSceneChoicePending = false;
            LogActionAnalytics("ChoiceMenuOpened", "DoorSceneItem", $"Choices:{itemChoices.Count}");
        }

        private bool IsDoorSceneItemChoiceSource(int? sourceDialogueId)
        {
            if (!sourceDialogueId.HasValue)
                return false;

            return sourceDialogueId.Value == 41 ||
                   sourceDialogueId.Value == 55 ||
                   sourceDialogueId.Value == 69 ||
                   sourceDialogueId.Value == 82 ||
                   sourceDialogueId.Value == 98 ||
                   sourceDialogueId.Value == 112;
        }

        private void NormalizeDoorSceneChoiceState()
        {
            if (doorSceneItemChoiceActive || (IsDoorScene(currentSceneId) && IsDoorSceneItemChoiceSource(activeChoiceSourceDialogueId)))
            {
                List<ChoiceEntry> expectedItemChoices = BuildDoorSceneItemChoices(currentSceneId);
                bool needsReset = activeChoices.Count != expectedItemChoices.Count ||
                                  activeChoices.Any(choice => expectedItemChoices.All(expected =>
                                      !string.Equals(expected.ChoiceText?.Trim(), choice.ChoiceText?.Trim(), StringComparison.OrdinalIgnoreCase)));

                if (needsReset)
                {
                    activeChoices = expectedItemChoices;
                    selectedChoiceIndex = 0;
                }

                doorSceneItemChoiceActive = true;
                doorSceneEdgeChoiceActive = false;

                return;
            }

            if (!doorSceneEdgeChoiceActive)
                return;

            List<ChoiceEntry> edgeChoices = activeChoices
                .Where(choice =>
                    string.Equals(choice.ChoiceText?.Trim(), "Stay", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(choice.ChoiceText?.Trim(), "Leave", StringComparison.OrdinalIgnoreCase))
                .OrderBy(choice => choice.ChoiceOrder)
                .ToList();

            bool needsEdgeReset = edgeChoices.Count != activeChoices.Count ||
                                  edgeChoices.Count == 0 ||
                                  edgeChoices.Any(choice => string.IsNullOrWhiteSpace(choice.ChoiceText));

            if (needsEdgeReset)
            {
                edgeChoices = BuildDoorSceneEdgeChoices();
            }

            activeChoices = edgeChoices;
            if (selectedChoiceIndex >= activeChoices.Count)
                selectedChoiceIndex = Math.Max(0, activeChoices.Count - 1);
        }

        private void UpdateChoiceSelection(KeyboardState previousKeyState, MouseState previousMouseState, GameTime gameTime)
        {
            if (activeChoices.Count == 0)
            {
                isChoiceActive = false;
                return;
            }

            NormalizeDoorSceneChoiceState();
            if (activeChoices.Count == 0)
            {
                isChoiceActive = false;
                doorSceneItemChoiceActive = false;
                doorSceneEdgeChoiceActive = false;
                doorSceneChoicePending = false;
                return;
            }

            if (scene35TimedChoiceActive && (activeChoiceSourceDialogueId == 298 || activeChoiceSourceDialogueId == Scene35PullChoiceSourceId))
            {
                scene35ChoiceTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (scene35ChoiceTimer <= 0f)
                {
                    HandleScene35ChoiceTimeout();
                    return;
                }
            }

            if (scene41TimedChoiceActive && activeChoiceSourceDialogueId == Scene41ChoiceSourceId)
            {
                scene41ChoiceTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (scene41ChoiceTimer <= 0f)
                {
                    HandleScene41ChoiceTimeout();
                    return;
                }
            }

            if ((keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W)) &&
                previousKeyState.IsKeyUp(Keys.Up) && previousKeyState.IsKeyUp(Keys.W))
            {
                selectedChoiceIndex = (selectedChoiceIndex - 1 + activeChoices.Count) % activeChoices.Count;
            }
            else if ((keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S)) &&
                     previousKeyState.IsKeyUp(Keys.Down) && previousKeyState.IsKeyUp(Keys.S))
            {
                selectedChoiceIndex = (selectedChoiceIndex + 1) % activeChoices.Count;
            }

            List<Rectangle> optionRects = GetChoiceOptionRectangles();
            Point mousePoint = new Point(mouseState.X, mouseState.Y);

            for (int i = 0; i < optionRects.Count; i++)
            {
                if (optionRects[i].Contains(mousePoint))
                {
                    selectedChoiceIndex = i;
                    break;
                }
            }

            if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < optionRects.Count; i++)
                {
                    if (optionRects[i].Contains(mousePoint))
                    {
                        selectedChoiceIndex = i;
                        ConfirmChoiceSelection();
                        return;
                    }
                }
            }

            if (keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter))
                ConfirmChoiceSelection();
        }

        private int GetDoorSceneItemChoiceDialogueId(int sceneId)
        {
            return sceneId switch
            {
                7 => 41,   // Chef
                8 => 55,   // Keeper
                9 => 69,   // Gamer
                10 => 82,  // Addict
                11 => 98,  // Host
                12 => 112, // Media
                _ => -1
            };
        }

        private List<ChoiceEntry> BuildDoorSceneItemChoices(int sceneId)
        {
            int sourceDialogueId = GetDoorSceneItemChoiceDialogueId(sceneId);
            if (sourceDialogueId <= 0)
                return new List<ChoiceEntry>();

            return sceneId switch
            {
                7 => new List<ChoiceEntry>
                {
                    new ChoiceEntry(sourceDialogueId, 1, "Order", null, null),
                    new ChoiceEntry(sourceDialogueId, 2, "Do not Order!", null, null)
                },
                8 => new List<ChoiceEntry>
                {
                    new ChoiceEntry(sourceDialogueId, 1, "Pick", null, null),
                    new ChoiceEntry(sourceDialogueId, 2, "Do not Pick!", null, null)
                },
                9 => new List<ChoiceEntry>
                {
                    new ChoiceEntry(sourceDialogueId, 1, "Give it a try", null, null),
                    new ChoiceEntry(sourceDialogueId, 2, "Do not Try!", null, null)
                },
                10 => new List<ChoiceEntry>
                {
                    new ChoiceEntry(sourceDialogueId, 1, "Take a Sip", null, null),
                    new ChoiceEntry(sourceDialogueId, 2, "Do not Sip!", null, null)
                },
                11 => new List<ChoiceEntry>
                {
                    new ChoiceEntry(sourceDialogueId, 1, "Use the Ticket", null, null),
                    new ChoiceEntry(sourceDialogueId, 2, "Don't.", null, null)
                },
                12 => new List<ChoiceEntry>
                {
                    new ChoiceEntry(sourceDialogueId, 1, "...", null, null),
                    new ChoiceEntry(sourceDialogueId, 2, "Hi.", null, null)
                },
                _ => new List<ChoiceEntry>()
            };
        }

        private int GetDoorSceneEndingId(int sceneId)
        {
            return sceneId switch
            {
                7 => 1,
                8 => 2,
                9 => 3,
                10 => 4,
                11 => 5,
                12 => 6,
                _ => -1
            };
        }

        private List<ChoiceEntry> BuildDoorSceneEdgeChoices()
        {
            int sourceDialogueId = doorSceneChoiceDialogueId > 0 ? doorSceneChoiceDialogueId : -1;
            int endingId = GetDoorSceneEndingId(currentSceneId);
            List<ChoiceEntry> databaseChoices = sourceDialogueId > 0 ? LoadChoices(sourceDialogueId) : new List<ChoiceEntry>();

            ChoiceEntry stayChoice = databaseChoices
                .Where(choice => string.Equals(choice.ChoiceText?.Trim(), "Stay", StringComparison.OrdinalIgnoreCase))
                .OrderBy(choice => choice.ChoiceOrder)
                .FirstOrDefault()
                ?? new ChoiceEntry(sourceDialogueId, 1, "Stay", null, null);

            ChoiceEntry leaveChoice = databaseChoices
                .Where(choice => string.Equals(choice.ChoiceText?.Trim(), "Leave", StringComparison.OrdinalIgnoreCase))
                .OrderBy(choice => choice.ChoiceOrder)
                .FirstOrDefault();

            if (leaveChoice == null || !leaveChoice.TargetEndingID.HasValue)
                leaveChoice = new ChoiceEntry(sourceDialogueId, 2, "Leave", endingId > 0 ? endingId : (int?)null, null);

            return new List<ChoiceEntry>
            {
                new ChoiceEntry(sourceDialogueId, 1, "Stay", stayChoice.TargetEndingID, stayChoice.TargetSceneID),
                new ChoiceEntry(sourceDialogueId, 2, "Leave", leaveChoice.TargetEndingID, leaveChoice.TargetSceneID)
            };
        }

        private void HandleDoorSceneItemChoice(int sceneId, int itemChoiceSourceId, string normalizedChoiceText, int choiceOrder)
        {
            isChoiceActive = false;
            activeChoices.Clear();
            selectedChoiceIndex = 0;
            activeChoiceSourceDialogueId = null;
            activeChoiceLoop = false;
            activeChoiceStayNarrationId = null;
            doorSceneItemChoiceActive = false;
            doorSceneEdgeChoiceActive = false;
            doorSceneChoicePending = false;

            if (choiceOrder == 1)
            {
                if (currentDoorAfterItemDialogues.Count > 0)
                {
                    StartAutomaticDialogueSequence(currentDoorAfterItemDialogues, () =>
                    {
                        doorSceneItemLoopCount++;
                        showDoorItemPopup = false;
                    });
                    LogActionAnalytics("DoorSceneItemChoice", "Accepted", $"Scene{sceneId}");
                }
                else
                {
                    List<DialogueEntry> bindingDialogues = LoadDoorSceneItemBindingDialogues(sceneId);
                    if (bindingDialogues.Count > 0)
                    {
                        StartAutomaticDialogueSequence(bindingDialogues, () =>
                        {
                            doorSceneItemLoopCount++;
                            showDoorItemPopup = false;
                        });
                        LogActionAnalytics("DoorSceneItemChoice", "Accepted", $"Scene{sceneId}");
                    }
                    else
                    {
                        showDoorItemPopup = false;
                        LogActionAnalytics("DoorSceneItemChoice", "Accepted", $"Scene{sceneId}-NoDialogues");
                    }
                }
            }
            else
            {
                showDoorItemPopup = false;
                LogActionAnalytics("DoorSceneItemChoice", "Rejected", $"Scene{sceneId}");
            }
        }

        private List<DialogueEntry> LoadDoorSceneItemBindingDialogues(int sceneId)
        {
            (string Speaker, string Dialogue)[] lines = sceneId switch
            {
                // Scene 7: Chef
                7 => new[]
                {
                    ("Player", "Seems so good! Thank you, chef!"),
                    ("Player", "*Munch* *Munch*"),
                    ("System", "<You have now been bound to this place>"),
                    ("Player", "?!")
                },
                // Scene 8: Keeper
                8 => new[]
                {
                    ("Player", "All options look very comfortable! I'll go with this one~"),
                    ("Player", "Z"),
                    ("Player", "ZZ"),
                    ("Player", "ZZZ"),
                    ("System", "<You have now been bound to this place>"),
                    ("Player", "?!")
                },
                // Scene 9: Gamer
                9 => new[]
                {
                    ("Player", "How interesting~ This is actually quite fun!"),
                    ("Player", "Hmm~ Hm~"),
                    ("System", "<You have now been bound to this place>"),
                    ("Player", "?!")
                },
                // Scene 10: Addict
                10 => new[]
                {
                    ("Player", "How interesting~ This is actually quite fun!"),
                    ("Player", "Hmm~ Hm~"),
                    ("System", "<You have now been bound to this place>"),
                    ("Player", "?!")
                },
                // Scene 11: Host
                11 => new[]
                {
                    ("Player", "I guess it wouldn't hurt to try a ride or two..?"),
                    ("Player", "Oohh? Is that"),
                    ("System", "<You have now been bound to this place>"),
                    ("Player", "?!")
                },
                // Scene 12: Media
                12 => new[]
                {
                    ("Player", "Is this a prank---"),
                    ("System", "<Go Find a Job>"),
                    ("System", "<You have now been bound to this place>"),
                    ("Player", "?!")
                },
                _ => Array.Empty<(string Speaker, string Dialogue)>()
            };

            return lines
                .Select(line => new DialogueEntry(GetTransientDialogueId(), sceneId, line.Speaker, ApplyDynamicDialogueTokens(line.Dialogue), null, null))
                .ToList();
        }

        private void ConfirmChoiceSelection()
        {
            if (selectedChoiceIndex < 0 || selectedChoiceIndex >= activeChoices.Count)
                return;

            ChoiceEntry selectedChoice = activeChoices[selectedChoiceIndex];
            string normalizedChoiceText = selectedChoice.ChoiceText.Trim();
            QuickLogHistory(FormatChoiceLogText(normalizedChoiceText));

            // Door-scene item choices use their own loop and must not fall through into
            // the generic edge/corner choice handling.
            if (doorSceneItemChoiceActive && activeChoiceSourceDialogueId.HasValue)
            {
                int itemChoiceSourceId = activeChoiceSourceDialogueId.Value;
                HandleDoorSceneItemChoice(currentSceneId, itemChoiceSourceId, normalizedChoiceText, selectedChoice.ChoiceOrder);
                return;
            }

            if (doorSceneEdgeChoiceActive)
            {
                if (string.Equals(normalizedChoiceText, "Stay", StringComparison.OrdinalIgnoreCase))
                {
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    doorSceneChoicePending = false;
                    doorSceneEdgeChoiceActive = false;
                    LogActionAnalytics("ChoiceSelected", "Stay", "ReturnedToScene");
                    return;
                }

                if (string.Equals(normalizedChoiceText, "Leave", StringComparison.OrdinalIgnoreCase) && selectedChoice.TargetEndingID.HasValue)
                {
                    int endingId = selectedChoice.TargetEndingID.Value;
                    activeEnding = LoadEnding(endingId);
                    SaveUnlockedEnding(endingId);
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    doorSceneChoicePending = false;
                    doorSceneEdgeChoiceActive = false;
                    LogActionAnalytics("ChoiceSelected", "Leave", $"Ending {endingId}");
                    FinalizeCurrentSceneAnalytics("EndingTriggered");
                    StartEndingTransition();
                    return;
                }

                return;
            }

            // If this choice came from a DialogueID flow (OpenChoiceMenuFromDialogue)
            if (activeChoiceSourceDialogueId.HasValue)
            {
                int sourceId = activeChoiceSourceDialogueId.Value;

                if (sourceId == 281)
                {
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    activeChoiceLoop = false;
                    activeChoiceStayNarrationId = null;

                    // If the player selected the 'Continue' option
                    if (string.Equals(normalizedChoiceText, "Continue", StringComparison.OrdinalIgnoreCase))
                    {
                        LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, "Scene33Continue");
                        scene33Blackout = true;
                        StartNarrationSequence(LoadNarrationPagesByIds(52, 53, 54), () =>
                        {
                            StartDialogueSequence(LoadDialogueEntriesByIds(282), () =>
                            {
                                scene33NotePromptReady = true;
                            });
                        });
                        return;
                    }

                    // If the player selected a 'Walk' option (commonly the second choice), send them to scene 34
                    if (selectedChoice.ChoiceOrder == 2 || string.Equals(normalizedChoiceText, "Walk", StringComparison.OrdinalIgnoreCase))
                    {
                        LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, "Scene33Walk->34");
                        LoadSceneWithDialogues(34);
                        return;
                    }

                    // Fallback: go back to start
                    LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, "Scene33GoBack");
                    StartNarrationSequence(new List<string>
                    {
                        "Warning: going back will erase your progress and return you to the beginning."
                    }, () =>
                    {
                        LoadSceneWithDialogues(1);
                    });
                    return;
                }

                if (sourceId == 282)
                {
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    activeChoiceLoop = false;
                    activeChoiceStayNarrationId = null;

                    if (string.Equals(normalizedChoiceText, "Sail", StringComparison.OrdinalIgnoreCase) ||
                        selectedChoice.ChoiceOrder == 1)
                    {
                        scene39LoopCount = 0;
                        scene39IntroStarted = false;
                        scene39CenterEventTriggered = false;
                        scene33Blackout = false;
                        scene33NotePromptReady = false;
                        showScene33NoteImage = false;
                        LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, "Scene33Sail->39");
                        LoadSceneWithDialogues(39);
                        position = new Vector2(0, GetSceneGroundY(39));
                        return;
                    }

                    if (string.Equals(normalizedChoiceText, "Walk", StringComparison.OrdinalIgnoreCase) ||
                        selectedChoice.ChoiceOrder == 2)
                    {
                        scene34LoopCount = 0;
                        scene34PostLoopTargetSceneId = 35;
                        scene33Blackout = false;
                        scene33NotePromptReady = false;
                        showScene33NoteImage = false;
                        LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, "Scene33Walk->34->35");
                        LoadSceneWithDialogues(34);
                        position = new Vector2(0, GetSceneGroundY(34));
                        return;
                    }

                    LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, "Scene33ChoiceFallback");
                    LoadSceneWithDialogues(33);
                    return;
                }

                if (sourceId == 298)
                {
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    activeChoiceLoop = false;
                    activeChoiceStayNarrationId = null;
                    scene35TimedChoiceActive = false;
                    scene35ChoiceTimer = 0f;
                    LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, $"Scene35Option{selectedChoice.ChoiceOrder}");
                    StartScene35TimedOptionSequence(selectedChoice.ChoiceOrder);
                    return;
                }

                if (sourceId == Scene35PullChoiceSourceId)
                {
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    activeChoiceLoop = false;
                    activeChoiceStayNarrationId = null;
                    scene35TimedChoiceActive = false;
                    scene35ChoiceTimer = 0f;
                    LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, "Scene35PullSuccess");
                    StartScene35PullSuccessSequence();
                    return;
                }

                if (sourceId == Scene41ChoiceSourceId)
                {
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    activeChoiceLoop = false;
                    activeChoiceStayNarrationId = null;
                    scene41TimedChoiceActive = false;
                    scene41ChoiceTimer = 0f;
                    LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, $"Scene41Option{selectedChoice.ChoiceOrder}");
                    StartScene41ChoiceSequence(selectedChoice.ChoiceOrder);
                    return;
                }

                if (IsScene30ChoiceSource(sourceId))
                {
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    activeChoiceLoop = false;
                    activeChoiceStayNarrationId = null;
                    LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, $"Scene30Stage{scene30PendingChoiceStage}");
                    ContinueScene30AfterChoice(sourceId, selectedChoice.ChoiceOrder);
                    return;
                }

                // If player chose Stay and the loop flag is set, show the stay narration then reopen choices
                if (string.Equals(normalizedChoiceText, "Stay", StringComparison.OrdinalIgnoreCase) && activeChoiceLoop)
                {
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;

                    int? narrId = activeChoiceStayNarrationId;
                    if (narrId.HasValue)
                    {
                        List<string> pages = LoadNarrationPagesByIds(narrId.Value);
                        StartNarrationSequence(pages, () =>
                        {
                            OpenChoiceMenuFromDialogue(sourceId, activeChoiceLoop, activeChoiceStayNarrationId);
                        });
                        return;
                    }
                    else
                    {
                        // no narration id provided -- just reopen immediately
                        OpenChoiceMenuFromDialogue(sourceId, activeChoiceLoop, activeChoiceStayNarrationId);
                        return;
                    }
                }

                if (HandleHiddenWallDialogueChoice(sourceId, normalizedChoiceText))
                    return;

                // If the choice has a target scene, go there
                if (selectedChoice.TargetSceneID.HasValue)
                {
                    int targetSceneId = selectedChoice.TargetSceneID.Value;
                    if (selectedChoice.DialogueID == 191 && targetSceneId == 21)
                        companionRoute = CompanionRoute.None;
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    activeChoiceLoop = false;
                    activeChoiceStayNarrationId = null;
                    LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, $"Scene {targetSceneId}");
                    // Scene21 branch destinations 22/23 should start without inherited static NPCs.
                    if (targetSceneId == 22 || targetSceneId == 23)
                        suppressStaticNpcsOnNextTransition = true;
                    // If this choice was opened from the scene21 companion dialogue and leads to 31,
                    // preserve the right companion route so scenes 25-28 can show the owl follower path.
                    if (activeChoiceSourceDialogueId == 191 || activeChoiceSourceDialogueId == null)
                    {
                        // activeChoiceSourceDialogueId was already cleared above; check the selectedChoice source instead
                    }
                    // The current left branch now routes to scene23 directly, so only the right branch
                    // needs to keep the companion follower route.
                    if (targetSceneId == 31)
                        companionRoute = CompanionRoute.Right;
                    LoadSceneWithDialogues(targetSceneId);
                    return;
                }

                // If no special handling matched, clear source and continue normal flow
                activeChoiceSourceDialogueId = null;
                activeChoiceLoop = false;
                activeChoiceStayNarrationId = null;
            }

            if (string.Equals(normalizedChoiceText, "Stay", StringComparison.OrdinalIgnoreCase))
            {
                isChoiceActive = false;
                activeChoices.Clear();
                selectedChoiceIndex = 0;
                doorSceneChoicePending = false;
                doorSceneEdgeChoiceActive = false;
                LogActionAnalytics("ChoiceSelected", "Stay", "ReturnedToScene");
                return;
            }

            if (selectedChoice.TargetSceneID.HasValue)
            {
                int targetSceneId = selectedChoice.TargetSceneID.Value;
                if (selectedChoice.DialogueID == 191 && targetSceneId == 21)
                    companionRoute = CompanionRoute.None;
                isChoiceActive = false;
                activeChoices.Clear();
                selectedChoiceIndex = 0;
                LogActionAnalytics("ChoiceSelected", selectedChoice.ChoiceText, $"Scene {targetSceneId}");
                LoadSceneWithDialogues(targetSceneId);
                return;
            }

            if (selectedChoice.TargetEndingID.HasValue)
            {
                int endingId = selectedChoice.TargetEndingID.Value;
                activeEnding = LoadEnding(endingId);
                SaveUnlockedEnding(endingId);
                isChoiceActive = false;
                activeChoices.Clear();
                selectedChoiceIndex = 0;
                doorSceneEdgeChoiceActive = false;
                LogActionAnalytics("ChoiceSelected", "Leave", $"Ending {endingId}");
                FinalizeCurrentSceneAnalytics("EndingTriggered");
                StartEndingTransition();
                return;
            }
        }

        private bool HandleHiddenWallDialogueChoice(int sourceId, string normalizedChoiceText)
        {
            bool matches(string expected) => normalizedChoiceText.Equals(expected, StringComparison.OrdinalIgnoreCase);

            switch (sourceId)
            {
                case 118:
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    if (matches("Create a makeshift ladder"))
                    {
                        hiddenWallCarryPosition = position;
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene13->14");
                        LoadSceneWithDialogues(HiddenWallRetrySceneId);
                    }
                    else
                    {
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene13->5");
                        StartHiddenWallDialogueSequence(
                            BuildHiddenWallDialogueEntries(HiddenWallSceneId, (GetHiddenWallPlayerSpeaker(), "Hm.. What a waste of time, I should check out the other path.")),
                            () => LoadSceneWithDialogues(5));
                    }
                    return true;

                case 122:
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    if (matches("Attempt to Climb"))
                    {
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene14Fail");
                        StartNarrationSequence(
                            LoadNarrationPagesByIds(1),
                            () => StartHiddenWallDialogueSequence(
                                BuildHiddenWallDialogueEntries(HiddenWallRetrySceneId,
                                    (GetHiddenWallPlayerSpeaker(), "..."),
                                    (GetHiddenWallPlayerSpeaker(), "Gosh, this is so embarrassing."),
                                    (GetHiddenWallPlayerSpeaker(), "You didn't have to narrate it right in front of my face, you know."),
                                    (GetHiddenWallPlayerSpeaker(), "You know I can hear you, you know?"),
                                    ("System", "You talked in the air."),
                                    (GetHiddenWallPlayerSpeaker(), "... It would be best if I ignored you, you creepy weird voice. And why are you following me? It's not like this is a gacha reaction.")),
                                () => OpenChoiceMenuFromDialogue(130)));
                    }
                    else
                    {
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene14->5");
                        StartHiddenWallDialogueSequence(
                            BuildHiddenWallDialogueEntries(HiddenWallRetrySceneId,
                                (GetHiddenWallPlayerSpeaker(), "..."),
                                (GetHiddenWallPlayerSpeaker(), "Since it will be dangerous, it wouldn't hurt if I checked out the other path, right? Better safe than sorry...")),
                            () => LoadSceneWithDialogues(5));
                    }
                    return true;

                case 130:
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    if (matches("Attempt to Climb (again lol)"))
                    {
                        hiddenWallCarryPosition = position;
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene14->15");
                        LoadSceneWithDialogues(HiddenWallFinalSceneId);
                    }
                    else
                    {
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene14->5");
                        StartHiddenWallDialogueSequence(
                            BuildHiddenWallDialogueEntries(HiddenWallRetrySceneId, (GetHiddenWallPlayerSpeaker(), "Soo~ I should leave for now... I want to try exploring the other path.")),
                            () => LoadSceneWithDialogues(5));
                    }
                    return true;

                case 133:
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    if (matches("Climb"))
                    {
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene15SuccessDirect");
                        StartHiddenWallSuccessSequence(includePrideLine: false);
                    }
                    else
                    {
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene15RetryPrompt");
                        StartHiddenWallDialogueSequence(
                            BuildHiddenWallDialogueEntries(HiddenWallFinalSceneId,
                                ("System", "Are you really sure??"),
                                ("System", "You don't believe in \"3rd time's a charm\"?"),
                                ("System", "What a unique person-- Are you sure you're leaving??")),
                            () => OpenChoiceMenuFromDialogue(136));
                    }
                    return true;

                case 136:
                    isChoiceActive = false;
                    activeChoices.Clear();
                    selectedChoiceIndex = 0;
                    activeChoiceSourceDialogueId = null;
                    if (matches("CLIMB (pls?)"))
                    {
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene15SuccessAfterRetry");
                        StartHiddenWallSuccessSequence(includePrideLine: true);
                    }
                    else
                    {
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene15->5");
                        StartHiddenWallDialogueSequence(
                            BuildHiddenWallDialogueEntries(HiddenWallFinalSceneId,
                                (GetHiddenWallPlayerSpeaker(), "Wah! Get away! I just want to check the other path, AND it seems to be an easier path to walk on rather than suffering here!")),
                            () => LoadSceneWithDialogues(5));
                    }
                    return true;

                case 139:
                    if (matches("Stay"))
                    {
                        isChoiceActive = false;
                        activeChoices.Clear();
                        selectedChoiceIndex = 0;
                        StartNarrationSequence(
                            LoadNarrationPagesByIds(6),
                            () => OpenChoiceMenuFromDialogue(139, loopOnStay: true, stayNarrationId: 6));
                        return true;
                    }

                    if (matches("Climb Down"))
                    {
                        isChoiceActive = false;
                        activeChoices.Clear();
                        selectedChoiceIndex = 0;
                        activeChoiceSourceDialogueId = null;
                        activeChoiceLoop = false;
                        activeChoiceStayNarrationId = null;
                        hiddenWallCarryPosition = null;
                        LogActionAnalytics("WallChoice", normalizedChoiceText, "Scene15->16");
                        LoadSceneWithDialogues(16);
                        return true;
                    }

                    return false;
            }

            return false;
        }

        private void StartHiddenWallSuccessSequence(bool includePrideLine)
        {
            Action openExitChoice = () => OpenChoiceMenuFromDialogue(139, loopOnStay: true, stayNarrationId: 6);

            if (includePrideLine)
            {
                StartHiddenWallDialogueSequence(
                    BuildHiddenWallDialogueEntries(HiddenWallFinalSceneId, ("System", "Whoever you are, you made the right decision. I'm so proud of you!")),
                    () => StartNarrationSequence(LoadNarrationPagesByIds(3, 4, 5), openExitChoice));
            }
            else
            {
                StartNarrationSequence(LoadNarrationPagesByIds(3, 4, 5), openExitChoice);
            }
        }

        private void StartEndingTransition()
        {
            endingTransitionActive = true;
            endingTransitionTimer = 0f;
            MediaPlayer.Stop();
            chibiFAnimation?.Reset();
        }

        private void UpdateEndingTransition(GameTime gameTime)
        {
            endingTransitionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            chibiFAnimation?.Update(gameTime);

            if (endingTransitionTimer >= EndingTransitionDuration)
            {
                endingTransitionActive = false;
                endingTransitionTimer = EndingTransitionDuration;
            }
        }

        private void DrawEndingTransition(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);

            float progress = MathHelper.Clamp(endingTransitionTimer / EndingTransitionDuration, 0f, 1f);
            int bandCount = 7 + (int)(progress * 8f);

            for (int i = 0; i < bandCount; i++)
            {
                int bandHeight = endingGlitchRandom.Next(10, 42);
                int y = endingGlitchRandom.Next(0, Math.Max(1, screenHeight - bandHeight));
                int offset = endingGlitchRandom.Next(-35, 36);
                Color bandColor = i % 2 == 0 ? Color.White * (0.03f + (progress * 0.09f)) : Color.Red * (0.02f + (progress * 0.05f));

                spriteBatch.Draw(ground, new Rectangle(offset, y, screenWidth, bandHeight), bandColor);
            }

            if (progress > 0.45f)
            {
                Texture2D chibiFrame = chibiFAnimation?.CurrentFrame ?? chibiFTextures[0];
                float scale = Math.Min(7f, screenWidth / (float)chibiFrame.Width / 5f);
                Vector2 origin = new Vector2(chibiFrame.Width / 2f, chibiFrame.Height);
                float jitterX = endingGlitchRandom.Next(-8, 9) * (1f - progress);
                float jitterY = endingGlitchRandom.Next(-4, 5) * (1f - progress);
                Vector2 drawPosition = new Vector2((screenWidth / 2f) + jitterX, screenHeight - 30f + jitterY);
                spriteBatch.Draw(chibiFrame, drawPosition, null, Color.White * progress, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        private void UpdateDoorNpcAnimation(GameTime gameTime)
        {
            Animation? npcAnimation = GetDoorSceneNpcAnimation(currentSceneId);
            npcAnimation?.Update(gameTime);
        }

        private void DrawDoorSceneNpc(SpriteBatch spriteBatch)
        {
            if (!IsDoorScene(currentSceneId))
                return;

            Animation? npcAnimation = GetDoorSceneNpcAnimation(currentSceneId);
            Texture2D? npcFrame = npcAnimation?.CurrentFrame;
            if (npcFrame == null)
                return;

            float npcX = GetDoorSceneNpcX();
            // Adjust NPC ground position for scene 13 (move it up 5 pixels)
            float groundAdjustment = (currentSceneId == 13) ? scene13GroundAdjustment : 0f;
            Vector2 npcPosition = new Vector2(npcX, GetSceneGroundY(currentSceneId) - groundAdjustment);
            Vector2 npcOrigin = new Vector2(npcFrame.Width / 2f, npcFrame.Height);
            spriteBatch.Draw(npcFrame, npcPosition, null, Color.White, 0f, npcOrigin, spriteScale, SpriteEffects.None, 0f);
        }

        private void PrepareScene17Dialogues()
        {
            scene17IntroDialogues = currentSceneDialogues
                .Where(entry => entry.DialogueID >= 146 && entry.DialogueID <= 148)
                .OrderBy(entry => entry.DialogueID)
                .ToList();

            scene17CenterDialogues = currentSceneDialogues
                .Where(entry => entry.DialogueID >= 149 && entry.DialogueID <= 166)
                .OrderBy(entry => entry.DialogueID)
                .ToList();

            scene17AfterNamingDialogues = currentSceneDialogues
                .Where(entry => entry.DialogueID >= 167)
                .OrderBy(entry => entry.DialogueID)
                .ToList();

            scene17NameInputPending = scene17CenterDialogues.Any(entry => entry.DialogueID == 166);
        }

        private void TryTriggerScene16CenterDialogue()
        {
            if (currentSceneId != 16 || scene16LoopCount == 0)
                return;

            if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isNarrationActive || isChoiceActive)
                return;

            if (!scene16CenterDialogueTriggered && position.X >= (_graphics.PreferredBackBufferWidth * 0.5f))
            {
                scene16CenterDialogueTriggered = true;

                if (scene16CenterDialogues.Count > 0)
                    dialogueManager.StartDialogue(scene16CenterDialogues, dialogueBoxTexture, dialogueBoxTexture2, true, true);
            }
        }

        private void PrepareStaticPartySceneDialogues(int sceneId, List<DialogueEntry> sourceDialogues, out List<DialogueEntry> introDialogues, out List<DialogueEntry> remainingDialogues)
        {
            var ordered = sourceDialogues
                .OrderBy(entry => entry.DialogueID)
                .ToList();

            if (ordered.Count == 0)
            {
                introDialogues = new List<DialogueEntry>();
                remainingDialogues = new List<DialogueEntry>();
                return;
            }

            introDialogues = new List<DialogueEntry> { ordered[0] };
            remainingDialogues = ordered.Skip(1).ToList();
        }

        private void TryTriggerStaticPartySceneRemainder()
        {
            if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isNarrationActive || isChoiceActive)
                return;

            if (currentSceneId == 20 &&
                !scene20RemainingStarted &&
                !scene20CenterDialogueTriggered &&
                currentScene.IntroDialoguePlayed &&
                dialogueManager.IsFinished &&
                position.X >= (_graphics.PreferredBackBufferWidth * 0.5f))
            {
                scene20CenterDialogueTriggered = true;
                scene20RemainingStarted = true;
                if (scene20RemainingDialogues.Count > 0)
                    dialogueManager.StartDialogue(scene20RemainingDialogues, dialogueBoxTexture, dialogueBoxTexture2, true, true);
            }
            else if (currentSceneId == 23 && !scene23RemainingStarted && currentScene.IntroDialoguePlayed && dialogueManager.IsFinished)
            {
                if (!scene23BridgeImageShown)
                {
                    scene23BridgeImageShown = true;
                    showScene23BridgeImage = true;
                    return;
                }

                scene23RemainingStarted = true;
                if (scene23RemainingDialogues.Count > 0)
                    dialogueManager.StartDialogue(scene23RemainingDialogues, dialogueBoxTexture, dialogueBoxTexture2, false, true);
            }
            else if (currentSceneId == 29 &&
                !scene29RemainingStarted &&
                !scene29CenterDialogueTriggered &&
                currentScene.IntroDialoguePlayed &&
                dialogueManager.IsFinished &&
                position.X >= (_graphics.PreferredBackBufferWidth * 0.5f))
            {
                scene29CenterDialogueTriggered = true;
                if (scene29IntroDialogues.Count > 0)
                    StartDialogueSequence(scene29IntroDialogues, StartScene29RemainingDialogueBlock, requireInteractBetween: true, startImmediately: false);
                else
                    StartScene29RemainingDialogueBlock();
            }
        }

        private bool TryAdvanceStaticPartySceneAfterDialogues()
        {
            if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || !dialogueManager.IsFinished)
                return false;

            if (currentSceneId == 20 && scene20RemainingStarted)
            {
                LoadSceneWithDialogues(21);
                return true;
            }

            if (currentSceneId == 29 && scene29RemainingStarted && !scene29GlitchTransitionActive)
            {
                scene29GlitchTransitionActive = true;
                scene29GlitchTransitionTimer = 0f;
                isMoving = false;
                currentAnimation = idleAnimation;
                return true;
            }

            return false;
        }

        private void StartScene29RemainingDialogueBlock()
        {
            scene29RemainingStarted = true;
            if (scene29RemainingDialogues.Count > 0)
                StartDialogueSequence(scene29RemainingDialogues, null, requireInteractBetween: false, startImmediately: true);
        }

        private void TryTriggerScene22CenterDialogue()
        {
            if (currentSceneId != 22 || scene22CenterDialogueTriggered)
                return;

            if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isNarrationActive || isChoiceActive)
                return;

            // Trigger when player reaches center/right side of the screen.
            float leftBound = _graphics.PreferredBackBufferWidth * 0.55f;
            if (position.X < leftBound)
                return;

            scene22CenterDialogueTriggered = true;
            if (scene22CenterDialogues.Count > 0)
                StartAutomaticDialogueSequence(scene22CenterDialogues, BeginScene22OwlExitFade);
            else
                BeginScene22OwlExitFade();
        }

        private void BeginScene22OwlExitFade()
        {
            scene22OwlFadeActive = true;
            scene22OwlFadeTimer = 0f;
            isMoving = false;
            currentAnimation = idleAnimation;
        }

        private void UpdateScene22OwlFade(GameTime gameTime)
        {
            scene22OwlFadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (scene22OwlFadeTimer < Scene22OwlFadeDuration)
                return;

            scene22OwlFadeActive = false;
            scene22OwlGone = true;
            scene22OwlFadeTimer = Scene22OwlFadeDuration;

            int nextSceneId = companionRoute == CompanionRoute.Left ? 25 : 23;
            LoadSceneWithDialogues(nextSceneId);
            position = new Vector2(0, GetSceneGroundY(nextSceneId));
        }

        private void TryTriggerScene24CenterDialogue()
        {
            if (currentSceneId != 24 || scene24CenterDialogueTriggered)
                return;

            if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isNarrationActive || isChoiceActive)
                return;

            if (position.X < (_graphics.PreferredBackBufferWidth * 0.5f))
                return;

            scene24CenterDialogueTriggered = true;
            if (scene24BeforeOwlExitDialogues.Count > 0)
                StartDialogueSequence(scene24BeforeOwlExitDialogues, BeginScene24OwlExitFade, requireInteractBetween: true, startImmediately: true);
            else if (scene24AfterOwlExitDialogues.Count > 0)
                StartDialogueSequence(scene24AfterOwlExitDialogues, null, requireInteractBetween: true, startImmediately: true);
        }

        private void BeginScene24OwlExitFade()
        {
            scene24OwlFadeActive = true;
            scene24OwlFadeTimer = 0f;
            isMoving = false;
            currentAnimation = idleAnimation;
        }

        private void UpdateScene24OwlFade(GameTime gameTime)
        {
            scene24OwlFadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (scene24OwlFadeTimer < Scene24OwlFadeDuration)
                return;

            scene24OwlFadeActive = false;
            scene24OwlGone = true;
            scene24OwlFadeTimer = Scene24OwlFadeDuration;

            if (scene24AfterOwlExitDialogues.Count > 0)
                StartDialogueSequence(scene24AfterOwlExitDialogues, null, requireInteractBetween: true, startImmediately: true);
        }

        private void TryTriggerScene31CenterDialogue()
        {
            if (currentSceneId != 31 || scene31CenterDialogueTriggered)
                return;

            if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isNarrationActive || isChoiceActive)
                return;

            // Trigger when player reaches roughly the center X of the screen
            if (position.X < (_graphics.PreferredBackBufferWidth * 0.45f) || position.X > (_graphics.PreferredBackBufferWidth * 0.55f))
                return;

            scene31CenterDialogueTriggered = true;
            if (scene31BeforeFriendExitDialogues.Count > 0)
                StartScene31DialogueBlock(scene31BeforeFriendExitDialogues, BeginScene31FriendExitFade, startImmediately: false);
            else if (scene31AfterFriendExitDialogues.Count > 0)
                StartScene31DialogueBlock(scene31AfterFriendExitDialogues, null, startImmediately: false);
        }

        private void StartScene31DialogueBlock(IEnumerable<DialogueEntry> entries, Action? onComplete, bool startImmediately)
        {
            List<DialogueEntry> dialogueEntries = entries.ToList();
            if (dialogueEntries.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            scene31DialogueCompletionAction = onComplete;
            scene31DialogueAwaitingCompletion = onComplete != null;
            dialogueManager.StartDialogue(dialogueEntries, dialogueBoxTexture, dialogueBoxTexture2, true, startImmediately);
        }

        private void BeginScene31FriendExitFade()
        {
            scene31FriendFadeActive = true;
            scene31FriendFadeTimer = 0f;
            isMoving = false;
            currentAnimation = idleAnimation;
        }

        private void UpdateScene31FriendFade(GameTime gameTime)
        {
            scene31FriendFadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (scene31FriendFadeTimer < Scene31FriendFadeDuration)
                return;

            scene31FriendFadeActive = false;
            scene31FriendGone = true;
            scene31FriendFadeTimer = Scene31FriendFadeDuration;

            if (scene31AfterFriendExitDialogues.Count > 0)
                StartScene31DialogueBlock(scene31AfterFriendExitDialogues, null, startImmediately: true);
        }

        private void UpdateScene32Sequence()
        {
            if (currentSceneId != 32 || isNarrationActive || isChoiceActive || activeEnding != null || endingTransitionActive)
                return;

            // Trigger the scene32 sequence when the player reaches the center of the screen.
            // This allows the player to walk into the center to begin the scripted sequence.
            float centerX = _graphics.PreferredBackBufferWidth * 0.5f;
            if (!scene32Started)
            {
                if (position.X < centerX)
                    return; // wait until player reaches center

                scene32Started = true;
                scene32OwlVisualState = 0;
                scene32SequenceStep = 1;
                // While the sequence plays, the player is frozen. The final behaviour after all
                // dialogues completes will allow movement again and let the player exit to the right.
                StartScene32DialogueBlock(
                    ("Player", $"({savedOwlName} seems to be getting weak day by day... The journey must have taken a great toll on its small body. It's been a decade now and we still haven't reached a clear end.. The journey's taken longer than I expected.)"),
                    ("Player", $"(I wonder what {savedOwlName} is trying to find... to find their end? Or to find a creature like them? A partner maybe..? Am I actually holding them back?)"),
                    ("Player", $"(A bird must be freely flying in the skies! Not to be stuck here with me... i'm stupid for thinking it just now... But I tried several times, letting it free, but they seem to always come back to me...)"));
                return;
            }

            if (scene32SequenceStep == 2)
            {
                scene32SequenceStep = 3;
                scene32OwlVisualState = 1;
                StartNarrationSequence(new List<string>
                {
                    "But then suddenly, the poor creature suddenly wavered. Its wingbeats falter, much slower than before.",
                    "Slowly, it glides. Thankfully, you managed to catch them before they fell helplessly to the ground, with a heart beating wildly.",
                    "You knew this would eventually happen."
                }, () =>
                {
                    scene32SequenceStep = 4;
                    StartScene32DialogueBlock(
                        ("Player", $"{savedOwlName}...? D-did you get tired of flying? You didn't have to scare me like that..."));
                });
                return;
            }

            if (scene32SequenceStep == 5)
            {
                scene32SequenceStep = 6;
                StartNarrationSequence(new List<string>
                {
                    $"{savedOwlName} turns its head, eyes half-lidded yet still sharp enough to hold recognition. Its feathers puff, not in pride but in exhaustion."
                }, () =>
                {
                    scene32SequenceStep = 7;
                    scene32OwlVisualState = 2;
                    StartScene32DialogueBlock(
                        ("Player", $"Wait- your wings... you're trembling so much.. Oh my dear {savedOwlName}, what's happening to you..."));
                });
                return;
            }

            if (scene32SequenceStep == 8)
            {
                scene32SequenceStep = 9;
                StartNarrationSequence(new List<string>
                {
                    $"{savedOwlName} shifts closer, leaning against your leg. Its chest rises and falls in a shallow rhythm, each breath weaker than the last.",
                    "The words catch in your throat. It shouldn't have come to this..."
                }, () =>
                {
                    scene32SequenceStep = 10;
                    StartScene32DialogueBlock(
                        ("Player", "Hey- hey... Please no, don't- don't tell me you're-"),
                        ("Player", "Stay with me! Ple..please! Just a little longer....!"));
                });
                return;
            }

            if (scene32SequenceStep == 11)
            {
                scene32SequenceStep = 12;
                StartNarrationSequence(new List<string>
                {
                    $"{savedOwlName} tucks its head under its wing, pressing against your side. No cry, no collapse -- just quiet surrender."
                }, () =>
                {
                    scene32SequenceStep = 13;
                    StartScene32DialogueBlock(
                        ("Player", "...You're not leaving me, are you? Not now."));
                });
                return;
            }

            if (scene32SequenceStep == 14)
            {
                scene32SequenceStep = 15;
                StartNarrationSequence(new List<string>
                {
                    "Your voice breaks, trembling with desperation.",
                    $"The gaze of this poor creature lingers one final time, calm and steady, before its breathing slows.",
                    "...",
                    "Silence.",
                    "Everything went silent.",
                    "The silence grows heavy. You bowed your head, clutching the still body close."
                }, () =>
                {
                    scene32SequenceStep = 16;
                    StartScene32DialogueBlock(
                        ("Player", $"I can't believe this is how you reach your ���... Fly, fly away, our precious {savedOwlName}... See the world beyond this void on our behalf... To the heavens may you fly..."),
                        ("Player", "I will miss you... Thank you for being a part of my journey..."));
                });
                return;
            }

            if (scene32SequenceStep == 17)
            {
                scene32SequenceStep = 18;
                if (playerAge <= 25)
                {
                    StartScene32DialogueBlock(
                        ("Player", $"You were the best guide I could have asked for. I'll continue my journey {savedOwlName}... I'll keep walking... It's hard.. It's painful but I must continue for your sake too..."));
                }
                else if (playerAge <= 50)
                {
                    StartScene32DialogueBlock(
                        ("Player", $"Hey {savedOwlName}, remember what Pal said..?"),
                        ("Player", "\"Should the day ever come that we are not together, you will continue to shine like gold in my memories.\""),
                        ("Player", "You carried me this far, now I'll carry your memory, our memories, and finish what we have started.."));
                }
                else
                {
                    StartNarrationSequence(new List<string>
                    {
                        $"With {savedOwlName} on your side, you began to realize the weight of ten years together presses down, not as grief alone but as a reminder of how far you two have come.",
                        "You sit quietly, staring at the still feathers, and for the first time in decades, you do not feel the urge to rise and continue walking. The road ahead stretches endlessly, but your heart no longer yearns for it.",
                        "\"Gosh, I'm so tired.. It wouldn't hurt if I rest here, right? I've wandered far already and seen enough.. Maybe it's time I stop chasing a perfect end... and actually start living.\"",
                        "\"How lucky are the youngsters... still have the time and energy to continue.. Only maybe if I started earlier, we could have saved ourselves...\"",
                        "Now alone in this unfamiliar land, you only imagine building a small home and maybe guiding young travelers who will stumble upon this place."
                    }, () =>
                    {
                        activeEnding = LoadEnding(8);
                        SaveUnlockedEnding(8);
                        StartEndingTransition();
                    });
                    return;
                }
                return;
            }

            if (scene32SequenceStep == 19)
            {
                scene32SequenceStep = 20;
                // mark dialogues completed and allow player movement
                scene32DialoguesCompleted = true;
                // ensure player is positioned slightly left of right edge so they can walk out
                position = new Vector2(_graphics.PreferredBackBufferWidth * 0.8f, GetSceneGroundY(32));
                // Immediately transition to Scene 23 to ensure the intended flow (Scene32 -> Scene23 -> Scene33)
                LoadSceneWithDialogues(23);
                // place player in the new scene consistently near the left/start area
                position = new Vector2(100, GetSceneGroundY(23));
                return;
            }
        }

        private void StartScene32DialogueBlock(params (string Speaker, string Dialogue)[] lines)
        {
            List<DialogueEntry> entries = new();
            foreach (var line in lines)
            {
                entries.Add(new DialogueEntry(GetTransientDialogueId(), 32, line.Speaker, line.Dialogue, null, null));
            }

            scene32DialogueCompletionAction = ContinueScene32Sequence;
            scene32DialogueAwaitingCompletion = true;
            dialogueManager.StartDialogue(entries, dialogueBoxTexture, dialogueBoxTexture2, true, true);
        }

        private void ContinueScene32Sequence()
        {
            scene32SequenceStep++;
        }

        private bool UpdateScene33Sequence(KeyboardState previousKeyState)
        {
            if (currentSceneId != 33)
                return false;

            float centerX = _graphics.PreferredBackBufferWidth * 0.5f;

            // While the scripted sequence is actively playing (dialogue, narration, or choice),
            // lock the player to the center so the cinematic plays correctly.
            if (scene33SequenceStarted)
            {
                if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isNarrationActive || isChoiceActive)
                {
                    position.X = centerX;
                    position.Y = GetSceneGroundY(33);
                    isMoving = false;
                    currentAnimation = idleAnimation;
                    return true;
                }

                // Sequence has finished presenting. Allow player movement so they can walk
                // into the black area on the right. Do not force position here.
            }

            if (!scene33SequenceStarted && position.X >= centerX)
            {
                scene33SequenceStarted = true;
                position.X = centerX;
                position.Y = GetSceneGroundY(33);
                isMoving = false;
                currentAnimation = idleAnimation;
                StartNarrationSequence(LoadNarrationPagesByIds(45, 46, 47, 48, 49, 50, 51), () =>
                {
                    OpenChoiceMenuFromDialogue(281);
                });
                return true;
            }

            if (scene33NotePromptReady)
            {
                if (keyState.IsKeyDown(Keys.F) && previousKeyState.IsKeyUp(Keys.F))
                {
                    showScene33NoteImage = true;
                    sceneInteractCount++;
                    LogActionAnalytics("NoteOpened", "Scene33Note", "OverlayShown");
                }

                // Allow the player to still interact with the note prompt, but do not block movement
                // so the player can walk into the black area while the prompt is available.
            }

            // Returning true prevents the main Update from processing movement. Only return true
            // when the scene is actively presenting content that must freeze the player.
            return (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isNarrationActive || isChoiceActive) && scene33SequenceStarted;
        }

        private bool UpdateScene34Sequence()
        {
            if (currentSceneId != 34)
                return false;

            if (scene34LoopCount >= 3 && !scene34NarrationStarted &&
                !dialogueManager.IsActive && !dialogueManager.IsWaitingForInteract &&
                !isNarrationActive && !isChoiceActive)
            {
                scene34NarrationStarted = true;
                position.X = _graphics.PreferredBackBufferWidth * 0.5f;
                isMoving = false;
                currentAnimation = idleAnimation;
                int targetSceneId = scene34PostLoopTargetSceneId;
                StartNarrationSequence(LoadNarrationPagesByIds(55, 56, 57), () =>
                {
                    scene34PostLoopTargetSceneId = 35;
                    LoadSceneWithDialogues(targetSceneId);
                });
                return true;
            }

            return false;
        }

        private void StartDialogueSequence(IEnumerable<DialogueEntry> entries, Action? onComplete)
        {
            StartDialogueSequence(entries, onComplete, requireInteractBetween: true, startImmediately: true);
        }

        private void StartAutomaticDialogueSequence(IEnumerable<DialogueEntry> entries, Action? onComplete)
        {
            StartDialogueSequence(entries, onComplete, requireInteractBetween: false, startImmediately: true);
        }

        private void StartDialogueSequence(IEnumerable<DialogueEntry> entries, Action? onComplete, bool requireInteractBetween, bool startImmediately)
        {
            List<DialogueEntry> dialogueEntries = entries.ToList();
            if (dialogueEntries.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            queuedDialogueCompletionAction = onComplete;
            queuedDialogueAwaitingCompletion = onComplete != null;
            dialogueManager.StartDialogue(dialogueEntries, dialogueBoxTexture, dialogueBoxTexture2, requireInteractBetween, startImmediately);
        }

        private List<DialogueEntry> LoadDialogueEntriesByIds(params int[] dialogueIds)
        {
            List<DialogueEntry> entries = new();
            if (dialogueIds.Length == 0)
                return entries;

            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                string placeholders = string.Join(",", dialogueIds.Select((_, index) => $"@id{index}"));
                using var cmd = new SqliteCommand(
                    $"SELECT DialogueID, SceneID, Speaker, Dialogue, MinAge, MaxAge FROM tblDialogues WHERE DialogueID IN ({placeholders}) ORDER BY DialogueID;",
                    conn);

                for (int i = 0; i < dialogueIds.Length; i++)
                    cmd.Parameters.AddWithValue($"@id{i}", dialogueIds[i]);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int sceneId = reader.GetInt32(1);
                    string speaker = reader.IsDBNull(2) ? "Player" : reader.GetString(2);
                    string dialogue = ApplyDynamicDialogueTokens(reader.IsDBNull(3) ? string.Empty : reader.GetString(3));
                    int? minAge = reader.IsDBNull(4) ? null : reader.GetInt32(4);
                    int? maxAge = reader.IsDBNull(5) ? null : reader.GetInt32(5);
                    entries.Add(new DialogueEntry(id, sceneId, speaker, dialogue, minAge, maxAge));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dialogue lookup failed: {ex.Message}");
            }

            return entries;
        }

        private DialogueEntry BuildTransientPlayerDialogue(string text)
        {
            return new DialogueEntry(GetTransientDialogueId(), currentSceneId, "Player", ApplyDynamicDialogueTokens(text), null, null);
        }

        private List<DialogueEntry> BuildTransientSceneDialogues(int sceneId, params string[] lines)
        {
            return lines
                .Select(line => new DialogueEntry(GetTransientDialogueId(), sceneId, "Player", ApplyDynamicDialogueTokens(line), null, null))
                .ToList();
        }

        private List<DialogueEntry> BuildTransientSceneDialogues(int sceneId, params (string Speaker, string Dialogue)[] lines)
        {
            return lines
                .Select(line => new DialogueEntry(GetTransientDialogueId(), sceneId, line.Speaker, ApplyDynamicDialogueTokens(line.Dialogue), null, null))
                .ToList();
        }

        private List<string> BuildNarrationPages(params string[] pages)
        {
            return pages
                .Select(page => string.IsNullOrWhiteSpace(page) ? string.Empty : ApplyDynamicDialogueTokens(page))
                .ToList();
        }

        private bool IsScene30ChoiceSource(int? sourceId)
        {
            return sourceId == Scene30IgnoreChoiceOneSourceId ||
                sourceId == Scene30IgnoreChoiceTwoSourceId ||
                sourceId == Scene30DoNotIgnoreChoiceSourceId ||
                sourceId == Scene30ListenChoiceSourceId ||
                sourceId == Scene30AgeChoiceSourceId;
        }

        private bool UpdateScene29GlitchTransition(GameTime gameTime)
        {
            if (!scene29GlitchTransitionActive)
                return false;

            scene29GlitchTransitionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            float transitionProgress = MathHelper.Clamp(scene29GlitchTransitionTimer / Scene29GlitchTransitionDuration, 0f, 1f);
            scene29PlayerAlpha = 1f - transitionProgress;
            isMoving = false;
            currentAnimation = idleAnimation;

            if (scene29GlitchTransitionTimer >= Scene29GlitchTransitionDuration)
            {
                scene29GlitchTransitionActive = false;
                scene29GlitchTransitionTimer = 0f;
                scene29PlayerAlpha = 1f;
                LoadSceneWithDialogues(30);
                position = new Vector2(_graphics.PreferredBackBufferWidth * 0.5f, GetSceneGroundY(30));
            }

            return true;
        }

        private bool UpdateScene30Sequence(GameTime gameTime)
        {
            if (scene30SequenceComplete)
                return false;

            if (scene30BackViewPauseActive)
            {
                chibiBAnimation?.Update(gameTime);
                currentAnimation = chibiBAnimation ?? idleAnimation;
            }
            else
            {
                idleAnimation?.Update(gameTime);
                currentAnimation = idleAnimation ?? currentAnimation;
            }
            position.X = _graphics.PreferredBackBufferWidth * 0.5f;
            position.Y = GetSceneGroundY(30);
            isMoving = false;

            if (scene30BackViewPauseActive)
            {
                scene30BackViewPauseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (scene30BackViewPauseTimer >= 1.35f)
                {
                    scene30BackViewPauseActive = false;
                    Action? completion = scene30BackViewPauseCompletion;
                    scene30BackViewPauseCompletion = null;
                    completion?.Invoke();
                }
                return true;
            }

            if (!scene30SequenceStarted)
            {
                scene30SequenceStarted = true;
                idleAnimation?.Reset();
                currentAnimation = idleAnimation ?? currentAnimation;
                StartNarrationSequence(new List<string> { ".", "..", "..." }, () =>
                {
                    StartScene30BackViewPause(() => OpenScene30FormalChoice(Scene30IgnoreChoiceOneSourceId));
                });
            }

            return true;
        }

        private void StartScene30BackViewPause(Action? onComplete)
        {
            scene30BackViewPauseActive = true;
            chibiBAnimation?.Reset();
            currentAnimation = chibiBAnimation ?? currentAnimation;
            scene30BackViewPauseTimer = 0f;
            scene30BackViewPauseCompletion = onComplete;
        }

        private void OpenScene30FormalChoice(int sourceId)
        {
            scene30BackViewPauseActive = false;
            idleAnimation?.Reset();
            currentAnimation = idleAnimation ?? currentAnimation;
            scene30PendingChoiceStage++;
            activeChoiceSourceDialogueId = sourceId;
            selectedChoiceIndex = 0;
            isChoiceActive = true;
            activeChoiceLoop = false;
            activeChoiceStayNarrationId = null;
            doorSceneChoicePending = false;

            activeChoices = sourceId switch
            {
                Scene30DoNotIgnoreChoiceSourceId => new List<ChoiceEntry>
                {
                    new ChoiceEntry(sourceId, 1, "Do", null, null),
                    new ChoiceEntry(sourceId, 2, "Not", null, null),
                    new ChoiceEntry(sourceId, 3, "Ignore", null, null)
                },
                Scene30ListenChoiceSourceId => new List<ChoiceEntry>
                {
                    new ChoiceEntry(sourceId, 1, "Listen", null, null),
                    new ChoiceEntry(sourceId, 2, "Listen to me", null, null)
                },
                Scene30AgeChoiceSourceId => new List<ChoiceEntry>
                {
                    new ChoiceEntry(sourceId, 1, "Forgive", null, null),
                    new ChoiceEntry(sourceId, 2, "Ignore", null, null),
                    new ChoiceEntry(sourceId, 3, "No reply", null, null)
                },
                _ => new List<ChoiceEntry>
                {
                    new ChoiceEntry(sourceId, 1, "Ignore", null, null),
                    new ChoiceEntry(sourceId, 2, "Ignore", null, null)
                }
            };
        }

        private void ContinueScene30AfterChoice(int sourceId, int choiceOrder)
        {
            if (sourceId == Scene30IgnoreChoiceOneSourceId)
            {
                StartNarrationSequence(new List<string> { string.Empty }, () => OpenScene30FormalChoice(Scene30IgnoreChoiceTwoSourceId));
                return;
            }

            if (sourceId == Scene30IgnoreChoiceTwoSourceId)
            {
                StartScene30BackViewPause(() => OpenScene30FormalChoice(Scene30DoNotIgnoreChoiceSourceId));
                return;
            }

            if (sourceId == Scene30DoNotIgnoreChoiceSourceId)
            {
                OpenScene30FormalChoice(Scene30ListenChoiceSourceId);
                return;
            }

            if (sourceId == Scene30ListenChoiceSourceId)
            {
                StartScene30CoreMessage();
                return;
            }

            if (sourceId == Scene30AgeChoiceSourceId)
            {
                StartScene30AgeChoiceResult(choiceOrder);
            }
        }

        private void StartScene30CoreMessage()
        {
            idleAnimation?.Reset();
            currentAnimation = idleAnimation ?? currentAnimation;

            StartNarrationSequence(BuildNarrationPages(
                "Hey {Player}.",
                "It's about time you stop this nonsense.",
                "Wake up you fool!",
                "I've been trying to reach you.",
                "Your friend also visited.",
                "Pal tried reaching out to you but you were too disoriented to listen to them. Whether you like it or not, I'm going to force you to listen!"), () =>
            {
                StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                    ("Player", "Why are you even doing this..?"),
                    ("System", "And why can't I?! You being so miserable is pissing me off."),
                    ("Player", "Huh."),
                    ("Player", "I don't even know you."),
                    ("Player", "I don't know what you look like."),
                    ("Player", "Whether you can be trusted or not..."),
                    ("Player", "I don't know you.")), () =>
                {
                    StartNarrationSequence(new List<string>
                    {
                        "...",
                        "And yet you didn't keep me away. You could have shut me out a long time ago. You could have ignored me.",
                        "But you. Well, I'm just repaying the debt."
                    }, () =>
                    {
                        StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30, ("Player", "...")), () =>
                        {
                            StartNarrationSequence(new List<string>
                            {
                                "\"Oh my dearest friend... I didn't mean for any of this to happen...\"",
                                "\"I'm so sorry. You may not forgive me for this, and I deserve it, but please... Please wake up.\"",
                                "\"I.. I was just desperate. I was angry... at myself.\"",
                                "\"Please don't beat yourself up for something I did wrong. Continue your journey, please. Go on without me. I'm.. I'm going to start my journey again. It'll be very far away, and I'm not sure if we'll ever cross paths again.\"",
                                "\"Leave this place, please- you deserve so much more than wallowing in misery here.\""
                            }, StartScene30AgeBranch);
                        });
                    });
                });
            });
        }

        private void StartScene30AgeBranch()
        {
            if (playerAge >= 51)
            {
                StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                    ("Player", "..."),
                    ("Player", "(Fool. They should have just focused on their own life. They didn't have to tell me that.. Deep down I already knew.)"),
                    ("Player", "(I'm already old and fragile as a dead tree, just waiting when I'll fully decay..)"),
                    ("Player", "(I'm already tired from all these journeys- I just want to rest.. I want to settle somewhere. This world may not be very ideal but it's worth giving a try. I could build a house... And maybe guide new travelers along the way.)"),
                    ("Player", "(I believe that's what they want for me to do too. Our journey doesn't necessarily have to be literal either.)"),
                    ("Player", "Hah.")), StartScene30AgeChoicePrompt);
                return;
            }

            if (playerAge >= 26)
            {
                StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                    ("Player", "..."),
                    ("Player", "(While I was not in a good condition, Pal took another step forward to reach their own goals.)"),
                    ("Player", "(What a disgrace.)"),
                    ("Player", "(It's clear that they don't want me to waste my whole life here, wallowing in self-loathing... To rebuild..)"),
                    ("Player", "Hah."),
                    ("Player", "?")), StartScene30AgeChoicePrompt);
                return;
            }

            StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                ("Player", "..."),
                ("Player", "Pal.."),
                ("Player", "*Sigh..*"),
                ("Player", "...I'm being stupid, aren't I? I-")), StartScene30AgeChoicePrompt);
        }

        private void StartScene30AgeChoicePrompt()
        {
            StartNarrationSequence(new List<string>
            {
                "You don't have to force yourself. You can forgive or forget this happening. You are your own person, you get to decide that."
            }, () => OpenScene30FormalChoice(Scene30AgeChoiceSourceId));
        }

        private void StartScene30AgeChoiceResult(int choiceOrder)
        {
            if (playerAge >= 51)
            {
                if (choiceOrder == 1)
                {
                    StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                        ("Player", "..I believe it's stupid to hold onto hatred in this age."),
                        ("Player", "*Sigh* Now don't disturb me, I'll rest for a bit before I get started with my.. little project.")), () =>
                    {
                        StartNarrationSequence(BuildNarrationPages("I see, then thank you for allowing me to join in your travels, {Player}."), TriggerScene30End13);
                    });
                    return;
                }

                if (choiceOrder == 2)
                {
                    StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                        ("Player", "*Sigh* Now go away now, I'll rest for a bit before I get started with my.. little project.")), () =>
                    {
                        StartNarrationSequence(BuildNarrationPages("How rude~ Anyways, thank you for allowing me to join in your travels, {Player}."), TriggerScene30End7);
                    });
                    return;
                }

                StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                    ("Player", "Don't mind me now, I'll rest here for a bit.")), () =>
                {
                    StartNarrationSequence(BuildNarrationPages("Then see you again soon. Thank you for allowing me to join in your travels, {Player}."), TriggerScene30End7);
                });
                return;
            }

            if (playerAge >= 26)
            {
                if (choiceOrder == 1)
                {
                    StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                        ("Player", "You didn't have to mimic what they said, you know? Just the thought would be enough. Though, I should still thank you. I should get going now..")), () =>
                    {
                        StartNarrationSequence(BuildNarrationPages("~", "{Player}, may your journey be as smooth as how you came into this world."), CompleteScene30Sequence);
                    });
                    return;
                }

                if (choiceOrder == 2)
                {
                    StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                        ("Player", "Hmm. I should get going now, thanks for delivering the message.")), () =>
                    {
                        StartNarrationSequence(BuildNarrationPages("No problem~ {Player}, may your journey be as smooth as how you came into this world."), CompleteScene30Sequence);
                    });
                    return;
                }

                StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                    ("Player", "Anyway, thanks for delivering the message. Staying in this place is no fun, after all.")), () =>
                {
                    StartNarrationSequence(BuildNarrationPages("Now that's more like it. {Player}, may your journey be as smooth as how you came into this world."), CompleteScene30Sequence);
                });
                return;
            }

            if (choiceOrder == 1)
            {
                StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                    ("Player", "...Well I guess I have to thank you for delivering their message"),
                    ("Player", "I should get going now.. I'm sorry for the embarrassing display earlier.")), () =>
                {
                    StartNarrationSequence(BuildNarrationPages("~", "{Player}, may your journey be as smooth as how you came into this world."), CompleteScene30Sequence);
                });
                return;
            }

            if (choiceOrder == 2)
            {
                StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                    ("Player", "...I should get going now. Thanks for the wake-up call, I guess.")), () =>
                {
                    StartNarrationSequence(BuildNarrationPages("No problem~ {Player}, may your journey be as smooth as how you came into this world."), CompleteScene30Sequence);
                });
                return;
            }

            StartAutomaticDialogueSequence(BuildTransientSceneDialogues(30,
                ("Player", "...I should get going now.")), () =>
            {
                StartNarrationSequence(BuildNarrationPages("I see. {Player}, may your journey be as smooth as how you came into this world."), CompleteScene30Sequence);
            });
        }

        private void CompleteScene30Sequence()
        {
            scene30SequenceComplete = true;
            suppressStaticNpcsOnNextTransition = true;
            LoadSceneWithDialogues(23);
            position = new Vector2(100, GetSceneGroundY(23));
        }

        private void TriggerScene30End7()
        {
            scene30SequenceComplete = true;
            activeEnding = LoadEnding(7);
            SaveUnlockedEnding(7);
            FinalizeCurrentSceneAnalytics("EndingTriggered");
            StartEndingTransition();
        }

        private void TriggerScene30End13()
        {
            scene30SequenceComplete = true;
            activeEnding = LoadEnding(13);
            SaveUnlockedEnding(13);
            FinalizeCurrentSceneAnalytics("EndingTriggered");
            StartEndingTransition();
        }

        private bool IsSailingScene(int sceneId)
        {
            return sceneId == 39 || sceneId == 40 || sceneId == 41;
        }

        private void StartFullscreenImage(Texture2D? image, string dismissText, Action? onComplete)
        {
            if (image == null)
            {
                onComplete?.Invoke();
                return;
            }

            activeFullscreenImage = image;
            activeFullscreenImageDismissText = dismissText;
            activeFullscreenImageCompletion = onComplete;
        }

        private void StartScene39AutoDialogues(List<DialogueEntry> dialogues, Action? onComplete)
        {
            scene39ActiveAutoDialogues = dialogues;
            scene39AutoDialogueIndex = 0;
            scene39AutoDialogueTimer = 0f;
            scene39AutoDialogueCompletion = onComplete;

            if (dialogues.Count == 0)
            {
                Action? completion = scene39AutoDialogueCompletion;
                scene39AutoDialogueCompletion = null;
                completion?.Invoke();
            }
        }

        private bool UpdateScene39AutoDialogues(GameTime gameTime, List<DialogueEntry> dialogues)
        {
            if (scene39AutoDialogueCompletion == null && scene39AutoDialogueIndex >= dialogues.Count)
                return false;

            if (dialogues.Count == 0)
            {
                Action? completion = scene39AutoDialogueCompletion;
                scene39AutoDialogueCompletion = null;
                completion?.Invoke();
                return true;
            }

            scene39AutoDialogueTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (scene39AutoDialogueTimer >= Scene39AutoDialogueDuration)
            {
                scene39AutoDialogueTimer = 0f;
                scene39AutoDialogueIndex++;
                if (scene39AutoDialogueIndex >= dialogues.Count)
                {
                    Action? completion = scene39AutoDialogueCompletion;
                    scene39AutoDialogueCompletion = null;
                    completion?.Invoke();
                }
            }

            return true;
        }

        private bool UpdateScene39Sequence(GameTime gameTime)
        {
            sailingAnimation?.Update(gameTime);
            position.Y = GetSceneGroundY(39);

            if (!scene39IntroStarted && scene39LoopCount == 1)
            {
                scene39IntroStarted = true;
                StartDialogueSequence(scene39IntroDialogues, () =>
                {
                    StartNarrationSequence(new List<string> { "Woooooshhhh~" });
                });
                return true;
            }

            if (!scene39CenterEventTriggered &&
                scene39LoopCount >= 3 &&
                position.X >= _graphics.PreferredBackBufferWidth * 0.5f)
            {
                scene39CenterEventTriggered = true;
                position.X = _graphics.PreferredBackBufferWidth * 0.5f;
                isMoving = false;
                StartFullscreenImage(wavesImage, "Press ENTER to Exit", () =>
                {
                    StartDialogueSequence(scene39CenterDialogues, () =>
                    {
                        LoadSceneWithDialogues(40);
                        position = new Vector2(_graphics.PreferredBackBufferWidth * 0.5f, GetSceneGroundY(40));
                    });
                });
                return true;
            }

            return false;
        }

        private bool UpdateScene40Sequence(GameTime gameTime)
        {
            sailing2Animation?.Update(gameTime);
            position.X = _graphics.PreferredBackBufferWidth * 0.5f;
            position.Y = GetSceneGroundY(40);
            isMoving = false;

            if (scene40SequenceStarted)
                return true;

            scene40SequenceStarted = true;
            StartNarrationSequence(new List<string>
            {
                "Huge waves soon began to form. Up ahead is a storm, appearing out of nowhere. Dark fog also began to appear.",
                "The bridge can no longer be seen- it's dangerous if the boat suddenly crashes into it. I can't also see any lights from both ends.",
                "I sure do hope I'm almost there.. It's getting harder to control the boat-"
            }, () =>
            {
                StartFullscreenImage(wavesImage, "Press ENTER to Exit", () =>
                {
                    StartNarrationSequence(new List<string>
                    {
                        "Qu-quick! I must grab any wood-",
                        "I need to stay afloat!"
                    }, () =>
                    {
                        LoadSceneWithDialogues(41);
                        position = new Vector2(_graphics.PreferredBackBufferWidth * 0.5f, GetSceneGroundY(41));
                    });
                });
            });

            return true;
        }

        private bool UpdateScene41Sequence(GameTime gameTime)
        {
            sailing2Animation?.Update(gameTime);
            position.X = _graphics.PreferredBackBufferWidth * 0.5f;
            position.Y = GetSceneGroundY(41);
            isMoving = false;

            if (scene41SequenceStarted)
                return true;

            scene41SequenceStarted = true;
            StartNarrationSequence(new List<string>
            {
                "Your time is running out. Choose quickly. What you do next will have a significant outcome for your journey."
            }, OpenScene41Choice);

            return true;
        }

        private void OpenScene41Choice()
        {
            activeChoices = new List<ChoiceEntry>
            {
                new ChoiceEntry(Scene41ChoiceSourceId, 1, "Cling to the wood", null, null),
                new ChoiceEntry(Scene41ChoiceSourceId, 2, "Release the wood and swim", null, null)
            };
            selectedChoiceIndex = 0;
            isChoiceActive = true;
            activeChoiceSourceDialogueId = Scene41ChoiceSourceId;
            activeChoiceLoop = false;
            activeChoiceStayNarrationId = null;
            doorSceneChoicePending = false;
            scene41TimedChoiceActive = true;
            scene41ChoiceTimer = Scene41ChoiceDuration;
        }

        private void HandleScene41ChoiceTimeout()
        {
            isChoiceActive = false;
            activeChoices.Clear();
            selectedChoiceIndex = 0;
            activeChoiceSourceDialogueId = null;
            scene41TimedChoiceActive = false;
            scene41ChoiceTimer = 0f;
            scene36EndingId = 9;
            StartNarrationSequence(new List<string>
            {
                "CRASHH!!",
                "Is.. is this how it ends? I didn't even get to choose yet-"
            }, () => LoadSceneWithDialogues(37));
            LogActionAnalytics("ChoiceTimeout", "Scene41Timer", "Scene37End9");
        }

        private void StartScene41ChoiceSequence(int choiceOrder)
        {
            switch (choiceOrder)
            {
                case 1:
                    StartDialogueSequence(BuildTransientSceneDialogues(41,
                        "He-hey, weird voice! Please help me-!",
                        "I don't know how to swim- I don't even know how to float-!",
                        "This is the only way I could live-"), () =>
                    {
                        scene36EndingId = 12;
                        StartNarrationSequence(new List<string>
                        {
                            "(Distorted and glitchy) I'm scared!",
                            "I-"
                        }, () => LoadSceneWithDialogues(37));
                    });
                    break;

                case 2:
                    StartDialogueSequence(BuildTransientSceneDialogues(41,
                        "If... if I let go, I'll die- If I stay, I'll also die-",
                        "If I'm going to die, right here today - I might as well learn how to swim!"), () =>
                    {
                        StartNarrationSequence(new List<string>
                        {
                            "Splash! Splash!",
                            "*Cough* Aa *Cough* I must-",
                            "!!",
                            "Crash!"
                        }, () =>
                        {
                            StartFullscreenImage(waves2Image, "Press ENTER to Exit", () =>
                            {
                                scene41SuccessBlackoutActive = true;
                                StartDialogueSequence(BuildTransientSceneDialogues(41, "I... I did it! I- I lived.."), () =>
                                {
                                    scene41SuccessBlackoutActive = false;
                                    StartNarrationSequence(new List<string>
                                    {
                                        "Slowly but surely, you managed to swim towards the other wooden piles. It's difficult and will take a while but at least it's keeping you alive",
                                        "For the first time in your life, you're learning how to swim-learning how to overcome your fears- learning how to stay alive."
                                    }, () => LoadSceneWithDialogues(38));
                                });
                            });
                        });
                    });
                    break;

                default:
                    HandleScene41ChoiceTimeout();
                    break;
            }
        }

        private void StartScene35TimedOptionSequence(int choiceOrder)
        {
            switch (choiceOrder)
            {
                case 1:
                    StartDialogueSequence(LoadDialogueEntriesByIds(299), () =>
                    {
                        scene36EndingId = 10;
                        StartNarrationSequence(LoadNarrationPagesByIds(63, 64), () => LoadSceneWithDialogues(37));
                    });
                    break;

                case 2:
                    StartDialogueSequence(LoadDialogueEntriesByIds(300, 301, 302, 303, 304, 305, 306), () =>
                    {
                        StartNarrationSequence(LoadNarrationPagesByIds(65, 66, 67, 68, 69), OpenScene35PullChoice);
                    });
                    break;

                case 3:
                    StartDialogueSequence(LoadDialogueEntriesByIds(307, 308, 309), () =>
                    {
                        StartNarrationSequence(LoadNarrationPagesByIds(73, 74, 75), () =>
                        {
                            string successLine = LoadNarrationTextById(76) ?? "I- I did it!";
                            StartDialogueSequence(new[] { BuildTransientPlayerDialogue(successLine.Trim('"')) }, BeginScene34BridgeSuccessRoute);
                        });
                    });
                    break;

                default:
                    scene36EndingId = 9;
                    LoadSceneWithDialogues(37);
                    break;
            }
        }

        private void OpenScene35PullChoice()
        {
            activeChoices = new List<ChoiceEntry>
            {
                new ChoiceEntry(Scene35PullChoiceSourceId, 1, "Pull your body with everything that you can", null, null)
            };
            selectedChoiceIndex = 0;
            isChoiceActive = true;
            activeChoiceSourceDialogueId = Scene35PullChoiceSourceId;
            activeChoiceLoop = false;
            activeChoiceStayNarrationId = null;
            doorSceneChoicePending = false;
            scene35TimedChoiceActive = true;
            scene35ChoiceTimer = Scene35PullChoiceDuration;
            LogActionAnalytics("ChoiceMenuOpened", "Scene35PullTimer", "Choices:1");
        }

        private void StartScene35PullSuccessSequence()
        {
            StartNarrationSequence(LoadNarrationPagesByIds(71, 72), BeginScene34BridgeSuccessRoute);
        }

        private void BeginScene34BridgeSuccessRoute()
        {
            scene36LoopCount = 0;
            LoadSceneWithDialogues(36);
            position = new Vector2(0, GetSceneGroundY(36));
        }

        private bool UpdateScene35Sequence()
        {
            if (currentSceneId != 35)
                return false;

            position.X = _graphics.PreferredBackBufferWidth * 0.5f;
            position.Y = GetSceneGroundY(35);
            isMoving = false;
            currentAnimation = idleAnimation;

            if (!scene35SequenceStarted)
            {
                scene35SequenceStarted = true;
            }

            if (!scene35NarrationStarted &&
                !dialogueManager.IsActive && !dialogueManager.IsWaitingForInteract &&
                dialogueManager.IsFinished && !isNarrationActive && !isChoiceActive)
            {
                scene35NarrationStarted = true;
                StartNarrationSequence(LoadNarrationPagesByIds(58, 59, 60), () =>
                {
                    OpenChoiceMenuFromDialogue(298);
                    scene35TimedChoiceActive = true;
                    scene35ChoiceTimer = Scene35ChoiceDuration;
                });
                return true;
            }

            return scene35TimedChoiceActive;
        }

        private void HandleScene35ChoiceTimeout()
        {
            bool pullChoiceTimedOut = activeChoiceSourceDialogueId == Scene35PullChoiceSourceId;
            isChoiceActive = false;
            activeChoices.Clear();
            selectedChoiceIndex = 0;
            activeChoiceSourceDialogueId = null;
            activeChoiceLoop = false;
            activeChoiceStayNarrationId = null;
            scene35TimedChoiceActive = false;
            scene35ChoiceTimer = 0f;

            if (pullChoiceTimedOut)
            {
                scene36EndingId = 9;
                StartNarrationSequence(LoadNarrationPagesByIds(70), () => LoadSceneWithDialogues(37));
                LogActionAnalytics("ChoiceTimeout", "Scene35PullTimer", "Scene37End9");
                return;
            }

            List<DialogueEntry> timeoutDialogues = new()
            {
                new DialogueEntry(GetTransientDialogueId(), 35, "Player", "?!", null, null)
            };

            scene35TimeoutDialogueAwaitingCompletion = true;
            dialogueManager.StartDialogue(timeoutDialogues, dialogueBoxTexture, dialogueBoxTexture2, true, true);
            LogActionAnalytics("ChoiceTimeout", "Scene35Timer", "Scene37End9");
        }

        private void UpdateScene37DrowningSequence(GameTime gameTime)
        {
            position.X = _graphics.PreferredBackBufferWidth * 0.5f;
            position.Y = GetSceneGroundY(37);
            isMoving = false;
            currentAnimation = idleAnimation;

            if (!scene36AnimationStarted)
            {
                scene36AnimationStarted = true;
                scene36AnimationFinished = false;
                scene37EndingDialogues = LoadDialogues(37, playerAge)
                    .OrderBy(entry => entry.DialogueID)
                    .ToList();
                scene37EndingDialogueIndex = 0;
                scene37EndingDialogueTimer = 0f;
                scene37EndingAnimationFrameIndex = 0;
                scene37EndingAnimationTimer = 0f;
                scene37EndingAnimationLooped = false;
                scene37AutoDialogueComplete = scene37EndingDialogues.Count == 0;
                if (scene37EndingDialogues.Count > 0)
                    dialogueManager.StartDialogue(scene37EndingDialogues, dialogueBoxTexture, dialogueBoxTexture2, false, true);
            }

            if (scene36AnimationFinished || activeEnding != null || endingTransitionActive)
                return;

            if (!scene37AutoDialogueComplete)
            {
                if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || !dialogueManager.IsFinished)
                    return;

                scene37AutoDialogueComplete = true;
                scene37EndingAnimationFrameIndex = 0;
                scene37EndingAnimationTimer = 0f;
                scene37EndingAnimationLooped = drowningTextures.Length <= 1;
                return;
            }

            UpdateScene37EndingAnimation(gameTime);

            if (scene37EndingAnimationLooped)
            {
                scene36AnimationFinished = true;
                activeEnding = LoadEnding(scene36EndingId);
                SaveUnlockedEnding(scene36EndingId);
                FinalizeCurrentSceneAnalytics("EndingTriggered");
                StartEndingTransition();
            }
        }

        private void UpdateScene37EndingAnimation(GameTime gameTime)
        {
            if (drowningTextures.Length == 0)
            {
                scene37EndingAnimationLooped = true;
                return;
            }

            scene37EndingAnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (scene37EndingAnimationTimer >= Scene37EndingAnimationFrameDuration)
            {
                scene37EndingAnimationTimer -= Scene37EndingAnimationFrameDuration;
                scene37EndingAnimationFrameIndex++;
                if (scene37EndingAnimationFrameIndex >= drowningTextures.Length)
                {
                    scene37EndingAnimationFrameIndex = drowningTextures.Length - 1;
                    scene37EndingAnimationLooped = true;
                    break;
                }
            }
        }

        private void UpdateScene37EndingAutoDialogue(GameTime gameTime)
        {
            if (scene37EndingDialogues.Count == 0 ||
                scene37EndingDialogueIndex >= scene37EndingDialogues.Count)
                return;

            scene37EndingDialogueTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (scene37EndingDialogueTimer >= Scene37EndingDialogueDuration)
            {
                scene37EndingDialogueTimer = 0f;
                scene37EndingDialogueIndex++;
            }
        }

        private bool UpdateScene37Sequence(GameTime gameTime)
        {
            if (currentSceneId != 38)
                return false;

            if (scene37EndingTriggered || activeEnding != null || endingTransitionActive)
                return true;

            if (scene37BlackoutActive)
            {
                scene37BlackoutTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                isMoving = false;
                currentAnimation = idleAnimation;

                if (scene37BlackoutTimer >= Scene37BlackoutDuration)
                {
                    scene37EndingTriggered = true;
                    activeEnding = LoadEnding(11);
                    SaveUnlockedEnding(11);
                    FinalizeCurrentSceneAnalytics("EndingTriggered");
                    StartEndingTransition();
                }

                return true;
            }

            if (!scene37SequenceStarted)
            {
                scene37SequenceStarted = true;
                position = new Vector2(100, GetSceneGroundY(38));
                currentAnimation = idleAnimation;
                scene37AutoDialogues = LoadDialogueEntriesByIds(310, 311, 312, 313);
                if (scene37AutoDialogues.Count == 0)
                {
                    scene37AutoDialogues = new List<DialogueEntry>
                    {
                        new DialogueEntry(GetTransientDialogueId(), 38, "Player", "...What is this place?", null, null),
                        new DialogueEntry(GetTransientDialogueId(), 38, "Player", "A crack in the sky?", null, null),
                        new DialogueEntry(GetTransientDialogueId(), 38, "Player", "Is this the ��� Pal talked about..?", null, null),
                        new DialogueEntry(GetTransientDialogueId(), 38, "Player", "Am I finally reaching the end of this journey?", null, null)
                    };
                }

                scene37AutoDialogueComplete = false;
                dialogueManager.StartDialogue(scene37AutoDialogues, dialogueBoxTexture, dialogueBoxTexture2, true, true);
                return true;
            }

            if (!scene37AutoDialogueComplete)
            {
                if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || !dialogueManager.IsFinished)
                    return true;

                scene37AutoDialogueComplete = true;
                return false;
            }

            if (!scene38CenterBlackoutTriggered && position.X >= _graphics.PreferredBackBufferWidth * 0.5f)
            {
                scene38CenterBlackoutTriggered = true;
                scene37BlackoutActive = true;
                scene37BlackoutTimer = 0f;
                position.X = _graphics.PreferredBackBufferWidth * 0.5f;
                position.Y = GetSceneGroundY(38);
                isMoving = false;
                currentAnimation = idleAnimation;
                return true;
            }

            return false;
        }

        private bool IsScene29ConflictDialogueActive()
        {
            if (currentSceneId != 29 || dialogueManager == null)
                return false;

            DialogueEntry? currentEntry = dialogueManager.CurrentEntry;
            return dialogueManager.IsActive && currentEntry != null && currentEntry.DialogueID >= 216;
        }

        private float GetScene29GlitchIntensity()
        {
            if (scene29GlitchTransitionActive)
            {
                float transitionProgress = MathHelper.Clamp(scene29GlitchTransitionTimer / Scene29GlitchTransitionDuration, 0f, 1f);
                return MathHelper.Lerp(0.42f, 0.75f, transitionProgress);
            }

            if (!IsScene29ConflictDialogueActive())
                return 0f;

            int dialogueId = dialogueManager?.CurrentEntry?.DialogueID ?? 216;
            float progress = MathHelper.Clamp((dialogueId - 216) / 24f, 0f, 1f);
            float baseIntensity = MathHelper.Lerp(0.10f, 0.50f, progress);

            if (dialogueId == 218)
                return MathF.Max(baseIntensity, 0.72f);

            if (dialogueId == 231)
                return MathF.Max(baseIntensity, 0.88f);

            return baseIntensity;
        }

        private void DrawScene29GlitchOverlay(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (ground == null)
                return;

            float intensity = GetScene29GlitchIntensity();
            if (intensity <= 0f)
                return;

            int bandCount = 3 + (int)MathF.Ceiling(intensity * 18f);
            int maxOffset = 4 + (int)MathF.Ceiling(intensity * 26f);
            for (int i = 0; i < bandCount; i++)
            {
                int bandHeight = endingGlitchRandom.Next(2, 5 + (int)MathF.Ceiling(intensity * 10f));
                int y = endingGlitchRandom.Next(0, Math.Max(1, screenHeight - bandHeight));
                int offset = endingGlitchRandom.Next(-maxOffset, maxOffset + 1);
                Color bandColor = i % 3 == 0
                    ? Color.White * (0.04f + intensity * 0.10f)
                    : (i % 3 == 1 ? Color.Red * (0.03f + intensity * 0.08f) : Color.Cyan * (0.03f + intensity * 0.08f));

                spriteBatch.Draw(ground, new Rectangle(offset, y, screenWidth, bandHeight), bandColor);
            }

            int noiseCount = 6 + (int)MathF.Ceiling(intensity * 34f);
            for (int i = 0; i < noiseCount; i++)
            {
                int x = endingGlitchRandom.Next(0, Math.Max(1, screenWidth));
                int y = endingGlitchRandom.Next(0, Math.Max(1, screenHeight));
                int width = endingGlitchRandom.Next(2, 5 + (int)MathF.Ceiling(intensity * 12f));
                spriteBatch.Draw(ground, new Rectangle(x, y, width, 1), Color.White * (0.04f + intensity * 0.10f));
            }

            if (intensity >= 0.30f)
            {
                int scanlineGap = Math.Max(8, 18 - (int)(intensity * 16f));
                for (int y = 0; y < screenHeight; y += scanlineGap)
                    spriteBatch.Draw(ground, new Rectangle(0, y, screenWidth, 1), Color.Black * 0.12f);
            }
        }

        private void DrawScene30GlitchOverlay(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (ground == null)
                return;

            float intensity = 0.18f;
            int bandCount = 4;
            int maxOffset = 8;
            for (int i = 0; i < bandCount; i++)
            {
                int bandHeight = endingGlitchRandom.Next(2, 7);
                int y = endingGlitchRandom.Next(0, Math.Max(1, screenHeight - bandHeight));
                int offset = endingGlitchRandom.Next(-maxOffset, maxOffset + 1);
                Color color = i % 2 == 0 ? Color.White * 0.07f : Color.Cyan * 0.05f;
                spriteBatch.Draw(ground, new Rectangle(offset, y, screenWidth, bandHeight), color);
            }

            for (int i = 0; i < 10; i++)
            {
                int x = endingGlitchRandom.Next(0, Math.Max(1, screenWidth));
                int y = endingGlitchRandom.Next(0, Math.Max(1, screenHeight));
                int width = endingGlitchRandom.Next(6, 38);
                spriteBatch.Draw(ground, new Rectangle(x, y, width, 1), Color.White * (0.04f + intensity * 0.08f));
            }
        }

        private void DrawScene37GlitchOverlay(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (ground == null)
                return;

            int bandCount = 7;
            for (int i = 0; i < bandCount; i++)
            {
                int bandHeight = endingGlitchRandom.Next(2, 8);
                int y = endingGlitchRandom.Next(0, Math.Max(1, screenHeight - bandHeight));
                int offset = endingGlitchRandom.Next(-18, 19);
                Color bandColor = i % 3 == 0
                    ? Color.White * 0.05f
                    : (i % 3 == 1 ? Color.Red * 0.05f : Color.Cyan * 0.04f);

                spriteBatch.Draw(ground, new Rectangle(offset, y, screenWidth, bandHeight), bandColor);
            }

            for (int i = 0; i < 18; i++)
            {
                int x = endingGlitchRandom.Next(0, Math.Max(1, screenWidth));
                int y = endingGlitchRandom.Next(0, Math.Max(1, screenHeight));
                spriteBatch.Draw(ground, new Rectangle(x, y, endingGlitchRandom.Next(2, 7), 1), Color.White * 0.08f);
            }
        }

        private void TryTriggerScene17FollowupDialogue()
        {
            if (currentSceneId != 17)
                return;

            if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isNarrationActive || isChoiceActive)
                return;

            if (!scene17CenterDialogueTriggered && position.X >= (_graphics.PreferredBackBufferWidth * 0.5f))
            {
                scene17CenterDialogueTriggered = true;

                if (scene17CenterDialogues.Count > 0)
                {
                    dialogueManager.StartDialogue(scene17CenterDialogues, dialogueBoxTexture, dialogueBoxTexture2, true, true);
                    return;
                }
            }

            if (scene17CenterDialogueTriggered &&
                scene17NameInputPending &&
                !scene17AfterNamingStarted &&
                dialogueManager.IsFinished)
            {
                BeginScene17NameInput();
            }
        }

        private bool TryTriggerScene19CenterNarration()
        {
            if (currentSceneId != 19 || scene19TransitionNarrationTriggered)
                return false;

            if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isNarrationActive || isChoiceActive || showExitPrompt)
                return false;

            if (position.X < (_graphics.PreferredBackBufferWidth * 0.5f))
                return false;

            scene19TransitionNarrationTriggered = true;
            StartNarrationSequence(
                LoadNarrationPagesByIds(7),
                () =>
                {
                // Include the blank spacer (20) and the trailing attribution (21) so the poem
                // preserves intended spacing and final line.
                string poemBlock = string.Join("\n", LoadNarrationPagesByIds(8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21));
                    StartNarrationSequence(new List<string> { poemBlock }, () =>
                    {
                        LoadSceneWithDialogues(20);
                    });
                });
            return true;
        }

        private bool TryTriggerScene23Narration()
        {
            if (currentSceneId != 23 || scene23NarrationTriggered)
                return false;

            if (dialogueManager.IsActive || dialogueManager.IsWaitingForInteract || isChoiceActive || isNarrationActive)
                return false;

            scene23NarrationTriggered = true;
            StartNarrationSequence(BuildNarrationPages(
                "Congratulations for making it this far.",
                "I know how painful this journey was... to be apart from friends- companions... losing them in the end. Life will always give and take away something precious from humans.",
                "Raise your head high! Be patient, be confident, be steadfast! Who know, maybe you'll finally find a way out of this void."), () =>
            {
                if (scene23IntroDialogues.Count > 0)
                {
                    dialogueManager.StartDialogue(scene23IntroDialogues, dialogueBoxTexture, dialogueBoxTexture2, false, true);
                }
            });

            return true;
        }

        private void BeginScene17NameInput()
        {
            scene17NameInputActive = true;
            scene17NameInputBuffer.Clear();
            scene17NameInputCompletion = CommitScene17NameInput;
        }

        private void UpdateScene17NameInput(KeyboardState previousKeyState)
        {
            if (keyState.IsKeyDown(Keys.Back) && previousKeyState.IsKeyUp(Keys.Back) && scene17NameInputBuffer.Length > 0)
                scene17NameInputBuffer.Length--;

            if (keyState.IsKeyDown(Keys.Space) && previousKeyState.IsKeyUp(Keys.Space) && scene17NameInputBuffer.Length < 50)
                scene17NameInputBuffer.Append(' ');

            bool shift = keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift);

            for (Keys key = Keys.A; key <= Keys.Z; key++)
            {
                if (keyState.IsKeyDown(key) && previousKeyState.IsKeyUp(key) && scene17NameInputBuffer.Length < 50)
                {
                    char letter = (char)('a' + (key - Keys.A));
                    scene17NameInputBuffer.Append(shift ? char.ToUpperInvariant(letter) : letter);
                }
            }

            for (Keys key = Keys.D0; key <= Keys.D9; key++)
            {
                if (keyState.IsKeyDown(key) && previousKeyState.IsKeyUp(key) && scene17NameInputBuffer.Length < 50)
                {
                    char digit = (char)('0' + (key - Keys.D0));
                    scene17NameInputBuffer.Append(digit);
                }
            }

            if (keyState.IsKeyDown(Keys.Enter) && previousKeyState.IsKeyUp(Keys.Enter))
            {
                string chosenName = scene17NameInputBuffer.ToString().Trim();
                if (string.IsNullOrWhiteSpace(chosenName))
                    return;

                scene17NameInputActive = false;
                Action<string>? completion = scene17NameInputCompletion;
                scene17NameInputCompletion = null;
                completion?.Invoke(chosenName);
            }
        }

        private void CommitScene17NameInput(string chosenName)
        {
            savedOwlName = chosenName;
            SaveScene17Name();
            scene17NameInputPending = false;
            scene17AfterNamingStarted = true;

            currentSceneDialogues = LoadDialogues(17, playerAge);
            PrepareScene17Dialogues();

            if (scene17AfterNamingDialogues.Count > 0)
                dialogueManager.StartDialogue(scene17AfterNamingDialogues, dialogueBoxTexture, dialogueBoxTexture2, true, true);
        }

        private void LoadScene17SavedName()
        {
            try
            {
                if (!File.Exists(Scene17NameSavePath))
                    return;

                string json = File.ReadAllText(Scene17NameSavePath);
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("OwlName", out JsonElement owlNameProp))
                {
                    string? candidate = owlNameProp.GetString();
                    if (!string.IsNullOrWhiteSpace(candidate))
                        savedOwlName = candidate.Trim();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load scene17 name: {ex.Message}");
            }
        }

        private void SaveScene17Name()
        {
            try
            {
                string json = JsonSerializer.Serialize(new { OwlName = savedOwlName }, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Scene17NameSavePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save scene17 name: {ex.Message}");
            }
        }

        private void DrawScene17NameInput(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);

            string prompt = "Enter a name for the \"fella\":";
            string typedValue = scene17NameInputBuffer.Length == 0 ? "_" : scene17NameInputBuffer.ToString() + "_";

            Vector2 promptSize = dialogueFont.MeasureString(prompt);
            Vector2 valueSize = dialogueFont.MeasureString(typedValue);

            float promptX = (screenWidth - promptSize.X) / 2f;
            float valueX = (screenWidth - valueSize.X) / 2f;
            float promptY = (screenHeight / 2f) - dialogueFont.LineSpacing;
            float valueY = promptY + dialogueFont.LineSpacing + 24f;

            spriteBatch.DrawString(dialogueFont, prompt, new Vector2(promptX, promptY), Color.White);
            spriteBatch.DrawString(dialogueFont, typedValue, new Vector2(valueX, valueY), Color.Yellow);
        }

        private void UpdateScene17NpcAnimation(GameTime gameTime)
        {
            if (currentSceneId != 17 && currentSceneId != 20 && currentSceneId != 22 && currentSceneId != 23 && currentSceneId != 24 &&
                currentSceneId != 25 && currentSceneId != 26 && currentSceneId != 27 && currentSceneId != 28 &&
                currentSceneId != 29 && currentSceneId != 31 && currentSceneId != 32)
                return;

            friendSadAnimation?.Update(gameTime);
            friendHAnimation?.Update(gameTime);
            owlFlyAnimation?.Update(gameTime);
            owlIdleAnimation?.Update(gameTime);
        }

        private void DrawScene17Npcs(SpriteBatch spriteBatch)
        {
            if (currentSceneId != 17)
                return;

            Texture2D? friendFrame = friendSadAnimation?.CurrentFrame;
            Texture2D? owlFrame = owlFlyAnimation?.CurrentFrame;
            if (friendFrame == null || owlFrame == null)
                return;

            float friendX = _graphics.PreferredBackBufferWidth * 0.76f;
            float owlX = _graphics.PreferredBackBufferWidth * 0.88f;
            float npcY = GetSceneGroundY(currentSceneId);

            Vector2 friendOrigin = new Vector2(friendFrame.Width / 2f, friendFrame.Height);
            Vector2 owlOrigin = new Vector2(owlFrame.Width / 2f, owlFrame.Height);

            spriteBatch.Draw(friendFrame, new Vector2(friendX, npcY), null, Color.White, 0f, friendOrigin, spriteScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(owlFrame, new Vector2(owlX, npcY), null, Color.White, 0f, owlOrigin, spriteScale, SpriteEffects.FlipHorizontally, 0f);
        }

        private void UpdateFollowerPartyAnimations(GameTime gameTime)
        {
            if (!IsFollowerPartyScene(currentSceneId))
                return;

            owlFlyAnimation?.Update(gameTime);
            GetFollowerFriendAnimation()?.Update(gameTime);
        }

        // Update companion follower animations for scenes 25-28 when the player chose a companion route.
        // This follows similar logic to UpdateFollowerPartyAnimations but is only enabled when companionRoute is set
        // and for scenes 25-28. It does not enable camera tracking.
        private void UpdateCompanionFollowerAnimations(GameTime gameTime)
        {
            if (companionRoute == CompanionRoute.None)
                return;

            if (currentSceneId < 25 || currentSceneId > 28)
                return;

            // If left companion route, friendHAnimation follows the player.
            if (companionRoute == CompanionRoute.Left)
            {
                friendHAnimation?.Update(gameTime);
            }
            else if (companionRoute == CompanionRoute.Right)
            {
                owlFlyAnimation?.Update(gameTime);
                owlIdleAnimation?.Update(gameTime);
            }
        }

        private void DrawFollowerPartyNpcs(SpriteBatch spriteBatch)
        {
            if (!IsFollowerPartyScene(currentSceneId))
                return;

            Texture2D? owlFrame = owlFlyAnimation?.CurrentFrame;
            Texture2D? friendFrame = GetFollowerFriendAnimation()?.CurrentFrame;
            if (owlFrame == null || friendFrame == null)
                return;

            float owlOffsetX = facingRight ? -110f : 110f;
            float friendOffsetX = facingRight ? -220f : 220f;

            Vector2 owlPosition = new Vector2(position.X + owlOffsetX, position.Y + FollowerOwlYOffset);
            Vector2 friendPosition = new Vector2(position.X + friendOffsetX, position.Y);

            Vector2 owlOrigin = new Vector2(owlFrame.Width / 2f, owlFrame.Height);
            Vector2 friendOrigin = new Vector2(friendFrame.Width / 2f, friendFrame.Height);

            SpriteEffects owlFlip = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            SpriteEffects friendFlip = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(friendFrame, friendPosition, null, Color.White, 0f, friendOrigin, spriteScale, friendFlip, 0f);
            spriteBatch.Draw(owlFrame, owlPosition, null, Color.White, 0f, owlOrigin, spriteScale, owlFlip, 0f);
        }

        private void DrawCompanionFollowerNpc(SpriteBatch spriteBatch)
        {
            if (companionRoute == CompanionRoute.None || currentSceneId < 25 || currentSceneId > 28)
                return;

            Texture2D? companionFrame = companionRoute == CompanionRoute.Left
                ? friendHAnimation?.CurrentFrame
                : (isMoving ? owlFlyAnimation?.CurrentFrame : owlIdleAnimation?.CurrentFrame);

            if (companionFrame == null)
                return;

            float offsetX = facingRight ? -120f : 120f;
            float offsetY = companionRoute == CompanionRoute.Right ? FollowerOwlYOffset : 0f;
            Vector2 companionPosition = new Vector2(position.X + offsetX, position.Y + offsetY);
            Vector2 companionOrigin = new Vector2(companionFrame.Width / 2f, companionFrame.Height);
            SpriteEffects companionFlip = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(companionFrame, companionPosition, null, Color.White, 0f, companionOrigin, spriteScale, companionFlip, 0f);
        }

        private Animation? GetFollowerFriendAnimation()
        {
            return friendHAnimation;
        }

        private void DrawStaticPartySceneNpcs(SpriteBatch spriteBatch)
        {
            if (currentSceneId != 20 && currentSceneId != 22 && currentSceneId != 24 && currentSceneId != 29 &&
                currentSceneId != 31 && currentSceneId != 32)
                return;

            float npcY = GetSceneGroundY(currentSceneId);
            float friendX = scene20FriendPosition == Vector2.Zero ? _graphics.PreferredBackBufferWidth * 0.76f : scene20FriendPosition.X;
            float owlX = scene20OwlPosition == Vector2.Zero ? _graphics.PreferredBackBufferWidth * 0.88f : scene20OwlPosition.X;

            if (currentSceneId == 32)
            {
                Texture2D? owlFrame = scene32OwlVisualState switch
                {
                    0 => owlFlyAnimation?.CurrentFrame,
                    1 => owlIdleAnimation?.CurrentFrame,
                    _ => owlDieTexture
                };

                if (owlFrame != null)
                {
                    Vector2 owlOrigin = new Vector2(owlFrame.Width / 2f, owlFrame.Height);
                    float owlCenterRightX = _graphics.PreferredBackBufferWidth * 0.75f;
                    spriteBatch.Draw(owlFrame, new Vector2(owlCenterRightX, npcY), null, Color.White, 0f, owlOrigin, spriteScale, SpriteEffects.FlipHorizontally, 0f);
                }
                return;
            }

            // Scene 22: present friend (joy) and owl on the right side. Friend should face left.
            if (currentSceneId == 22)
            {
                // Use animation frames (animated) instead of static textures so they are not frozen
                Texture2D? friend22Frame = friendHAnimation?.CurrentFrame ?? friendSadAnimation?.CurrentFrame;
                Texture2D? owl22Frame = owlFlyAnimation?.CurrentFrame;
                float fx = _graphics.PreferredBackBufferWidth * 0.82f;
                float ox = _graphics.PreferredBackBufferWidth * 0.92f;

                if (friend22Frame != null)
                {
                    Vector2 originF = new Vector2(friend22Frame.Width / 2f, friend22Frame.Height);
                    // draw friend facing left
                    spriteBatch.Draw(friend22Frame, new Vector2(fx, npcY), null, Color.White, 0f, originF, spriteScale, SpriteEffects.FlipHorizontally, 0f);
                }

                if (!scene22OwlGone && owl22Frame != null)
                {
                    float fadeProgress = scene22OwlFadeActive
                        ? MathHelper.Clamp(scene22OwlFadeTimer / Scene22OwlFadeDuration, 0f, 1f)
                        : 0f;
                    float alpha = 1f - fadeProgress;
                    Vector2 originO = new Vector2(owl22Frame.Width / 2f, owl22Frame.Height);
                    // owl on the far right, flipped to face left
                    spriteBatch.Draw(owl22Frame, new Vector2(ox, npcY), null, Color.White * alpha, 0f, originO, spriteScale, SpriteEffects.FlipHorizontally, 0f);
                }

                return;
            }

            if (currentSceneId == 29)
            {
                Texture2D? friendFadeFrame = friendSadAnimation?.CurrentFrame;
                if (friendFadeFrame == null)
                    return;

                Vector2 friendOrigin = new Vector2(friendFadeFrame.Width / 2f, friendFadeFrame.Height);
                SpriteEffects friendFlip = IsScene29ConflictDialogueActive()
                    ? SpriteEffects.FlipHorizontally
                    : SpriteEffects.None;
                spriteBatch.Draw(friendFadeFrame, new Vector2(friendX, npcY), null, Color.White * MathHelper.Clamp(scene29FriendAlpha, 0f, 1f), 0f, friendOrigin, spriteScale, friendFlip, 0f);
                return;
            }

            if (currentSceneId == 24)
            {
                Texture2D? friend24Frame = friendSadAnimation?.CurrentFrame;
                Texture2D? owl24Frame = owlFlyAnimation?.CurrentFrame;

                if (friend24Frame != null)
                {
                    Vector2 friend24Origin = new Vector2(friend24Frame.Width / 2f, friend24Frame.Height);
                    spriteBatch.Draw(friend24Frame, new Vector2(friendX, npcY), null, Color.White, 0f, friend24Origin, spriteScale, SpriteEffects.None, 0f);
                }

                if (!scene24OwlGone && owl24Frame != null)
                {
                    float fadeProgress = scene24OwlFadeActive
                        ? MathHelper.Clamp(scene24OwlFadeTimer / Scene24OwlFadeDuration, 0f, 1f)
                        : 0f;
                    float alpha = 1f - fadeProgress;
                    Vector2 owl24Origin = new Vector2(owl24Frame.Width / 2f, owl24Frame.Height);
                    spriteBatch.Draw(owl24Frame, new Vector2(owlX, npcY), null, Color.White * alpha, 0f, owl24Origin, spriteScale, SpriteEffects.FlipHorizontally, 0f);
                }

                return;
            }

            if (currentSceneId == 31)
            {
                Texture2D? friend31Frame = friendSadAnimation?.CurrentFrame;
                Texture2D? owl31Frame = owlFlyAnimation?.CurrentFrame;
                if (owl31Frame == null)
                    return;

                float friend31X = _graphics.PreferredBackBufferWidth * 0.82f;
                float owl31X = _graphics.PreferredBackBufferWidth * 0.92f;
                Vector2 owl31Origin = new Vector2(owl31Frame.Width / 2f, owl31Frame.Height);

                if (!scene31FriendGone && friend31Frame != null)
                {
                    float fadeProgress = scene31FriendFadeActive
                        ? MathHelper.Clamp(scene31FriendFadeTimer / Scene31FriendFadeDuration, 0f, 1f)
                        : 0f;
                    float alpha = 1f - fadeProgress;
                    Vector2 friend31Origin = new Vector2(friend31Frame.Width / 2f, friend31Frame.Height);
                    spriteBatch.Draw(friend31Frame, new Vector2(friend31X, npcY), null, Color.White * alpha, 0f, friend31Origin, spriteScale, SpriteEffects.FlipHorizontally, 0f);
                }

                spriteBatch.Draw(owl31Frame, new Vector2(owl31X, npcY), null, Color.White, 0f, owl31Origin, spriteScale, SpriteEffects.FlipHorizontally, 0f);
                return;
            }

            Texture2D? friendFrame = friendSadAnimation?.CurrentFrame;
            Texture2D? owlPartyFrame = owlFlyAnimation?.CurrentFrame;
            if (friendFrame == null || owlPartyFrame == null)
                return;

            Vector2 partyFriendOrigin = new Vector2(friendFrame.Width / 2f, friendFrame.Height);
            Vector2 partyOwlOrigin = new Vector2(owlPartyFrame.Width / 2f, owlPartyFrame.Height);

            spriteBatch.Draw(friendFrame, new Vector2(friendX, npcY), null, Color.White, 0f, partyFriendOrigin, spriteScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(owlPartyFrame, new Vector2(owlX, npcY), null, Color.White, 0f, partyOwlOrigin, spriteScale, SpriteEffects.FlipHorizontally, 0f);

            // (scene 22 handled above)
        }

        private float GetDoorSceneNpcX()
        {
            return _graphics.PreferredBackBufferWidth * 0.72f;
        }

        private void DrawDoorSceneItem(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (!IsDoorScene(currentSceneId) || !showDoorItemPopup || dialogueFont == null)
                return;

            Texture2D? itemTexture = GetDoorSceneItemTexture(currentDoorItemToken);
            if (itemTexture == null)
                return;

            spriteBatch.Draw(ground, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.7f);
            spriteBatch.Draw(itemTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);

            string dismissText = "Press ENTER to Continue";
            Vector2 dismissSize = dialogueFont.MeasureString(dismissText);
            float dismissX = (screenWidth - dismissSize.X) / 2f;
            float dismissY = screenHeight - dismissSize.Y - 24f;
            spriteBatch.Draw(ground, new Rectangle(0, (int)dismissY - 12, screenWidth, (int)dismissSize.Y + 24), Color.Black * 0.75f);
            spriteBatch.DrawString(dialogueFont, dismissText, new Vector2(dismissX, dismissY), Color.Yellow);
        }

        private Animation? GetDoorSceneNpcAnimation(int sceneId)
        {
            if (!doorSceneNpcKeys.TryGetValue(sceneId, out string npcKey))
                return null;

            return npcKey switch
            {
                "chef" => chefAnimation,
                "keeper" => keeperAnimation,
                "gamer" => gamerAnimation,
                "addict" => addictAnimation,
                "host" => hostAnimation,
                "media" => mediaAnimation,
                _ => null
            };
        }

        private Texture2D? GetDoorSceneItemTexture(string dialogueText)
        {
            string normalizedToken = dialogueText?.Trim().ToUpperInvariant() ?? string.Empty;

            return normalizedToken switch
            {
                "ITEM1" => item1Texture,                
                "ITEM2" => item2Texture,
                "ITEM3" => item3Texture,
                "ITEM4" => item4Texture,
                "ITEM5" => item5Texture,
                "ITEM6" => item6Texture,
                _ => null
            };
        }

        private int GetMapOffsetX()
        {
            Dictionary<Vector2, int> currentMap = GetSceneMap(currentSceneId);
            int display_tilesize = GetTopDownDisplayTileSize(currentMap);
            int minX = currentMap.Keys.Min(k => (int)k.X);
            int maxX = currentMap.Keys.Max(k => (int)k.X);
            int mapWidth = (maxX - minX + 1) * display_tilesize;
            int screenWidth = _graphics.PreferredBackBufferWidth;
            return (screenWidth - mapWidth) / 2;
        }

        private int GetMapOffsetY()
        {
            Dictionary<Vector2, int> currentMap = GetSceneMap(currentSceneId);
            int display_tilesize = GetTopDownDisplayTileSize(currentMap);
            int minY = currentMap.Keys.Min(k => (int)k.Y);
            int maxY = currentMap.Keys.Max(k => (int)k.Y);
            int mapHeight = (maxY - minY + 1) * display_tilesize;
            int screenHeight = _graphics.PreferredBackBufferHeight;
            return (screenHeight - mapHeight) / 2;
        }

        private int GetTopDownDisplayTileSize(Dictionary<Vector2, int> currentMap)
        {
            if (currentMap.Count == 0)
                return 64;

            int minX = currentMap.Keys.Min(k => (int)k.X);
            int minY = currentMap.Keys.Min(k => (int)k.Y);
            int maxX = currentMap.Keys.Max(k => (int)k.X);
            int maxY = currentMap.Keys.Max(k => (int)k.Y);

            int tilesWide = (maxX - minX + 1);
            int fillWidth = Math.Max(16, (int)Math.Ceiling(_graphics.PreferredBackBufferWidth / (float)Math.Max(1, tilesWide)));

            // Top-down scenes should always fill the screen horizontally.
            // Top and bottom may crop if needed, but the side edges should never show gaps.
            return fillWidth;
        }
    }
}

