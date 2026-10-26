# Correspondencia con la evaluación

La temática es libre. La evaluación no acepta entregar la granja de clase con cambios superficiales. Este escenario se construye desde cero como un faro y su muelle.

| Requisito | Implementación preparada | Comprobación pendiente |
|---|---|---|
| R1 Dos zonas | Muelle y sala de la lente; raíces separadas en una escena; teletransporte | Explorar y reconocer ambas en Play y teléfono |
| R2 Dos acciones distintas | Cristal se instala y alimenta balizas; rueda modifica orientación hasta alinear la lente | Ver cambios y bloqueos de progreso |
| R3 Mirada y autoclic | GazeInteractor, DwellTimer, GazeTarget, ReticleVisual; 1,6 s, cambio de color y escala | Cancelación al salir, nuevo objetivo y ausencia de repetición |
| R4 Tres o más destinos | Seis marcadores; cinco posiciones distintas, dos marcadores regresan al mismo muelle | Piso, altura, vista y ausencia de obstáculos |
| R5 ScreenFade | Canvas World Space hijo de la cámara; negro → mover raíz → revelar | Cobertura de ambos ojos y desbloqueo |
| R6 Diálogos World Space | Un emisor por zona; se ubica en el mundo al mostrar, se orienta hacia la cámara y se oculta tras un plazo | Legibilidad sin tapar la retícula |
| R7 Ambientación | Faro, muelle, balizas, cristal, cajas, boya, instrumentos, rueda, barco, rocas, mar y estrellas | Visibilidad y fluidez móvil |
| R8 Editor y teléfono | Mouse en editor; pose Cardboard en Android; SDK y configuración Android incluidos | PRUEBA OBLIGATORIA pendiente en teléfono |
| R9 Inicio, final y reinicio | Ayuda inicial, final condicionado por ambas acciones y recarga de escena | Completar dos recorridos consecutivos |
| R10 Organización y explicación | Carpetas, nombres, ficha, fuentes, declaración de IA y explicación del código | Completar el aporte real del estudiante y defensa individual |

La rúbrica asigna 20 puntos al funcionamiento móvil y exige la demostración en vivo. La implementación preparada no garantiza una nota: faltan ejecución, revisión visual, compilación y prueba real.
