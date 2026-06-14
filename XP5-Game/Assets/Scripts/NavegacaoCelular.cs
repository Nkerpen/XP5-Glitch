using System.Collections.Generic;
using UnityEngine;

public class NavegacaoCelular : MonoBehaviour
{
    [Header("Configurações de Telas")]
    [SerializeField] private GameObject telaHome;

    [Tooltip("Arraste aqui o objeto pai 'Tela_Aplicativos'")]
    [SerializeField] private GameObject telaAplicativos;

    private GameObject telaAtual;
    private Stack<GameObject> historicoTelas = new Stack<GameObject>();

    private void Start()
    {
        telaAtual = telaHome;
    }

    // Chame essa função nos botões dos aplicativos (ex: Clicou no app de E-mail)
    public void AbrirApp(GameObject novoApp)
    {
        if (telaAtual != null)
        {
            // PROTEÇÃO 1: Só salva no histórico se a tela atual NÃO for a tela mãe/container
            if (telaAtual != telaAplicativos)
            {
                historicoTelas.Push(telaAtual); // Salva a tela atual na pilha
            }

            // Só desativa a tela atual se ela NÃO for a tela mãe
            if (telaAtual != telaAplicativos)
            {
                telaAtual.SetActive(false); // Esconde a tela atual
            }
        }

        telaAtual = novoApp;
        telaAtual.SetActive(true); // Mostra o novo app
    }

    // Anexe ao botão "Seta / Triângulo" da barra inferior
    public void BotaoVoltar()
    {
        if (historicoTelas.Count > 0)
        {
            telaAtual.SetActive(false);

            // Pega a tela do topo do histórico
            telaAtual = historicoTelas.Pop();

            // PROTEÇÃO 2: Se mesmo com a proteção 1 a tela mãe conseguiu entrar na pilha 
            // (por causa do botão do puzzle), o 'while' vai limpando até achar uma tela válida
            while (telaAtual == telaAplicativos && historicoTelas.Count > 0)
            {
                telaAtual = historicoTelas.Pop();
            }

            telaAtual.SetActive(true);
        }
    }

    // Anexe ao botão "Círculo" da barra inferior
    public void BotaoHome()
    {
        if (telaAtual != telaHome)
        {
            telaAtual.SetActive(false);

            // Garante que se um app filho estava aberto dentro da Tela_Aplicativos, ele fecha
            if (telaAtual.transform.IsChildOf(telaAplicativos.transform))
            {
                telaAtual.SetActive(false);
            }

            historicoTelas.Clear(); // Limpa a memória de navegação
            telaAtual = telaHome;
            telaAtual.SetActive(true);
        }
    }
}