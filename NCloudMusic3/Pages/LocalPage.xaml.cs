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
using System.Collections.ObjectModel;
using Microsoft.International.Converters.PinYinConverter;
using DotNext.Collections.Generic;
using static NCloudMusic3.Pages.LocalPage;
using Microsoft.UI.Xaml.Shapes;
using CommunityToolkit.Mvvm.Input;
using NCloudMusic3.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NCloudMusic3.Pages {
    class LocalPageData : NotifyPropertyChanged {
        private bool isScanning;

        public RangeObservableCollection<string> LocalMusicFolders { get; set; } = new();
        public bool IsFolderListEmpty => LocalMusicFolders.Count == 0;

        public RangeObservableCollection<Music> LocalMusic { get; set; } = new();
        public bool IsLocalMusicEmpty => LocalMusic.Count == 0;

        public bool IsScanning {
            get => isScanning; set {
                isScanning = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(NotScanning));
            }
        }
        public bool NotScanning {
            get => !isScanning; set {
                isScanning = !value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsScanning));
            }
        }

        public LocalPageData() {
            LocalMusicFolders.CollectionChanged += (s, arg) => {
                RaisePropertyChanged(nameof(IsFolderListEmpty));
                App.Instance.AppConfig.LocalMusicFolders = (s as IEnumerable<string>).ToList();
            };
            LocalMusic.CollectionChanged += (s, arg) => {
                RaisePropertyChanged(nameof(IsLocalMusicEmpty));
            };
        }

    }

    public class GroupInfoList {
        public GroupInfoList(IEnumerable<object> items) {
            Items = new(items);
        }
        public KeyWrapper Key { get; set; }
        public ObservableCollection<object> Items { get; set; }
        public override string ToString() {
            return Key?.ToString();
        }

        public static ObservableCollection<GroupInfoList> From(IEnumerable<IGrouping<KeyWrapper, object>> items) {
            return new ObservableCollection<GroupInfoList>(items.Select(group => new GroupInfoList(group) { Key = group.Key }));
        }

        public static Comparison<GroupInfoList> Comparison => (a, b) => a.Key.CompareTo(b.Key);
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LocalPage : Page {
        LocalPageData LocalPageData { get; set; } = new();

        public LocalPage() {
            this.InitializeComponent();

            cacheLimit = new(1800, async () => {
                var f = await App.Instance.LocalCacheFolder.CreateFileAsync("list.local", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(f, JsonSerializer.Serialize(musicFiles));
            });

            LocalPageData.LocalMusic.CollectionChanged += (s, e) => {
                // TODO: ???????????????????????????????????????
                if ((s as RangeObservableCollection<Music>).Count > 0)
                    cacheLimit.Touch();
            };
        }
        DownTick cacheLimit;

        private async void Page_Loaded(object sender, RoutedEventArgs e) {
            LocalPageData.LocalMusicFolders.AddRange(App.Instance.AppConfig.LocalMusicFolders);
            LocalPageData.LocalMusic.CollectionChanged += (s, e) => {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) {
                    GroupUpdate(GroupInfoList.From(groupUpdater(e.NewItems.Cast<Music>())));
                }
                else
                    GroupUpdate();
            };

            if (LocalPageData.IsFolderListEmpty) {
                dialog.XamlRoot = App.Instance.m_window.Content.XamlRoot;
                var r = await dialog.ShowAsync();
                if (r == ContentDialogResult.None)
                    App.Instance.PageNavigateBack();
                if (r == ContentDialogResult.Primary) {
                    var f = await App.Instance.FolderPicker.PickSingleFolderAsync();
                    if (f != null) {
                        LocalPageData.LocalMusicFolders.Add(f.Path);
                        App.Instance.AddMusicLibrary(f);
                    }
                }
            }

            if (!LocalPageData.IsFolderListEmpty) {
                //GetChanges();
                await GetLocalMusic();
            }
        }

        private void GroupUpdate(IEnumerable<GroupInfoList> increments = null) {
            if (increments is null) {
                Groups.Clear();
                Groups.AddRange(GroupInfoList.From(groupUpdater(LocalPageData.LocalMusic)));
            }
            else {
                foreach (var i in increments) {
                    if (Groups.Where(g => g.Key == i.Key).FirstOrNone().TryGet(out var group)) {
                        group.Items.AddAll(i.Items);
                    }
                    else {
                        Groups.InsertOrdered(i, GroupInfoList.Comparison);
                    }
                }
            }
        }

        SortedSet<string> musicFiles = new();
        List<Task<bool>> tasks = new();
        /// <summary>
        /// 
        /// </summary>
        /// <returns>(???????????????????????????)</returns>
        public async Task<(IEnumerable<Music>, IEnumerable<Music>)> MatchLocalMusic() {
            var m = LocalPageData.LocalMusic.ToList();
            tasks = m.Select(m => m.Match()).ToList();

            await Task.WhenAll(tasks);
            // ?
            var r = m.Zip(tasks).GroupBy(z => z.Second.Result).OrderBy(z => z.Key);

            return (r.First().Select(z => z.First), r.Last().Select(z => z.First));
        }

        private void musicAddition(Music m) {
            musicFiles.Add(m.LocalPath);
            LocalPageData.LocalMusic.Add(m);
        }

        private void ApplyMusicChanges(IEnumerable<string> files, bool adding = true) {
            LocalPageData.IsScanning = true;
            List<string> failures = new();
            using (LocalPageData.LocalMusic.BatchUpdate()) {
                if (!adding)
                    foreach (var f in files) {
                        LocalPageData.LocalMusic.Remove(Music.GetLocal(f));
                        musicFiles.Remove(f);
                    }
                else
                    foreach (var f in files) {
                        var create = Music.CreateLocal;
                        var result = create.TryInvoke(f);
                        if (result.TryGet(out var m))
                            musicAddition(m);
                        else
                            failures.Add(f);
                    }
            }

            if (failures.Count > 0)
                App.Instance.SendTip("????????????????????????????????????????????????", string.Join("\n", failures));
            LocalPageData.IsScanning = false;
        }
        private async Task ApplyAsyncMusicChanges(Task<IEnumerable<StorageFile>> files) {
            LocalPageData.IsScanning = true;
            List<string> failures = new();
            foreach (var file in await files) {
                // await Task.Yield() ???????????????????????????
                // ????????????????????? await ??????????????????????????? Task.Yield() ??????????????????????????????
                //var f = await StorageFile.GetFileFromPathAsync(path);

                await Task.Delay(1);
                var create = Music.CreateLocalFromFile;
                var f = file.Path;

                if (create.TryInvoke(file).TryGet(out var m))
                    musicAddition(m);
                else
                    failures.Add(f);
            }
            if (failures.Count > 0)
                App.Instance.SendTip("????????????????????????????????????????????????", string.Join("\n", failures));
            LocalPageData.IsScanning = false;
        }
        private async Task ApplyAsyncMusicChanges(Task<IEnumerable<string>> files) {
            LocalPageData.IsScanning = true;
            List<string> failures = new();
            foreach (var f in await files) {
                // await Task.Yield() ???????????????????????????
                // ????????????????????? await ??????????????????????????? Task.Yield() ??????????????????????????????
                //var f = await StorageFile.GetFileFromPathAsync(path);

                await Task.Delay(1);
                var create = Music.CreateLocal;

                if (create.TryInvoke(f).TryGet(out var m))
                    musicAddition(m);
                else
                    failures.Add(f);

            }
            if (failures.Count > 0)
                App.Instance.SendTip("????????????????????????????????????????????????", string.Join("\n", failures));

            LocalPageData.IsScanning = false;

        }

        private async Task GetLocalMusic() {
            // ????????????
            var ls = await App.Instance.LocalCacheFolder.TryGetItemAsync("list.local");
            if (ls is not null) {
                var l = await File.ReadAllTextAsync(ls.Path);
                List<string> toAdd = JsonSerializer.Deserialize<List<string>>(l);


                if (toAdd.Count > 0) {
                    //DispatcherQueue.TryEnqueue(() =>
                    //{
                    //    ApplyMusicChanges(toAdd);
                    //});
                    var t = Task.FromResult(toAdd.AsEnumerable());
                    await ApplyAsyncMusicChanges(t);

                    // ??????changes 
                    CollectChanges();
                }
            }
            return;
        }

        ContentDialog dialog = new() {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "?????????????????????????????????????????????????????????????????????",
            PrimaryButtonText = "??????",
            CloseButtonText = "??????",
            DefaultButton = ContentDialogButton.Primary,
        };

        Task scanTask;
        void FullScan() {
            musicFiles.Clear();
            LocalPageData.LocalMusic.Clear();

            scanTask = ApplyAsyncMusicChanges(LocalPageData.LocalMusicFolders.Select(async f => {
                var l = await App.Instance.GetMusicLibrary(f);
                return await ScanFolder(l);
            }).Aggregate(async (f1, f2) => {
                await Task.WhenAll(f1, f2);
                return f1.Result.Concat(f2.Result);
            })).ContinueWith(t => {
                // ??? ApplyAsyncMusicChanges ????????? Task.Delay ??????????????????????????????????????????????????????
                GC.Collect();
            });
        }

        async Task<IEnumerable<StorageFile>> ScanFolder(StorageFolder library) {
            return await library.CreateFileQueryWithOptions(new(Windows.Storage.Search.CommonFileQuery.DefaultQuery, new List<string> { ".flac", ".mp3", ".ncm", ".ape", ".m4a", ".wav" }) { FolderDepth = Windows.Storage.Search.FolderDepth.Deep }).GetFilesAsync();
        }

        async void CollectChanges() {
            List<Task> rescanTask = new();
            foreach (var f in LocalPageData.LocalMusicFolders) {
                var lib = await App.Instance.GetMusicLibrary(f);
                await Task.Run(async () => {
                    lib.TryGetChangeTracker().Enable();
                    var changeTracker = lib.TryGetChangeTracker();
                    var changeReader = changeTracker.GetChangeReader();

                    List<string> toAdd = new();
                    List<string> toRemove = new();
                    bool changed = false;
                    foreach (var change in await changeReader.ReadBatchAsync()) {
                        if (change.ChangeType == StorageLibraryChangeType.ChangeTrackingLost) {
                            DispatcherQueue.TryEnqueue(() => {
                                App.Instance.SendTip("??????????????????", $"???????????????????????????????????????????????????\n{f}");
                            });
                            changeTracker.Reset();

                            DispatcherQueue.TryEnqueue(() => {
                                ApplyMusicChanges(musicFiles.Where(f => f.StartsWith(lib.Path)).ToList(), false);
                                rescanTask.Add(ApplyAsyncMusicChanges(ScanFolder(lib)));
                            });
                            break;
                        }
                        if (change.IsOfType(StorageItemTypes.File)) {
                            changed = true;
                            if (change.ChangeType == StorageLibraryChangeType.Created || change.ChangeType == StorageLibraryChangeType.MovedIntoLibrary)
                                toAdd.Add(change.Path);
                            else if (change.ChangeType == StorageLibraryChangeType.Deleted || change.ChangeType == StorageLibraryChangeType.MovedOutOfLibrary) {
                                toRemove.Add(change.PreviousPath);
                            }
                            else if (change.ChangeType == StorageLibraryChangeType.MovedOrRenamed) {
                                toRemove.Add(change.PreviousPath);
                                toAdd.Add(change.Path);
                            }
                        }
                        if (change.IsOfType(StorageItemTypes.Folder)) {
                            changed = true;
                            if (change.ChangeType == StorageLibraryChangeType.Created || change.ChangeType == StorageLibraryChangeType.MovedIntoLibrary)
                                await AddFolder(change.Path);
                            else if (change.ChangeType == StorageLibraryChangeType.Deleted || change.ChangeType == StorageLibraryChangeType.MovedOutOfLibrary) {
                                toRemove = musicFiles.Where(str => str.StartsWith(change.PreviousPath, StringComparison.OrdinalIgnoreCase)).ToList();
                            }
                            else if (change.ChangeType == StorageLibraryChangeType.MovedOrRenamed) {
                                toRemove = musicFiles.Where(str => str.StartsWith(change.PreviousPath, StringComparison.OrdinalIgnoreCase)).ToList();

                                await AddFolder(change.Path);
                            }

                            async Task AddFolder(string path) {
                                var sf = await StorageFolder.GetFolderFromPathAsync(path);
                                foreach (var file in await sf.GetFilesAsync())
                                    toAdd.Add(file.Path);
                                foreach (var folder in await sf.GetFoldersAsync()) {
                                    await AddFolder(folder.Path);
                                }
                            }
                        }
                        if (change.IsOfType(StorageItemTypes.None)) {
                            // TODO: What is None type change?
                        }

                    }
                    if (changed)
                        DispatcherQueue.TryEnqueue(() => {
                            ApplyMusicChanges(toRemove, false);
                            ApplyMusicChanges(toAdd);
                        });

                    await Task.WhenAll(rescanTask);
                    if (rescanTask.Count > 0) {
                        // ??? ApplyAsyncMusicChanges ????????? Task.Delay ????????????????????????????????????????????????????????????
                        GC.Collect();
                    }
                    await changeReader.AcceptChangesAsync();

                    //App.Instance.SendTip($"??????????????? {f} ??????", "");
                });

            }
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e) {
            FullScan();

        }
        RangeObservableCollection<GroupInfoList> Groups { get; set; } = new();
        Func<IEnumerable<Music>, IEnumerable<IGrouping<KeyWrapper, Music>>> groupUpdater;
        public record KeyWrapper(int Priority, string Key) : IComparable<KeyWrapper> {
            public int CompareTo(KeyWrapper other) {
                if (Priority.Equals(other.Priority)) return (Key ?? "").CompareTo(other.Key);
                return Priority.CompareTo(other.Priority);
            }

            public override string ToString() {
                return Key;
            }
        }

        private async void MatchButton_Click(object sender, RoutedEventArgs e) {
            var (failed, success) = await MatchLocalMusic();
            var failure = string.Join('\n', failed.Take(10).Select(f => f.LocalPath));

            using (LocalPageData.LocalMusic.BatchUpdate()) {

            }

            App.Instance.SendTip("????????????", $"?????? {success.Count()}, ?????? {failed.Count()}\n\n{failure}");
        }

        private async void HandleReplicationButton_Click(object sender, RoutedEventArgs e) {
            HandleReplicationDialog dialog = new(LocalPageData.LocalMusic);

            var result = await dialog.ShowAsync();
        }

        private void Pivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args) {
            switch (args.Item.Tag) {
                case "content":
                    groupUpdater = (musics) => musics.GroupBy(m => {
                        var first = (m.Title ?? "").FirstOrDefault();
                        if (ChineseChar.IsValidChar(first))
                            return new KeyWrapper(1, $"?????? {new ChineseChar(first).Pinyins[0][0]}");
                        else if (char.IsAsciiLetter(first))
                            return new KeyWrapper(3, $"{char.ToUpper(first)}");
                        else if (char.IsDigit(first))
                            return new KeyWrapper(0, "#");
                        else if (char.IsSymbol(first))
                            return new KeyWrapper(0, "&");
                        else return new KeyWrapper(2, "...");
                    }).OrderBy(g => g.Key);
                    break;
                case "album":
                    groupUpdater = (musics) => musics.GroupBy(m =>
                        string.IsNullOrEmpty(m.Album.Name) ? new KeyWrapper(0, "* ??????") : new KeyWrapper(1, m.Album.Name)
                        ).OrderBy(g => g.Key);
                    break;
                case "artist":
                    break;
                case "library":
                    break;
            }
            GroupUpdate();
        }

    }
}
