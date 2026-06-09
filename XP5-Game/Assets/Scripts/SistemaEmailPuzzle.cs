using UnityEngine;
using UnityEngine.UI;

public class SistemaEmailPuzzle : MonoBehaviour
{
    [Header("Telas do Sistema")]
    [SerializeField] private GameObject painelGlitchGameOver; 
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private GameObject corpoDoEmail; 
    [SerializeField] private GameObject painelCaixaVazia; 

    [Header("Elementos Interativos")]
    [SerializeField] private Button botaoLinkArmadilha; 
    [SerializeField] private Button[] botoesDeErro; 

    private int errosEncontrados = 0;
    private int totalErros;
    
    // TRAVA DE SEGURANÇA: Salva se o puzzle já foi finalizado nesta sessão
    private bool puzzleConcluido = false;

    private void OnEnable()
    {
        // Se o jogador já completou o puzzle, força a tela a ficar vazia e ignora o resto
        if (puzzleConcluido)
        {
            if (corpoDoEmail != null) corpoDoEmail.SetActive(false);
            if (painelCaixaVazia != null) painelCaixaVazia.SetActive(true);
            painelGlitchGameOver.SetActive(false);
            painelVitoria.SetActive(false);
            return; // Corta a execução aqui
        }

        // Reset normal do jogo caso ele ainda esteja tentando passar do puzzle
        totalErros = botoesDeErro.Length;
        errosEncontrados = 0;
        
        painelGlitchGameOver.SetActive(false);
        painelVitoria.SetActive(false);
        
        if (corpoDoEmail != null) corpoDoEmail.SetActive(true);
        if (painelCaixaVazia != null) painelCaixaVazia.SetActive(false);

        botaoLinkArmadilha.interactable = true;
        botaoLinkArmadilha.onClick.RemoveAllListeners();
        botaoLinkArmadilha.onClick.AddListener(ClicarNaArmadilha);

        foreach (Button btn in botoesDeErro)
        {
            btn.interactable = true;
            
            Color corInvisivel = Color.red;
            corInvisivel.a = 0f;
            btn.GetComponent<Image>().color = corInvisivel;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => EncontrarErro(btn));
        }
    }

    private void ClicarNaArmadilha()
    {
        painelGlitchGameOver.SetActive(true);
        foreach (Button btn in botoesDeErro) btn.interactable = false;
        botaoLinkArmadilha.interactable = false;
    }

    private void EncontrarErro(Button botaoClicado)
    {
        errosEncontrados++;

        Color corMarcada = Color.red;
        corMarcada.a = 0.4f; 
        botaoClicado.GetComponent<Image>().color = corMarcada;

        botaoClicado.interactable = false;

        if (errosEncontrados >= totalErros)
        {
            puzzleConcluido = true; // Ativa a trava para nunca mais resetar

            if (corpoDoEmail != null) corpoDoEmail.SetActive(false); 
            if (painelCaixaVazia != null) painelCaixaVazia.SetActive(true); 
            
            painelVitoria.SetActive(true);
            botaoLinkArmadilha.interactable = false; 
        }
    }
}