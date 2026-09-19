using System.Collections.Generic;
using ImmersiveVRTools.Editor.Common.WelcomeScreen;
using ImmersiveVRTools.Editor.Common.WelcomeScreen.GuiElements;
using ImmersiveVRTools.Editor.Common.WelcomeScreen.PreferenceDefinition;
using ImmersiveVRTools.Editor.Common.WelcomeScreen.Utilities;
using TransformChangesDebugger.API;
using TransformChangesDebugger.Editor;
using UnityEditor;
using UnityEngine;

public class TransformChangesDebuggerWelcomeScreen : ProductWelcomeScreenBase
{
    public static string BaseUrl = "https://immersivevrtools.com";
    public static string GenerateGetUpdatesUrl(string userId, string versionId)
    {
        return $"{BaseUrl}/updates/transform-changes-debugger/{userId}?CurrentVersion={versionId}";
    }
    public static string VersionId = "1.0";
    private static readonly string ProjectIconName = "ProductIcon64";
    public static readonly string ProjectName = "transform-changes-debugger";

    private static Vector2 _WindowSizePx = new Vector2(650, 500);
    private static string _WindowTitle = "Visual Transform Changes Debugger";
    
    private static readonly string QuickStartSectionName = "Quick Start";
    
    private static readonly LaunchSceneButton LaunchHardToPinpointJitterSceneButton = new LaunchSceneButton("Hard to pinpoint jitter", (s) => GetScenePath("HardToPinpointIntermittentJitter"),
        (screen) =>
        {
            GUILayout.Label(
                @"In that scenario you have a cube that's rotating in circular motion. Unfortunately from time to time it'll jitter on screen. 

<b>Following steps will introduce you to core workflow that can be used to easily debug this kind of issues.</b>

1) Press play to see what's happening

2) Select tracked object that's having issues

3) Open debugger window and 'View Changes' once the issue happens

4) From here it's easiest to see which TransformModifiers (right sidebar) are changing tracked object

5) You can also drag Frame Selection (top bar) to show more frames and maybe view different sections of captured data

6) At some point you should see FrustratingDisruptor-1 that's in TransformModifiers

7) Click on three dots menu next to TransformModifier and hit 'Show next' - that'll bring the change to view

8) Now that you can see it it'd be beneficial to focus just on ChangeType that you're interested in, in that case position, untick 'rotation' from top bar

9) Let's see those changes, go ahead and click 'preview' on top bar, now just click on change nodes from left to right going frame by frame

10) You'll see that game object in scene view is moving to selected change

11) Now go ahead and click FrustratingDisruptor change as part of your preview, you should see that it's moving the transform far away from it's normal circular motion

12) Lets confirm that's the issue, tick 'Disable Changes' checkbox next to FrustratingDisruptor modifier

13) Unpause and confirm that jitter is indeed gone (and once confirmed you can reenable that change just to confirm that issue is back)

14) That's it, you've now narrowed down the issue to specific component / script and method

15) You can right click on FrustratingDisruptor change node header and select 'Goto code' - it should now be clear why this happened to begin with
", screen.TextStyle);
            
            if (GUILayout.Button($"Next goto to '{Launch3rdPartyCodeCausingIssuesSceneButton.Text}' demo", screen.ButtonStyle))
            {
                Launch3rdPartyCodeCausingIssuesSceneButton.OnClick(screen);
            }
        });
    
    private static readonly LaunchSceneButton Launch3rdPartyCodeCausingIssuesSceneButton = new LaunchSceneButton("3rd Party Code Causing Issues", (s) => GetScenePath("ThirdPartyToolCausingIssues"),
        (screen) =>
        {
            GUILayout.Label(
                @"This scenario will present 3rd party code that's causing the issue and how to debug that. Scenario assumes you've already seen 1st demo scene 'Hard to pinpoint jitter'.

1) Hit play and you'll see transform moves in circular motion but jitters quite badly

2) As previously select affected object and click 'View changes'

3) You're going to see that every 3rd frame there's a node 'Position Mismatch' in the graph - that suggest some code is making changes but it's not captured

4) In the 'Assemblies' bottom bar you'll find all assemblies that are available, in 'Third Party' group find 'ThirdPartyTool.dll' and select checkbox next to it

5) Now that assembly has been processed - unpause, let it go for a second and pause again

6) Debugger window will let you know that view is out of date, that is due to new frame data that's captured. Simply hit 'Refresh' button

7) You'll now see that 'Position Mismatch' node is gone and in it's place there's a change being made via 3rd party script

<b>That's more or less all you need to know to get most of the tool, if you have some more complex scenario have a look at 'Advanced API Usage' demo scene and documentation section.</b>

", screen.TextStyle);

            if (GUILayout.Button($"Goto to '{LaunchAdvancedAPIUsageSceneButton.Text}' demo", screen.ButtonStyle))
            {
                LaunchAdvancedAPIUsageSceneButton.OnClick(screen);
            }
        });

    private static readonly LaunchSceneButton LaunchAdvancedAPIUsageSceneButton = new LaunchSceneButton("Advanced API Usage", (s) => GetScenePath("AdvancedAPIUsage"), (screen) =>
    {
        GUILayout.Label(
@"This demo is for more complex scenarios and shows what's possible using API.

Have a look at components in 'APIUsageExamples' game object.

<b>CustomCallback</b>
This will show how you can register your custom callback that'll be executed whenever change to position/rotation/scale of specified object occurs

<b>PositionChangeHandler</b>
Similar to above - although you can set it up as UnityEvent. In our example 'TrackTransformChanges' PositionChange event for 'TrackedObject-1' is linked with this handler

<b>SkipSpecificChanges</b>
This will allow you to specify custom code when change should be skipped, you have access to all change details.

",  screen.TextStyle);
    });

    private static readonly List<GuiSection> LeftSections = new List<GuiSection>() {
        new GuiSection("", new List<ClickableElement>
        {
            new LastUpdateButton("New Update!", (screen) => LastUpdateUpdateScrollViewSection.RenderMainScrollViewSection(screen)),
            new ChangeMainViewButton("Welcome", (screen) => MainScrollViewSection.RenderMainScrollViewSection(screen)),
            new ChangeMainViewButton(QuickStartSectionName, (screen) => QuickStartScrollViewSection.RenderMainScrollViewSection(screen))
        }),
        new GuiSection("Options", new List<ClickableElement>
        {
            new ChangeMainViewButton("General", (screen) =>
            {
                GUILayout.Label(
                    @"By default tool will capture changes from last 1000 frames. You can extend that if it's not enough - bear in mind this will affect performance.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderGuiAndPersistInput(TransformChangesDebuggerPreference
                        .KeepChangesDataForMaximumNumberOfFramesPreferenceDefinition);
                }

                const int sectionBreakHeight = 15;
                GUILayout.Space(sectionBreakHeight);

                GUILayout.Label(
                    "You can see 100 change nodes on main graph by default. As above viewing more at same time can affect performance. In most cases you should be " +
                    "able to use top navigation bar to easily change visible frames.",
                    screen.TextStyle
                );

                using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderGuiAndPersistInput(TransformChangesDebuggerPreference.MaxAllowedFramesToShowOnScreenPreferenceDefinition);
                }
                
                GUILayout.Space(sectionBreakHeight);
                GUILayout.Label(
                    "Assemblies tab 'User Code' can be controlled via this setting for convenience. You can specify what should be included. Put what your DLLs start with " +
                    "- eg \n'Assembly-CSharp.dll;your.custom.dll;your.namespace.'" +
                    "\n\n You can specify more than one by using ; as a delimiter",
                    screen.TextStyle
                );
                
                using (LayoutHelper.LabelWidth(200))
                {
                    ProductPreferenceBase.RenderGuiAndPersistInput(TransformChangesDebuggerPreference.UserCodeDllsStartWithSemicolonDelimitedPreferenceDefinition);
                }
            })
        }),
        new GuiSection("Launch Demo", new List<ClickableElement>
        {
            LaunchHardToPinpointJitterSceneButton,
            Launch3rdPartyCodeCausingIssuesSceneButton,
            LaunchAdvancedAPIUsageSceneButton,
        })
    };

    private static readonly string RedirectBaseUrl = "https://immersivevrtools.com/redirect/transform-changes-debugger"; 
    private static readonly GuiSection TopSection = new GuiSection("", new List<ClickableElement>
        {
            new OpenUrlButton("Documentation", $"{RedirectBaseUrl}/documentation"),
            new OpenUrlButton("Unity Forum", $"{RedirectBaseUrl}/unity-forum"),
            new OpenUrlButton("Contact", $"{RedirectBaseUrl}/contact")
        }
    );

    private static readonly GuiSection BottomSection = new GuiSection(
        "I want to make this tool better. And I need your help!",
        $"It'd be great if you could share your feedback (good and bad) with me. I'm very keen to make this tool better and that can only happen with your help. Please use:",
        new List<ClickableElement>
        {
            new OpenUrlButton(" Unity Forum", $"{RedirectBaseUrl}/unity-forum"),
            new OpenUrlButton(" or Contact Me Directly", $"{RedirectBaseUrl}/contact"),
        }
    );

    private static GameObject SelectedGameObject;
    private static readonly ScrollViewGuiSection QuickStartScrollViewSection = new ScrollViewGuiSection(
        "", (screen) =>
        {
            GUILayout.Label(
@"It's really simple to start tracking.

1) Select Game Object that you want to track

2) Add new component 'Transform Debugger -> Track Transform Changes'

3) Press 'Open Visual Transform Changes Debugger Window'

4) Play

5) If there are any changes made to your object you'll be able to click 'View Changes' in inspector

<b>I've put together simple examples that'll introduce you to the features and will help you get most of the tool.</b>
", screen.TextStyle);
            
            if (GUILayout.Button($"Goto to '{LaunchHardToPinpointJitterSceneButton.Text}' demo", screen.ButtonStyle))
            {
                LaunchHardToPinpointJitterSceneButton.OnClick(screen);
            }
        }
    );
    
    private static readonly ScrollViewGuiSection MainScrollViewSection = new ScrollViewGuiSection(
        "", (screen) =>
        {
            GenerateCommonWelcomeText(TransformChangesDebuggerPreference.ProductName, screen);

            GUILayout.Label(
                @"It's really simple to get started with the tracking",
                screen.BoldTextStyle
            );
            if (GUILayout.Button("Goto to Quick Start section", screen.ButtonStyle))
            {
                screen.ChangeMainScrollViewRenderFn(QuickStartScrollViewSection.RenderMainScrollViewSection, QuickStartSectionName);
            };
            GUILayout.Space(30);
            
            GUILayout.Label("Quick adjustments:", screen.LabelStyle);
            using (LayoutHelper.LabelWidth(220))
            {
                ProductPreferenceBase.RenderGuiAndPersistInput(TransformChangesDebuggerPreference.MaxAllowedFramesToShowOnScreenPreferenceDefinition);
                ProductPreferenceBase.RenderGuiAndPersistInput(TransformChangesDebuggerPreference.KeepChangesDataForMaximumNumberOfFramesPreferenceDefinition);
                ProductPreferenceBase.RenderGuiAndPersistInput(TransformChangesDebuggerPreference.UserCodeDllsStartWithSemicolonDelimitedPreferenceDefinition);
            }
        }
    );

    private static readonly ScrollViewGuiSection LastUpdateUpdateScrollViewSection = new ScrollViewGuiSection(
        "New Update", (screen) =>
        {
            GUILayout.Label(screen.LastUpdateText, screen.BoldTextStyle, GUILayout.ExpandHeight(true));
        }
    );

    public override string WindowTitle { get; } = _WindowTitle;
    public override Vector2 WindowSizePx { get; } = _WindowSizePx;


    [MenuItem("Window/Transform Changes Debugger/Start Screen", false, 1999)]
    public static void Init()
    {
        OpenWindow<TransformChangesDebuggerWelcomeScreen>(_WindowTitle, _WindowSizePx);
    }

    public void OnEnable()
    {
        OnEnableCommon(ProjectIconName);
    }

    public void OnGUI()
    {
        RenderGUI(LeftSections, TopSection, BottomSection, MainScrollViewSection);
    }
}

public class TransformChangesDebuggerPreference : ProductPreferenceBase
{
    public const string ProductName = "Visual Transform Changes Debugger";
    private static string[] ProductKeywords = new[] { "start", "vr", "tools" };
    
    public static readonly IntProjectEditorPreferenceDefinition KeepChangesDataForMaximumNumberOfFramesPreferenceDefinition = new IntProjectEditorPreferenceDefinition(
        "Keep changes data for frames (max)", "KeepChangesDataForMaximumNumberOfFrames", 1000,
        (newValue, oldValue) =>
        {
            TransformChangesTracker.KeepChangesDataForMaximumNumberOfFrames = (int)newValue;
        },
        (value) =>
        {
            TransformChangesTracker.KeepChangesDataForMaximumNumberOfFrames = (int)value;
        });
    
    public static readonly IntProjectEditorPreferenceDefinition MaxAllowedFramesToShowOnScreenPreferenceDefinition = new IntProjectEditorPreferenceDefinition(
        "Max change nodes visible on graph", "MaxAllowedFramesToShowOnScreen", 100,
        (newValue, oldValue) =>
        {
            TransformChangesDebuggerGuiManager.MaxAllowedFramesToShowOnScreen = (int)newValue;
        },
        (value) =>
        {
            TransformChangesDebuggerGuiManager.MaxAllowedFramesToShowOnScreen = (int)value; 
        });
    
    public static readonly TextProjectEditorPreferenceDefinition UserCodeDllsStartWithSemicolonDelimitedPreferenceDefinition = new TextProjectEditorPreferenceDefinition(
        "User Code Dlls Start With", "UserCodeDllsStartWithSemicolonDelimited", "Assembly-CSharp;"
    );

    public static List<ProjectEditorPreferenceDefinitionBase> PreferenceDefinitions = new List<ProjectEditorPreferenceDefinitionBase>()
    {
        CreateDefaultShowOptionPreferenceDefinition(),
        KeepChangesDataForMaximumNumberOfFramesPreferenceDefinition,
        MaxAllowedFramesToShowOnScreenPreferenceDefinition,
    };

    private static bool PrefsLoaded = false;



#if UNITY_2019_1_OR_NEWER
    [SettingsProvider]
    public static SettingsProvider ImpostorsSettings()
    {
        return GenerateProvider(ProductName, ProductKeywords, PreferencesGUI);
    }

#else
	[PreferenceItem(ProductName)]
#endif
    public static void PreferencesGUI()
    {
        if (!PrefsLoaded)
        {
            LoadDefaults(PreferenceDefinitions);
            PrefsLoaded = true;
        }

        RenderGuiCommon(PreferenceDefinitions);
    }
}

[InitializeOnLoad]
public class TransformChangesDebuggerWelcomeScreenInitializer : WelcomeScreenInitializerBase
{
    static TransformChangesDebuggerWelcomeScreenInitializer()
    {
        var userId = ProductPreferenceBase.CreateDefaultUserIdDefinition(TransformChangesDebuggerWelcomeScreen.ProjectName).GetEditorPersistedValueOrDefault().ToString();

        HandleUnityStartup(
            TransformChangesDebuggerWelcomeScreen.Init,
            TransformChangesDebuggerWelcomeScreen.GenerateGetUpdatesUrl(userId, TransformChangesDebuggerWelcomeScreen.VersionId), 
            TransformChangesDebuggerPreference.PreferenceDefinitions,
            (isFirstRun) =>
        {

        });
    }
}