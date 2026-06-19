using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using UnityEngine.InputSystem; // Importante para o Novo Input System da Unity

public class AddContatoManager : MonoBehaviour
{
    [Header("Botão de Ativação")]
    [SerializeField] private Button botaoAddContato; // Arraste o botão "+" do topo aqui

    [Header("Paineis de UI")]
    [SerializeField] private GameObject painelMaeAddContatoScreen; // O objeto "AddContatoScreen" inteiro
    [SerializeField] private GameObject painelPretoDoShake; // O quadrado preto interno que vai tremer

    [Header("UI Elementos")]
    [SerializeField] private TMP_InputField[] numeroInputs; // Arraste os 4 inputs na ordem (0 a 3)

    [Header("Contatos (Hierarquia Unity)")]
    [SerializeField] private GameObject contatoBloqueadoAnthony; // Requer um componente 'Canvas Group'
    [SerializeField] private GameObject contatoDesbloqueadoAnthony; // Requer um componente 'Canvas Group'

    private string codigoCorreto = "8345";
    private bool estaVerificando = false;

    void Start()
    {
        // Vincula o clique do botão "+" à função de abrir a tela
        if (botaoAddContato != null)
        {
            botaoAddContato.onClick.AddListener(AbrirTelaAddContato);
        }

        // Configura os listeners de mudança de texto dos inputs
        for (int i = 0; i < numeroInputs.Length; i++)
        {
            int index = i;
            numeroInputs[i].onValueChanged.AddListener(delegate { AoDigitar(index); });
        }

        // Garante que a tela comece FECHADA ao iniciar o jogo
        if (painelMaeAddContatoScreen != null)
        {
            painelMaeAddContatoScreen.SetActive(false);
        }
    }

    void Update()
    {
        // Se a tela mãe não estiver ativa, não precisa monitorar o teclado
        if (painelMaeAddContatoScreen == null || !painelMaeAddContatoScreen.activeSelf) return;

        // SUPORTE PARA PC/NOTE (Novo Input System): Detecta Backspace para voltar o quadrado
        if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            for (int i = 1; i < numeroInputs.Length; i++)
            {
                if (numeroInputs[i].isFocused)
                {
                    if (string.IsNullOrEmpty(numeroInputs[i].text))
                    {
                        numeroInputs[i - 1].text = "";
                        numeroInputs[i - 1].ActivateInputField();
                    }
                    break;
                }
            }
        }
    }

    public void AbrirTelaAddContato()
    {
        if (painelMaeAddContatoScreen != null)
        {
            // Reseta a opacidade da tela mãe caso use Canvas Group nela
            CanvasGroup cgTelaCodigo = painelMaeAddContatoScreen.GetComponent<CanvasGroup>();
            if (cgTelaCodigo != null) cgTelaCodigo.alpha = 1f;

            painelMaeAddContatoScreen.SetActive(true);
            StartCoroutine(FocarPrimeiroCampoComDelay());
        }
    }

    IEnumerator FocarPrimeiroCampoComDelay()
    {
        LimparCampos();
        yield return new WaitForEndOfFrame();
        if (numeroInputs.Length > 0 && numeroInputs[0] != null)
        {
            numeroInputs[0].ActivateInputField();
        }
    }

    void AoDigitar(int index)
    {
        if (estaVerificando) return;

        // Avanço automático ao digitar
        if (numeroInputs[index].text.Length >= 1)
        {
            if (numeroInputs[index].text.Length > 1)
            {
                numeroInputs[index].text = numeroInputs[index].text.Substring(0, 1);
            }

            if (index < numeroInputs.Length - 1)
            {
                numeroInputs[index + 1].ActivateInputField();
            }
        }

        // Se preencheu os 4 campos, roda a validação
        if (VerificarCamposPreenchidos())
        {
            ChecarCodigo();
        }
    }

    bool VerificarCamposPreenchidos()
    {
        foreach (var input in numeroInputs)
        {
            if (string.IsNullOrEmpty(input.text)) return false;
        }
        return true;
    }

    void ChecarCodigo()
    {
        string codigoDigitado = numeroInputs[0].text + numeroInputs[1].text + numeroInputs[2].text + numeroInputs[3].text;

        if (codigoDigitado == codigoCorreto)
        {
            Sucesso();
        }
        else
        {
            StartCoroutine(ErroEFeedback());
        }
    }

    void Sucesso()
    {
        // 1. Esconde a tela de digitar o ID usando Fade Out do DOTween
        CanvasGroup cgTelaCodigo = painelMaeAddContatoScreen.GetComponent<CanvasGroup>();
        if (cgTelaCodigo != null)
        {
            cgTelaCodigo.DOFade(0f, 0.25f).OnComplete(() => {
                painelMaeAddContatoScreen.SetActive(false);
            });
        }
        else
        {
            painelMaeAddContatoScreen.SetActive(false);
        }

        // 2. Coleta os Canvas Groups dos cards dos contatos
        CanvasGroup cgBloqueado = contatoBloqueadoAnthony.GetComponent<CanvasGroup>();
        CanvasGroup cgDesbloqueado = contatoDesbloqueadoAnthony.GetComponent<CanvasGroup>();

        // Fail-safe: Caso falte o componente Canvas Group na Unity, avisa e faz a troca seca padrão
        if (cgBloqueado == null || cgDesbloqueado == null)
        {
            Debug.LogWarning("⚠️ Falta adicionar o componente 'Canvas Group' nos cards do Anthony para ver o efeito!");
            contatoBloqueadoAnthony.SetActive(false);
            contatoDesbloqueadoAnthony.SetActive(true);

            // Se der ruim no componente visual, ainda assim precisamos empurrar a história para a Etapa 5
            if (GerenciadorDeNarrativa.Instancia != null)
            {
                GerenciadorDeNarrativa.Instancia.AvancarHistoria();
            }
            return;
        }

        // 3. Preparação do card desbloqueado (ativa invisível para a fusão suave)
        contatoDesbloqueadoAnthony.SetActive(true);
        cgDesbloqueado.alpha = 0f;
        cgBloqueado.alpha = 1f;

        // Dá um leve feedback de pulo "yoyo" no card antigo indicando que começou a decodificação
        contatoBloqueadoAnthony.transform.DOScale(1.05f, 0.15f).SetLoops(2, LoopType.Yoyo);

        // 4. CROSSFADE (Efeito de fusão visual)
        // O bloqueado vai sumindo e desativa de vez no final da animação
        cgBloqueado.DOFade(0f, 0.6f).SetDelay(0.2f).OnComplete(() => {
            contatoBloqueadoAnthony.SetActive(false);
            contatoBloqueadoAnthony.transform.localScale = Vector3.one;

            // LINK DO GATILHO DA NARRATIVA:
            // Assim que o card antigo termina de sumir e o novo se consolida, a história vai para a Etapa 5!
            if (GerenciadorDeNarrativa.Instancia != null)
            {
                Debug.Log("[ADD CONTATO] Sucesso! Anthony liberado. Avançando para a Etapa 5.");
                GerenciadorDeNarrativa.Instancia.AvancarHistoria();
            }
        });

        // O desbloqueado vai aparecendo suavemente no mesmo ritmo
        cgDesbloqueado.DOFade(1f, 0.6f).SetDelay(0.2f);
    }

    IEnumerator ErroEFeedback()
    {
        estaVerificando = true;

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        Handheld.Vibrate();
#endif

        // Animação de tremor (Shake) usando o DOTween especificamente no painel do quadrado preto
        painelPretoDoShake.transform.DOShakePosition(0.4f, new Vector3(15f, 0f, 0f), 20, 90, false, true);

        // Aguarda a animação de erro acabar antes de limpar os campos
        yield return new WaitForSeconds(0.4f);

        yield return FocarPrimeiroCampoComDelay();
        estaVerificando = false;
    }

    void LimparCampos()
    {
        foreach (var input in numeroInputs)
        {
            if (input != null) input.text = "";
        }
    }
}