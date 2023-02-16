// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NCloudMusic3.Models;
using NCloudMusic3.Helpers;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Media.Core;
using System.Text.Json;
using Windows.Devices.PointOfService;
using DotNext;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Pages
{
    class LocalPageData : NotifyPropertyChanged
    {
        private bool isScanning;

        public RangeObservableCollection<string> LocalMusicFolders { get; set; } = new();
        public bool IsFolderListEmpty => LocalMusicFolders.Count == 0;

        public RangeObservableCollection<Music> LocalMusic { get; set; } = new();
        public bool IsLocalMusicEmpty => LocalMusic.Count == 0;

        public bool IsScanning
        {
            get => isScanning; set
            {
                isScanning = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(NotScanning));
            }
        }
        public bool NotScanning
        {
            get => !isScanning; set
            {
                isScanning = !value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsScanning));
            }
        }

        public LocalPageData()
        {
            LocalMusicFolders.CollectionChanged += (s, arg) =>
            {
                RaisePropertyChanged(nameof(IsFolderListEmpty));
                App.Instance.AppConfig.LocalMusicFolders = (s as IEnumerable<string>).ToList();
            };
            LocalMusic.CollectionChanged += (s, arg) =>
            {
                RaisePropertyChanged(nameof(IsLocalMusicEmpty));
            };
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LocalPage : Page
    {
        LocalPageData LocalPageData { get; set; } = new();

        public LocalPage()
        {
            this.InitializeComponent();

            cacheLimit = new(800, async () =>
            {
                var f = await App.Instance.LocalCacheFolder.CreateFileAsync("list.local", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(f, JsonSerializer.Serialize(musicFiles));
            });

            LocalPageData.LocalMusic.CollectionChanged += (s, e) =>
            {
                if ((s as RangeObservableCollection<Music>).Count > 0)
                    cacheLimit.Touch();
            };
        }
        DownTick cacheLimit;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LocalPageData.LocalMusicFolders.AddRange(App.Instance.AppConfig.LocalMusicFolders);

            if (LocalPageData.IsFolderListEmpty)
            {
                dialog.XamlRoot = App.Instance.m_window.Content.XamlRoot;
                var r = await dialog.ShowAsync();
                if (r == ContentDialogResult.None)
                    App.Instance.PageNavigateBack();
                if (r == ContentDialogResult.Primary)
                {
                    var f = await App.Instance.FolderPicker.PickSingleFolderAsync();
                    if (f != null)
                    {
                        LocalPageData.LocalMusicFolders.Add(f.Path);
                        App.Instance.AddMusicLibrary(f);
                    }
                }
            }

            if (!LocalPageData.IsFolderListEmpty)
            {
                //GetChanges();
                GetLocalMusic();
            }
        }
        SortedSet<string> musicFiles = new();
        private void ApplyMusicChanges(IEnumerable<string> files, bool adding = true)
        {

            LocalPageData.IsScanning = true;
            using (LocalPageData.LocalMusic.BatchUpdate())
            {
                if (!adding)
                {
                    foreach (var f in files)
                    {
                        LocalPageData.LocalMusic.Remove(Music.GetLocal(f));
                        musicFiles.Remove(f);
                    }
                }
                else
                {
                    foreach (var f in files)
                    {
                        var create = Music.CreateLocal;
                        var result = create.TryInvoke(f);
                        if (result.TryGet(out var m))
                        {
                            LocalPageData.LocalMusic.Add(m);
                            musicFiles.Add(f);
                        }

                    }
                }

            }
            LocalPageData.IsScanning = false;


        }
        private async Task ApplyAsyncMusicChanges(Task<IEnumerable<StorageFile>> files)
        {
            LocalPageData.IsScanning = true;
            foreach (var file in await files)
            {
                // await Task.Yield() 并不能防止页面卡死
                // 关于我试了各种 await 数据的姿势最后发现 Task.Yield() 并未按预期工作这件事
                await Task.Delay(1);
                var create = Music.CreateLocal;
                var f = file.Path;

                if (create.TryInvoke(f).TryGet(out var m))
                {
                    musicFiles.Add(f);
                    LocalPageData.LocalMusic.Add(m);
                }

            }
            LocalPageData.IsScanning = false;

        }

        private async void GetLocalMusic()
        {
            // 查看缓存
            var ls = await App.Instance.LocalCacheFolder.TryGetItemAsync("list.local");
            if (ls is not null)
            {
                var l = await File.ReadAllTextAsync(ls.Path);
                List<string> toAdd = JsonSerializer.Deserialize<List<string>>(l);


                if (toAdd.Count > 0)
                {

                    ApplyMusicChanges(toAdd);

                    // 检查changes 
                    CollectChanges();
                }
            }
            return;
        }

        ContentDialog dialog = new()
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "需要至少添加一个音乐库以进行下一步，是否添加？",
            PrimaryButtonText = "添加",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
        };

        Task scanTask;
        void FullScan()
        {
            musicFiles.Clear();
            LocalPageData.LocalMusic.Clear();

            scanTask = ApplyAsyncMusicChanges(LocalPageData.LocalMusicFolders.Select(async f =>
            {
                var l = await App.Instance.GetMusicLibrary(f);
                return await ScanFolder(l);
            }).Aggregate(async (f1, f2) =>
            {
                await Task.WhenAll(f1, f2);
                return f1.Result.Concat(f2.Result);
            })).ContinueWith(t =>
            {
                // 在 ApplyAsyncMusicChanges 中使用 Task.Delay 似乎会阻止垃圾回收，此处手动进行回收
                GC.Collect();
            });
        }

        async Task<IEnumerable<StorageFile>> ScanFolder(StorageFolder library)
        {
            return await library.CreateFileQueryWithOptions(new(Windows.Storage.Search.CommonFileQuery.DefaultQuery, new List<string> { ".flac", ".mp3", ".ncm", ".ape", ".m4a", ".wav" }) { FolderDepth = Windows.Storage.Search.FolderDepth.Deep }).GetFilesAsync();
        }

        async void CollectChanges()
        {
            List<Task> rescanTask = new();
            foreach (var f in LocalPageData.LocalMusicFolders)
            {
                var lib = await App.Instance.GetMusicLibrary(f);
                await Task.Run(async () =>
                {
                    lib.TryGetChangeTracker().Enable();
                    var changeTracker = lib.TryGetChangeTracker();
                    var changeReader = changeTracker.GetChangeReader();

                    List<string> toAdd = new();
                    List<string> toRemove = new();
                    bool changed = false;
                    foreach (var change in await changeReader.ReadBatchAsync())
                    {
                        if (change.ChangeType == StorageLibraryChangeType.ChangeTrackingLost)
                        {
                            App.Instance.SendTip("获取变更失败", "文件变更过期，正在重新扫描文件夹。");
                            changeTracker.Reset();

                            DispatcherQueue.TryEnqueue(() =>
                            {
                                ApplyMusicChanges(musicFiles.Where(f => f.StartsWith(lib.Path)).ToList(), false);
                                rescanTask.Add(ApplyAsyncMusicChanges(ScanFolder(lib)));
                            });
                            break;
                        }
                        if (change.IsOfType(StorageItemTypes.File))
                        {
                            changed = true;
                            if (change.ChangeType == StorageLibraryChangeType.Created || change.ChangeType == StorageLibraryChangeType.MovedIntoLibrary)
                            {
                                toAdd.Add(change.Path);
                            }
                            else if (change.ChangeType == StorageLibraryChangeType.Deleted || change.ChangeType == StorageLibraryChangeType.MovedOutOfLibrary)
                            {
                                toRemove.Add(change.PreviousPath);
                            }
                            else if (change.ChangeType == StorageLibraryChangeType.MovedOrRenamed)
                            {
                                toRemove.Add(change.PreviousPath);
                                toAdd.Add(change.Path);
                            }
                        }
                        if (change.IsOfType(StorageItemTypes.Folder))
                        {
                            changed = true;
                            if (change.ChangeType == StorageLibraryChangeType.Created || change.ChangeType == StorageLibraryChangeType.MovedIntoLibrary)
                            {
                                await AddFolder(change.Path);
                            }
                            else if (change.ChangeType == StorageLibraryChangeType.Deleted || change.ChangeType == StorageLibraryChangeType.MovedOutOfLibrary)
                            {
                                toRemove = musicFiles.Where(str => str.StartsWith(change.PreviousPath, StringComparison.OrdinalIgnoreCase)).ToList();
                            }
                            else if (change.ChangeType == StorageLibraryChangeType.MovedOrRenamed)
                            {
                                toRemove = musicFiles.Where(str => str.StartsWith(change.PreviousPath, StringComparison.OrdinalIgnoreCase)).ToList();

                                await AddFolder(change.Path);
                            }

                            async Task AddFolder(string path)
                            {
                                var sf = await StorageFolder.GetFolderFromPathAsync(path);
                                foreach (var file in await sf.GetFilesAsync())
                                {
                                    toAdd.Add(file.Path);
                                }
                                foreach (var folder in await sf.GetFoldersAsync())
                                {
                                    await AddFolder(folder.Path);
                                }
                            }
                        }
                        if (change.IsOfType(StorageItemTypes.None))
                        {
                            // TODO: What is None type change?
                        }

                    }
                    if (changed)
                        DispatcherQueue.TryEnqueue(() =>
                         {
                             ApplyMusicChanges(toAdd);
                             ApplyMusicChanges(toRemove, false);
                         });

                    await Task.WhenAll(rescanTask);
                    if (rescanTask.Count > 0)
                    {
                        // 在 ApplyAsyncMusicChanges 中使用 Task.Delay 似乎会阻止垃圾回收，此处我们手动进行回收
                        GC.Collect();
                    }
                    await changeReader.AcceptChangesAsync();
                });

            }
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            FullScan();
        }

    }
}