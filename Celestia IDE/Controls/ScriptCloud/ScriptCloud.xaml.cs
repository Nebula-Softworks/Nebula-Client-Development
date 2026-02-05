using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Windows.Threading;
using System.Net;

namespace Celestia_IDE.Controls.ScriptCloud
{
    public class jsonstrings
    {
        public class ScriptHub
        {
            public string Image { get; set; }

            public string Name { get; set; }

            public string GameName { get; set; }

            public string Script { get; set; }

            public string id { get; set; }

            public string url { get; set; }
        }

        public List<ScriptHub> Script;
    }

    /// <summary>
    /// Interaction logic for ScriptCloud.xaml
    /// </summary>
    public partial class ScriptCloud : UserControl
    {
        bool _initialized;
        MainWindow mainWindow = null;
        public ScriptCloud(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            Loaded += delegate
            {
                if (_initialized) return;
                _initialized = true;
                MainGrid.Children.Remove(Rscripts);
                MainGrid.Children.Remove(FavoritedScripts);
                LoadScriptBlox();
                LoadRscripts();
                LoadFavoriteScripts();
            };
            if (!Directory.Exists(MainWindow.NebulaClientPath + @"\cache\fs\"))
                Directory.CreateDirectory(MainWindow.NebulaClientPath + @"\cache\fs\");
        }

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void SourceSelector_Checked(object sender, RoutedEventArgs e)
        {
            if (MainContentScroller == null) return;
            CurrentPage = 1;
            SearchTextString = "";
            SearchTextString_Changed = true;
            ScriptCloudSearch.Text = "";
            MainContentScroller.ScrollToTop();
            switch (((RadioButton)sender).Name)
            {
                case "ScriptBloxButton":
                    MainContentScroller.Content = ScriptBlox;
                    LoadScriptBlox();
                    CurrentCloudTitleBlock.Text = "Powered by Scriptblox.com";
                    break;
                case "RScriptsButton":
                    MainContentScroller.Content = Rscripts;
                    LoadRscripts();
                    CurrentCloudTitleBlock.Text = "Powered by Rscripts.net";
                    break;
                case "FavoritedScriptsButton":
                    MainContentScroller.Content = FavoritedScripts;
                    CurrentCloudTitleBlock.Text = "";
                    break;
            }
        }

        HttpClient httpClient;
        string SearchTextString = "";
        private bool SearchTextString_Changed = true;
        private double CurrentPage = 1;
        private double MaxPage = 500;

        private async void LoadScriptBlox()
        {
            httpClient = new HttpClient();
            await Task.Run(async delegate
            {

                try
                {

                    HttpResponseMessage response;
                    if (!string.IsNullOrEmpty(SearchTextString))
                    {
                        response = await httpClient.GetAsync("https://scriptblox.com/api/script/search?q=" + SearchTextString + "&mode=free&max=18&strict=true&sortBy=likeCount&page=" + CurrentPage);
                    }
                    else
                    {
                        response = await httpClient.GetAsync("https://scriptblox.com/api/script/fetch?mode=free&max=18&page=" + CurrentPage);
                    }
                    try
                    {
                        HttpContent content = response.Content;
                        try
                        {
                            dynamic val = JsonConvert.DeserializeObject(await content.ReadAsStringAsync());
                            if (val.message != null) return;
                            if (SearchTextString_Changed)
                            {
                                SearchTextString_Changed = false;
                                double.TryParse(val.result.totalPages.ToString(), out MaxPage);
                            }
                            if (CurrentPage <= MaxPage)
                            {
                                foreach (dynamic item in val.result.scripts)
                                {
                                    string imgurl = null;
                                    bool isPatched = false;
                                    bool flag = false;
                                    if (item.ContainsKey("isPatched"))
                                    {
                                        isPatched = item.isPatched;
                                    }
                                    flag = !((!item.ContainsKey("key")) ? true : false) && (bool)item.key;
                                    Dispatcher.Invoke(delegate
                                    {
                                        ScriptCloudControl ScriptBloxWindow = new ScriptCloudControl(isPatched, flag);
                                        try
                                        {
                                            if (item.image.ToString().Contains("rbxcdn.com"))
                                            {
                                                ScriptBloxWindow.ImageBrushBg.ImageSource = new BitmapImage(new Uri(item.image.ToString()))
                                                {
                                                    DecodePixelHeight = Convert.ToInt32(Math.Ceiling(ScriptBloxWindow.ActualHeight)),
                                                    DecodePixelWidth = Convert.ToInt32(Math.Ceiling(ScriptBloxWindow.ActualWidth)),
                                                };
                                                if (ScriptBloxWindow.ImageBrushBg.ImageSource.CanFreeze) ScriptBloxWindow.ImageBrushBg.ImageSource.Freeze();
                                                imgurl = item.image.ToString();
                                            }
                                            else
                                            {
                                                ScriptBloxWindow.ImageBrushBg.ImageSource = new BitmapImage(new Uri("https://scriptblox.com" + item.image.ToString()))
                                                {
                                                    DecodePixelHeight = Convert.ToInt32(Math.Ceiling(ScriptBloxWindow.ActualHeight)),
                                                    DecodePixelWidth = Convert.ToInt32(Math.Ceiling(ScriptBloxWindow.ActualWidth)),
                                                };
                                                if (ScriptBloxWindow.ImageBrushBg.ImageSource.CanFreeze) ScriptBloxWindow.ImageBrushBg.ImageSource.Freeze();
                                                imgurl = "https://scriptblox.com" + item.image.ToString();
                                            }
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                if (item.game.imageUrl.ToString().Contains("rbxcdn.com"))
                                                {
                                                    ScriptBloxWindow.ImageBrushBg.ImageSource = new BitmapImage(new Uri(item.game.imageUrl.ToString()))
                                                    {
                                                        DecodePixelHeight = Convert.ToInt32(Math.Ceiling(ScriptBloxWindow.ActualHeight)),
                                                        DecodePixelWidth = Convert.ToInt32(Math.Ceiling(ScriptBloxWindow.ActualWidth)),
                                                    };
                                                    if (ScriptBloxWindow.ImageBrushBg.ImageSource.CanFreeze) ScriptBloxWindow.ImageBrushBg.ImageSource.Freeze();
                                                    imgurl = item.game.imageUrl.ToString();
                                                }
                                                else
                                                {
                                                    ScriptBloxWindow.ImageBrushBg.ImageSource = new BitmapImage(new Uri("https://scriptblox.com" + item.game.imageUrl.ToString()))
                                                    {
                                                        DecodePixelHeight = Convert.ToInt32(Math.Ceiling(ScriptBloxWindow.ActualHeight)),
                                                        DecodePixelWidth = Convert.ToInt32(Math.Ceiling(ScriptBloxWindow.ActualWidth)),
                                                    };
                                                    if (ScriptBloxWindow.ImageBrushBg.ImageSource.CanFreeze) ScriptBloxWindow.ImageBrushBg.ImageSource.Freeze();
                                                    imgurl = "https://scriptblox.com" + item.game.imageUrl.ToString();
                                                }
                                            }
                                            catch
                                            {
                                                ScriptBloxWindow.ImageBrushBg.ImageSource = new BitmapImage(new Uri("https://tr.rbxcdn.com/180DAY-59af3523ad8898216dbe1043788837bf/480/270/Image/Png/noFilter"))
                                                {
                                                    DecodePixelHeight = Convert.ToInt32(Math.Ceiling(ScriptBloxWindow.ActualHeight)),
                                                    DecodePixelWidth = Convert.ToInt32(Math.Ceiling(ScriptBloxWindow.ActualWidth)),
                                                };
                                                if (ScriptBloxWindow.ImageBrushBg.ImageSource.CanFreeze) ScriptBloxWindow.ImageBrushBg.ImageSource.Freeze();
                                                imgurl = "https://tr.rbxcdn.com/180DAY-59af3523ad8898216dbe1043788837bf/480/270/Image/Png/noFilter";

                                            }
                                        }
                                        ScriptBloxWindow.TitleBlock.Text = (string)item.title.ToString();
                                        ScriptBloxWindow.ContentBlock.Text = (string)item.game.name.ToString();
                                        ScriptBloxWindow.LoadInEditorButton.Click += delegate
                                        {
                                            mainWindow.TabSystemz.maintabs.Items.Add(mainWindow.TabSystemz.CreateTab(item.script.ToString(), item.title.ToString()));
                                            mainWindow.DialogBackground_MouseLeftButtonDown(new Button(), new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
                                        };
                                        ScriptBloxWindow.OpenButton.Click += delegate
                                        {
                                            mainWindow.Redirect("https://scriptblox.com/script/" + item.slug?.ToString());
                                        };
                                        if (File.Exists(MainWindow.NebulaClientPath + @"\cache\fs\" + item._id?.ToString() + ".json"))
                                        {
                                            ScriptBloxWindow.FavoriteButton.FontFamily = (FontFamily)FindResource("solid");
                                        }
                                        else
                                        {
                                            ScriptBloxWindow.FavoriteButton.FontFamily = (FontFamily)FindResource("regular");
                                        }
                                        ScriptBloxWindow.FavoriteButton.Click += delegate
                                        {
                                            if (File.Exists(MainWindow.NebulaClientPath + @"\cache\fs\" + item._id?.ToString() + ".json"))
                                            {
                                                try
                                                {
                                                    File.Delete(MainWindow.NebulaClientPath + @"\cache\fs\" + item._id?.ToString() + ".json");
                                                    ScriptBloxWindow.FavoriteButton.FontFamily = (FontFamily)FindResource("regular");
                                                    return;
                                                }
                                                catch
                                                {
                                                    return;
                                                }
                                                finally
                                                {
                                                    ((IDisposable)content)?.Dispose();
                                                }
                                            }
                                            jsonstrings jsonstrings = new jsonstrings
                                            {
                                                Script = new List<jsonstrings.ScriptHub>
                                                {
                                                        new jsonstrings.ScriptHub
                                                        {
                                                            Name = item.title.ToString(),
                                                            GameName = item.game.name.ToString(),
                                                            Image = imgurl,
                                                            Script = item.script.ToString(),
                                                            id = item._id?.ToString(),
                                                            url = "https://scriptblox.com/script/" + item.slug?.ToString(),
                                                        }
                                                }
                                            };
                                            try
                                            {
                                                File.WriteAllText(MainWindow.NebulaClientPath + @"\cache\fs\" + item._id?.ToString() + ".json", JsonConvert.SerializeObject((object)jsonstrings, (Newtonsoft.Json.Formatting)1));
                                                ScriptBloxWindow.FavoriteButton.FontFamily = (FontFamily)FindResource("solid");
                                            }
                                            catch (Exception ex)
                                            {
                                                mainWindow.ApplicationPrint(2, "failed scriptcloud func. error: " + ex.Message);
                                            }
                                            finally
                                            {
                                                ((IDisposable)content)?.Dispose();
                                            }
                                            LoadFavoriteScripts();
                                        };
                                            ScriptBlox.Children.Insert(ScriptBlox.Children.IndexOf(ScriptBloxBtn), ScriptBloxWindow);
                                    });
                                }
                            }
                        }
                        finally
                        {
                            ((IDisposable)content)?.Dispose();
                        }
                    }
                    finally
                    {
                        ((IDisposable)response)?.Dispose();
                    }
                }

                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        mainWindow.ApplicationPrint(2, "Failed To Connect To Scriptblox: " + ex.Message + " | " + ex.StackTrace);
                        mainWindow.Prompt("The Script Blox Integration System has encountered a Connectivity Issue. Please Read The Output For The Full Error and Send it to us on Discord.", "Script Blox Error");
                    });
                }
                GC.Collect(2, GCCollectionMode.Forced);
            });

        }

        private void ScriptBloxSearch()
        {
            CurrentPage = 1;
            foreach (UIElement control in ScriptBlox.Children.Cast<Visual>().ToList())
            {
                if (control != ScriptBloxBtn) ScriptBlox.Children.Remove(control);
            };
            MainContentScroller.ScrollToTop();
            LoadScriptBlox();
        }

        private async void LoadRscripts()
        {
            httpClient = new HttpClient();
            await Task.Run(async delegate
            {
                try
                {
                    string requestUri = $"https://rscripts.net/api/v2/scripts?page={CurrentPage}&notPaid=true&orderBy=date&q={SearchTextString}&orderBy=likes";
                    if (string.IsNullOrEmpty(SearchTextString)) requestUri = $"https://rscripts.net/api/v2/scripts?page={CurrentPage}&notPaid=true&orderBy=date";
                    HttpResponseMessage response = await httpClient.GetAsync(requestUri);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    try
                    {
                        try
                        {
                            dynamic val = JsonConvert.DeserializeObject(responseBody);

                            if (SearchTextString_Changed)
                            {
                                SearchTextString_Changed = false;
                                MaxPage = val.info.maxPages;
                            }

                            if (CurrentPage <= MaxPage)
                            {
                                foreach (dynamic item in val.scripts)
                                {
                                    var actlscript = mainWindow.HttpGet(item.rawScript?.ToString()).ToString();
                                    var keysys = false;
                                    if (item.keySystem != null) keysys = item.keySystem;
                                    Dispatcher.Invoke(delegate
                                    {
                                        ScriptCloudControl RScriptsWindow = new ScriptCloudControl(false, keysys);
                                        try
                                        {
                                            RScriptsWindow.ImageBrushBg.ImageSource = new BitmapImage(new Uri(item.image?.ToString()))
                                            {
                                                DecodePixelHeight = Convert.ToInt32(Math.Ceiling(RScriptsWindow.ActualHeight)),
                                                DecodePixelWidth = Convert.ToInt32(Math.Ceiling(RScriptsWindow.ActualWidth)),
                                            };
                                            if (RScriptsWindow.ImageBrushBg.ImageSource.CanFreeze) RScriptsWindow.ImageBrushBg.ImageSource.Freeze();
                                        }
                                        catch
                                        {
                                            RScriptsWindow.ImageBrushBg.ImageSource = new BitmapImage(new Uri(item.game?.imgurl?.ToString()))
                                            {
                                                DecodePixelHeight = Convert.ToInt32(Math.Ceiling(RScriptsWindow.ActualHeight)),
                                                DecodePixelWidth = Convert.ToInt32(Math.Ceiling(RScriptsWindow.ActualWidth)),
                                            };
                                            if (RScriptsWindow.ImageBrushBg.ImageSource.CanFreeze) RScriptsWindow.ImageBrushBg.ImageSource.Freeze();
                                        }
                                        RScriptsWindow.TitleBlock.Text = item.title?.ToString();
                                        RScriptsWindow.ContentBlock.Text = item.game?.title?.ToString();
                                        RScriptsWindow.LoadInEditorButton.Click += delegate
                                        {
                                            mainWindow.TabSystemz.maintabs.Items.Add(mainWindow.TabSystemz.CreateTab(actlscript, item.title?.ToString()));
                                            mainWindow.DialogBackground_MouseLeftButtonDown(new Button(), new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
                                        };
                                        RScriptsWindow.OpenButton.Click += delegate
                                        {
                                            mainWindow.Redirect("https://rscripts.net/script/" + item.slug?.ToString());
                                        };
                                        if (File.Exists(MainWindow.NebulaClientPath + @"\cache\fs\" + item._id?.ToString() + ".json"))
                                        {
                                            RScriptsWindow.FavoriteButton.FontFamily = (FontFamily)FindResource("solid");
                                        }
                                        else
                                        {
                                            RScriptsWindow.FavoriteButton.FontFamily = (FontFamily)FindResource("regular");
                                        }
                                        RScriptsWindow.FavoriteButton.Click += delegate
                                        {
                                            if (File.Exists(MainWindow.NebulaClientPath + @"\cache\fs\" + item._id?.ToString() + ".json"))
                                            {
                                                try
                                                {
                                                    File.Delete(MainWindow.NebulaClientPath + @"\cache\fs\" + item._id?.ToString() + ".json");
                                                    RScriptsWindow.FavoriteButton.FontFamily = (FontFamily)FindResource("regular");
                                                    return;
                                                }
                                                catch
                                                {
                                                    return;
                                                }
                                            }
                                            jsonstrings jsonstrings = new jsonstrings
                                            {
                                                Script = new List<jsonstrings.ScriptHub>
                                                {
                                                        new jsonstrings.ScriptHub
                                                        {
                                                            Name = item.title?.ToString(),
                                                            GameName = item.game?.title?.ToString(),
                                                            Image = item.image?.ToString() != null ? item.image?.ToString() : item.game?.imgurl?.ToString(),
                                                            Script = actlscript,
                                                            id = item._id?.ToString(),
                                                            url = "https://rscripts.net/script/" + item.slug?.ToString(),
                                                        }
                                                }
                                            };
                                            try
                                            {
                                                File.WriteAllText(MainWindow.NebulaClientPath + @"\cache\fs\" + item._id.ToString() + ".json", JsonConvert.SerializeObject(jsonstrings, (Newtonsoft.Json.Formatting)1));
                                                RScriptsWindow.FavoriteButton.FontFamily = (FontFamily)FindResource("solid");
                                            }
                                            catch (Exception ex)
                                            {
                                                mainWindow.ApplicationPrint(2, "failed scriptcloud func. error: " + ex.Message);
                                            }
                                            finally
                                            {
                                                ((IDisposable)response)?.Dispose();
                                            }
                                            LoadFavoriteScripts();
                                        };
                                        Rscripts.Children.Insert(Rscripts.Children.IndexOf(RscriptBtn), RScriptsWindow);
                                    });
                                }
                            }
                        }
                        finally
                        {
                            ((IDisposable)response)?.Dispose();
                        }
                    }
                    finally
                    {
                        ((IDisposable)response)?.Dispose();
                    }

                }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        mainWindow.ApplicationPrint(2, "Failed To Connect To Rscripts: " + ex.Message + " | " + ex.StackTrace);
                        mainWindow.Prompt("The Rscripts Integration System has encountered a Connectivity Issue. Please Read The Output For The Full Error and Send it to us on Discord.", "Rscripts Error");
                    });
                }
                GC.Collect(2, GCCollectionMode.Forced);
            });
        }

        private void RscriptsSearch()
        {
            CurrentPage = 1;
            foreach (UIElement control in Rscripts.Children.Cast<Visual>().ToList())
            {
                if (control != RscriptBtn) Rscripts.Children.Remove(control);
            };
            MainContentScroller.ScrollToTop();
            LoadRscripts();
        }

        private async void LoadFavoriteScripts()
        {
            await Task.Run(delegate
            {
                Dispatcher.Invoke(() => FavoritedScripts.Children.Clear());
                try
                {
                    foreach (string enumerateFile in Directory.EnumerateFiles(MainWindow.NebulaClientPath + @"\cache\fs\", "*.json"))
                        ((IEnumerable<JToken>)JObject.Parse(File.ReadAllText(enumerateFile))["Script"]).ToList().ForEach(item =>
                        {
                            bool isPatched = false;
                            bool flag = false;
                            if (item.Contains("isPatched"))
                            {
                                isPatched = true;
                            }
                            flag = !((!item.Contains("key")) ? true : false);
                            Dispatcher.Invoke(() =>
                            {
                                if (!item["Name"].ToString().ToLower().Contains(ScriptCloudSearch.Text.ToLower()))
                                    return;
                                FavoritedScriptControl FavoritedScript = new FavoritedScriptControl(isPatched, flag);
                                try
                                {
                                    FavoritedScript.ImageBrushBg.ImageSource = new BitmapImage(new Uri(item["Image"].ToString())) 
                                    {
                                        DecodePixelHeight = Convert.ToInt32(Math.Ceiling(FavoritedScript.ActualHeight)),
                                        DecodePixelWidth = Convert.ToInt32(Math.Ceiling(FavoritedScript.ActualWidth)),
                                    };
                                    if (FavoritedScript.ImageBrushBg.ImageSource.CanFreeze) FavoritedScript.ImageBrushBg.ImageSource.Freeze();
                                }
                                catch
                                {
                                }
                                FavoritedScript.TitleBlock.Text = item["Name"].ToString();
                                FavoritedScript.ContentBlock.Text = item["GameName"].ToString();
                                FavoritedScript.LoadInEditorButton.Click += (s, e) =>
                                {
                                    mainWindow.TabSystemz.maintabs.Items.Add(mainWindow.TabSystemz.CreateTab(item["Script"].ToString(), item["Name"].ToString()));
                                    mainWindow.DialogBackground_MouseLeftButtonDown(new Button(), new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left));
                                };
                                FavoritedScript.OpenButton.Click += delegate
                                {
                                    mainWindow.Redirect(item["url"].ToString());
                                };
                                FavoritedScript.FavoriteButton.Click += (s, e) =>
                                {
                                    (FavoritedScript.Parent as StackPanel).Children.Remove(FavoritedScript);
                                    File.Delete(MainWindow.NebulaClientPath + @"\cache\fs\" + item["id"].ToString() + ".json");
                                };
                                FavoritedScripts.Children.Add(FavoritedScript);
                            });
                        });
                }
                catch
                {
                }
                GC.Collect(2, GCCollectionMode.Forced);
            });
        }

        private void FavoriteScriptsSearch()
        {
            CurrentPage = 1;
            FavoritedScripts.Children.Clear();
            MainContentScroller.ScrollToTop();
            LoadFavoriteScripts();
        }

        private void ScriptCloudSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SearchTextString_Changed = true;
                SearchTextString = ScriptCloudSearch.Text;
                switch (((StackPanel)MainContentScroller.Content).Name)
                {
                    case "ScriptBlox":
                        ScriptBloxSearch();
                        break;
                    case "Rscripts":
                        RscriptsSearch();
                        break;
                    case "FavoritedScripts":
                        FavoriteScriptsSearch();
                        break;
                }
            }
        }

        private void ScriptScroller_ScrollChanged(object sender, RoutedEventArgs e)
        {
            if (((StackPanel)MainContentScroller.Content).Name == "FavoritedScripts") return;

            CurrentPage++;
            LoadScriptBlox();
            LoadRscripts();
        }

    }
}
