﻿using System.IO;
using System.Linq;
using System.Windows;
using MVVMFramework.ViewNavigator;
using MVVMFramework.Views;
using VideoEditorUi.ViewModels;

namespace VideoEditorUi.Views
{
    /// <summary>
    /// Interaction logic for ConverterView.xaml
    /// </summary>
    public partial class MergerView : ViewBaseControl
    {
        private MergerViewModel viewModel;
        public MergerView() : base(Navigator.Instance.CurrentViewModel)
        {
            InitializeComponent();
            viewModel = Navigator.Instance.CurrentViewModel as MergerViewModel;
        }

        private void ImagePanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (!files.Any(f => FormatTypeViewModel.IsVideoFile($".{Path.GetExtension(f)}")))
                    MessageBox.Show("cant add non video");
                else
                    viewModel.DragFiles?.Invoke(files);
            }
        }
    }
}
