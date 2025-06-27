using BepInEx;
using BepInEx.Logging;
using MonoDetour;

namespace GrabUnlocked;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    internal static float MaxGrabDistance { get; set; }
    internal static bool CanGrabFromAnyHeight { get; set; }
    internal static bool CanGrabWhenNotClimbing { get; set; }

    private void Awake()
    {
        Log = Logger;
        
        MaxGrabDistance = Config.Bind("General", "MaxGrabDistance", 4f, "How far away you can initiate a friend grab from.").Value;
        CanGrabFromAnyHeight = Config.Bind("General", "CanGrabFromAnyHeight", true, "If true, you can initiate a friend grab whether or not you are higher up than the friend.").Value;
        CanGrabWhenNotClimbing = Config.Bind("General", "CanGrabWhenNotClimbing", true, "If true, you can initiate a friend grab even when the friend is not climbing (ie. mid-air, on the ground, etc.)").Value;

        MonoDetourManager.InvokeHookInitializers(typeof(Plugin).Assembly);

        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}
