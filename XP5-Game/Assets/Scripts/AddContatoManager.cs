using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using UnityEngine.InputSystem;

public class AddContatoManager : MonoBehaviour
{
    [Header("Botão de Ativação")]
    [SerializeField] private Button botaoAddContato;

    [Header("Paineis de UI")]
    [SerializeField] private GameObject painelMaeAddContatoScreen;
    [SerializeField] private GameObject painelPretoDoShake;

    [Header("UI Elementos")]
    [SerializeField] private TMP_InputField[] numeroInputs;

    [Header("Efeito de Brilho (Glow)")]
    [SerializeField] private Image[] glowImages;
    [SerializeField] private float duracaoPulsoGlow = 0.8f;

    [Header("Contatos (Hierarquia Unity)")]
    [SerializeField] private GameObject contatoBloqueadoAnthony;
    [SerializeField] private GameObject contatoDesbloqueadoAnthony;

    private string codigoCorreto = "8345";
    private bool estaVerificando = false;
    private Coroutine mudancaFocoCoroutine;

    void Start()
    {
        if (botaoAddContato != null)
        {
            botaoAddContato.onClick.AddListener(AbrirTelaAddContato);
        }

        for (int i = 0; i < numeroInputs.Length; i++)
        {
            int index = i;
            numeroInputs[i].onValueChanged.AddListener(delegate { AoDigitar(index); });
        }

        if (painelMaeAddContatoScreen != null)
        {
            painelMaeAddContatoScreen.SetActive(false);
        }

        DesativarBrilho();
    }

    void Update()
    {
        if (painelMaeAddContatoScreen == null || !painelMaeAddContatoScreen.activeSelf) return;

        if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            for (int i = 1; i < numeroInputs.Length; i++)
            {
                if (numeroInputs[i].isFocused)
                {
                    if (string.IsNullOrEmpty(numeroInputs[i].text))
                    {
                        numeroInputs[i - 1].text = "";
                        MudarFocoPara(i - 1);
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
            CanvasGroup cgTelaCodigo = painelMaeAddContatoScreen.GetComponent<CanvasGroup>();
            if (cgTelaCodigo != null) cgTelaCodigo.alpha = 1f;

            painelMaeAddContatoScreen.SetActive(true);
            StartCoroutine(FocarPrimeiroCampoComDelay());
            IniciarBrilhoPulsante();
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
                MudarFocoPara(index + 1);
            }
        }

        if (VerificarCamposPreenchidos())
        {
            ChecarCodigo();
        }
    }

    // Método auxiliar para transferir o foco sem fechar o teclado mobile
    void MudarFocoPara(int próximoIndex)
    {
        if (mudancaFocoCoroutine != null) StopCoroutine(mudancaFocoCoroutine);
        mudancaFocoCoroutine = StartCoroutine(MudarFocoCoroutine(próximoIndex));
    }

    IEnumerator MudarFocoCoroutine(int próximoIndex)
    {
        // Aguarda o fim do frame atual para que a Unity processe a mudança de texto interna
        yield return new WaitForEndOfFrame();

        if (próximoIndex >= 0 && próximoIndex < numeroInputs.Length)
        {
            numeroInputs[próximoIndex].ActivateInputField();
            // Garante a seleção total do texto interno (caso já tenha algo) para o teclado continuar ativo
            numeroInputs[próximoIndex].Select();
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
        DesativarBrilho();

        // Desfoca o input ativo para fechar o teclado mobile de forma limpa no sucesso
        foreach (var input in numeroInputs)
        {
            if (input.isFocused) input.DeactivateInputField();
        }

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

        CanvasGroup cgBloqueado = contatoBloqueadoAnthony.GetComponent<CanvasGroup>();
        CanvasGroup cgDesbloqueado = contatoDesbloqueadoAnthony.GetComponent<CanvasGroup>();

        if (cgBloqueado == null || cgDesbloqueado == null)
        {
            Debug.LogWarning("⚠️ Falta adicionar o componente 'Canvas Group' nos cards do Anthony para ver o efeito!");
            contatoBloqueadoAnthony.SetActive(false);
            contatoDesbloqueadoAnthony.SetActive(true);

            if (GerenciadorDeNarrativa.Instancia != null)
            {
                GerenciadorDeNarrativa.Instancia.AvancarHistoria();
            }
            return;
        }

        contatoDesbloqueadoAnthony.SetActive(true);
        cgDesbloqueado.alpha = 0f;
        cgBloqueado.alpha = 1f;

        contatoBloqueadoAnthony.transform.DOScale(1.05f, 0.15f).SetLoops(2, LoopType.Yoyo);

        cgBloqueado.DOFade(0f, 0.6f).SetDelay(0.2f).OnComplete(() => {
            contatoBloqueadoAnthony.SetActive(false);
            contatoBloqueadoAnthony.transform.localScale = Vector3.one;

            if (GerenciadorDeNarrativa.Instancia != null)
            {
                Debug.Log("[ADD CONTATO] Sucesso! Anthony liberado. Avançando para a Etapa 5.");
                GerenciadorDeNarrativa.Instancia.AvancarHistoria();
            }
        });

        cgDesbloqueado.DOFade(1f, 0.6f).SetDelay(0.2f);
    }

    IEnumerator ErroEFeedback()
    {
        estaVerificando = true;

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        Handheld.Vibrate();
#endif

        painelPretoDoShake.transform.DOShakePosition(0.4f, new Vector3(15f, 0f, 0f), 20, 90, false, true);

        yield return new WaitForSeconds(0.4f);

        yield return FocarPrimeiroCampoComDelay();
        estaVerificando = false;

        IniciarBrilhoPulsante();
    }

    void LimparCampos()
    {
        foreach (var input in numeroInputs)
        {
            if (input != null) input.text = "";
        }
    }

    private void IniciarBrilhoPulsante()
    {
        foreach (var glow in glowImages)
        {
            if (glow != null)
            {
                glow.DOKill();
                glow.DOFade(0.1f, 0f);
                glow.DOFade(0.8f, duracaoPulsoGlow).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
        }
    }

    private void DesativarBrilho()
    {
        foreach (var glow in glowImages)
        {
            if (glow != null)
            {
                glow.DOKill();
                glow.DOFade(0f, 0.1f);
            }
        }
    }
}