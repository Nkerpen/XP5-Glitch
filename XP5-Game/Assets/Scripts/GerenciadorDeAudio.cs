using UnityEngine;
using System.Collections; // Necessário para usarmos a Coroutine de tempo

public class GerenciadorDeAudio : MonoBehaviour
{
    public static GerenciadorDeAudio Instancia;

    [Header("Fontes de Áudio")]
    [SerializeField] private AudioSource canalSFX;      
    [SerializeField] private AudioSource canalNPC;      
    [SerializeField] private AudioSource canalMusica;   

    [Header("Clipes de Áudio")]
    public AudioClip somClique;
    public AudioClip somEnvioMensagem;
    public AudioClip somMensagemNPC;
    public AudioClip somNotificacao;
    public AudioClip somPlay;

    // Nova variável para guardar o volume original da música
    private float volumeMaximoMusica = 1f;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Assim que o jogo liga, o script olha qual volume você deixou no Inspector e salva!
        if (canalMusica != null)
        {
            volumeMaximoMusica = canalMusica.volume;
        }
    }

    // --- Funções de Efeitos Sonoros (SFX) ---
    public void TocarClique() { if (somClique != null) canalSFX.PlayOneShot(somClique); }
    public void TocarEnvioMensagem() { if (somEnvioMensagem != null) canalSFX.PlayOneShot(somEnvioMensagem); }
    public void TocarNotificacao() { if (somNotificacao != null) canalSFX.PlayOneShot(somNotificacao); }
    public void TocarPlay() { if (somPlay != null) canalSFX.PlayOneShot(somPlay); }

    public void TocarMensagemNPC() 
    { 
        if (somMensagemNPC != null) 
        {
            canalNPC.pitch = Random.Range(0.85f, 1.15f); 
            canalNPC.PlayOneShot(somMensagemNPC); 
        }
    }

    // --- Funções de Música com Fade ---
    public void IniciarMusicaFundo() 
    {
        if (canalMusica != null && !canalMusica.isPlaying)
        {
            canalMusica.volume = 0f; // Tira todo o som
            canalMusica.Play();      // Dá o play no mudo
            StartCoroutine(FadeInMusica(1.5f)); // Chama a mágica para durar 1.5 segundos
        }
    }

    // A "linha do tempo" que aumenta o volume aos poucos
    private IEnumerator FadeInMusica(float duracaoFade)
    {
        float tempo = 0f;

        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;
            // O Lerp calcula matematicamente a transição suave do 0 até o volume original
            canalMusica.volume = Mathf.Lerp(0f, volumeMaximoMusica, tempo / duracaoFade);
            yield return null; // Espera o próximo frame
        }

        // Garante que cravou no volume exato no final
        canalMusica.volume = volumeMaximoMusica; 
    }
}