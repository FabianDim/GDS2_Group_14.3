using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Fusion;
using _Experimenation.K.Game_Manager.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.Fabian.Scripts.PhaseController;

[InitializeOnLoad]
public static class LiveBuyMenuCheck
{
    const string Request = "Temp/live-buy-check.request";
    static string lastState;
    static double nextCheck;
    static Vector3 hostPosition;
    static LiveBuyMenuCheck() { EditorApplication.update += Update; }
    static void Update()
    {
        if (EditorApplication.isPlaying && EditorApplication.timeSinceStartup > nextCheck)
        {
            nextCheck = EditorApplication.timeSinceStartup + 0.25;
            var live = GameData.Instance;
            if (live != null && live.Object != null && live.Object.IsValid)
            {
                var snapshot = $"round={live.CurrentRound} P1={live.P1Data.Points} P2={live.P2Data.Points} " +
                    string.Join(",", live.PurchasedAbilities.Select(p => p.Key + ":" + p.Value)) + " objects=" +
                    string.Join(",", UnityEngine.Object.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None)
                        .Where(o => o.name.Contains("Smoke") || o.name.Contains("Banana")).Select(o => o.name + ":" + o.InputAuthority));
                if (snapshot != lastState) { File.AppendAllText("Temp/live-buy-history.txt", DateTime.Now.ToString("HH:mm:ss") + " " + snapshot + "\n"); lastState = snapshot; }
            }
        }
        if (!File.Exists(Request) || EditorApplication.isCompiling) return;
        var command = File.ReadAllText(Request).Trim();
        File.Delete(Request);
        try
        {
            if (command == "build")
            {
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                    locationPathName = "Temp/BuyMenuQA/Libero.exe", target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development });
                File.WriteAllText("Temp/live-buy-build.txt", report.summary.result + " errors=" + report.summary.totalErrors);
                return;
            }
            var data = GameData.Instance;
            if (data == null || data.Object == null || !data.Object.IsValid) throw new Exception("No live GameData");
            var phase = UnityEngine.Object.FindAnyObjectByType<PhaseManager>();
            if (command == "run")
                typeof(PhaseManager).GetProperty("IsRunPhase").SetValue(phase, (NetworkBool)true);
            if (command == "round") EventBus.Raise(new RoundOverEvent(true));
            if (command == "zone-out")
            {
                var host = GameManager.SpawnedPlayers[data.Runner.LocalPlayer];
                hostPosition = host.transform.position;
                host.GetComponent<Fusion.Addons.SimpleKCC.SimpleKCC>().SetPosition(hostPosition + Vector3.right * 20);
            }
            if (command == "zone-in") GameManager.SpawnedPlayers[data.Runner.LocalPlayer].GetComponent<Fusion.Addons.SimpleKCC.SimpleKCC>().SetPosition(hostPosition);
            if (command == "cube")
            {
                var client = GameManager.SpawnedPlayers.First(p => p.Key != data.Runner.LocalPlayer).Value;
                var prefabPath = AssetDatabase.FindAssets("t:Prefab Token").Select(AssetDatabase.GUIDToAssetPath)
                    .First(p => AssetDatabase.LoadAssetAtPath<GameObject>(p).GetComponent<_Experimenation.K.Cube_Tokens.Scripts.Token>() != null);
                data.Runner.Spawn(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), client.transform.position);
            }
            if (command == "report")
            {
                var text = $"round={data.CurrentRound} run={phase.IsRunPhase} remaining={phase.BuyTimeRemaining} cursor={Cursor.lockState}\n";
                text += $"P1={data.P1Data.Points} P2={data.P2Data.Points}\n";
                foreach (var pair in GameManager.SpawnedPlayers)
                {
                    data.PurchasedAbilities.TryGet(pair.Key, out var mask);
                    text += $"{pair.Key} mask={mask} position={pair.Value.transform.position}\n";
                    var movement = pair.Value.GetComponent<_Experimenation.Fraser.Scripts.PlayerMovement>();
                    foreach (var field in movement.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Where(f=>f.Name.Contains("Multiplier") || f.Name.Contains("Boost")))
                        text += $"  {field.Name}={field.GetValue(movement)}\n";
                    foreach (var property in movement.GetType().GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Where(p=>p.Name.Contains("Multiplier") || p.Name.Contains("JumpForce") || p.Name.Contains("SprintSpeed")))
                        text += $"  {property.Name}={property.GetValue(movement)}\n";
                }
                text += "Network objects: " + string.Join(", ", UnityEngine.Object.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None).Select(o=>o.name+" owner="+o.InputAuthority));
                File.WriteAllText("Temp/live-buy-report.txt", text);
            }
        }
        catch (Exception e) { File.WriteAllText("Temp/live-buy-error.txt", e.ToString()); }
    }
}
