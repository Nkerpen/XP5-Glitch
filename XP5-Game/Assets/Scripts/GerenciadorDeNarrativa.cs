using UnityEngine;

public class GerenciadorDeNarrativa : MonoBehaviour
{
    public static GerenciadorDeNarrativa Instancia;

    [Header("Sistemas")]
    [SerializeField] private SistemaDeNotificacao sistemaNotificacao;
    [SerializeField] private NavegacaoCelular navegacaoCelular;
    
    [Header("Aplicativos (Para abrir)")]
    [SerializeField] private GameObject telaAppChat; // Arraste a Tela_Aplicativos/Tela_Chat (ou Tela_Contatos)
    [SerializeField] private GameObject telaAppEmail; // Arraste a Tela_Aplicativos/Tela_Email

    private int etapaAtual = 0;

    private void Awake()
    {
        // Garante que só exista um cérebro no jogo
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    // Chamado pelo Loading quando o jogo começa
    public void IniciarJogo()
    {
        Invoke(nameof(DispararNotificacaoAtual), 1.5f);
    }

    // Chamado pelos Puzzles quando o jogador vence/termina o diálogo
    public void AvancarHistoria()
    {
        etapaAtual++;
        Invoke(nameof(DispararNotificacaoAtual), 3f); // Pausa dramática de 3 segundos
    }

    private void DispararNotificacaoAtual()
    {
        switch (etapaAtual)
        {
            case 0:
                sistemaNotificacao.MostrarNotificacao(
                    "Grupo de Investigação", 
                    "Charles: o John tá online", 
                    () => navegacaoCelular.AbrirApp(telaAppChat)
                );
                break;
            case 1:
                sistemaNotificacao.MostrarNotificacao(
                    "Suporte PayPal", 
                    "Sua conta será bloqueada hoje!", 
                    () => navegacaoCelular.AbrirApp(telaAppEmail)
                );
                break;
            case 2:
                sistemaNotificacao.MostrarNotificacao(
                    "Grupo de Investigação", 
                    "Eva: Vocês viram aquele email?", 
                    () => navegacaoCelular.AbrirApp(telaAppChat)
                );
                break;
            case 3:
                sistemaNotificacao.MostrarNotificacao(
                    "Desconhecido", 
                    "Me passe o código agora.", 
                    () => navegacaoCelular.AbrirApp(telaAppChat)
                );
                break;
        }
    }
}