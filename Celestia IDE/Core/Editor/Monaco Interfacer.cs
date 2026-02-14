using CefSharp;
using CefSharp.Wpf;
using CefSharp.Core;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Celestia_IDE.Core.Editor
{
    public class monaco_api : ChromiumWebBrowser
    {
        public bool isDOMLoaded { get; private set; }
        public bool isMinimapEnabled { get; private set; }

        private string ToSetText;
        private bool toSetbool;

        public event EventHandler EditorReady;

        public monaco_api(string text, bool setTextOnLoad = true)
        {
            ToSetText = text;
            toSetbool = setTextOnLoad;

            UseLayoutRounding = true;
            SnapsToDevicePixels = true;
            this.VisualBitmapScalingMode = BitmapScalingMode.HighQuality;

            IsBrowserInitializedChanged += OnBrowserInitialized;
            FrameLoadEnd += OnFrameLoadEnd;
            JavascriptMessageReceived += OnJavascriptMessageReceived;

            Address = $"file:///{AppDomain.CurrentDomain.BaseDirectory}/bin/Monaco/Monaco.html";
        }

        private void OnBrowserInitialized(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsBrowserInitialized)
                return;

            Cef.UIThreadTaskFactory.StartNew(() =>
            {
                GetBrowser().GetHost().SetZoomLevel(0);
            });
            var dpi = VisualTreeHelper.GetDpi(this);
            this.SetZoomLevel(0);
        }

        private async void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (!e.Frame.IsMain)
                return;

            await Task.Delay(500);

            isDOMLoaded = true;

            if (toSetbool)
                await SetText(ToSetText);

            OnEditorReady();
        }

        private void OnJavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
        {
            var msg = e.Message?.ToString();
        }

        protected virtual void OnEditorReady()
        {
            EditorReady?.Invoke(this, EventArgs.Empty);

            if (Settings.Minimap) enable_minimap();
            if (Settings.InlayHints) InlayTypes(true);
            if (!Settings.AutoComplete) disable_autocomplete();
            if (!Settings.AutoFormat) SwitchAutoIndent(false);
            if (!Settings.Intellisense) disable_intellisense();

            FontSize(Settings.FontSize);

            if (Settings.AntiSkid)
                Blur(10);
                MouseEnter += (_, _) =>
                {
                    if (Settings.AntiSkid) Blur(0);
                };
                MouseLeave += (_, _) =>
                {
                    if (Settings.AntiSkid) Blur(10);
                };

            if (Settings.Ligatures)
                SwitchLig(true);
        }

        public async Task<string> GetText()
        {
            if (!isDOMLoaded)
                return "";

            var response = await BrowserCore.EvaluateScriptAsync(
                "monaco.editor.getModels()[0].getValue()"
            );

            return response.Success
                ? response.Result?.ToString() ?? ""
                : "";
        }

        public async Task SetText(string text)
        {
            if (!isDOMLoaded)
                return;

            text = text.Replace("\\", "\\\\").Replace("`", "\\`");

            await BrowserCore.EvaluateScriptAsync("editor.setValue('');");
            await BrowserCore.EvaluateScriptAsync($"editor.setValue(`{text}`);");
        }

        public void Cut() => Exec("Cut();");
        public void Copy() => Exec("Copy();");
        public void Paste() => Exec("Paste();");
        public void Undo() => Exec("Undo();");
        public void Redo() => Exec("Redo();");
        public void Find() => Exec("Find();");
        public void Replace() => Exec("Replace();");
        public void BlockC() => Exec("BlockComment();");
        public void LineC() => Exec("LineComment();");
        public void Format() => Exec("Format()");
        public void refresh() => Exec("Refresh();");

        public void enable_minimap()
        {
            Exec("SwitchMinimap(true);");
            isMinimapEnabled = true;
        }

        public void disable_minimap()
        {
            Exec("SwitchMinimap(false);");
            isMinimapEnabled = false;
        }

        public void enable_autocomplete() => Exec("SwitchAutoComplete(true);");
        public void disable_autocomplete() => Exec("SwitchAutoComplete(false);");

        public void enable_intellisense() => Exec("SwitchIntellisense(true);");
        public void disable_intellisense() => Exec("SwitchIntellisense(false);");

        public void SetTheme(string name) => Exec($"SetTheme({name});");
        public void Blur(double n) => Exec($"BlurEditor({n});");
        public void SmoothScroll(bool f) => Exec($"SetScroll({f});");
        public void ReadOnly(bool f) => Exec($"SwitchReadonly({f});");
        public void FontSize(double n) => Exec($"SwitchFontSize({n});");
        public void InlayTypes(bool t) => Exec($"SetInlays({t});");
        public void SwitchLig(bool f) => Exec($"SwitchFontLigatures({f});");
        public void SwitchAutoIndent(bool f) => Exec($"SwitchAutoIndent({f});");

        private void Exec(string script)
        {
            if (isDOMLoaded)
                BrowserCore.EvaluateScriptAsync(script);
        }
    }

    public enum Types
    {
        Class,
        Color,
        Constructor,
        Enum,
        Field,
        File,
        Folder,
        Function,
        Interface,
        Keyword,
        Method,
        Module,
        Property,
        Reference,
        Snippet,
        Text,
        Unit,
        Value,
        Variable,
        None
    }
}
