﻿using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.ViewModels
{
  public class MainWindowViewModel : ViewModelBase, IKeyHandler
  {
    private readonly SMPSettings smpSettings;
    private MainViewModel mainViewModel;
    private PlaylistsViewModel playlistsViewModel;
    private MedialibViewModel medialibViewModel;
    private EqualizerViewModel equalizerViewModel;
    private ICommand showOnGitHubCmd;
    private ICommand showEqualizerCommand;
    private ICommand closeEqualizerCommand;
    private CustomWindowPlacementSettings customWindowPlacementSettings;

    public MainWindowViewModel(Dispatcher dispatcher) {
      this.smpSettings = this.ReadSettings();
      this.CustomWindowPlacementSettings = new CustomWindowPlacementSettings(this.smpSettings.MainSettings);
      this.PlayerEngine.Configure(dispatcher, this.smpSettings);
      this.MedialibViewModel = new MedialibViewModel(dispatcher, this.smpSettings);
      this.PlaylistsViewModel = new PlaylistsViewModel(dispatcher, this.smpSettings);
      this.MainViewModel = new MainViewModel(dispatcher) {
        PlayControlViewModel = new PlayControlViewModel(dispatcher, this.smpSettings, this.PlaylistsViewModel, this.MedialibViewModel),
        PlayInfoViewModel = new PlayInfoViewModel(dispatcher)
      };
    }

    public CustomWindowPlacementSettings CustomWindowPlacementSettings {
      get { return customWindowPlacementSettings; }
      set {
        if (Equals(value, this.customWindowPlacementSettings)) {
          return;
        }
        this.customWindowPlacementSettings = value;
        this.OnPropertyChanged(() => this.CustomWindowPlacementSettings);
      }
    }

    public PlayerEngine PlayerEngine {
      get { return PlayerEngine.Instance; }
    }

    public SMPSettings SMPSettings {
      get { return this.smpSettings; }
    }

    private SMPSettings ReadSettings() {
      if (File.Exists(SMPSettings.SettingsFile)) {
        var jsonString = File.ReadAllText(SMPSettings.SettingsFile);
        return JsonConvert.DeserializeObject<SMPSettings>(jsonString);
      }
      return SMPSettings.GetEmptySettings();
    }

    private void WriteSettings(SMPSettings settings) {
      var settingsAsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
      File.WriteAllText(SMPSettings.SettingsFile, settingsAsJson);
    }

    public void SaveSettings() {
      this.WriteSettings(this.smpSettings);
    }

    public MainViewModel MainViewModel {
      get { return this.mainViewModel; }
      set {
        if (Equals(value, this.mainViewModel)) {
          return;
        }
        this.mainViewModel = value;
        this.OnPropertyChanged(() => this.MainViewModel);
      }
    }

    public PlaylistsViewModel PlaylistsViewModel {
      get { return this.playlistsViewModel; }
      set {
        if (Equals(value, this.playlistsViewModel)) {
          return;
        }
        this.playlistsViewModel = value;
        this.OnPropertyChanged(() => this.PlaylistsViewModel);
      }
    }

    public MedialibViewModel MedialibViewModel {
      get { return this.medialibViewModel; }
      set {
        if (Equals(value, this.medialibViewModel)) {
          return;
        }
        this.medialibViewModel = value;
        this.OnPropertyChanged(() => this.MedialibViewModel);
      }
    }

    public EqualizerViewModel EqualizerViewModel {
      get { return this.equalizerViewModel; }
      set {
        if (Equals(value, this.equalizerViewModel)) {
          return;
        }
        this.equalizerViewModel = value;
        this.OnPropertyChanged(() => this.EqualizerViewModel);
      }
    }

    public ICommand ShowOnGitHubCmd {
      get { return this.showOnGitHubCmd ?? (this.showOnGitHubCmd = new DelegateCommand(this.ShowOnGitHub, () => true)); }
    }

    private void ShowOnGitHub() {
      System.Diagnostics.Process.Start("https://github.com/punker76/simple-music-player");
    }

    public ICommand ShowEqualizerCommand {
      get { return this.showEqualizerCommand ?? (this.showEqualizerCommand = new DelegateCommand(this.ShowEqualizer, this.CanShowEqualizer)); }
    }

    private bool CanShowEqualizer() {
      return this.PlayerEngine.Initializied;
    }

    private void ShowEqualizer() {
      this.EqualizerViewModel = new EqualizerViewModel(this.PlayerEngine.Equalizer);
    }

    public ICommand CloseEqualizerCommand {
      get { return this.closeEqualizerCommand ?? (this.closeEqualizerCommand = new DelegateCommand(this.CloseEqualizer, this.CanCloseEqualizer)); }
    }

    private bool CanCloseEqualizer() {
      return true;
    }

    private void CloseEqualizer() {
      if (this.EqualizerViewModel.Equalizer.IsEnabled) {
        this.EqualizerViewModel.Equalizer.SaveEqualizerSettings();
      }
      this.EqualizerViewModel.Equalizer = null;
      this.EqualizerViewModel = null;
    }

    public bool HandleKeyDown(Key key) {
      if (this.MainViewModel.PlayControlViewModel.HandleKeyDown(key)) {
        return true;
      }
      if (key == Key.E && this.ShowEqualizerCommand.CanExecute(null)) {
        this.ShowEqualizerCommand.Execute(null);
        return true;
      }
      return false;
    }
  }
}