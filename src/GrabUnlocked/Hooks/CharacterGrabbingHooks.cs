using MonoDetour;
using pworld.Scripts;
using System;
using System.Collections.Generic;
using System.Text;
using MonoDetour.HookGen;
using On.CharacterGrabbing;
using MonoDetour.Cil;
using MonoMod.Cil;
using UnityEngine;
using Mono.Cecil.Cil;

namespace GrabUnlocked.Hooks;


[MonoDetourTargets(typeof(CharacterGrabbing))]
static class GameHandlerHooks
{

    [MonoDetourHookInitialize]
    static void Init()
    {
        Reach.ILHook(ILHook_Reach);
    }

    static void ILHook_Reach(ILManipulationInfo info)
    {
        ILWeaver weaver = new(info);

        // Change the maximum grab distance to the value set in the config
        weaver
            .MatchRelaxed(
                x => x.MatchLdloc(2) && weaver.SetCurrentTo(x),
                x => x.MatchLdcR4(4f),
                x => x.MatchBgt(out _)
            )
            .ThrowIfFailure()
            .Current.Operand = Plugin.MaxGrabDistance;

        // Skip the check for whether the to-be-grabbed player is climbing or not, if the config option is enabled
        if (Plugin.CanGrabWhenNotClimbing)
        {
            Instruction firstMatch = null!;
            Instruction lastMatch = null!;

            weaver
               .MatchRelaxed(
                   x => x.MatchLdloc(1) && weaver.SetInstructionTo(ref firstMatch, x),
                   x => x.MatchLdfld<Character>(nameof(Character.data)),
                   x => x.MatchLdfld<CharacterData>(nameof(CharacterData.isClimbing)),
                   x => x.MatchBrfalse(out _) && weaver.SetInstructionTo(ref lastMatch, x)
               )
               .ThrowIfFailure()
               .InsertBefore(firstMatch, weaver.Create(OpCodes.Br, lastMatch.Next));
        }

        // Skip the check for whether the player y position is higher than the to-be-grabbed player or not, if the config option is enabled
        if (Plugin.CanGrabFromAnyHeight)
        {
            Instruction firstMatch = null!;
            Instruction lastMatch = null!;

            weaver
               .MatchRelaxed(
                   x => x.MatchLdloc(1) && weaver.SetInstructionTo(ref firstMatch, x),
                   x => x.MatchCallvirt<Character>("get_" + nameof(Character.Center)),
                   x => x.MatchLdfld<Vector3>(nameof(Vector3.y)),
                   x => x.MatchLdarg(0),
                   x => x.MatchLdfld<CharacterGrabbing>(nameof(CharacterGrabbing.character)),
                   x => x.MatchCallvirt<Character>("get_" + nameof(Character.Center)),
                   x => x.MatchLdfld<Vector3>(nameof(Vector3.y)),
                   x => x.MatchBgt(out _) && weaver.SetInstructionTo(ref lastMatch, x)
               )
               .ThrowIfFailure()
               .InsertBefore(firstMatch, weaver.Create(OpCodes.Br, lastMatch.Next));
        }
    }
}
