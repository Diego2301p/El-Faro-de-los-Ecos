# Cómo explicar el proyecto

Lee cada archivo mientras pruebas su función. El objetivo es comprender lo que sucede y poder hacer cambios pequeños, no memorizar respuestas.

## Qué hace cada script

| Archivo | Responsabilidad |
|---|---|
| MissionState.cs | Guarda energía, ángulo, alineación y final. No depende de Unity. |
| DwellTimer.cs | Cuenta el tiempo sobre un objetivo. Si cambia, borra la cuenta. Después de activar, no repite hasta cambiar de objetivo. |
| GazeInteractor.cs | Lanza un rayo al centro de la mirada y usa el primer collider. Cambia el foco y conecta el contador con OnPointerClick. |
| GazeTarget.cs | Indica qué acción representa cada objeto y resalta al entrar/salir de la mirada. |
| ReticleVisual.cs | Dibuja el aro central y su progreso con LineRenderer. |
| EditorMouseLook.cs | Botón derecho y mouse simulan el giro de cabeza en editor. |
| CardboardSession.cs | Configura seguimiento de rotación en teléfono, ajustes QR, recentrado y parámetros de pantalla. |
| ScreenFadeVR.cs | Interpola el alpha del CanvasGroup usando tiempo no escalado. |
| MissionController.cs | Coordina animaciones, condiciones, sonidos, diálogos, traslados, final y reinicio. |
| WorldDialogue.cs | Coloca el diálogo en el mundo, orienta su cara hacia el jugador y lo oculta con el tiempo. |
| Editor/FaroSceneBuilder.cs | Crea una escena editable con geometría, materiales, textos, referencias y audio. No se incluye en la ejecución móvil. |
| Editor/FaroSetup.cs | Montaje inicial, configuración Android/XR, comprobación de referencias y compilación. |
| Editor/CardboardGradle.cs | Añade dependencias Android a la salida Gradle de Unity. |

## Por qué se cancela la mirada

DwellTimer compara el identificador actual con el anterior. Si cambia o no hay objeto, borra el tiempo acumulado. Al completar 1,6 segundos devuelve true una vez. GazeInteractor además conserva el identificador activado durante una animación o traslado: así no se dispara otra acción al terminar la transición si todavía se mira el mismo objeto.

## Por qué no se interactúa a través de las paredes

El rayo incluye los colliders del entorno, no solo la capa Interactive. Consulta el primer impacto y busca GazeTarget en ese objeto o sus padres. Si toca una pared sin GazeTarget, no hay activación. Los Canvas y la retícula usan Ignore Raycast y no tienen colliders.

## Cómo se teletransporta

MissionController impide iniciar otra operación mientras Busy es true. Primero espera fade.To(1), después orienta y traslada PlayerRoot, espera un frame y ejecuta fade.To(0). Compensa el desplazamiento horizontal de la cámara. La posición del destino representa los pies; la cámara está a 1,65 m. Cardboard sigue la rotación de la cabeza y no ofrece caminar con seguimiento de posición.

## Por qué son dos acciones distintas

La primera mueve el cristal a un zócalo, enciende las balizas y habilita un acceso. La segunda modifica una orientación hasta satisfacer una condición; la lente solo queda alineada a 0°. Mostrar un texto o trasladarse no cuentan como esas acciones.

## Cómo se impide terminar antes de tiempo

MissionState.Finish devuelve false mientras falte energía o alineación. La condición se verifica en la lógica, además de los mensajes visibles. La rueda tampoco cambia si falta energía. Las pruebas C# verifican estos bloqueos.

## Cómo se reinicia

Se oscurece la pantalla y SceneManager.LoadScene vuelve a cargar la escena guardada. Todos los objetos recuperan sus transformaciones y referencias iniciales, y MissionState se crea de nuevo. No se utiliza PlayerPrefs para conservar progreso.

## Cambios sencillos para estudiar

1. Cambia dwellSeconds en GazeInteractor de 1,6 a 2 segundos y verifica el efecto.
2. Cambia duration de ScreenFadeVR y compara 0,3 y 0,8 segundos.
3. Cambia el texto de Help en MissionController y prueba la esfera «?».
4. Mueve un destino en Hierarchy y ejecuta Faro > Revisar escena; no lo pongas dentro de una pared.
5. Describe con tus palabras qué hiciste, cómo lo probaste y qué errores corregiste. Regístralo en la ficha.
