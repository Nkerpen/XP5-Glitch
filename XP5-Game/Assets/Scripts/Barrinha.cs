using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System.Collections;

public class RotatingBarSlider : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Referências")]
    public RectTransform barsContainer;
    public RectTransform barTemplate;

    [Header("UI")]
    public TMP_Text positionText;
    public TMP_Text previewText;
    public TMP_Text objectiveText;

    [Header("Configuração")]
    public int barCount = 25;
    public float spacing = 40f;

    [Header("Visual")]
    public float maxHeight = 100f;
    public float minHeight = 35f;
    public float maxAlpha = 1f;
    public float minAlpha = 0.3f;
    public float maxRotation = 12f;

    [Header("Movimento")]
    public float dragSensitivity = 0.02f;
    public float snapSpeed = 12f;

    [Header("Puzzle - Cifra de César")]
    [TextArea]
    public string encryptedMessage = "PRWT D PCIWDCN.";

    public int correctShift = 15;

    private readonly List<RectTransform> bars =
        new List<RectTransform>();

    private float currentPosition = 0f;
    private float targetPosition = 0f;
    private bool dragging = false;

    private int lastShift = -1;
    private bool isLocked = false;
    private bool successTriggered = false;
    private bool hasBeenSolved = false; // Garante que a trava só aconteça uma vez

    // CORES
    private readonly Color wrongColor = new Color(0.55f, 0.35f, 0.35f, 1f);
    private readonly Color correctColor = new Color(0.05f, 0.35f, 0.1f, 1f); // Verde escuro

    void Start()
    {
        GenerateBars();

        currentPosition = 0f;
        targetPosition = 0f;

        if (previewText != null)
            previewText.text = encryptedMessage;

        if (objectiveText != null)
            objectiveText.text =
                "Descriptografe a nota usando a Cifra de César.";

        UpdateBars();
        UpdatePuzzle();
    }

    void Update()
    {
        if (!dragging && !isLocked)
        {
            currentPosition = Mathf.Lerp(
                currentPosition,
                targetPosition,
                Time.deltaTime * snapSpeed
            );

            if (Mathf.Abs(currentPosition - targetPosition) < 0.001f)
                currentPosition = targetPosition;
        }

        UpdateBars();
        UpdatePuzzle();
    }

    void GenerateBars()
    {
        bars.Clear();

        for (int i = barsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = barsContainer.GetChild(i);

            if (child != barTemplate)
                Destroy(child.gameObject);
        }

        for (int i = 0; i < barCount; i++)
        {
            RectTransform newBar =
                Instantiate(barTemplate, barsContainer);

            newBar.gameObject.SetActive(true);
            bars.Add(newBar);
        }

        barTemplate.gameObject.SetActive(false);
    }

    void UpdateBars()
    {
        float selectionX = 0f;

        for (int i = 0; i < bars.Count; i++)
        {
            RectTransform bar = bars[i];

            float x =
                (i * spacing) -
                (currentPosition * spacing);

            bar.anchoredPosition = new Vector2(x, 0);

            float distance = Mathf.Abs(x - selectionX);

            float normalized =
                Mathf.Clamp01(distance / (spacing * 5f));

            float height =
                Mathf.Lerp(maxHeight, minHeight, normalized);

            bar.sizeDelta = new Vector2(bar.sizeDelta.x, height);

            float angle =
                Mathf.Clamp(-(x / spacing) * maxRotation,
                -maxRotation, maxRotation);

            bar.localRotation = Quaternion.Euler(0f, 0f, angle);

            Image img = bar.GetComponent<Image>();

            if (img != null)
            {
                Color c = img.color;
                c.a = Mathf.Lerp(maxAlpha, minAlpha, normalized);
                img.color = c;
            }

            float scaleX = Mathf.Lerp(1f, 0.7f, normalized);
            float scaleY = Mathf.Lerp(1.4f, 1f, normalized);

            bar.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }

    void UpdatePuzzle()
    {
        int shift = Mathf.RoundToInt(currentPosition);

        if (positionText != null)
            positionText.text = shift.ToString();

        if (shift != lastShift)
        {
            lastShift = shift;

            string decodedText =
                CaesarDecrypt(encryptedMessage, shift);

            if (previewText != null)
            {
                previewText.text = decodedText;
                previewText.DOKill();

                // COR / ERRO DINÂMICO
                Color targetColor =
                    (shift == correctShift)
                    ? correctColor
                    : wrongColor;

                previewText.color = targetColor;

                previewText
                    .DOFade(0.75f, 0.08f)
                    .SetLoops(2, LoopType.Yoyo);
            }
        }

        // ACERTO - Só trigga a trava se nunca tiver sido resolvido antes
        if (!successTriggered && !hasBeenSolved && shift == correctShift)
        {
            StartCoroutine(SuccessRoutine());
        }
    }

    IEnumerator SuccessRoutine()
    {
        successTriggered = true;
        isLocked = true;
        hasBeenSolved = true; // Marca que o "feedback inicial" já aconteceu

        if (objectiveText != null)
            objectiveText.text = "Mensagem descriptografada.";

        // FLASH VERDE ESCURO
        if (previewText != null)
        {
            previewText.DOKill();

            Sequence seq = DOTween.Sequence();

            // Modificado de Color.green para correctColor (seu verde escuro)
            seq.Append(previewText.DOColor(correctColor, 0.15f));
            seq.Append(previewText.DOFade(0.4f, 0.1f));
            seq.Append(previewText.DOFade(1f, 0.1f));
        }

        // Interrompe o arrasto atual se o player estiver segurando a barra
        dragging = false;
        targetPosition = correctShift;

        yield return new WaitForSeconds(2.0f); // 2 segundinhos como planejado

        isLocked = false;
        successTriggered = false;
    }

    string CaesarDecrypt(string input, int shift)
    {
        char[] output = new char[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (char.IsLetter(c))
            {
                char offset = char.IsUpper(c) ? 'A' : 'a';

                int pos = c - offset;
                pos = (pos - shift + 26) % 26;

                output[i] = (char)(offset + pos);
            }
            else
            {
                output[i] = c;
            }
        }

        return new string(output);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        currentPosition -= eventData.delta.x * dragSensitivity;

        currentPosition = Mathf.Clamp(
            currentPosition,
            0f,
            barCount - 1
        );
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        dragging = false;

        targetPosition =
            Mathf.Round(currentPosition);
    }

    public int GetSelectedShift()
    {
        return Mathf.RoundToInt(currentPosition);
    }
}