using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuWidgetComp : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private Button _applyButton;

    [Header("UI Elements/Resolution")]
    [SerializeField] private TMP_Text _resolutionText;
    [SerializeField] private Button _previousResolutionButton;
    [SerializeField] private Button _nextResolutionButton;

    public event Action Opened;
    public event Action Closed;

    private Resolution[] _resolutions;
    private Resolution _selectedResolution;
    private int _selectedResolutionIndex;
    private bool _selectedFullscreenState;
    private CanvasGroup _rootCanvasGroup;

    private void Awake()
    {
        transform.GetChild(0).TryGetComponent(out _rootCanvasGroup);

        InitResolutions();
        InitFullscreen();

        _rootCanvasGroup.DisableWidgetInstant();
    }

    private void Start()
    {
        _fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleClicked);

        _previousResolutionButton.onClick.AddListener(OnPreviousResolutionButtonClicked);
        _nextResolutionButton.onClick.AddListener(OnNextResolutionButtonClicked);

        _applyButton.onClick.AddListener(ApplySettings);
    }

    public void Open()
    {
        _rootCanvasGroup.EnableWidgetTween();
        Opened?.Invoke();
    }

    public void Close()
    {
        _rootCanvasGroup.DisableWidgetTween();
        Closed?.Invoke();
    }

    private void InitResolutions()
    {
        SelectResolution(Screen.currentResolution);
        _resolutions = Screen.resolutions
            .OrderBy(r => r.width)
            .ThenBy(r => r.height)
            .ThenBy(r => r.refreshRateRatio)
            .ToArray();

        for (int i = 0; i < _resolutions.Length; i++)
        {
            Debug.Log($"i: {_resolutions[i]}");
        }

        _selectedResolutionIndex = _resolutions.ToList().IndexOf(_selectedResolution);
    }

    private void InitFullscreen()
    {
        _selectedFullscreenState = Screen.fullScreen;
    }

    private void SelectResolution(Resolution targetResolution)
    {
        _selectedResolution = targetResolution;
        _resolutionText.SetText(FormatResolution(targetResolution));
    }

    private string FormatResolution(Resolution resolution)
    {
        return $"{resolution.width} x {resolution.height}";
    }

    private void OnFullscreenToggleClicked(bool isFullscreen)
    {
        _selectedFullscreenState = isFullscreen;
    }

    private void OnPreviousResolutionButtonClicked()
    {
        _selectedResolutionIndex = Mathf.Max(_selectedResolutionIndex - 1, 0);
        SelectResolution(_resolutions[_selectedResolutionIndex]);
    }

    private void OnNextResolutionButtonClicked()
    {
        _selectedResolutionIndex = Mathf.Min(_selectedResolutionIndex + 1, _resolutions.Length - 1);
        SelectResolution(_resolutions[_selectedResolutionIndex]);
    }

    private void ApplySettings()
    {
        Screen.SetResolution(_selectedResolution.width, _selectedResolution.height, _selectedFullscreenState);
        Close();
    }
}
