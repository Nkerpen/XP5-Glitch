using System.Collections.Generic;
using UnityEngine;

public class NavegacaoCelular : MonoBehaviour
{
    [Header("Configura��es de Telas")]
    [SerializeField] private GameObject telaHome;

    [Tooltip("Arraste aqui o objeto pai 'Tela_Aplicativos'")]
    [SerializeField] private GameObject telaAplicativos;

    private GameObject telaAtual;
    private Stack<GameObject> historicoTelas = new Stack<GameObject>();

    private void Start()
    {
        telaAtual = telaHome;
    }

    // Chame essa fun��o nos bot�es dos aplicativos (ex: Clicou no app de E-mail)
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

        // --- A SOLUÇÃO ENTRA AQUI ---
        // Se o novo app que estamos abrindo fica dentro da Tela_Aplicativos, 
        // precisamos garantir que o PAI (Tela_Aplicativos) seja ligado primeiro!
        if (telaAplicativos != null && novoApp.transform.IsChildOf(telaAplicativos.transform))
        {
            telaAplicativos.SetActive(true);
            telaHome.SetActive(false); // Esconde a home caso estivéssemos lá
        }
        // ------------------------------

        telaAtual = novoApp;
        telaAtual.SetActive(true); // Mostra o novo app
    }

    // Anexe ao bot�o "Seta / Tri�ngulo" da barra inferior
    public void BotaoVoltar()
    {
        if (historicoTelas.Count > 0)
        {
            telaAtual.SetActive(false);

            // Pega a tela do topo do hist�rico
            telaAtual = historicoTelas.Pop();

            // PROTE��O 2: Se mesmo com a prote��o 1 a tela m�e conseguiu entrar na pilha 
            // (por causa do bot�o do puzzle), o 'while' vai limpando at� achar uma tela v�lida
            while (telaAtual == telaAplicativos && historicoTelas.Count > 0)
            {
                telaAtual = historicoTelas.Pop();
            }

            telaAtual.SetActive(true);
        }
    }

    // Anexe ao bot�o "C�rculo" da barra inferior
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

            historicoTelas.Clear(); // Limpa a mem�ria de navega��o
            telaAtual = telaHome;
            telaAtual.SetActive(true);
        }
    }
}