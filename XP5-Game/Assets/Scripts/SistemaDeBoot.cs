using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Importante para usar o componente Image

public class SistemaDeBoot : MonoBehaviour
{
    [Header("Telas do Sistema")]
    [SerializeField] private GameObject telaLoading;
    [SerializeField] private GameObject telaLogo;
    [SerializeField] private GameObject telaHome;

    [Header("Elementos de UI")]
    [SerializeField] private Image barraDeProgresso; // Arraste a Barra_Preenchimento aqui

    [Header("Configurações de Tempo")]
    [SerializeField] private float tempoTotalCarregamento = 2.3f; // Tempo em segundos para ir de 0 a 100%

    [Header("Configurações de Cor (Degradê)")]
    [SerializeField] private Color corInicial = Color.blue;   // Cor quando a barra está vazia (0%)
    [SerializeField] private Color corFinal = Color.red;     // Cor quando a barra está cheia (100%)

    [Header("Efeitos Visuais (UI Particle)")]
    [SerializeField] private ParticleSystem sistemaParticulas; // Arraste o seu UI Particle aqui

    private RectTransform rectBarra;

    private void Start()
    {
        if (barraDeProgresso != null) rectBarra = barraDeProgresso.GetComponent<RectTransform>();

        StartCoroutine(FluxoDeInicializacaoEPreparos());
    }

    private IEnumerator FluxoDeInicializacaoEPreparos()
    {
        // 1. Inicia a Tela de Loading
        telaLoading.SetActive(true);
        telaLogo.SetActive(false);
        telaHome.SetActive(false);

        barraDeProgresso.fillAmount = 0f;
        barraDeProgresso.color = corInicial;

        // Configura o formato inicial do shape das partículas
        ConfigurarShapeInicial();
        AtualizarPosicaoDasParticulas(0f);

        if (sistemaParticulas != null)
        {
            var emission = sistemaParticulas.emission;
            emission.enabled = true;
            sistemaParticulas.Play();
        }

        float tempoDecorrido = 0f;

        while (tempoDecorrido < tempoTotalCarregamento)
        {
            tempoDecorrido += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempoDecorrido / tempoTotalCarregamento);

            barraDeProgresso.fillAmount = progresso;

            Color corAtual = Color.Lerp(corInicial, corFinal, progresso);
            barraDeProgresso.color = corAtual;

            if (sistemaParticulas != null)
            {
                var mainModule = sistemaParticulas.main;
                mainModule.startColor = corAtual;

                // Move o ponto de emissão das partículas
                AtualizarPosicaoDasParticulas(progresso);
            }

            yield return null;
        }

        barraDeProgresso.fillAmount = 1f;
        barraDeProgresso.color = corFinal;
        AtualizarPosicaoDasParticulas(1f);

        if (sistemaParticulas != null)
        {
            var emission = sistemaParticulas.emission;
            emission.enabled = false;
        }

        yield return new WaitForSeconds(0.5f);

        telaLoading.SetActive(false);
        telaLogo.SetActive(true);
        yield return new WaitForSeconds(2f);

        telaLogo.SetActive(false);
        telaHome.SetActive(true);
    }

    private void ConfigurarShapeInicial()
    {
        if (sistemaParticulas == null) return;

        var shape = sistemaParticulas.shape;
        // Força o tipo de emissão para ser um ponto (Cone com raio zero) para não espalhar a origem
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = 0.01f;
        shape.angle = 25f; // Ângulo para elas abrirem em cone para fora
    }

    private void AtualizarPosicaoDasParticulas(float progresso)
    {
        if (rectBarra == null || sistemaParticulas == null) return;

        // Pega a largura real atual da barra de progresso multiplicada pela escala
        float larguraDaBarra = rectBarra.rect.width;

        // Calcula a posição local X da ponta com base no pivot da barra
        float pivotOffset = rectBarra.pivot.x * larguraDaBarra;
        float xDaPonta = (progresso * larguraDaBarra) - pivotOffset;

        // Move a ORIGEM de emissão das partículas (o Shape) ao invés do GameObject inteiro
        var shape = sistemaParticulas.shape;
        shape.position = new Vector3(xDaPonta, 0f, 0f);
    }
}