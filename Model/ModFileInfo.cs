using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkyLauncher.FluentCore
{
    public class ModFileInfo : INotifyPropertyChanged
    {
        private string _fileName;
        private string _fullPath;
        private long _fileSize;
        private bool _isEnabled;

        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FullPath
        {
            get => _fullPath;
            set
            {
                if (_fullPath != value)
                {
                    _fullPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public long FileSize
        {
            get => _fileSize;
            set
            {
                if (_fileSize != value)
                {
                    _fileSize = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FormattedSize)); 
                }
            }
        }

        public string FormattedSize => FormatSize(FileSize);

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        // 文件大小格式化
        private static string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}