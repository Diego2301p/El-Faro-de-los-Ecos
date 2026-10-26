# El faro de los ecos

Videojuego académico de realidad virtual para Android con Google Cardboard, desarrollado en Unity. Un barco espera mar adentro sin encontrar el muelle entre la niebla. El jugador debe recuperar la energía del faro, orientar su luz y enviar una señal para guiar a la tripulación a salvo.

**Estudiante:** Diego Ahumada  
**Asignatura:** Realidad Virtual y Aumentada  
**Unity utilizado:** 6.5 — 6000.5.10f1  
**Google Cardboard XR Plugin:** 1.36.0  
**Escena principal:** `Assets/Faro/Scenes/ElFaro.unity`

## Estado del proyecto

El estudiante instaló y probó el juego en computador y en un teléfono Android. Confirmó el funcionamiento de iniciar y salir, la cancelación de la activación al retirar la mirada, los teletransportes y el movimiento del barco después de activar la señal.

La versión actual incorpora el menú de inicio con faro y barco animado, una introducción narrativa, mejoras de legibilidad y una pantalla de cierre con opciones para reiniciar la misión o volver al muelle.

También se realizaron 71 comprobaciones auxiliares de lógica y sintaxis C#. Estas comprobaciones no sustituyen las pruebas dentro de Unity ni en el teléfono. Los resultados anteriores corresponden a las pruebas comunicadas por el estudiante y no garantizan la ausencia de todos los errores.

## Abrir en Unity

1. Descarga y extrae el proyecto completo, o clona el repositorio. No abras el proyecto desde un ZIP.
2. En Unity Hub, selecciona **Projects → Add → Add project from disk**.
3. Elige la carpeta que contiene juntas `Assets`, `Packages` y `ProjectSettings`.
4. Ábrela con **Unity 6000.5.10f1** y espera a que termine la importación de paquetes. La primera apertura necesita conexión a Internet.
5. En la ventana **Project**, abre `Assets → Faro → Scenes → ElFaro`.
6. Pulsa **Play** para mostrar el menú inicial.

El paquete de Cardboard está incluido en `Packages/com.google.xr.cardboard`. No es necesario importar nuevamente los paquetes de las guías del profesor. El constructor de escena solo sirve para preparar o reconstruir el escenario; no lo ejecutes para abrir una escena ya guardada y modificada.

## Controles

| Acción | Computador | Android con Cardboard |
|---|---|---|
| Mirar alrededor | Mantener el botón derecho y mover el mouse | Girar el teléfono o la cabeza si se utiliza visor |
| Activar un objeto | Mantener la retícula sobre él durante 1,6 segundos | Mantener la mirada sobre él durante 1,6 segundos |
| Cancelar la espera | Apartar la retícula antes de completar el aro | Apartar la mirada antes de completar el aro |
| Desplazarse | Activar los círculos de teletransporte | Activar los círculos de teletransporte |
| Repetir las instrucciones | Mirar la esfera con el símbolo `?` | Mirar la esfera con el símbolo `?` |

En computador, el clic izquierdo es una alternativa para activar el objetivo. Para comprobar el autoclick, realiza las pruebas sin utilizarlo.

## Recorrido

1. Selecciona **Iniciar** en el menú. **Salir** detiene Play en el editor o cierra la aplicación en Android.
2. Lee la historia y mira **Comenzar misión**. La historia permanece visible hasta que la actives; aparta primero la mirada hacia el texto para evitar una activación accidental.
3. En el **Muelle de los ecos**, activa el cristal turquesa. Se instala en su zócalo, enciende las balizas y habilita el acceso al faro.
4. Activa el círculo **FARO** para viajar a la **Sala de la lente**.
5. Activa la rueda para girar la lente en pasos de 90°. Parte en 90° y necesita tres giros para llegar a 0°. **Aparta la mirada entre giros**.
6. Cuando la lente esté alineada, activa **SEÑAL**. El barco comienza a moverse y aparece la pantalla de misión completada.
7. Elige **Reiniciar misión** para volver al menú y comenzar desde el estado inicial, o **Volver al muelle** para seguir explorando después del rescate.

El teletransporte utiliza la secuencia oscurecer → trasladar → revelar. Los mensajes World Space orientan al jugador en ambas zonas. El tiempo de lectura del menú y la historia no se suma al tiempo de la misión.

## Generar e instalar el APK

1. En Unity Hub, comprueba que el editor tenga **Android Build Support**, **Android SDK & NDK Tools** y **OpenJDK**.
2. Detén Play y abre **File → Build Profiles** en Unity.
3. Selecciona Android y cambia a ese perfil si todavía no está activo. Comprueba que la escena `ElFaro` esté incluida y habilitada.
4. Mantén desactivadas **Export Project** y **Build App Bundle** para generar un APK.
5. Si Unity solicita instalar **Android SDK Platform 35**, pulsa **Update Android SDK** y espera a que termine.
6. Conecta el teléfono por USB, activa **Depuración USB** y acepta en el teléfono la autorización para ese computador.
7. Selecciona el teléfono en **Run Device**; utiliza **Refresh** si no aparece.
8. Pulsa **Build And Run** y guarda el archivo como `El_Faro_Diego_Ahumada.apk`. Unity compilará, instalará y abrirá el juego si el proceso termina correctamente.

Si solo necesitas el instalador, utiliza **Build**. El archivo se guarda en la ubicación que elijas; no se debe asumir que el repositorio ya contiene un APK. Si modificas el proyecto, vuelve a compilar e instalar para actualizar la versión del celular.

## Organización del código

| Archivo | Función |
|---|---|
| `GazeInteractor.cs` y `GazeTarget.cs` | Detectan la mirada, resaltan objetivos y comunican la acción elegida. |
| `DwellTimer.cs` | Controla la espera del autoclick, su cancelación y el bloqueo de repeticiones. |
| `MissionState.cs` | Guarda la energía, la alineación de la lente y el estado del rescate. |
| `MissionController.cs` | Coordina las acciones, el teletransporte, el barco, el cierre y el reinicio. |
| `ScreenFadeVR.cs` | Oscurece y revela la imagen durante las transiciones. |
| `WorldDialogue.cs` | Muestra y oculta los mensajes de las zonas. |
| `MenuInicioVR.cs` | Presenta el menú, el barco animado y la historia. |
| `ReticleVisual.cs` y `LegibilidadVR.cs` | Gestionan la retícula y los ajustes visuales de lectura. |
| `EditorMouseLook.cs` y `CardboardSession.cs` | Permiten mirar con mouse en el editor y con seguimiento de rotación en Android. |

Los scripts de `Assets/Faro/Editor` preparan la escena y la compilación; no forman parte del código de ejecución del juego en el teléfono.

## Fuentes y licencias

- **Google Cardboard XR Plugin:** [repositorio oficial de Google](https://github.com/googlevr/cardboard-xr-plugin). Licencia **Apache 2.0**, con los avisos de terceros incluidos. Conservar `Packages/com.google.xr.cardboard/LICENSE.md` y `Packages/com.google.xr.cardboard/Runtime/Android/THIRD_PARTY_NOTICES.txt`. `CardboardSession.cs` adapta el ciclo del ejemplo oficial `CardboardStartup`.
- **Input System, XR Management, Legacy Input Helpers y uGUI:** paquetes oficiales de Unity, bajo **Unity Companion License** y los avisos particulares que los acompañan. La fuente integrada `LegacyRuntime.ttf` y los recursos del motor se utilizan bajo las condiciones de Unity.
- **Modelos, decoración y materiales:** construidos para este proyecto mediante primitivas de Unity y código generado con ayuda de IA; no se descargaron modelos externos ni assets de pago.
- **Sonidos:** generados para el proyecto mediante síntesis matemática, sin muestras de audio ni música comercial de terceros.
- **Guías del docente:** referencia académica para las técnicas de mirada, autoclick, teletransporte, ScreenFade y diálogos. No se redistribuyen los assets ni los paquetes de la granja utilizados en clases.

## Uso de inteligencia artificial y aporte personal

Se utilizó **ChatGPT con funciones de Codex** principalmente para crear, revisar y actualizar scripts C#, preparar verificaciones, analizar errores y mejorar versiones anteriores del juego. La asistencia también incluyó el constructor de escena, recursos generados, menús, historia y documentación.

El estudiante abrió y configuró el proyecto en Unity, incorporó las actualizaciones, lo instaló en el teléfono y realizó las pruebas en computador y Android. Detectó dificultades de legibilidad y del cierre, solicitó correcciones y propuso el menú animado y la introducción narrativa.

## Entrega y revisión final

Conservar `Assets`, `Packages` y `ProjectSettings`, incluyendo los archivos `.meta` y `Packages/packages-lock.json` cuando exista. Añadir la ficha técnica actualizada y los avisos de licencias. Se pueden excluir `Library`, `Temp`, `Logs`, `Obj`, `.vs` y `UserSettings`.

Para la entrega se preparan el proyecto editable, el APK final probado y la ficha técnica. Antes de presentar, revisar también el reinicio completo, la legibilidad de los diálogos, los fundidos y la ausencia de activaciones repetidas. La evaluación incluye demostrar el recorrido en vivo en Android y explicar el código y el aporte personal.
