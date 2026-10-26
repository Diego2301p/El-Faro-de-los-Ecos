# Fuentes, licencias y uso de IA

## Material del curso revisado

- TIEL12_RVA_EVA2_Desafio_VR_1.docx: temática libre, requisitos R1–R10, entregables y rúbrica.
- Unidad2_Guia_HelloCardboard_Unity6.pdf y Guia_HelloCardboard_Unity6_AndroidXR (1).docx: base Android/Cardboard.
- Guías 2 y 3: organización, ambientación y rendimiento.
- Guía 4 y guía de retícula automática del ZIP 5: mouse, eventos de mirada y autoclic.
- Guías 6 y 7 y guía de burbuja: Canvas World Space, fundido y teletransporte.

Los scripts entregados aquí son una implementación nueva de esas técnicas. No se importan CardboardUnityAdventure.unitypackage ni Teleport.unitypackage: no son necesarios y sus licencias no están documentadas en los adjuntos. Tampoco se redistribuyen assets de la granja ni imágenes del profesor.

## Google Cardboard

SDK oficial 1.36.0, commit `7fb2c0e438d2ef686f51cd59b53da16cc07b1b68`, incluido como paquete local en Packages/com.google.xr.cardboard. Se conserva LICENSE.md (Apache-2.0) y los avisos de terceros. CardboardSession adapta el ciclo de CardboardStartup de la muestra oficial. Los archivos del SDK no se modifican. La marca Cardboard pertenece a Google; este proyecto no es un producto oficial de Google.

- Repositorio: https://github.com/googlevr/cardboard-xr-plugin
- Quickstart: https://developers.google.com/cardboard/develop/unity/quickstart
- Licencia conservada: Packages/com.google.xr.cardboard/LICENSE.md
- Avisos nativos: Packages/com.google.xr.cardboard/Runtime/Android/THIRD_PARTY_NOTICES.txt

## Unity

Editor y paquetes sujetos a las condiciones de Unity. El manifest fija Input System 1.11.2, XR Management 4.5.0, Legacy Input Helpers 2.1.11 y uGUI 2.0.0. Los paquetes se resuelven al abrir; no se redistribuye el editor de Unity. La tipografía es LegacyRuntime.ttf incluida por Unity.

- Android: https://docs.unity3d.com/6000.0/Documentation/Manual/android-BuildProcess.html
- XR Management: https://docs.unity3d.com/Packages/com.unity.xr.management@4.5/manual/index.html
- Activity: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerSettings.Android-applicationEntry.html

## Recursos propios del proyecto

Geometría compuesta con primitivas de Unity y materiales de colores: muelle, faro, instrumentos, barco y decoración. Audio generado por síntesis matemática, sin copiar grabaciones ni utilizar música comercial. No hay assets de pago, personajes licenciados ni descargas de modelos externas.

ChatGPT/Codex generó diseño, scripts, constructor de escena, sonidos sintetizados, pruebas y documentación a petición del estudiante. Debe declararse esa ayuda. El aporte posterior del estudiante debe describir solo tareas que haya realizado. El resultado requiere verificación real en Unity y Android; las pruebas preparadas no equivalen a esa verificación.
