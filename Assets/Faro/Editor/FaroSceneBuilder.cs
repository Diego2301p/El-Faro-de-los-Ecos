using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
namespace Faro.Editor
{
    // Construye y guarda una escena normal y editable; solo se ejecuta desde el editor.
    public static class FaroSceneBuilder
    {
        public const string ScenePath = "Assets/Faro/Scenes/ElFaro.unity";
        private static Material wood, dark, stone, brass, teal, glow, sea, white, red;
        private static MissionController mission;
        private static Transform dock, tower;
        private static Font font;
        private static readonly List<GameObject> powered = new List<GameObject>();

        [MenuItem("Faro/1 - Crear o reconstruir escenario")]
        public static void BuildMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Reconstruir escenario",
                "Se reemplazará ElFaro.unity por el diseño original. Guarda una copia si lo modificaste.", "Reconstruir", "Cancelar")) return;
            Build();
        }
        public static void Build()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Directory.CreateDirectory("Assets/Faro/Materials");
            powered.Clear();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            wood = Mat("Madera", "#765B4A"); dark = Mat("MetalOscuro", "#1B303E");
            stone = Mat("Piedra", "#4B5F70"); brass = Mat("Laton", "#C39B59");
            teal = Mat("Turquesa", "#238E97"); glow = Mat("LuzTurquesa", "#72FFD8", true);
            sea = Mat("Mar", "#123847"); white = Mat("Marfil", "#DAD8C3"); red = Mat("Coral", "#CE7658");
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.43f, .52f, .62f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(.07f, .15f, .21f);
            RenderSettings.fogStartDistance = 22; RenderSettings.fogEndDistance = 95;
            var moon = new GameObject("Luna_Direccional").AddComponent<Light>();
            moon.type = LightType.Directional; moon.color = new Color(.65f, .8f, 1);
            moon.intensity = 1.05f; moon.shadows = LightShadows.None;
            moon.transform.rotation = Quaternion.Euler(40, -35, 0);
            var systems = Group("Sistemas");
            mission = new GameObject("Mision_ElFaro").AddComponent<MissionController>();
            mission.transform.SetParent(systems);
            BuildPlayer(systems);
            dock = Group("Zona_1_Muelle_de_los_ecos");
            tower = Group("Zona_2_Sala_de_la_lente");
            BuildDock(); BuildTower(); BuildOcean();
            mission.powerVisuals = powered.ToArray();
            mission.effects = Audio("Efectos", systems, null, 1, false);
            mission.powerClip = Clip("Energia"); mission.tickClip = Clip("Mecanismo"); mission.successClip = Clip("Rescate");
            mission.dockAmbient = Audio("Mar_ambiente", systems, Clip("Olas"), .3f, true);
            mission.towerAmbient = Audio("Faro_ambiente", systems, Clip("Faro"), 0, true);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.LookAt(new Vector3(0, 1, 2), Quaternion.Euler(25, 0, 0), 18);
            Debug.Log("FARO: escena creada y guardada. Pulsa Play; botón derecho + mouse para mirar.");
        }
        private static void BuildPlayer(Transform parent)
        {
            var rig = Group("PlayerRoot", parent); rig.position = new Vector3(0, 0, -5);
            var cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(rig, false); cameraObject.transform.localPosition = new Vector3(0, 1.65f, 0);
            var cam = cameraObject.AddComponent<Camera>(); cam.nearClipPlane = .05f; cam.farClipPlane = 140;
            cam.fieldOfView = 70; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = RenderSettings.fogColor;
            cam.allowHDR = false; cam.allowMSAA = true;
            cameraObject.AddComponent<AudioListener>(); cameraObject.AddComponent<CardboardSession>();
            var look = rig.gameObject.AddComponent<EditorMouseLook>(); look.head = cam.transform;
            mission.playerRoot = rig; mission.head = cam.transform;
            var interactor = cameraObject.AddComponent<GazeInteractor>(); interactor.mission = mission;
            var reticle = Group("Reticula", cam.transform); reticle.localPosition = new Vector3(0, 0, 1);
            var rv = reticle.gameObject.AddComponent<ReticleVisual>();
            rv.rim = Ring("Aro", reticle, .017f, .002f, Color.white);
            rv.progress = Ring("Carga", reticle, .024f, .004f, new Color(.3f, 1, .75f));
            interactor.reticle = rv;
            var fadeCanvas = CanvasAt("FadeCanvas", cam.transform, new Vector3(0, 0, .12f), new Vector2(1000, 1000), .01f);
            fadeCanvas.overrideSorting = true; fadeCanvas.sortingOrder = 30000;
            var black = ImageAt("Negro", fadeCanvas.transform, Color.black); Stretch(black.rectTransform, 0);
            var group = fadeCanvas.gameObject.AddComponent<CanvasGroup>(); group.alpha = 1;
            var fade = fadeCanvas.gameObject.AddComponent<ScreenFadeVR>(); fade.panel = group; mission.fade = fade;
            var hud = CanvasAt("Objetivo", cam.transform, new Vector3(0, -.36f, 1.6f), new Vector2(900, 75), .001f);
            mission.objective = TextAt("ObjetivoTexto", hud.transform, "1 / 2   RESTAURA LA ENERGÍA", 29, Color.white);
        }
        private static void BuildDock()
        {
            Box("Base_muelle", dock, V(0,-.24f,1), V(12,.45f,17), wood);
            for (int z=-7; z<=9; z++) Box("Tablon_"+z, dock, V(0,.006f,z), V(12,.035f,.055f), dark, false);
            for (int z=-6; z<=8; z+=2)
                foreach (int x in new[]{-6,6})
                {
                    Box("Pilote", dock, V(x,-.2f,z), V(.23f,2.4f,.23f), wood);
                    Box("Baranda", dock, V(x,.75f,z+.7f), V(.1f,.1f,1.8f), brass, false);
                }
            // Silueta del faro y acceso: la segunda zona es una sala separada en la misma escena.
            Prim("Torre_del_faro", PrimitiveType.Cylinder, dock, V(2,5,10), V(3.6f,5,3.6f), white);
            for(int i=1;i<5;i++) Prim("Franja_faro", PrimitiveType.Cylinder,dock,V(2,i*1.8f,10),V(3.65f,.22f,3.65f),teal,false);
            Prim("Linterna_faro",PrimitiveType.Cylinder,dock,V(2,10.5f,10),V(4.1f,.6f,4.1f),brass,false);
            Prim("Techo_faro",PrimitiveType.Sphere,dock,V(2,11.3f,10),V(4.5f,.9f,4.5f),dark,false);
            Box("Puerta_faro",dock,V(2,1.3f,8.15f),V(1.4f,2.6f,.15f),dark);
            Label("EL FARO\nDE LOS ECOS",dock,V(0,3,6),new Vector2(1000,260),.004f,66,white.color);
            Label("01   /   EL MUELLE",dock,V(-3,2.6f,5),new Vector2(900,100),.0027f,48,glow.color);
            for(int i=0;i<3;i++)
            {
                Box("Caja_de_carga",dock,V(-4.4f,.45f,4+i*1.2f),V(.9f,.9f,.9f),wood);
                Box("Cincha",dock,V(-4.4f,.45f,3.53f+i*1.2f),V(.12f,.95f,.03f),brass,false);
            }
            var buoy=Prim("Boya_de_rescate",PrimitiveType.Capsule,dock,V(5,.6f,4),V(.6f,.8f,.6f),red);
            Box("Generador",dock,V(0,.6f,3),V(1.7f,1.2f,1.2f),dark);
            Box("Panel_generador",dock,V(0,1,2.35f),V(1.4f,.4f,.12f),brass);
            mission.socket=Group("Zocalo_del_cristal",dock);mission.socket.position=V(0,1.7f,3);
            var crystal=Prim("INT_Cristal",PrimitiveType.Cube,dock,V(-2,1.15f,2.5f),V(.48f,.8f,.48f),glow);
            crystal.transform.rotation=Quaternion.Euler(0,35,15);
            Target(crystal,TargetAction.Crystal,"Mantén la mirada para instalar el cristal y restaurar la energía.");
            mission.crystal=crystal.transform;
            Box("Pedestal_cristal",dock,V(-2,.35f,2.5f),V(.85f,.7f,.85f),stone);
            Label("CRISTAL",dock,V(-2,1.95f,2.5f),new Vector2(500,100),.002f,45,glow.color);
            for(int i=0;i<4;i++)
            {
                var lamp=Prim("Baliza_encendida",PrimitiveType.Sphere,dock,V(4.5f,1.3f,-3+i*2.8f),V(.23f,.23f,.23f),glow,false);
                powered.Add(lamp); lamp.SetActive(false);
                Box("Poste_baliza",dock,V(4.5f,.55f,-3+i*2.8f),V(.1f,1.1f,.1f),dark);
            }
            mission.dockDialogue=Dialogue("Dialogo_muelle",dock,V(.1f,3.35f,1),4.6f);
            Teleport("MUELLE",dock,V(1.7f,.06f,-2.7f),V(0,0,-5),0,false);
            Teleport("CRISTAL",dock,V(-2,.06f,.25f),V(-2,0,-1.3f),0,false);
            Teleport("FARO",dock,V(3,.06f,3.2f),V(40,0,-4),1,true);
            Help(dock,V(-4,1.35f,-.5f));
        }
        private static void BuildTower()
        {
            Box("Suelo_sala",tower,V(40,-.2f,0),V(12,.4f,13),stone);
            Box("Pared_izquierda",tower,V(34,2,0),V(.25f,4,13),dark);
            Box("Pared_fondo",tower,V(40,2,6.5f),V(12,4,.25f),dark);
            for(int x=35;x<=45;x+=2)
            {
                Box("Marco_ventana",tower,V(x,2,-6.5f),V(.15f,4,.15f),brass);
                Box("Soporte_ventanal",tower,V(46,2,x-40),V(.15f,4,.15f),brass);
            }
            Box("Pasamanos",tower,V(46,1,0),V(.15f,.15f,13),brass);
            Label("02   /   LA SALA DE LA LENTE",tower,V(40,3.6f,5.9f),new Vector2(1300,100),.004f,50,brass.color);
            for(int i=0;i<3;i++)
            {
                Box("Armario_instrumentos",tower,V(35,.8f,i*1.6f),V(1.3f,1.6f,1.2f),wood);
                Prim("Indicador",PrimitiveType.Sphere,tower,V(35.7f,1.3f,i*1.6f),V(.12f,.12f,.12f),glow,false);
            }
            Box("Consola_lente",tower,V(40,.6f,2.6f),V(2.8f,1.2f,1.4f),dark);
            var wheel=Group("INT_Rueda_lente",tower);wheel.position=V(40,1.8f,2);
            var collider=wheel.gameObject.AddComponent<BoxCollider>();collider.size=V(1.8f,1.8f,.4f);
            var disc=Prim("Disco",PrimitiveType.Cylinder,wheel,V(0,0,0),V(1.65f,.12f,1.65f),brass,false,true);
            disc.transform.localRotation=Quaternion.Euler(90,0,0);
            var target=Target(wheel.gameObject,TargetAction.Lens,"Gira 90°. La aguja turquesa debe apuntar a la marca dorada de arriba.");
            target.highlight=disc.GetComponent<Renderer>();
            var rotor=Group("Aguja_giratoria",wheel);rotor.localPosition=V(0,0,-.17f);
            mission.lensPivot=rotor;
            Box("Aguja",rotor,V(0,.32f,0),V(.13f,.65f,.08f),glow,false,true);
            Prim("Centro_aguja",PrimitiveType.Sphere,rotor,V(0,0,0),V(.25f,.25f,.12f),dark,false,true);
            Box("Marca_objetivo",tower,V(40,2.87f,1.85f),V(.22f,.28f,.18f),brass,false);
            Label("ALINEA ARRIBA",tower,V(40,3.2f,2),new Vector2(750,100),.002f,42,brass.color);
            mission.lensReadout=Label("GIRO 90°",tower,V(40,.85f,1.8f),new Vector2(650,100),.002f,42,Color.white);
            // Lente superior y haz estilizado: no se emplea niebla volumétrica ni postprocesado.
            var beacon=Group("Linterna_rotatoria",tower);beacon.position=V(42,3.4f,4);
            mission.beaconRotor=beacon;
            Prim("Lente_Fresnel",PrimitiveType.Sphere,beacon,V(0,0,0),V(1.2f,1.2f,1.2f),glow,false,true);
            var beam=Box("Haz_de_luz",beacon,V(0,0,7),V(.18f,.18f,14),glow,false,true);
            mission.beam=beam;beam.SetActive(false);
            var button=Prim("INT_Enviar_senal",PrimitiveType.Cylinder,tower,V(43,1.25f,1.4f),V(.65f,.15f,.65f),red);
            button.transform.rotation=Quaternion.Euler(90,0,0);
            Target(button,TargetAction.Beacon,"Envía la señal de rescate después de alinear la lente.");
            Box("Pedestal_senal",tower,V(43,.5f,1.5f),V(.75f,1,.75f),dark);
            Label("SEÑAL",tower,V(43,1.95f,1.4f),new Vector2(550,100),.002f,46,brass.color);
            mission.towerDialogue=Dialogue("Dialogo_faro",tower,V(40,3.45f,-.5f),4.8f);
            Teleport("LENTE",tower,V(38,.06f,-1),V(39,0,-.6f),1,false);
            Teleport("VENTANAL",tower,V(43,.06f,-1.5f),V(43,0,-3),1,false);
            Teleport("MUELLE",tower,V(36,.06f,-3),V(0,0,-5),0,false);
            Help(tower,V(36.6f,1.4f,.2f));
            var ending=Group("Final_y_reinicio",tower);
            Label("EL MAR VUELVE A TENER CAMINO\nRescate completado",ending,V(40,2.5f,-.1f),new Vector2(1100,240),.0035f,52,glow.color);
            var restart=Box("INT_Reiniciar",ending,V(40,1,-.1f),V(1.8f,.55f,.25f),teal);
            Target(restart,TargetAction.Restart,"Mantén la mirada para comenzar una misión nueva.");
            Label("VOLVER A EMPEZAR",ending,V(40,1,-.25f),new Vector2(900,100),.0018f,42,Color.white);
            mission.finalPanel=ending.gameObject;ending.gameObject.SetActive(false);
        }
        private static void BuildOcean()
        {
            var ocean=Group("Mar_y_costas");
            Box("Mar",ocean,V(20,-1.2f,15),V(180,.3f,170),sea,false);
            for(int i=0;i<24;i++)
            {
                float x=Mathf.Sin(i*2.4f)*48+20, z=22+i*2.5f;
                Prim("Roca_costera",PrimitiveType.Sphere,ocean,V(x,-.5f,z),V(2+i%3,1+i%2,3),stone,false);
                Box("Espuma",ocean,V(x,-1.02f,z-2),V(4,.015f,.08f),teal,false);
            }
            var boat=Group("Barco_perdido",ocean);boat.position=V(66,-.2f,9);
            mission.boat=boat;
            Prim("Casco",PrimitiveType.Sphere,boat,V(0,0,0),V(3,1.1f,6),wood,false,true);
            Box("Cubierta",boat,V(0,.45f,0),V(2.5f,.2f,5),dark,false,true);
            Box("Cabina",boat,V(0,1,0),V(1.8f,1.1f,1.8f),white,false,true);
            Box("Ventana_barco",boat,V(0,1.1f,-.95f),V(1.2f,.45f,.04f),glow,false,true);
            Box("Mastil",boat,V(0,2,1.5f),V(.12f,3.5f,.12f),brass,false,true);
            var stars=Group("Estrellas",ocean);
            for(int i=0;i<36;i++)
            {
                float a=i*2.399f;
                Prim("Estrella",PrimitiveType.Sphere,stars,V(20+Mathf.Sin(a)*65,22+i%12,Mathf.Cos(a)*65),V(.13f,.13f,.13f),glow,false);
            }
        }
        private static void Teleport(string title, Transform parent, Vector3 marker, Vector3 destination, int zone, bool power)
        {
            var pad=Prim("TP_"+title,PrimitiveType.Cylinder,parent,marker,V(1.15f,.05f,1.15f),power?brass:teal);
            var t=Target(pad,TargetAction.Teleport,"Viajar: "+title+(power?". Requiere energía.":"."));
            var d=Group("Destino_"+title,parent); d.position=destination;
            t.destination=d;t.zone=zone;t.requiresPower=power;
            Label(title,parent,marker+V(0,.48f,.15f),new Vector2(650,110),.002f,40,Color.white);
        }
        private static void Help(Transform parent,Vector3 at)
        {
            var g=Prim("INT_Ayuda",PrimitiveType.Sphere,parent,at,V(.4f,.4f,.4f),brass);
            Target(g,TargetAction.Help,"Mantén la mirada para repetir la misión y las instrucciones.");
            Label("?",parent,at+V(0,0,-.22f),new Vector2(100,100),.003f,75,dark.color);
        }
        private static GazeTarget Target(GameObject g,TargetAction action,string hint)
        {
            g.layer=8;var t=g.AddComponent<GazeTarget>();t.action=action;t.hint=hint;t.mission=mission;
            t.highlight=g.GetComponent<Renderer>();return t;
        }
        private static WorldDialogue Dialogue(string name,Transform parent,Vector3 at,float width)
        {
            var c=CanvasAt(name,parent,at,new Vector2(1100,310),width/1100);
            var bg=ImageAt("Fondo",c.transform,new Color(.025f,.065f,.09f,.94f));Stretch(bg.rectTransform,0);
            var text=TextAt("Mensaje",c.transform,"",40,Color.white);Stretch(text.rectTransform,28);
            var cg=c.gameObject.AddComponent<CanvasGroup>();cg.alpha=0;cg.blocksRaycasts=false;
            var d=c.gameObject.AddComponent<WorldDialogue>();d.text=text;d.group=cg;d.head=mission.head;
            return d;
        }
        private static Text Label(string text,Transform parent,Vector3 at,Vector2 size,float scale,int fontSize,Color color)
        {var c=CanvasAt("Cartel_"+text.Replace('\n','_'),parent,at,size,scale);return TextAt("Texto",c.transform,text,fontSize,color);}
        private static Canvas CanvasAt(string name,Transform parent,Vector3 at,Vector2 size,float scale)
        {
            var go=new GameObject(name,typeof(RectTransform));go.layer=2;go.transform.SetParent(parent,false);
            go.transform.localPosition=at;go.transform.localScale=Vector3.one*scale;
            var rect=(RectTransform)go.transform;rect.sizeDelta=size;
            var c=go.AddComponent<Canvas>();c.renderMode=RenderMode.WorldSpace;return c;
        }
        private static Image ImageAt(string name,Transform parent,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform));go.layer=2;go.transform.SetParent(parent,false);
            var image=go.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;
        }
        private static Text TextAt(string name,Transform parent,string value,int size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform));go.layer=2;go.transform.SetParent(parent,false);
            var text=go.AddComponent<Text>();text.font=font;text.text=value;text.fontSize=size;text.color=color;
            text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;text.horizontalOverflow=HorizontalWrapMode.Wrap;
            text.verticalOverflow=VerticalWrapMode.Overflow;Stretch(text.rectTransform,4);return text;
        }
        private static void Stretch(RectTransform r,float margin)
        {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.one*margin;r.offsetMax=-Vector2.one*margin;}
        private static LineRenderer Ring(string name,Transform parent,float radius,float width,Color color)
        {
            var go=new GameObject(name);go.layer=2;go.transform.SetParent(parent,false);
            var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=false;line.widthMultiplier=width;
            line.sharedMaterial=Mat("ReticulaSprite","#FFFFFF",true,"Sprites/Default");line.startColor=line.endColor=color;
            line.positionCount=49;
            for(int i=0;i<=48;i++) {float a=i/48f*Mathf.PI*2;line.SetPosition(i,V(Mathf.Sin(a)*radius,Mathf.Cos(a)*radius,0));}
            return line;
        }
        private static Material Mat(string name,string hex,bool unlit=false,string shader=null)
        {
            string path="Assets/Faro/Materials/"+name+".mat";
            var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing)return existing;
            ColorUtility.TryParseHtmlString(hex,out Color color);
            var m=new Material(Shader.Find(shader??(unlit?"Unlit/Color":"Standard")));m.name=name;m.color=color;
            if(!unlit){m.SetFloat("_Glossiness",.18f);m.enableInstancing=true;}
            AssetDatabase.CreateAsset(m,path);return m;
        }
        private static Transform Group(string name,Transform parent=null)
        {var go=new GameObject(name);if(parent)go.transform.SetParent(parent,false);return go.transform;}
        private static Vector3 V(float x,float y,float z){return new Vector3(x,y,z);}
        private static GameObject Box(string name,Transform parent,Vector3 p,Vector3 s,Material m,bool collider=true,bool local=false)
        {return Prim(name,PrimitiveType.Cube,parent,p,s,m,collider,local);}
        private static GameObject Prim(string name,PrimitiveType type,Transform parent,Vector3 p,Vector3 s,Material m,bool collider=true,bool local=false)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);
            if(local)go.transform.localPosition=p;else go.transform.position=p;
            go.transform.localScale=s;
            var r=go.GetComponent<Renderer>();r.sharedMaterial=m;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
            if(!collider)Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
        private static AudioClip Clip(string name){return AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Faro/Audio/"+name+".wav");}
        private static AudioSource Audio(string name,Transform parent,AudioClip clip,float volume,bool loop)
        {var g=Group(name,parent);var a=g.gameObject.AddComponent<AudioSource>();a.clip=clip;a.volume=volume;a.loop=loop;a.playOnAwake=false;return a;}
    }
}
