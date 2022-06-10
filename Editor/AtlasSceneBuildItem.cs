using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Atlas;
using BepInEx;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
#if H3VR_IMPORTED
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;
#endif

namespace MeatKit
{
    [CreateAssetMenu(menuName = "MeatKit/Build Items/Atlas Scene", fileName = "New scene")]
    public class AtlasSceneBuildItem : BuildItem
    {
        [Tooltip("Drag and drop your scene asset file here.")]
        public SceneAsset SceneFile;

        [Tooltip("Give a name to your scene")]
        public string SceneName;

        [Tooltip("The game mode this scene uses")]
        public string GameMode;
        
        [FormerlySerializedAs("Mode")]
        [Tooltip("Where the user will be able to load your scene")]
        public string DisplayMode;

        [Tooltip("Your scene thumbnail / preview")]
        public Texture2D Thumbnail;

        [Tooltip("Your name")]
        public string Author;

        [Tooltip("The scene's description text")]
        [TextArea]
        public string Description;

        public override IEnumerable<string> RequiredDependencies
        {
            get
            {
                IEnumerable<string> baseRequirements = new[] {"nrgill28-Atlas-1.0.0"};

                AtlasGameModeDefinition def;
                if (AtlasGameModeDefinition.AllGameModes.TryGetValue(GameMode, out def))
                    baseRequirements = baseRequirements.Concat(def.ExtraDependencies);
                
                return baseRequirements;
            }
        }

        public override Dictionary<string, BuildMessage> Validate()
        {
            var messages = base.Validate();

            if (string.IsNullOrEmpty(SceneName))
                messages["SceneName"] = BuildMessage.Error("Scene name cannot be empty");
            if (!Thumbnail) messages["Thumbnail"] = BuildMessage.Warning("Scene is missing a thumbnail.");


            var knownModes = AtlasGameModeDefinition.AllGameModes;
            if (string.IsNullOrEmpty(GameMode))
                messages["GameMode"] = BuildMessage.Error("Game mode cannot be empty.");
            else if (!knownModes.ContainsKey(GameMode))
                messages["GameMode"] = BuildMessage.Warning("Game mode unknown in project.");
            
            if (string.IsNullOrEmpty(DisplayMode))
                messages["DisplayMode"] = BuildMessage.Error("Display mode cannot be empty.");
            else if (!knownModes.ContainsKey(DisplayMode) || !knownModes[DisplayMode].AlsoADisplayMode)
                messages["DisplayMode"] = BuildMessage.Warning("Display mode unknown in project.");
            
            return messages;
        }

        public override List<AssetBundleBuild> ConfigureBuild()
        {
#if H3VR_IMPORTED
            // We need to export the thumbnail and scene metadata
            var sceneFileName = BuildWindow.SelectedProfile.ExportPath + SceneFile.name.ToLower();
            File.Copy(AssetDatabase.GetAssetPath(Thumbnail), sceneFileName + ".png");
            var obj = new JObject();
            obj["DisplayName"] = SceneName;
            obj["Identifier"] = SceneFile.name;
            obj["DisplayMode"] = DisplayMode;
            obj["GameMode"] = GameMode;
            obj["Author"] = Author;
            obj["Description"] = Description;
            File.WriteAllText(sceneFileName + ".json", JsonConvert.SerializeObject(obj));
#endif
            // Return the configuration to build the scene bundle file
            List<AssetBundleBuild> bundles = new List<AssetBundleBuild>
            {
                new AssetBundleBuild
                {
                    assetBundleName = SceneFile.name,
                    assetNames = new[] { AssetDatabase.GetAssetPath(SceneFile) }
                }
            };

            return bundles;
        }

        public override void GenerateLoadAssets(TypeDefinition plugin, ILProcessor il)
        {
#if H3VR_IMPORTED
            EnsurePluginDependsOn(plugin, AtlasConstants.Guid, AtlasConstants.Version);
            
            /*
             * We need to add this line: AtlasPlugin.RegisterScene(Path.Combine(BasePath, "scene name"))
             * Which translates to this IL:
             *  ldsfld  string MeatKitPlugin::BasePath
             *  ldstr   "scene name"
             *  call    string [mscorlib]System.IO.Path::Combine(string, string)
             *  call    string [Atlas]Atlas.AtlasPlugin::RegisterScene(string)
             */

            // Get some references
            var publicStatic = BindingFlags.Public | BindingFlags.Static;
            FieldReference basePath = plugin.Fields.First(f => f.Name == "BasePath");
            var pathCombine = typeof(Path).GetMethod("Combine", publicStatic);
            var atlasRegisterScene = typeof(AtlasPlugin).GetMethod("RegisterScene", publicStatic);

            // Emit our opcodes
            il.Emit(OpCodes.Ldsfld, basePath);
            il.Emit(OpCodes.Ldstr, SceneFile.name.ToLower());
            il.Emit(OpCodes.Call, plugin.Module.ImportReference(pathCombine));
            il.Emit(OpCodes.Call, plugin.Module.ImportReference(atlasRegisterScene));
#endif
        }
    }
}
