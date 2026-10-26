# Pruebas pendientes en Unity y Android

Todas las casillas empiezan sin marcar. No constituyen evidencia de pruebas ya realizadas.

Datos: Unity ____ / Teléfono ____ / Android ____ / Visor ____ / Fecha ____.

## Importación y editor

- [ ] Se importan los paquetes; Console no muestra errores rojos.
- [ ] El montaje automático genera ambas zonas, cámara, sonidos y materiales.
- [ ] Faro > Revisar escena pasa las referencias y los seis destinos.
- [ ] Play abre la vista desde negro y el botón derecho mueve la mirada.
- [ ] La misión y los carteles se leen desde cada destino.
- [ ] La retícula detecta cristal, rueda, señal, ayuda y marcadores.
- [ ] Mirar medio segundo y salir cancela; volver empieza desde cero.
- [ ] Cambiar entre dos objetivos no conserva el progreso del primero.
- [ ] Mirar de forma continua no activa dos veces, tampoco al acabar una animación.
- [ ] Una pared tapa la interacción con objetos de detrás.
- [ ] FARO muestra bloqueo antes de instalar el cristal.
- [ ] El cristal se mueve, las cuatro balizas se encienden y suena la confirmación.
- [ ] Todos los traslados oscurecen antes de cambiar la posición y revelan después.
- [ ] No se pueden solapar dos teletransportes ni terminar con pantalla negra.
- [ ] Se llega sobre el suelo, fuera de objetos y mirando en una dirección útil.
- [ ] La rueda gira 90 → 180 → 270 → 0; cada giro requiere una mirada nueva.
- [ ] SEÑAL no termina antes de alinear la lente.
- [ ] Lente alineada activa el haz; SEÑAL mueve el barco y muestra final.
- [ ] Reiniciar devuelve cristal, balizas, ángulo, barco y misión al inicio.
- [ ] Un segundo recorrido completo también funciona.

## APK y teléfono

- [ ] Módulos Android, SDK Platform 35 y OpenJDK instalados desde Unity.
- [ ] Cardboard activado en la pestaña Android de XR Plug-in Management.
- [ ] Active Input Handling en New; Activity activado, GameActivity desactivado.
- [ ] APK compilado, ruta ____ y fecha ____.
- [ ] APK instalado y abierto sin cierre inesperado.
- [ ] Dos vistas y orientación por sensores funcionando.
- [ ] QR del visor configurado y recentrado verificado.
- [ ] Autoclic funciona sin depender de un mouse o de tocar la pantalla.
- [ ] Fundido cubre ambos ojos, sin bordes visibles ni negro persistente.
- [ ] Mensajes legibles y situados sin ocultar el objetivo central.
- [ ] Recorrido completo, final y reinicio realizados en teléfono.
- [ ] Fluidez aceptable comprobada; registrar FPS si se mide: ____.
- [ ] Duración real del recorrido: ____; ajustar textos o ritmo si procede.
- [ ] Ficha actualizada con resultados, errores y contribución personal.

Si falla algo, registra primer error completo, paso, dispositivo y qué cambió. La ejecución en editor no valida la estereoscopía ni los sensores del teléfono.
