using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UI_ButtonHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _buttonImage;
    private TextMeshProUGUI _text;

    [Header("Yazý Renk Ayarlarý")]
    public Color normalTextColor = Color.white;
    public Color hoverTextColor = Color.black;

    [Header("Görsel (Sprite) Ayarlarý")]
    public Sprite hoverSprite;

    [Header("Yumuþak Geçiþ (Fade) Ayarlarý")]
    [Range(1f, 20f)] public float fadeSpeed = 12f; // Deðer büyüdükçe fade hýzlanýr

    private Color _targetImageColor;
    private Color _targetTextColor;
    private bool _isHovered = false;

    private void Awake()
    {
        _buttonImage = GetComponent<Image>();
        _text = GetComponentInChildren<TextMeshProUGUI>();

        // Ýlk baþta hedef renkleri varsayýlan (gizli) olarak belirle
        _targetImageColor = new Color(1f, 1f, 1f, 0f);
        _targetTextColor = normalTextColor;
    }

    private void Update()
    {
        // Her karede mevcut renkleri hedef renklere doðru pürüzsüzce yaklaþtýr (Fade efekti)
        if (_buttonImage != null)
        {
            _buttonImage.color = Color.Lerp(_buttonImage.color, _targetImageColor, Time.deltaTime * fadeSpeed);
        }

        if (_text != null)
        {
            _text.color = Color.Lerp(_text.color, _targetTextColor, Time.deltaTime * fadeSpeed);
        }
    }

    private void OnEnable()
    {
        ResetButtonInstant();
    }

    private void OnDisable()
    {
        ResetButtonInstant();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;

        if (_buttonImage != null && hoverSprite != null)
        {
            _buttonImage.sprite = hoverSprite;
            _targetImageColor = new Color(1f, 1f, 1f, 1f); // Hedef: Tam görünür arka plan
        }

        _targetTextColor = hoverTextColor; // Hedef: Siyah yazý
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;

        _targetImageColor = new Color(1f, 1f, 1f, 0f); // Hedef: Þeffaf arka plan
        _targetTextColor = normalTextColor; // Hedef: Beyaz yazý
    }

    // Menü kapandýðýnda veya açýldýðýnda arayüzün kaymamasý için anýnda sýfýrlayan emniyet fonksiyonu
    private void ResetButtonInstant()
    {
        _isHovered = false;
        _targetImageColor = new Color(1f, 1f, 1f, 0f);
        _targetTextColor = normalTextColor;

        if (_buttonImage != null)
        {
            _buttonImage.color = _targetImageColor;
            _buttonImage.sprite = null;
        }

        if (_text != null)
        {
            _text.color = _targetTextColor;
        }
    }
}