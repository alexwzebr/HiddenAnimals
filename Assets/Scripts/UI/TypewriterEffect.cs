using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private bool playOnStart = true;

    private TextMeshProUGUI textComponent;
    private string fullText;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        fullText = textComponent.text;
        if (!playOnStart)
        {
            textComponent.text = "";
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartTyping();
        }
    }

    public void StartTyping()
    {
        // Stop any existing typing coroutine
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        textComponent.text = "";
        typingCoroutine = StartCoroutine(TypeText());
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        textComponent.text = fullText;
    }

    private IEnumerator TypeText()
    {
        if (startDelay > 0)
        {
            yield return new WaitForSeconds(startDelay);
        }

        int charIndex = 0;
        while (charIndex < fullText.Length)
        {
            textComponent.text = fullText.Substring(0, charIndex + 1);
            charIndex++;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }

    // Call this if you want to change the text and start typing again
    public void SetNewText(string newText)
    {
        fullText = newText;
        StartTyping();
    }
} 