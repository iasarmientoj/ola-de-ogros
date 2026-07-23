# Plan de Clases de Unity - Lorenzo (Sesiones de 1 Hora)

Este plan divide minuciosamente los temas del archivo original en clases individuales de **1 hora**, manteniendo **exactamente el mismo texto, orden y estructura sin simplificar**, adaptado al ritmo de un niño de 10 años.

---

## Clase 21: NavMesh, Sistema de Daño, Sonidos y Arreglos de Enemigos
- [ ] recalcular navmesh y fijarse que por donde pasan los ogros este conectado
- [ ] poner una imagen de daño, roja
- [ ] actualizar el script con daño y camera shake
- [ ] descargar sonido de daño
- [ ] ajustar todos los sonidos https://editor.audio/ elimina vacios
- [ ] sonido de ogro
- [ ] sonido de caminar sobre pasto
- [ ] sonidos en wav
- [ ] arreglar scripts de enemigos y shooting, porque el daño del arma o estaba definido

---

## Clase 22: Creación e Integración de Puertas y Arreglo de Enemigos
- [ ] crear puertas simples
	- [ ] un cubo
	- [ ] animacion open, close, subir y bajar
	- [ ] navmesh obstacle en un cubo estatico
	- [ ] trigger y poner script
- [ ] poner todas las puertas
- [ ] arreglar ogro rojo, no dispara
	- [ ] el fire point no hacia parte del prefab, no estaba vinculado

---

## Clase 23: WaveManager, Checkpoints, UI Game Over y Hitbox Cabeza
- [ ] seguir configurando wavemanager
- [ ] cada horda es un checkpoint
	- [ ] poner player spawn en cada zona para cada horda
- [ ] poner pantalla de gameover, reintentar horda (ver publicidad), o reiniciar juego
	- [ ] conectar spawmers y botones
- [ ] en la cabeza mas daño, hitbox en collider

---

## Clase 24: Items del Mapa (Cartuchos, Barriles Explosivos y Botiquines)
- [ ] cartucho +500 balas al cargarlo, sin la R
	- [ ] descargar
	- [ ] poner en mapa
	- [ ] crear empty parent
	- [ ] poner script
	- [ ] crear prefab
	- [ ] ubicar en todo el mapa
- [ ] barril explosivo
	- [ ] descargar
	- [ ] poner en mapa
	- [ ] crear empty parent
	- [ ] poner script
	- [ ] crear prefab
	- [ ] ubicar en todo el mapa
- [ ] botiquin
	- [ ] descargar
	- [ ] poner en mapa
	- [ ] crear empty parent
	- [ ] poner script
	- [ ] crear prefab
	- [ ] ubicar en todo el mapa
- [ ] cambiar material a urp apra que brille
- [ ] sonidos

---

## Clase 25: Corrección de Animaciones, Balance de Hordas y Knockback
- [ ] areglar al enemigo1 al disparar se gira raro
	- [ ] marcar Bake Into Pose en las animaciones que "esten mal", atacar y disparo
- [ ] aumento de parametros en enemigos en hordas superiores
- [ ] player hacia atras cuando recibe ataque

---

## Clase 26: Creación de Nuevo Enemigo - Ogro Mini (Generación 3D e Importación)
- [ ] ogro mini
	- [ ] crear imagen 3d 
		- [ ] Crea un ogro miniautra, en fondo blanco, t pose, de frente, sin cuernos, con cabeza grnade, que se sienta que es como un ogro enano
	- [ ] descargar sonidos
	- [ ] editar audios
	- [ ] modelar en 3d hunyuan
	- [ ] texturizar
	- [ ] descargar glb
	- [ ] convertir a fbx https://convert3d.org/glb-to-fbx https://www.3dpea.com/en/convert/GLB-to-FBX
	- [ ] mixamo
		- [ ] tpose
		- [ ] run
		- [ ] hit
		- [ ] attack
		- [ ] die
	- [ ] duplicar enemigo 1, ya que es muy similar
	- [ ] configurar todo lo de las animaciones, huesos rig y eso, tambien texturas
	- [ ] duplicar animator y reemplazar animaciones
	- [ ] ajustar collider y parametros script
	- [ ] crear prefab

---

## Clase 27: Creación de Nuevo Enemigo - Drone (Modelado, Proyectiles y Setup)
- [ ] drone campo visual, cada 5 segundos dispara una rafaga
	- [ ] descargar sonidos
	- [ ] descargar de skechtfab
	- [ ] importar
	- [ ] duplicar enemigo flecha para usar de base
	- [ ] unpack prefab
	- [ ] poner spawners de balas y fuegos del player
		- [ ] SOLO PONER UNO Y DOS BALAS, SOLO COMO TRUCO PARA REUTILIZAR EL SCRIPT
	- [ ] duplicar animator
	- [ ] duplicar la flecha y ajustar poniendo dos proyectiles
		- [ ] ayudarse poniendo el prefab en la escena para ajustar bien, y colider ancho, solo uno
	- [ ] crear bala, duplicando el prefab de la flecha y ajustando tamaño y direccion
	- [ ] ajustar parametros de script enemigo flecha

---

## Clase 28: Creación de Nuevo Enemigo - Lobo (Generación 3D, Rigging y Animaciones)
- [ ] lobo
	- [ ] crear imagen 3d 
		- [ ] Crea un lobo ogro, en fondo blando, cuadurpedo, no tan diabolico, mas 3d con este estilo
	- [ ] descargar sonidos
	- [ ] modelar en 3d hunyuan
	- [ ] texturizar
	- [ ] descargar gbl
	- [ ] importar y exportar en blender fbx para que pese menos de 30 megas, si hace falta, optimizar, y exportar con Copy y activar boton azul
	- [ ] crear rig y animaciones en https://everythinguniver.se/
		- [ ] AJUSTAR EL RIG ANTES DE GENERAR ANIMACIONES
		- [ ] run
	- [ ] importar a unity
	- [ ] duplicar enemigo mini, es similar
	- [ ] unpack
	- [ ] collider cubo
	- [ ] duplicar animator
	- [ ] crear animaciones a mano, basicas
		- [ ] hit
		- [ ] attack
		- [ ] die
	- [ ] poner en el animator sin loop
	- [ ] config parametros script enemy

---

## Clase 29: Creación del Jefe Final - Boss (Modelado, Mixamo y Rigging)
- [ ] boss
	- [ ] crear imagen: Ahora genera un ogro que sea el "jefe final" grande, gordo, bestial
		- [ ] ogro
		- [ ] garrote por aparte
	- [ ] crear modelo hunyuan
	- [ ] descragar glb
	- [ ] convertir boss fbx
	- [ ] mixamo, axe
		- [ ] idle
		- [ ] attack con garrote
		- [ ] die
		- [ ] hit
	- [ ] importar, configurar animaciones humanoid etc
	- [ ] copiar animator de ogro mini
		- [ ] reemplazar anims
	- [ ] crear emty
	- [ ] poner el boss, poner el garrote
	- [ ] poner el script y unir todo
	- [ ] descargar audio boss

---

## Clase 30: Audio General, Menú con IA y Pantalla de Pausa
- [ ] descargar o generar musica de fondo del juego
- [ ] hacer la pantalla de pausa
- [ ] descargar UI sonidos
- [ ] menu, 
	- [ ] todo con la IA, solo reemplazamos imagenes, ella lo hace muchas veces mejro y mas completo, pero toca saber qué se esta haciendo, ya lo probamos en el juego 2d
		- [ ] Crea una nueva escena de menu, que tenga una imagen de fondo en blanco por ahora, luego yo reemplazo el sprite, y los botones tambien con imagen y texto, haz todas las pantallas que dijiste, instruccione sy eso, yo reemplazo las imagenes donde sea necesario
	- [ ] generar menu con ia
	- [ ] separar fondo y botones
	- [ ] reemplazar en unity

---

## Clase 31: Balance de Hordas y Control de Versiones con GitHub
- [ ] config build scenes
- [ ] config todo el wave, pensando en como se siente el juego, que la curva de dificultad se disfrute
- [ ] subir a github
	- [ ] ir a https://github.com/
	- [ ] crear nuevo repo con el gitignore de unity
	- [ ] copiar el gitignore de unity
	- [ ] inicializar en antigravity
	- [ ] Conectar tu repositorio de GitHub:
		- [ ] Presiona Ctrl + Shift + P para abrir la paleta de comandos.
		- [ ] Escribe Git: Add Remote y presiona Enter.
		- [ ] Pega la URL de tu repositorio: https://github.com/iasarmientoj/ola-de-ogros.git y presiona Enter.
		- [ ] Escribe origin como nombre del remoto.
	- [ ] ejecutar esto por el otro gitignore duplicado
		- [ ] git push origin main --force
	- [ ] y esto para que se vea en el ide
		- [ ] git branch --set-upstream-to=origin/main main

---

## Clase 32: Exportación a WebGL y Publicación en Itch.io
- [ ] exportar a web
	- [ ] que en installs esté en web
	- [ ] en build cambiar a web, switch
	- [ ] projext settings, texture, quarter
	- [ ] crear carpeta export-web
	- [ ] exportar
- [ ] publicar
	- [ ] comprimir en zip y que el html esté en la raiz
	- [ ] ir a https://itch.io/
	- [ ] crear
	- [ ] llenar
	- [ ] publicar
	- [ ] probar
