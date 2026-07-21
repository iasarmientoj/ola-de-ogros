using UnityEngine;
using UnityEditor;

public class CreateExplosion
{
    [MenuItem("Tools/Generar Explosion Simple")]
    public static void CrearParticulasExplosion()
    {
        // Crear un objeto vacío
        GameObject go = new GameObject("ExplosionEffect");
        
        // Añadir Particle System
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        
        // Configuración Principal
        var main = ps.main;
        main.duration = 1.0f;
        main.loop = false;
        main.startLifetime = 1.0f;
        main.startSpeed = 15.0f;
        main.startSize = 0.5f;
        main.startColor = new Color(1f, 0.6f, 0f, 1f); // Naranja fuego
        main.playOnAwake = true;

        // Configuración de Emisión (Explosión de golpe)
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]{ new ParticleSystem.Burst(0.0f, 100) });

        // Configuración de la Forma
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        // Color a lo largo de la vida (Se vuelve humo y desaparece)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0.0f), new GradientColorKey(Color.red, 0.3f), new GradientColorKey(Color.gray, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = grad;

        // Tamaño a lo largo de la vida
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 1.0f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(2.0f, curve);

        // Añadir script para que se autodestruya al terminar
        go.AddComponent<DestroyAfterSeconds>().seconds = 1.5f;

        // Guardar como Prefab
        if (!System.IO.Directory.Exists("Assets/Recursos/granada"))
        {
            System.IO.Directory.CreateDirectory("Assets/Recursos/granada");
        }
        
        string localPath = "Assets/Recursos/granada/ExplosionEffect.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, localPath);
        
        // Destruir el objeto de la escena (solo queríamos el prefab)
        Object.DestroyImmediate(go);
        
        Debug.Log("¡Prefab ExplosionEffect creado con éxito en Assets/Recursos/granada!");
    }
}
