﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Microsoft.WindowsAPICodePack.Dialogs;
using MVVMFramework.Localization;
using MVVMFramework.ViewModels;
using VideoEditorUi.Views;
using VideoUtilities;
using static VideoUtilities.Enums;

namespace VideoEditorUi.ViewModels
{
    public class DownloaderViewModel : EditorViewModel
    {
        #region Fields and props

        private string outputPath;
        private RelayCommand addUrlCommand;
        private RelayCommand downloadCommand;
        private RelayCommand removeCommand;
        private RelayCommand selectOutputFolderCommand;
        private string textInput;
        private string selectedFile;
        private bool fileSelected;
        private ObservableCollection<UrlClass> urlCollection;
        private bool extractAudio;

        public string OutputPath
        {
            get => outputPath;
            set => SetProperty(ref outputPath, value);
        }

        public ObservableCollection<UrlClass> UrlCollection
        {
            get => urlCollection;
            set => SetProperty(ref urlCollection, value);
        }

        public string TextInput
        {
            get => textInput;
            set => SetProperty(ref textInput, value);
        }

        public string SelectedFile
        {
            get => selectedFile;
            set
            {
                SetProperty(ref selectedFile, value);
                FileSelected = !string.IsNullOrEmpty(value);
            }
        }

        public bool FileSelected
        {
            get => fileSelected;
            set => SetProperty(ref fileSelected, value);
        }

        public bool ExtractAudio
        {
            get => extractAudio;
            set => SetProperty(ref extractAudio, value);
        }

        public Action AddUrl;

        #endregion

        #region Commands

        public RelayCommand AddUrlCommand => addUrlCommand ?? (addUrlCommand = new RelayCommand(AddUrlCommandExecute, () => true));
        public RelayCommand DownloadCommand => downloadCommand ?? (downloadCommand = new RelayCommand(DownloadCommandExecute, () => UrlCollection?.Count > 0));
        public RelayCommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand(RemoveExecute, () => FileSelected));
        public RelayCommand SelectOutputFolderCommand => selectOutputFolderCommand ?? (selectOutputFolderCommand = new RelayCommand(SelectOutputFolderCommandExecute, () => true));

        #endregion

        #region Labels

        public string MergeLabel => new MergeLabelTranslatable();
        public string AddUrlLabel => new AddUrlLabelTranslatable();
        public string MoveUpLabel => new MoveUpLabelTranslatable();
        public string MoveDownLabel => new MoveDownLabelTranslatable();
        public string RemoveLabel => new RemoveLabelTranslatable();
        public string OutputFormatLabel => $"{new OutputFormatLabelTranslatable()}:";
        public string OutputFolderLabel => new OutputFolderLabelTranslatable();
        public string DownloadLabel => new DownloadLabelTranslatable();
        public string ConvertFormatLabel => new ConvertFormatLabelTranslatable();
        public string ExtractAudioLabel => new DownloadAudioOnlyTranslatable();
        public string TagText => new EnterUrlTranslatable();
        public string AddLabel => new AddTranslatable();
        public string CancelLabel => new CancelTranslatable();

        #endregion

        private static readonly object _lock = new object();

        public override void OnUnloaded()
        {
            UrlCollection.Clear();
            base.OnUnloaded();
        }

        protected override void Initialize()
        {
            AddUrl = () =>
            {
                UrlCollection.Add(new UrlClass(TextInput, IsPlaylist(TextInput)));
                TextInput = string.Empty;
            };
            UrlCollection = new ObservableCollection<UrlClass>();
            BindingOperations.EnableCollectionSynchronization(UrlCollection, _lock);
        }

        private void AddUrlCommandExecute() => new UrlDialogView(this).ShowDialog();

        private void DownloadCommandExecute()
        {
            if (string.IsNullOrEmpty(OutputPath))
            {
                ShowMessage(new MessageBoxEventArgs(new SelectOutputFolderTranslatable(), MessageBoxEventArgs.MessageTypeEnum.Information, MessageBoxButton.OK, MessageBoxImage.Information));
                return;
            }
            var urls = UrlCollection.OrderByDescending(u => u.IsPlaylist);
            VideoEditor = new VideoDownloader(urls.Select(u => (u.Url, u.IsPlaylist)), ExtractAudio, OutputPath);
            Setup(true, UrlCollection.Count, urls.ToList());
            Execute(StageEnum.Primary, new DownloadingLabelTranslatable());
        }

        private void RemoveExecute() => UrlCollection.Remove(UrlCollection.First(u => u.Url == SelectedFile));

        private void SelectOutputFolderCommandExecute()
        {
            var openFolderDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            };

            if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Cancel)
                return;

            OutputPath = openFolderDialog.FileName;
        }

        private bool IsPlaylist(string url) => url.Contains("playlist");

        protected override void FinishedDownload(object sender, FinishedEventArgs e)
        {
            base.FinishedDownload(sender, e);
            var message = e.Cancelled
                ? $"{new OperationCancelledTranslatable()}\n{e.Message}"
                : new VideoSuccessfullyDownloadedTranslatable();
            ShowMessage(new MessageBoxEventArgs(message, MessageBoxEventArgs.MessageTypeEnum.Information, MessageBoxButton.OK, MessageBoxImage.Information));
        }

        protected override void CleanUp()
        {
            UrlCollection.Clear();
            ExtractAudio = false;
            OutputPath = null;
            base.CleanUp();
        }

        public class UrlClass
        {
            public string Url { get; set; }
            public bool IsPlaylist { get; set; }

            public UrlClass(string url, bool isPlaylist)
            {
                Url = isPlaylist || !url.Contains("list") ? url : url.Substring(0, url.IndexOf("list") - 1);
                IsPlaylist = isPlaylist;
            }
        }
    }
}