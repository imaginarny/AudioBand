﻿using AudioBand.Models;
using AudioBand.Settings;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AudioBand.Commands;

namespace AudioBand.UI
{
    /// <summary>
    /// ViewModel for ProfileRepo.
    /// </summary>
    public class ProfileRepoViewModel : ViewModelBase
    {
        private readonly IAppSettings _appSettings;
        private readonly GitHubHelper _gitHub;

        private readonly AudioBandSettings _model = new AudioBandSettings();
        private readonly AudioBandSettings _backup = new AudioBandSettings();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileRepoViewModel"/> class.
        /// </summary>
        /// <param name="appSettings">The app's settings.</param>
        /// /// <param name="gitHub">The GitHub communicator.</param>
        public ProfileRepoViewModel(IAppSettings appSettings, GitHubHelper gitHub)
        {
            _appSettings = appSettings;
            _gitHub = gitHub;

            AvailableProfiles = new ObservableCollection<CommunityProfile>(_gitHub.GetCommunityProfiles().GetAwaiter().GetResult());
            InstallButtonCommand = new RelayCommand<string>(OnInstallButtonCommand);
        }

        /// <summary>
        /// Gets or sets the list of available profiles.
        /// </summary>
        public ObservableCollection<CommunityProfile> AvailableProfiles { get; set; }

        /// <summary>
        /// Gets or sets the install button command.
        /// </summary>
        public ICommand InstallButtonCommand { get; set; }

        /// <inheritdoc />
        protected override void OnReset()
        {
            base.OnReset();
            ResetObject(_model);
        }

        /// <inheritdoc />
        protected override void OnBeginEdit()
        {
            base.OnBeginEdit();
            MapSelf(_model, _backup);
        }

        /// <inheritdoc />
        protected override void OnCancelEdit()
        {
            base.OnCancelEdit();
            MapSelf(_backup, _model);
        }

        /// <inheritdoc />
        protected override void OnEndEdit()
        {
            base.OnEndEdit();
            MapSelf(_model, _appSettings.AudioBandSettings);
        }

        private void OnInstallButtonCommand(string name)
        {
            AvailableProfiles[1].IsInstalled = false;
            var profiles = AvailableProfiles.ToArray();

            AvailableProfiles.Clear();
            AvailableProfiles = new ObservableCollection<CommunityProfile>(profiles);
            RaisePropertyChangedAll();
        }
    }
}
