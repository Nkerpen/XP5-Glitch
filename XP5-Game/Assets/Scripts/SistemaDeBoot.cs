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

    private void Start()
    {
        StartCoroutine(FluxoDeInicializacaoEPreparos());
    }

    private IEnumerator FluxoDeInicializacaoEPreparos()
    {
        // 1. Inicia a Tela de Loading
        telaLoading.SetActive(true);
        telaLogo.SetActive(false);
        telaHome.SetActive(false);
        
        barraDeProgresso.fillAmount = 0f;

        /* * AQUI COMEÇAM OS PREPARATIVOS!
         * Na Sprint 1, como ainda não temos o sistema de save pronto, 
         * vamos simular o progresso em 3 etapas (ex: 33% para cada etapa concluída).
         */

        // Tarefa 1: Simular a inicialização do Gerenciador de Áudio/Configs
        yield return new WaitForSeconds(0.5f); // Tempo que a tarefa levou
        barraDeProgresso.fillAmount = 0.33f;

        // Tarefa 2: Simular a leitura do JSON do sistema de Chats/Rotas
        yield return new WaitForSeconds(1.0f);
        barraDeProgresso.fillAmount = 0.66f;

        // Tarefa 3: Simular a instancialização dos ícones e apps da Home
        yield return new WaitForSeconds(0.8f);
        barraDeProgresso.fillAmount = 1f;

        // Dá um pequeno respiro visual de 0.5s para o jogador ver a barra cheia (100%)
        yield return new WaitForSeconds(0.5f); 

        // 2. Troca para a Tela de Logo
        telaLoading.SetActive(false);
        telaLogo.SetActive(true);
        yield return new WaitForSeconds(2f); // Tempo de exibição da logo

        // 3. Finalmente, abre a Home do celular
        telaLogo.SetActive(false);
        telaHome.SetActive(true);
    }
}