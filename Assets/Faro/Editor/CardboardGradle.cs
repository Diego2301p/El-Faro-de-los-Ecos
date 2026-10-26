#if UNITY_ANDROID
using System.IO;
using UnityEditor.Android;
using UnityEditor.Build;
namespace Faro.Editor
{
    // Añade las dependencias oficiales a la copia Gradle generada por Unity.
    // Evita reemplazar las plantillas del editor por otras de una versión distinta.
    public class CardboardGradle : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 100;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string gradle=Path.Combine(path,"build.gradle");
            string text=File.ReadAllText(gradle);
            string[] dependencies={"androidx.appcompat:appcompat:1.6.1",
                "com.google.android.gms:play-services-vision:20.1.3",
                "com.google.android.material:material:1.12.0",
                "com.google.protobuf:protobuf-javalite:3.19.4"};
            int index=text.IndexOf("dependencies {");
            if(index<0) throw new BuildFailedException("No se encontró dependencies en unityLibrary/build.gradle.");
            index+="dependencies {".Length;
            foreach(string dep in dependencies)
                if(!text.Contains(dep))text=text.Insert(index,"\n    implementation '"+dep+"'");
            File.WriteAllText(gradle,text);
            string properties=Path.Combine(Directory.GetParent(path).FullName,"gradle.properties");
            string props=File.Exists(properties)?File.ReadAllText(properties):"";
            foreach(string key in new[]{"android.useAndroidX","android.enableJetifier"})
            {
                props=System.Text.RegularExpressions.Regex.Replace(props,@"(?m)^\s*"+System.Text.RegularExpressions.Regex.Escape(key)+@"\s*=.*\r?\n?","");
                props+="\n"+key+"=true\n";
            }
            File.WriteAllText(properties,props);
        }
    }
}
#endif
