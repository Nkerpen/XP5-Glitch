using UnityEngine;
using UnityEngine.UI;

public class GerenciadorDeNarrativa : MonoBehaviour
{
    public static GerenciadorDeNarrativa Instancia;

    [Header("Sistemas")]
    [SerializeField] private SistemaDeNotificacao sistemaNotificacao;
    [SerializeField] private NavegacaoCelular navegacaoCelular;
    
    [Header("Aplicativos (Abertura Direta)")]
    [SerializeField] private GameObject telaAppEmail; 

    [Header("Botões para Simular Clique (E Esconder)")]
    [SerializeField] private Button botaoContatoGrupo;     
    [SerializeField] private Button botaoContatoGolpista;  

    [Header("Travas Visuais (O que começa escondido)")]
    [SerializeField] private GameObject iconeAppEmailNaHome; // O ícone clicável do Email na tela inicial

    private int etapaAtual = 0;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // NOVO: Garante que os ícones comecem desligados (invisíveis)
        if (iconeAppEmailNaHome != null) iconeAppEmailNaHome.SetActive(false);
        if (botaoContatoGolpista != null) botaoContatoGolpista.gameObject.SetActive(false);
    }

    public void IniciarJogo()
    {
        Invoke(nameof(DispararNotificacaoAtual), 1.5f);
    }

    public void AvancarHistoria()
    {
        etapaAtual++;
        Invoke(nameof(DispararNotificacaoAtual), 3f); 
    }

    private void DispararNotificacaoAtual()
    {
        switch (etapaAtual)
        {
            case 0: // Jogo começa -> Notificação do Grupo
                sistemaNotificacao.MostrarNotificacao(
                    "Grupo de Investigação", 
                    "Charles: o John tá online", 
                    () => { if(botaoContatoGrupo != null) botaoContatoGrupo.onClick.Invoke(); }
                );
                break;
                
            case 1: // Jogador conversou -> Notificação do E-mail
                if (iconeAppEmailNaHome != null) iconeAppEmailNaHome.SetActive(true); // DESTRANCA O EMAIL

                sistemaNotificacao.MostrarNotificacao(
                    "Suporte PayPal", 
                    "Sua conta será bloqueada hoje!", 
                    () => navegacaoCelular.AbrirApp(telaAppEmail)
                );
                break;
                
            case 2: // Jogador venceu o e-mail -> Notificação do Grupo (Parte 2)
                sistemaNotificacao.MostrarNotificacao(
                    "Grupo de Investigação", 
                    "Eva: Vocês viram aquele email?", 
                    () => { if(botaoContatoGrupo != null) botaoContatoGrupo.onClick.Invoke(); }
                );
                break;
                
            case 3: // Jogador falou com o grupo -> Notificação do Golpista
                if (botaoContatoGolpista != null) botaoContatoGolpista.gameObject.SetActive(true); // DESTRANCA O GOLPISTA

                sistemaNotificacao.MostrarNotificacao(
                    "Desconhecido", 
                    "Me passe o código agora.", 
                    () => { if(botaoContatoGolpista != null) botaoContatoGolpista.onClick.Invoke(); }
                );
                break;
        }
    }
}