using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
namespace Faro.Editor
{
    [InitializeOnLoad]
    public static class FaroSetup
    {
        private const string Ready = "ProjectSettings/FaroGenerado.txt";
        static FaroSetup() { EditorApplication.delayCall += FirstOpen; }
        private static void FirstOpen()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            { EditorApplication.delayCall += FirstOpen; return; }
            if (Application.isBatchMode || File.Exists(Ready)) return;
            try
            {
                FaroSceneBuilder.Build();
                Configure();
                File.WriteAllText(Ready, "1.0\nEscena creada por Unity " + Application.unityVersion);
                EditorUtility.DisplayDialog("El faro de los ecos", "Escenario preparado.\n\n1. Pulsa Play.\n2. Mantén botón derecho y mueve el mouse.\n3. Mira el cristal hasta completar el aro.\n\nEl menú Faro permite configurar Android y generar el APK.", "Comenzar");
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        [MenuItem("Faro/2 - Configurar Android y Cardboard")]
        public static void Configure()
        {
            PlayerSettings.companyName = "ProyectoAcademico";
            PlayerSettings.productName = "El faro de los ecos";
            PlayerSettings.bundleVersion = "1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"cl.diegoahumada.farodelosecos");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)35;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.optimizedFramePacing = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
            // Unity 6 añade este enum; reflexión permite un diagnóstico claro si cambia el editor.
            var entry = typeof(PlayerSettings.Android).GetProperty("applicationEntry");
            if (entry == null) throw new InvalidOperationException("Se requiere Unity 6 con Application Entry Point = Activity.");
            entry.SetValue(null, Enum.Parse(entry.PropertyType,"Activity"));
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input != null && input.intValue != 1) { input.intValue = 1; settings.ApplyModifiedPropertiesWithoutUndo(); }
            QualitySettings.antiAliasing = 2; QualitySettings.vSyncCount = 0;
            QualitySettings.shadows = ShadowQuality.Disable;
            ConfigureXR();
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            tags.FindProperty("layers").GetArrayElementAtIndex(8).stringValue = "Interactive";
            tags.ApplyModifiedPropertiesWithoutUndo();
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorUserBuildSettings.buildAppBundle = false;
            AssetDatabase.SaveAssets();
            Debug.Log("FARO: Android ARM64 / IL2CPP / OpenGLES3 / Activity / Cardboard configurados. Usa SDK objetivo 35 o superior instalado.");
        }
        private static void ConfigureXR()
        {
            XRGeneralSettingsPerBuildTarget perTarget;
            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,out perTarget) || !perTarget)
            {
                perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                if (!AssetDatabase.IsValidFolder("Assets/XR")) AssetDatabase.CreateFolder("Assets","XR");
                AssetDatabase.CreateAsset(perTarget,"Assets/XR/FaroXRSettings.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,perTarget,true);
            }
            if (!perTarget.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
                perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            var android = perTarget.SettingsForBuildTarget(BuildTargetGroup.Android);
            android.InitManagerOnStart = true;
            if (!XRPackageMetadataStore.AssignLoader(android.Manager,"Google.XR.Cardboard.XRLoader",BuildTargetGroup.Android))
                throw new InvalidOperationException("No se pudo activar Cardboard. Revisa XR Plug-in Management > Android.");
            EditorUtility.SetDirty(android.Manager); EditorUtility.SetDirty(android); EditorUtility.SetDirty(perTarget);
        }
        [MenuItem("Faro/3 - Revisar escena")]
        public static void ValidateScene()
        {
            var mission=UnityEngine.Object.FindFirstObjectByType<MissionController>();
            if (!mission) throw new InvalidOperationException("Abre Assets/Faro/Scenes/ElFaro.unity.");
            if (!mission.playerRoot || !mission.head || !mission.fade || !mission.dockDialogue || !mission.towerDialogue ||
                !mission.crystal || !mission.socket || !mission.lensPivot || !mission.beam || !mission.finalPanel ||
                !mission.objective || !mission.effects || !mission.powerClip || !mission.tickClip || !mission.successClip ||
                !mission.dockAmbient || !mission.towerAmbient || !mission.boat || !mission.beaconRotor || !mission.lensReadout)
                throw new InvalidOperationException("Faltan referencias en MissionController.");
            int count=0;
            foreach(var target in UnityEngine.Object.FindObjectsByType<GazeTarget>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(!target.mission || !target.GetComponent<Collider>()) throw new InvalidOperationException("Referencia o collider faltante: "+target.name);
                if(target.action==TargetAction.Teleport)
                {
                    count++;
                    if(!target.destination) throw new InvalidOperationException("Destino vacío: "+target.name);
                    var p=target.destination.position;
                    if(!Physics.Raycast(p+Vector3.up*.3f,Vector3.down,.7f)) throw new InvalidOperationException("Destino sin suelo: "+target.name);
                    if(Physics.CheckCapsule(p+Vector3.up*.45f,p+Vector3.up*1.65f,.25f,~(1<<2)))
                        throw new InvalidOperationException("Destino ocupado: "+target.name);
                }
            }
            if(count<3) throw new InvalidOperationException("Faltan destinos de teletransporte.");
            Debug.Log("FARO: referencias y "+count+" destinos revisados. Falta validar recorrido y visión en el dispositivo.");
        }
        [MenuItem("Faro/4 - Generar APK")]
        public static void BuildAPK()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
            { EditorUtility.DisplayDialog("Faltan módulos Android","En Unity Hub > Installs > tu Unity > Add modules, instala Android Build Support, SDK & NDK Tools y OpenJDK.","Entendido");return; }
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android);
                Debug.Log("Cuando termine el cambio de plataforma, vuelve a Faro > 4 - Generar APK.");return;
            }
            Configure();
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            EditorSceneManager.OpenScene(FaroSceneBuilder.ScenePath); ValidateScene();
            Directory.CreateDirectory("Builds");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{FaroSceneBuilder.ScenePath},target=BuildTarget.Android,
                locationPathName="Builds/ElFaro_DiegoAhumada.apk",options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded)throw new BuildFailedException("APK no generado. Lee el primer error rojo de Console.");
            EditorUtility.RevealInFinder("Builds/ElFaro_DiegoAhumada.apk");
        }
    }
}
