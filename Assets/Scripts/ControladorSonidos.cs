using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ControladorSonidos : MonoBehaviour
{
    public static ControladorSonidos Instancia;
    public List<SonidosJuego> misSonidos;

    [Range(0.7f, 1.3f)]
    public float pitchMinimo;

    [Range(0.7f, 1.3f)]
    public float pitchMaximo;

    // public AudioMixerGroup canalMixer;

    void Awake()
    {
        if (Instancia != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instancia = this;
        }

        IniciarControlador();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void IniciarControlador()
    {
        foreach (var sonido in misSonidos)
        {
            AudioSource nuevoParlante = gameObject.AddComponent<AudioSource>();
            nuevoParlante.clip = sonido.clipSonido;
            sonido.parlanteSonido = nuevoParlante;
        }
    }

    public void ReproducirSonido(string sonidoObjetivo)
    {
        foreach (var sonido in misSonidos)
        {
            if (sonido.nombreSonido.Equals(sonidoObjetivo))
            {
                sonido.parlanteSonido.pitch = Random.Range(pitchMinimo, pitchMaximo);
                sonido.parlanteSonido.Play();
                break;
            }
            else if (!sonido.nombreSonido.Equals(sonidoObjetivo))
            {
                continue;
            }
            else
            {
                Debug.Log("No existe ese sonido :(");
            }
        }
    }
}

[System.Serializable]
public class SonidosJuego
{
    public string nombreSonido;
    public AudioClip clipSonido;
    public AudioSource parlanteSonido;
}
