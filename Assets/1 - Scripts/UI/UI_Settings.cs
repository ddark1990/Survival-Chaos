using Doozy.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;

namespace JankTown
{
    public class UI_Settings : MonoBehaviour
    {
        public static UI_Settings Instance;

        [Header("ref")]
        [SerializeField] UIView settingsMenuView;
        //text label for mouse sensitivity
        [SerializeField] TextMeshProUGUI mouseSensitivityText;
        //text label for mouse smooth
        [SerializeField] TextMeshProUGUI mouseSmoothText;
        [SerializeField] TextMeshProUGUI volumeText;

        //slider for mouse sensitivity
        public Slider mouseSensitivitySlider;
        //slider for mouse smooth
        public Slider mouseSmoothSlider;
        [SerializeField] TMP_Dropdown resolutionDropdown;
        [SerializeField] TMP_Dropdown qualityDropdown;
        [SerializeField] TMP_Dropdown fullScreenModeDropdown;
        [SerializeField] TMP_Dropdown fpsDropDown;

        bool _isOpen;

        public static event Action OnSettingsOpen;
        public static event Action<bool> OnSettingsOpenBool;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            PopulateResolutionDropdown();
            PopulateQualityDropdown();
            PopulateFullscreenModeDropdown();

            mouseSensitivitySlider.value = PlayerPrefs.GetFloat("Mouse Sensitivity");
            mouseSmoothSlider.value = PlayerPrefs.GetFloat("Mouse Smooth");
            
            resolutionDropdown.value = PlayerPrefs.GetInt("Resolution");
            qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel");
            fullScreenModeDropdown.value = PlayerPrefs.GetInt("FullScreenMode");
            fpsDropDown.value = PlayerPrefs.GetInt("TargetFPSOptionIndex");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_isOpen)
                {
                    settingsMenuView.Hide();
                    _isOpen = false;
                }
                else
                {
                    settingsMenuView.Show();
                    _isOpen = true;
                }

                OnSettingsOpen?.Invoke();
                OnSettingsOpenBool?.Invoke(_isOpen);
            }
        }

        public bool IsOpen()
        {
            return _isOpen;
        }

        public void ToggleSettingsMenu()
        {
            _isOpen = !_isOpen;

            if (_isOpen)
                settingsMenuView.Show();
            else
                settingsMenuView.Hide();
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = volume;

            volumeText.text = volume.ToString();
        }

        public void SetTargetFPS(int optionIndex)
        {
            PlayerPrefs.SetInt("TargetFPSOptionIndex", optionIndex);

            switch (optionIndex)
            {
                case 0:
                    Application.targetFrameRate = 30;
                    break;
                case 1:
                    Application.targetFrameRate = 60;
                    break;
                case 2:
                    Application.targetFrameRate = 120;
                    break;
                case 3:
                    Application.targetFrameRate = 240;
                    break;
            }
        }

        public void SetQuality(int qualityIndex)
        {
            PlayerPrefs.SetInt("QualityLevel", qualityIndex);

            QualitySettings.SetQualityLevel(qualityIndex);
        }

        public void SetFullscreenMode(int modeIndex)
        {
            PlayerPrefs.SetInt("FullScreenMode", modeIndex);

            Screen.fullScreenMode = (FullScreenMode)modeIndex;
        }

        public void SetResolution(int resolutionIndex)
        {
            PlayerPrefs.SetInt("Resolution", resolutionIndex);

            Resolution resolution = Screen.resolutions[resolutionDropdown.value];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            PlayerPrefs.SetFloat("Mouse Sensitivity", sensitivity);
            
            mouseSensitivityText.text = sensitivity.ToString();
        }

        public void SetMouseSmooth(float smooth)
        {
            PlayerPrefs.SetFloat("Mouse Smooth", smooth);
            
            mouseSmoothText.text = smooth.ToString();
        }

        private void PopulateResolutionDropdown()
        {
            resolutionDropdown.ClearOptions();

            List<string> options = new List<string>();

            int currentResolutionIndex = 0;
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                //string option = Screen.resolutions[i].width + " x " + Screen.resolutions[i].height;
                string option = Screen.resolutions[i].ToString();
                options.Add(option);

                if (Screen.resolutions[i].width == Screen.currentResolution.width && Screen.resolutions[i].height == Screen.currentResolution.height)
                    currentResolutionIndex = i;
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.RefreshShownValue();
        }
        
        private void PopulateQualityDropdown()
        {
            qualityDropdown.ClearOptions();

            List<string> options = new List<string>();

            int currentQualityIndex = 0;
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                string option = QualitySettings.names[i];
                options.Add(option);

                if (QualitySettings.names[i] == QualitySettings.GetQualityLevel().ToString())
                    currentQualityIndex = i;
            }

            qualityDropdown.AddOptions(options);
            qualityDropdown.RefreshShownValue();
        }

        private void PopulateFullscreenModeDropdown()
        {
            fullScreenModeDropdown.ClearOptions();

            List<string> options = new List<string>();

            int currentModeIndex = 0;
            for (int i = 0; i < Enum.GetNames(typeof(FullScreenMode)).Length; i++)
            {
                string option = Enum.GetNames(typeof(FullScreenMode))[i];
                options.Add(option);

                if (Enum.GetNames(typeof(FullScreenMode))[i] == Screen.fullScreenMode.ToString())
                    currentModeIndex = i;
            }

            fullScreenModeDropdown.AddOptions(options);
            fullScreenModeDropdown.RefreshShownValue();
        }
    }
}