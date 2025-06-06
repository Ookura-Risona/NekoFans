using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;
using Neko.Sources;
using Neko.Sources.APIS;

namespace Neko.Gui;

/// <summary>
/// The "Image Sources" tab in the Config Menu
/// </summary>
public class ImageSourcesWindow
{
    private sealed class ImageSourceConfig
    {
        public string Name;
        public string Description;
        public string Help;
        public Type Type;
        public IImageConfig Config;

        public ImageSourceConfig(string name, string description, string help, Type type, IImageConfig config)
        {
            Name = name;
            Description = description;
            Help = help;
            Type = type;
            Config = config;
        }
    }

    private readonly ImageSourceConfig[] SourceList = {
            new ImageSourceConfig("Nekos.life", "Anime Catgirls", "https://nekos.life/",
                typeof(NekosLife), Plugin.Config.Sources.NekosLife),
            new ImageSourceConfig("nekos.best", "Anime Catgirls", "https://nekos.best/",
                typeof(NekosBest), Plugin.Config.Sources.NekosBest),
            new ImageSourceConfig("shibe.online", "Shiba Inu Dogs", "https://shibe.online/",
                typeof(ShibeOnline), Plugin.Config.Sources.ShibeOnline),
            new ImageSourceConfig("Catboys", "Anime Catboys","https://catboys.com/",
                typeof(Catboys), Plugin.Config.Sources.Catboys),
            new ImageSourceConfig("WAIFU.IM", "Anime Waifus","https://waifu.im/",
                typeof(Waifuim), Plugin.Config.Sources.Waifuim),
            new ImageSourceConfig("Waifu.pics", "Anime Waifus","https://waifu.pics/",
                typeof(WaifuPics), Plugin.Config.Sources.WaifuPics),
            new ImageSourceConfig("Pic.re", "High resolution Anime Images","https://pic.re/",
                typeof(PicRe), Plugin.Config.Sources.PicRe),
            new ImageSourceConfig("Dog CEO", "Dogs","https://dog.ceo/",
                typeof(DogCEO), Plugin.Config.Sources.DogCEO),
            new ImageSourceConfig("The Cat API", "Cats","https://thecatapi.com/",
                typeof(TheCatAPI), Plugin.Config.Sources.TheCatAPI),
            new ImageSourceConfig("Twitter", "Twitter","https://twitter.com/",
                typeof(Twitter), Plugin.Config.Sources.Twitter),
        };

    private static readonly Vector4 TwitterDark = new(0.0549f, 0.29411f, 0.4431372f, 1f);
    private static readonly Vector4 TwitterLight = new(0.11372549f, 0.6313725f, 0.94901960f, 0.8f);
    private static readonly Vector4 TableTextBG = new(0.29019607f, 0.29019607f, 0.29019607f, 0.823529f);
    private static readonly Vector4 TableTextRed = new(0.38823529f, 0.1098039f, 0.1098039f, 1f);

    private const float INDENT = 32f;
    private static (TheCatAPI.Breed[], string[])? TheCatAPIBreedNames;
    private static (DogCEO.Breed[], string[])? DogCEOBreedNames;

    private readonly HeaderImage.Individual Header = new();

    private static readonly DateTime TwitterTimeout = DateTime.MinValue;

    public void Draw()
    {
        // ------------ Header --------------
        if (Plugin.Config.ShowHeaders)
            DrawHeader();
        // ------------ Mock Images for debugging --------------
        DrawMock();
        //  ------------ nekos.life --------------
        SourceCheckbox(SourceList[0], ref Plugin.Config.Sources.NekosLife.enabled);
        if (Plugin.Config.Sources.NekosLife.enabled)
            DrawNekosLife(SourceList[0]);
        //  ------------ nekos.best --------------
        SourceCheckbox(SourceList[1], ref Plugin.Config.Sources.NekosBest.enabled);
        if (Plugin.Config.Sources.NekosBest.enabled)
            DrawNekosBest(SourceList[1]);
        //  ------------ shibe.online --------------
        SourceCheckbox(SourceList[2], ref Plugin.Config.Sources.ShibeOnline.enabled);
        //  ------------ Catboys --------------
        // SourceCheckbox(SourceList[3], ref Plugin.Config.Sources.Catboys.enabled);
        //  ------------ waifu.im --------------
        SourceCheckbox(SourceList[4], ref Plugin.Config.Sources.Waifuim.enabled);
        if (Plugin.Config.Sources.Waifuim.enabled)
            DrawWaifuim(SourceList[4]);
        //  ------------ Waifu.pics --------------
        SourceCheckbox(SourceList[5], ref Plugin.Config.Sources.WaifuPics.enabled);
        if (Plugin.Config.Sources.WaifuPics.enabled)
            DrawWaifuPics(SourceList[5]);
        //  ------------ Pic.re --------------
        SourceCheckbox(SourceList[6], ref Plugin.Config.Sources.PicRe.enabled);
        //  ------------ Dog CEO --------------
        SourceCheckbox(SourceList[7], ref Plugin.Config.Sources.DogCEO.enabled);
        if (Plugin.Config.Sources.DogCEO.enabled)
            DrawDogCEO();
        //  ------------ TheCatAPI --------------
        SourceCheckbox(SourceList[8], ref Plugin.Config.Sources.TheCatAPI.enabled);
        if (Plugin.Config.Sources.TheCatAPI.enabled)
            DrawTheCatAPI();
        //  ------------ Twitter --------------
        /*
        SourceCheckbox(SourceList[9], ref Plugin.Config.Sources.Twitter.enabled);
        if (Twitter.IsRateLimited)
        {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1f, 0, 0f, 1f), "API limit reached");
            ImGui.SameLine();
            Common.HelpMarker("The free Twitter API is limited to 2 million tweets per Month. This limit is shared between all users of this plugin and will usually be reset on the 26st of every month.");
            // Make the Twitter config unable to open
            if (Plugin.Config.Sources.Twitter.enabled)
            {
                Plugin.Config.Sources.Twitter.enabled = false;
                Plugin.Config.Save();
                Plugin.UpdateImageSource();
            }
        }

        if (Plugin.Config.Sources.Twitter.enabled)
            DrawTwitter();
        */

        CheckIfNoSource();
    }

    private void DrawHeader()
    {
        var imgSize = Header.TryGetSize();
        if (imgSize == null)
            return;

        var regionMax = ImGui.GetWindowContentRegionMax();
        var regionMin = ImGui.GetWindowContentRegionMin();
        var height = (regionMax.Y - regionMin.Y) * 0.25f;
        var width = regionMax.X - regionMin.X - (2 * ImGui.GetStyle().WindowPadding.X);

        var (start, end) = Common.AlignImage(imgSize.Value, new Vector2(width, height), Configuration.ImageAlignment.Top);
        var cursorPos = ImGui.GetCursorPos();
        start += new Vector2(cursorPos.X + ImGui.GetStyle().WindowPadding.X, cursorPos.Y);
        end += cursorPos;

        Header.Draw((start, end));
        Common.ToolTip($"您使用 Neko Fans 下载的图片数量为 {Plugin.Config.LocalDownloadCount}");
    }

    private static void DrawMock()
    {
#if DEBUG
        if (ImGui.Checkbox("Mock Images##Mock", ref Mock.Enabled))
            Plugin.UpdateImageSource();

        ImGui.SameLine();
        ImGui.TextDisabled("这应该只在调试模式下可见");

        if (Mock.Enabled && ImGui.Button("Update Mock Images##Mock"))
        {
            Mock.UpdateImages();
            Plugin.UpdateImageSource();
        }
#endif
    }

    private static void DrawNekosLife(ImageSourceConfig source)
    {
        ImGui.Indent(INDENT);
        var nl = Plugin.Config.Sources.NekosLife;
        var preview = "";
        foreach (var f in Helper.GetFlags(nl.categories))
        {
            if (NekosLife.CategoryInfo.TryGetValue(f, out var info))
            {
                if (info.NSFW && !NSFW.AllowNSFW)
                    continue;
                preview += info.DisplayName + ", ";
            }
        }
        preview = preview.Length > 3 ? preview[..^2] : "未选择分类";

        var dic = NekosLife.CategoryInfo;
        var enums = (NekosLife.Category[])Enum.GetValues(typeof(NekosLife.Category));

        if (ImGui.BeginCombo("分类##NekosLife", preview, ImGuiComboFlags.HeightLarge))
        {
            foreach (var e in enums)
            {
                if (dic.TryGetValue(e, out var info))
                {
                    if (info.NSFW && !NSFW.AllowNSFW)
                        continue;
                    EnumSelectable(source, info.DisplayName, e, ref nl.categories);
                }
            }
            ImGui.EndCombo();
        }
        if (preview.Length > 35)
            Common.ToolTip(preview);

        if (nl.categories == NekosLife.Category.None)
        {
            ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), "WARNING:"); ImGui.SameLine();
            ImGui.TextWrapped("未选择分类。请选择至少一个图片分类。");
        }
        ImGui.Unindent(INDENT);
    }

    private static void DrawWaifuPics(ImageSourceConfig source)
    {
        ImGui.Indent(INDENT);
        var wp = Plugin.Config.Sources.WaifuPics;
        var preview = "";
        foreach (var f in Helper.GetFlags(wp.sfwCategories))
        {
            preview += (Enum.GetName(typeof(WaifuPics.CategoriesSFW), f) ?? "unknown") + ", ";
        }
        if (NSFW.AllowNSFW) // NSFW Check
        {
            foreach (var f in Helper.GetFlags(wp.nsfwCategories))
            {
                preview += "NSFW " + (Enum.GetName(typeof(WaifuPics.CategoriesNSFW), f) ?? "unknown") + ", ";
            }
        }

        preview = preview.Length > 3 ? preview[..^2] : "未选择分类";

        if (ImGui.BeginCombo("分类##WaifuPics", preview))
        {
            EnumSelectable(source, "Waifu", WaifuPics.CategoriesSFW.Waifu, ref wp.sfwCategories);
            EnumSelectable(source, "Neko", WaifuPics.CategoriesSFW.Neko, ref wp.sfwCategories);
            EnumSelectable(source, "Shinobi", WaifuPics.CategoriesSFW.Shinobu, ref wp.sfwCategories);
            EnumSelectable(source, "Megumin", WaifuPics.CategoriesSFW.Megumin, ref wp.sfwCategories);
            EnumSelectable(source, "Awoo", WaifuPics.CategoriesSFW.Awoo, ref wp.sfwCategories);
            if (NSFW.AllowNSFW) // NSFW Check
            {
                EnumSelectable(source, "NSFW Waifu", WaifuPics.CategoriesNSFW.Waifu, ref wp.nsfwCategories);
                EnumSelectable(source, "NSFW Neko", WaifuPics.CategoriesNSFW.Neko, ref wp.nsfwCategories);
                EnumSelectable(source, "NSFW Trap", WaifuPics.CategoriesNSFW.Trap, ref wp.nsfwCategories);
            }
            ImGui.EndCombo();
        }
        if (preview.Length > 35)
            Common.ToolTip(preview);

        if (wp.sfwCategories == WaifuPics.CategoriesSFW.None && (wp.nsfwCategories == WaifuPics.CategoriesNSFW.None || !NSFW.AllowNSFW))
        {
            ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), "WARNING:"); ImGui.SameLine();
            ImGui.TextWrapped("未选择分类。请选择至少一个图片分类。");
        }
        ImGui.Unindent(INDENT);
    }

    private static void DrawNekosBest(ImageSourceConfig source)
    {
        ImGui.Indent(INDENT);
        var nb = Plugin.Config.Sources.NekosBest;
        var preview = "";
        foreach (var f in Helper.GetFlags(nb.categories))
        {
            if (NekosBest.CategoryInfo.TryGetValue(f, out var info))
            {
                preview += $"{info.DisplayName}, ";
            }
        }
        preview = preview.Length > 3 ? preview[..^2] : "未选择分类";

        var enums = (NekosBest.Category[])Enum.GetValues(typeof(NekosBest.Category));

        if (ImGui.BeginCombo("分类##NekosBest", preview, ImGuiComboFlags.HeightLarge))
        {
            foreach (var e in enums)
            {
                if (NekosBest.CategoryInfo.TryGetValue(e, out var info))
                {
                    EnumSelectable(source, info.DisplayName, e, ref nb.categories);
                }
            }
            ImGui.EndCombo();
        }
        if (preview.Length > 35)
            Common.ToolTip(preview);

        if (nb.categories == NekosBest.Category.None)
        {
            ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), "WARNING:"); ImGui.SameLine();
            ImGui.TextWrapped("未选择分类。请选择至少一个图片分类。");
        }
        ImGui.Unindent(INDENT);
    }

    private static void DrawWaifuim(ImageSourceConfig source)
    {
        ImGui.Indent(INDENT);
        var wai = Plugin.Config.Sources.Waifuim;
        var preview = "";
        foreach (var f in Helper.GetFlags(wai.categories))
        {
            if (Waifuim.CategoryInfo.TryGetValue(f, out var info))
            {
                if (!info.NSFW || NSFW.AllowNSFW)
                {
                    preview += $"{info.DisplayName}, ";
                }
            }
        }
        preview = preview.Length > 3 ? preview[..^2] : "未选择分类";

        var enums = (Waifuim.Category[])Enum.GetValues(typeof(Waifuim.Category));

        if (ImGui.BeginCombo("分类##Waifuim", preview, ImGuiComboFlags.HeightLarge))
        {
            foreach (var e in enums)
            {
                if (Waifuim.CategoryInfo.TryGetValue(e, out var info))
                {
                    if (!info.NSFW || NSFW.AllowNSFW)
                    {
                        EnumSelectable(source, info.DisplayName, e, ref wai.categories);
                    }
                }
            }
            ImGui.EndCombo();
        }
        if (preview.Length > 35)
            Common.ToolTip(preview);

        if (wai.categories == Waifuim.Category.None)
        {
            ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), "WARNING:"); ImGui.SameLine();
            ImGui.TextWrapped("未选择分类。请选择至少一个图片分类。");
        }
        ImGui.Unindent(INDENT);
    }

    private static void DrawDogCEO()
    {
        if (DogCEOBreedNames == null) // Load names only once, then use cached
        {
            var b = (DogCEO.Breed[])Enum.GetValues(typeof(DogCEO.Breed));
            var n = new string[b.Length];
            for (var i = 0; i < b.Length; i++)
            {
                n[i] = b[i] == DogCEO.Breed.all ? "全部" : DogCEO.BreedName(b[i]);
            }
            DogCEOBreedNames = (b, n);
        }

        ImGui.Indent(INDENT);
        var (breeds, names) = DogCEOBreedNames ?? default;

        if (ImGui.BeginCombo("品种##DogCeo", names[Plugin.Config.Sources.DogCEO.selected], ImGuiComboFlags.HeightLarge))
        {
            for (var i = 0; i < names.Length; i++)
            {
                if (ImGui.Selectable(names[i] + "##" + i, i == Plugin.Config.Sources.DogCEO.selected))
                {
                    Plugin.Config.Sources.DogCEO.selected = i;
                    Plugin.Config.Sources.DogCEO.breed = breeds[i];
                    Plugin.Config.Save();
                    Plugin.UpdateImageSource();
                }

                if (ImGui.IsItemHovered()
                    && breeds[i] != DogCEO.Breed.all
                    && DogCEO.BreedDictionary.TryGetValue(breeds[i], out var info))
                {
                    Common.ToolTip(info.Description);
                }
            }

            ImGui.EndCombo();
        }
        ImGui.Unindent(INDENT);
    }

    private static void DrawTheCatAPI()
    {
        if (TheCatAPIBreedNames == null) // Load names only once, then use cached
        {
            var b = (TheCatAPI.Breed[])Enum.GetValues(typeof(TheCatAPI.Breed));
            var n = new string[b.Length];
            for (var i = 0; i < b.Length; i++)
            {
                n[i] = b[i] == TheCatAPI.Breed.All ? "全部" : TheCatAPI.BreedDictionary[b[i]].Name;
            }
            TheCatAPIBreedNames = (b, n);
        }

        ImGui.Indent(INDENT);
        var (breeds, names) = TheCatAPIBreedNames ?? default;

        if (ImGui.BeginCombo("品种##TheCatApi", names[Plugin.Config.Sources.TheCatAPI.selected], ImGuiComboFlags.HeightLarge))
        {
            for (var i = 0; i < names.Length; i++)
            {
                if (ImGui.Selectable(names[i] + "##" + i, i == Plugin.Config.Sources.TheCatAPI.selected))
                {
                    Plugin.Config.Sources.TheCatAPI.selected = i;
                    Plugin.Config.Sources.TheCatAPI.breed = breeds[i];
                    Plugin.Config.Save();
                    Plugin.UpdateImageSource();
                }
                if (ImGui.IsItemHovered()
                    && breeds[i] != TheCatAPI.Breed.All
                    && TheCatAPI.BreedDictionary.TryGetValue(breeds[i], out var info))
                {
                    Common.ToolTip(info.Description);
                }
            }
            ImGui.EndCombo();
        }
        ImGui.Unindent(INDENT);
    }

    private static List<TwitterTableEntry>? TwitterTableEntries;

    private sealed class TwitterTableEntry
    {
        public Twitter.Config.Query Query;
        public Twitter.Config.Query QueryDirty;

        public Twitter? ImageSource;
        public bool IsDirty;

        public TwitterTableEntry(Twitter.Config.Query query, Twitter? imageSource, bool isDirty)
        {
            Query = query;
            QueryDirty = query.Clone(); // Make a copy
            ImageSource = imageSource;
            IsDirty = isDirty;
        }
    }

    private int selectedTwitterEntry = -1;
    private bool twitterHelpOpen;

    private void DrawTwitter()
    {
        // Create Table if there is none
        if (TwitterTableEntries == null)
        {
            TwitterTableEntries = new();
            var imageSources = Plugin.ImageSource.GetAll<Twitter>();
            foreach (var query in Plugin.Config.Sources.Twitter.queries)
            {
                var source = imageSources.Find((s) => s.ConfigQuery == query);
                TwitterTableEntries.Add(new(query, source, false));
            }
        }

        // Add a default entry if the Table is empty
        if (TwitterTableEntries.Count == 0)
        {
            Twitter.Config.Query query = new();
            Plugin.Config.Sources.Twitter.queries.Add(query);
            TwitterTableEntries.Add(new(query, null, false));
            Plugin.Config.Save();
        }

        // Status of the Tweet (message, helptext?)
        static (string, string?) TweetStatus(TwitterTableEntry? entry)
            => entry == null
                ? (" ", null)
                : entry.ImageSource == null
                ? ("?", null)
                : entry?.ImageSource?.TweetStatus() ?? ("?", null);

        // Find max width needed of TweetCount Column or use default
        var tweetStatusColumWidth = TwitterTableEntries.Count > 0
            ? TwitterTableEntries.Max((e) => ImGui.CalcTextSize(TweetStatus(e).Item1).X + 5)
            : ImGui.CalcTextSize(TweetStatus(null).Item1).X;

        // It should be bigger than the header
        var statusWidth = ImGui.CalcTextSize("Status").X;
        if (tweetStatusColumWidth < statusWidth)
            tweetStatusColumWidth = statusWidth;

        ImGui.Indent(INDENT);
        ImGui.BeginTable("Twitter设置##Twitter", 3, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.RowBg);

        ImGui.TableSetupColumn("已启用##Twitter", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.NoSort);
        ImGui.TableSetupColumn("搜索文本##Twitter", ImGuiTableColumnFlags.WidthStretch, 100f - ImGui.GetColumnWidth(0) - tweetStatusColumWidth);
        ImGui.TableSetupColumn("Status##Twitter", ImGuiTableColumnFlags.WidthFixed, tweetStatusColumWidth);
        ImGui.TableHeadersRow();

        // Color of Selectable
        ImGui.PushStyleColor(ImGuiCol.Header, TwitterDark);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, TwitterLight);

        // Frame Background Color
        ImGui.PushStyleColor(ImGuiCol.FrameBg, TableTextBG);

        for (var i = 0; i < TwitterTableEntries.Count; i++)
        {
            ImGui.TableNextColumn();
            var entry = TwitterTableEntries[i];

            // Enabled Checkbox
            var checkboxSize = ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.X * 2);
            var checkboxX = (ImGui.GetColumnWidth() / 2) - (checkboxSize / 2) + ImGui.GetCursorPosX();
            ImGui.SetCursorPosX(checkboxX);
            if (ImGui.Checkbox($"##TwitterTableEntryEnabled_{i}", ref entry.QueryDirty.enabled) && entry.QueryDirty.searchText != "")
                entry.IsDirty = true;

            // Search Text
            ImGui.TableNextColumn();
            ImGui.PushItemWidth(-1);  // Remove Label
            if (entry.ImageSource?.Faulted ?? false)
                ImGui.PushStyleColor(ImGuiCol.FrameBg, TableTextRed);
            if (ImGui.InputText($"##TwitterTableEntrySearchText_{i}", ref entry.QueryDirty.searchText, 450))
                entry.IsDirty = true;
            if (ImGui.IsItemClicked())
                selectedTwitterEntry = i;
            if (entry.ImageSource?.Faulted ?? false)
                ImGui.PopStyleColor();

            ImGui.PopItemWidth();

            // Status
            ImGui.TableNextColumn();
            var (text, tooltip) = TweetStatus(entry);
            var tooltipPos = ImGui.GetCursorScreenPos();
            ImGui.Text(text);
            if (!string.IsNullOrEmpty(tooltip))
            {
                var height = ImGui.GetFrameHeight();
                var end = tooltipPos + new Vector2(ImGui.GetColumnWidth(), height);
                Common.ToolTip(tooltip, tooltipPos, end);
            }

            // Selectabel Row
            ImGui.SameLine();
            if (ImGui.Selectable("##TwitterTableEntrySelectable_" + i, selectedTwitterEntry == i, ImGuiSelectableFlags.SpanAllColumns))
                selectedTwitterEntry = i;
        }

        // Pop Style Colors
        ImGui.PopStyleColor(3);

        ImGui.EndTable();

        // Add Button
        if (ImGui.Button("添加 ##Twitter"))
        {
            Twitter.Config.Query query = new();
            Plugin.Config.Sources.Twitter.queries.Add(query);
            TwitterTableEntries.Add(new(query, null, false));
            Plugin.Config.Save();
        }

        static string GetHelpText() => ImGui.IsPopupOpen("Twitter 帮助##Twitter") ? "隐藏帮助" : "显示帮助";

        // Save button only when there are changes to save
        if (TwitterTableEntries.Find((e) => e.IsDirty) != null)
        {
            var lengthSave = ImGui.CalcTextSize("保存更改").X + (ImGui.GetStyle().FramePadding.X * 2);
            var lengthHelp = ImGui.CalcTextSize(GetHelpText()).X + (ImGui.GetStyle().FramePadding.X * 2);
            ImGui.SameLine(((ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - INDENT) / 2) - ((lengthSave + lengthHelp + ImGui.GetStyle().ItemSpacing.X) / 2) + INDENT);
            if (ImGui.Button("保存更改##Twitter"))
            {
                // Remove all changed entries
                foreach (var entry in TwitterTableEntries)
                {
                    if (!entry.IsDirty)
                        continue;

                    if (entry.Query.searchText != entry.QueryDirty.searchText)
                        Plugin.Log.Verbose("更改 Twitter 搜索文本: \"" + entry.Query.searchText + "\" to: \"" + entry.QueryDirty.searchText + "\"");

                    if (entry.Query.enabled != entry.QueryDirty.enabled)
                        Plugin.Log.Verbose((entry.QueryDirty.enabled ? "启用" : "禁用") + " Twitter 搜索文本: \"" + entry.QueryDirty.searchText + "\"");

                    // Remove the old source
                    if (entry.ImageSource != null)
                        Plugin.ImageSource.RemoveSource(entry.ImageSource);
                    entry.ImageSource = null;

                    // Remove the old query and add the new one
                    var query = entry.QueryDirty.Clone();
                    var oldIndex = Plugin.Config.Sources.Twitter.queries.FindIndex((q) => q == entry.Query);
                    if (oldIndex != -1)
                        Plugin.Config.Sources.Twitter.queries.RemoveAt(oldIndex);
                    Plugin.Config.Sources.Twitter.queries.Insert(oldIndex, query);
                    entry.Query = query;
                }

                Plugin.Config.Save();
                Plugin.UpdateImageSource();

                // Update ImageSource references and reset dirty flag
                foreach (var entry in TwitterTableEntries)
                {
                    if (entry.IsDirty || entry.ImageSource == null)
                    {
                        entry.ImageSource = Plugin.ImageSource.GetAll<Twitter>().Find((s) => s.ConfigQuery.Equals(entry.Query));
                        entry.IsDirty = false;
                    }
                }
            }
        }

        // Help Button
        {
            var lengthSave = ImGui.CalcTextSize("保存更改").X + (ImGui.GetStyle().FramePadding.X * 2);
            var lengthHelp = ImGui.CalcTextSize(GetHelpText()).X + (ImGui.GetStyle().FramePadding.X * 2);
            // If there is a Save button, align it to the right
            if (TwitterTableEntries.Find((e) => e.IsDirty) != null)
                ImGui.SameLine(((ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - INDENT) / 2) - ((lengthSave + lengthHelp + ImGui.GetStyle().ItemSpacing.X) / 2) + INDENT + (lengthSave + ImGui.GetStyle().ItemSpacing.X));
            else
                ImGui.SameLine(((ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - INDENT) / 2) - (lengthHelp / 2) + INDENT);

            if (ImGui.Button($"{GetHelpText()}##Twitter"))
                twitterHelpOpen = !twitterHelpOpen;
        }
        // Draw the Help
        if (twitterHelpOpen)
            DrawTwitterHelp();

        // Remove Button (Right align)
        if (selectedTwitterEntry >= 0)
        {
            var length = ImGui.CalcTextSize("清除").X;
            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - length);
            if (ImGui.Button("清除##Twitter") && selectedTwitterEntry >= 0)
            {
                if (!Plugin.Config.Sources.Twitter.queries.Remove(TwitterTableEntries[selectedTwitterEntry].Query))
                {
                    Plugin.Log.Error("Failed to remove Twitter query: " + TwitterTableEntries[selectedTwitterEntry].Query.searchText);
                }
                TwitterTableEntries.RemoveAt(selectedTwitterEntry);
                if (TwitterTableEntries.Count == 0)
                {
                    Twitter.Config.Query query = new();
                    Plugin.Config.Sources.Twitter.queries.Add(query);
                    TwitterTableEntries.Add(new(query, null, false));
                }
                Plugin.Config.Save();
                Plugin.UpdateImageSource();
                selectedTwitterEntry = selectedTwitterEntry < 0
                ? -1
                : selectedTwitterEntry > TwitterTableEntries.Count - 1
                ? TwitterTableEntries.Count - 1
                : selectedTwitterEntry;
                // Update ImageSource references
                foreach (var entry in TwitterTableEntries)
                {
                    entry.ImageSource ??= Plugin.ImageSource.GetAll<Twitter>().Find((s) => s.ConfigQuery.Equals(entry.Query));
                }
            }
        }
    }

    private void DrawTwitterHelp()
    {
        var fontScale = ImGui.GetIO().FontGlobalScale;
        var minSize = new Vector2(400 * fontScale, 200 * fontScale);
        ImGui.SetNextWindowSize(minSize * 2, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(minSize, minSize * 20);

        // Begin Window
        if (!ImGui.Begin("Neko Fans Twitter 帮助##NekoTwitter", ref twitterHelpOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse)) return;

        // Close Button
        ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 20f - ImGui.CalcTextSize("X").X);
        if (Common.IconButton(Dalamud.Interface.FontAwesomeIcon.Times, "##twitterCloseButton"))
            twitterHelpOpen = false;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGui.GetTextLineHeightWithSpacing());
        ImGui.SetCursorPosX(ImGui.GetStyle().WindowPadding.X);

        // Ignore spacing inbeween Text, TextWrapped and TextColored
        var spacing = ImGui.GetStyle().ItemSpacing;
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, spacing.Y));

        // How to use the Table:
        ImGui.TextColored(TwitterLight, "怎么使用这个表:");
        ImGui.Separator();
        ImGui.TextWrapped("该表用于添加和删除 Twitter 搜索。状态列显示 API 请求的当前状态，并显示找到的匹配推文数量。 "
                        + "请确保启用要使用的每一行，然后点击“保存更改”按钮以保存更改。 \n"
                        + "可以通过点击“添加”按钮添加新行。可以通过选择行并点击“删除”按钮来删除行。\n"
                        + "如果搜索文本无效，输入字段将显示为红色。");

        // Search Text
        ImGui.Spacing(); ImGui.Spacing();
        ImGui.TextColored(TwitterLight, "如何选择您想查看的推文:");
        ImGui.Separator();

        // Button to Twitter Advanced Search
        ImGui.Spacing();
        if (ImGui.Button("打开 Twitter 高级搜索", new Vector2(ImGui.GetWindowContentRegionMax().X - ImGui.GetStyle().WindowPadding.X, 25f * fontScale)))
            Helper.OpenInBrowser("https://twitter.com/search-advanced");

        ImGui.TextWrapped("有两种模式。您可以查看特定用户的推文，也可以查看符合查询条件的所有推文。如果您正在查看特定用户的推文或符合查询条件的推文数量，状态栏将显示“OK”。");

        // By User  
        ImGui.Spacing();
        ImGui.TextColored(TwitterLight, "按用户名搜索:");
        Common.TextWithColorsWrapped(new Common.Segment[]{
            new("输入 Twitter 用户名即可查看特定用户的最近 600 条推文。如果该用户存在，则状态栏将显示“OK”字样"),
        });
        ImGui.Spacing();
        Common.TextWithColorsWrapped(new Common.Segment[]{
            new("@username",  Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(" (e.g. "),
            new("@nasa",      Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(") will show you the last 600 Tweets from Nasa."),
        });

        // By Query  
        ImGui.Spacing();
        ImGui.TextColored(TwitterLight, "按查询搜索:");
        Common.TextWithColorsWrapped(new Common.Segment[]{
            new("您可以组合多个搜索词。系统仅显示过去 7 天内发布的推文。状态栏将显示匹配的推文数量。"),
        });
        ImGui.Spacing();
        Common.TextWithColorsWrapped(new Common.Segment[]{
            new("#hashtag",         Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(" (e.g. "),
            new("#gposers",         Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(") 匹配任何包含主题标签 #gposers 的推文\n"),
            new("keyword",          Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(" (e.g. "),
            new("neko",             Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(") 匹配任何包含单词“neko”的推文\n"),
            new("@username",        Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(" (e.g. "),
            new("@ff_xiv_en",       Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(") 匹配任何提及用户@ff_xiv_en 的推文\n"),
            new("lang:language",    Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(" (e.g. "),
            new("lang:en",          Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(") 匹配任何被归类为英语的推文\n"),
            new("a OR b",           Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(" (e.g. "),
            new("Miqo'te OR Viera", Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(") 匹配任何包含单词“Miqo'te”或“Viera”的推文\"\n"),
            new("-a",               Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(" (e.g. "),
            new("-Lalafell",        Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
            new(") 匹配任何不包含单词“Lalafell”的推文"),
        });
        ImGui.Spacing();
        ImGui.Spacing();
        Common.TextWithColorsWrapped(new Common.Segment[]{
            new("以下是查询的示例:\n"),
            new("lang:en #ffxiv #gposers -#miqote -#aura -#lala -#lalafell -(#meme OR funny)", Dalamud.Interface.Colors.ImGuiColors.DalamudGrey),
        });
        ImGui.Spacing();
        Common.TextWithColorsWrapped(new Common.Segment[]{
            new("还有更多选择。更多信息，请访问 "),
        }); ImGui.SameLine();
        // Clickable Link
        Common.ClickLinkWrapped("Twitter API 文档。", () => Helper.OpenInBrowser("https://developer.twitter.com/en/docs/twitter-api/tweets/search/integrate/build-a-query"));

        // Itemspacing
        ImGui.PopStyleVar();

        ImGui.End();
        ImGui.Unindent(INDENT);
    }

    private static void CheckIfNoSource()
    {
        var hasSome = Plugin.ImageSource.Count() > 0;
        var hasNoneFaulted = Plugin.ImageSource.ContainsNonFaulted();
        // If any are enabled, enable the queue again
        if (hasSome && hasNoneFaulted)
        {
            if (Plugin.GuiMain != null)
                Plugin.GuiMain.Queue.StopQueue = false;
            return;
        }

        // Stop queue new images if there are no image sources
        if (Plugin.GuiMain != null)
            Plugin.GuiMain.Queue.StopQueue = true;

        ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), "WARNING:");
        ImGui.SameLine();

        if (hasSome && !hasNoneFaulted)
        {
            ImGui.TextWrapped("所有图源目前均出现故障。您可以禁用并重新启用它们以重启它们。");
            return;
        }

        ImGui.TextWrapped("未选择图源。这会导致无法加载新图片。");
    }

    private static void EnumSelectable<T>(ImageSourceConfig source, string name, T single, ref T combined) where T : Enum
    {
        if (ImGui.Selectable(name + "##" + source.Name, combined.HasFlag(single), ImGuiSelectableFlags.DontClosePopups))
        {
            var comb = Convert.ToInt64(combined);
            var sing = Convert.ToInt64(single);
            if (combined.HasFlag(single))
                comb &= ~sing;
            else
                comb |= sing;
            combined = (T)Enum.ToObject(typeof(T), comb);
            Plugin.Config.Save();
            Plugin.UpdateImageSource();
        }
    }

    private static void SourceCheckbox(ImageSourceConfig source, ref bool enabled)
    {
        var hasFaulted = false;
        if (enabled)
        {
            var all = Plugin.ImageSource.GetAll(s => source.Type.IsAssignableFrom(s.GetType()));
            hasFaulted = all.Count != 0 && all.All(s => s.Faulted);
        }

        if (hasFaulted)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, ConfigWindow.RedColor);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0f, 0f, 1f));
        }

        if (ImGui.Checkbox(source.Name, ref enabled))
        {
            Plugin.Config.Save();
            Plugin.UpdateImageSource();
        }

        if (hasFaulted)
            ImGui.PopStyleColor(2);

        ImGui.SameLine();
        ImGui.TextDisabled(source.Description);
        ImGui.SameLine();
        Common.HelpMarker(source.Help);
        if (ImGui.IsItemClicked())
            Helper.OpenInBrowser(source.Help);
    }
}
