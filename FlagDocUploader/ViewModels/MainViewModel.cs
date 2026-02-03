using FlagDocUploader.Helpers;
using FlagDocUploader.Models;
using FlagDocUploader.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FlagDocUploader.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IZipProcessingService _zipProcessingService;
        private readonly IDocumentService _service;
        private readonly IFolderService _folderService;
        private readonly ILogger<MainViewModel> _logger;
        private CancellationTokenSource? _cancellationTokenSource;

        // Properties
        private string _selectedFilePath = string.Empty;
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set => SetProperty(ref _selectedFilePath, value);
        }

        private int _workspaceId;
        public int WorkspaceId
        {
            get => _workspaceId;
            set => SetProperty(ref _workspaceId, value);
        }

        private int _categoryId;
        public int CategoryId
        {
            get => _categoryId;
            set => SetProperty(ref _categoryId, value);
        }

        private string _categoryName = "Circular";
        public string CategoryName
        {
            get => _categoryName;
            set => SetProperty(ref _categoryName, value);
        }

        private int? _subCategoryId;
        public int? SubCategoryId
        {
            get => _subCategoryId;
            set => SetProperty(ref _subCategoryId, value);
        }

        private string _subCategoryName = "Circular Flag";
        public string SubCategoryName
        {
            get => _subCategoryName;
            set => SetProperty(ref _subCategoryName, value);
        }

        private int _userId;
        public int UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        private ObservableCollection<FolderItem> _availableFolders = new();
        public ObservableCollection<FolderItem> AvailableFolders
        {
            get => _availableFolders;
            set => SetProperty(ref _availableFolders, value);
        }

        private FolderItem? _selectedFolder;
        public FolderItem? SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (SetProperty(ref _selectedFolder, value))
                {
                    OnPropertyChanged(nameof(CanProcess));
                }
            }
        }

        private bool _isLoadingFolders;
        public bool IsLoadingFolders
        {
            get => _isLoadingFolders;
            set => SetProperty(ref _isLoadingFolders, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                SetProperty(ref _isProcessing, value);
                OnPropertyChanged(nameof(CanProcess));
            }
        }

        private string _currentOperation = "Ready";
        public string CurrentOperation
        {
            get => _currentOperation;
            set => SetProperty(ref _currentOperation, value);
        }

        private int _progressPercentage;
        public int ProgressPercentage
        {
            get => _progressPercentage;
            set => SetProperty(ref _progressPercentage, value);
        }

        private int _totalFolders;
        public int TotalFolders
        {
            get => _totalFolders;
            set => SetProperty(ref _totalFolders, value);
        }

        private int _totalFiles;
        public int TotalFiles
        {
            get => _totalFiles;
            set => SetProperty(ref _totalFiles, value);
        }

        private int _processedFolders;
        public int ProcessedFolders
        {
            get => _processedFolders;
            set => SetProperty(ref _processedFolders, value);
        }

        private int _processedFiles;
        public int ProcessedFiles
        {
            get => _processedFiles;
            set => SetProperty(ref _processedFiles, value);
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _connectionStatus = "Checking connection...";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                SetProperty(ref _isConnected, value);
                OnPropertyChanged(nameof(CanProcess));
            }
        }

        public bool CanProcess => !IsProcessing && IsConnected && !string.IsNullOrEmpty(SelectedFilePath) && SelectedFolder != null;

        // Commands
        public ICommand BrowseCommand { get; }
        public ICommand ProcessCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand TestConnectionCommand { get; }
        public ICommand LoadFoldersCommand { get; } 
        public ICommand RefreshFoldersCommand { get; }

        public MainViewModel(
            IZipProcessingService zipProcessingService,
            IDocumentService service,
            IFolderService folderService,
            ILogger<MainViewModel> logger)
        {
            _zipProcessingService = zipProcessingService;
            _service = service;
            _folderService = folderService;
            _logger = logger;

            BrowseCommand = new RelayCommand(_ => BrowseFile());
            ProcessCommand = new RelayCommand(async _ => await ProcessZipFile(), _ => CanProcess);
            CancelCommand = new RelayCommand(_ => CancelProcessing(), _ => IsProcessing);
            TestConnectionCommand = new RelayCommand(async _ => await TestConnection());
            LoadFoldersCommand = new RelayCommand(async _ => await LoadFolders()); 
            RefreshFoldersCommand = new RelayCommand(async _ => await LoadFolders()); 

            // Test connection on startup
            Task.Run(async () => await TestConnection());
        }

        private void BrowseFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*",
                Title = "Select ZIP File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
                StatusMessage = $"Selected: {Path.GetFileName(SelectedFilePath)}";
            }
        }

        private async Task TestConnection()
        {
            try
            {
                ConnectionStatus = "Testing connection...";
                var connected = await _service.TestConnectionAsync();
                IsConnected = connected;
                ConnectionStatus = connected ? "Connected ✓" : "Connection Failed ✗";

                if (!connected)
                {
                    StatusMessage = "Database connection failed. Please check your connection string.";
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ConnectionStatus = "Connection Error ✗";
                StatusMessage = $"Connection error: {ex.Message}";
                _logger.LogError(ex, "Database connection test failed");
            }
        }
        private async Task LoadFolders()
        {
            IsLoadingFolders = true;
            StatusMessage = "Loading folders...";

            try
            {
                var folders = await _folderService.GetCircularFlagFoldersAsync();
                WorkspaceId = folders.ElementAt(0).WorkspaceId;
                CategoryId = folders.ElementAt(0).CategoryId;
                SubCategoryId = folders.ElementAt(0).SubCategoryId;
                UserId = folders.ElementAt(0).OwnerUserId??1;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableFolders.Clear();

                    if (folders != null && folders.Any())
                    {
                        foreach (var folder in folders)
                        {
                            AvailableFolders.Add(folder);
                        }

                        StatusMessage = $"Loaded {AvailableFolders.Count} folder(s)";
                        _logger.LogInformation($"Loaded {AvailableFolders.Count} Circular Flag folders");
                    }
                    else
                    {
                        StatusMessage = "No Circular Flag folders found";
                        _logger.LogWarning("No Circular Flag folders found in database");
                    }
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading folders: {ex.Message}";
                _logger.LogError(ex, "Error loading folders");

                MessageBox.Show(
                    $"Failed to load folders: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoadingFolders = false;
            }
        }
        private async Task ProcessZipFile()
        {
            if (string.IsNullOrEmpty(SelectedFilePath) || !File.Exists(SelectedFilePath))
            {
                MessageBox.Show("Please select a valid ZIP file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (WorkspaceId <= 0 || CategoryId <= 0)
            {
                MessageBox.Show("Please enter valid Workspace ID and Category ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IsProcessing = true;
            _cancellationTokenSource = new CancellationTokenSource();

            var progress = new Progress<ProcessingProgress>(p =>
            {
                CurrentOperation = p.CurrentOperation;
                ProgressPercentage = p.ProgressPercentage;
                TotalFolders = p.TotalFolders;
                TotalFiles = p.TotalFiles;
                ProcessedFolders = p.ProcessedFolders;
                ProcessedFiles = p.ProcessedFiles;

                if (!string.IsNullOrEmpty(p.CurrentFile))
                {
                    StatusMessage = $"Processing: {p.CurrentFile}";
                }
            });

            try
            {
                var result = await _zipProcessingService.ProcessZipFileAsync(
                    SelectedFilePath,
                    WorkspaceId,
                    CategoryId,
                    CategoryName,
                    SubCategoryId,
                    SubCategoryName,
                    UserId,
                    SelectedFolder?.FolderId,
                    progress,
                    _cancellationTokenSource.Token);

                if (result.Success)
                {
                    StatusMessage = result.Message;
                    MessageBox.Show(
                        $"{result.Message}\n\nRoot Folder ID: {result.RootFolderId}",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = result.Message;
                    MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _logger.LogError(ex, "Error processing ZIP file");
            }
            finally
            {
                IsProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void CancelProcessing()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancelling...";
        }
    }
}
