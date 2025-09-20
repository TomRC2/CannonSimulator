# CannonSimulator
https://youtu.be/OfTQTOSTcgo (VIDEO)
Unity Version: 6000.1.4f1

How To Play: You can change Force, Angle and Mass parameters for the shooting direction. Then with the "Fire" button you can shoot.
Controls: Mouse

Requisitos mínimos


Controles de disparo en pantalla:
Ángulo y fuerza con Slider o InputField.
Masa del proyectil seleccionable


Disparo físico:
Proyectil con Rigidbody y Collider.
Lanzamiento por AddForce o velocity según el ángulo configurado.


Escena de objetivos:
Estructuras armadas con Rigidbodies y Joints (FixedJoint, HingeJoint o SpringJoint).
Estabilidad inicial correcta. Si se cae sola, está mal configurada.


Registro del resultado:
Guardar datos como tiempo de vuelo, punto de impacto, velocidad relativa, impulso de colisión y piezas derribadas.
Mostrar al final de cada intento: puntuación y un breve “reporte de tiro”.


Entrega


Repositorio en Git:
README.md con cómo jugar, versión de Unity, controles y criterios de evaluación.
Carpeta Assets/ con scripts y prefabs ordenados.
Commits claros. Evitar subir Library/ y builds.


Video en YouTube:
1 a 3 minutos. Mostrar interfaz, 2 o 3 tiros distintos y el registro de resultados.
Link en el README.
