using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;

namespace Neko.Gui;

/// <summary>
/// The Configuration GUI (/nekocfg)
/// </summary>
public class ConfigWindow
{
    public bool Visible;

    public static readonly Vector4 RedColor = new(0.38f, 0.1f, 0.1f, 0.55f);

    private readonly ImageSourcesWindow imageSourcesGUI = new();
    private readonly HeaderImage.Total headerImage = new();

    private int QueueDonwloadCount;
    private int QueuePreloadCount;
    private readonly string Title;

    public ConfigWindow()
    {
        QueueDonwloadCount = Plugin.Config.QueueDownloadCount;
        QueuePreloadCount = Plugin.Config.QueuePreloadCount;
        Title = "Neko Fans 设置";

        // Add debug info to the title
        if (Plugin.PluginInterface.IsDev)
            Title += " (Dev)";
#if DEBUG
        Title += " (Debug)";
#endif
#if THROW
        Title += " (Random)";
#endif
#if DELAY
        Title += " (Delay)";
#endif
#if NETWORK
        Title += " (Network)";
#endif
    }

    public void Draw()
    {
        if (!Visible) return;
        try
        {
            var fontScale = ImGui.GetIO().FontGlobalScale;
            var size = new Vector2(450 * fontScale, 300 * fontScale);

            ImGui.SetNextWindowSize(size * 2, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(size, size * 20);

            if (!ImGui.Begin(Title, ref Visible)) return;

            // The Tab Bar
            if (ImGui.BeginTabBar("##tabBar"))
            {
                if (ImGui.BeginTabItem("查看设置"))
                {
                    DrawLook();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("图源设置"))
                {
                    imageSourcesGUI.Draw();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("高级设置"))
                {
                    DrawAdvanced();
                    ImGui.EndTabItem();
                }
                if (Plugin.PluginInterface.IsDevMenuOpen && ImGui.BeginTabItem("Dev"))
                {
                    DrawDev();
                    ImGui.EndTabItem();
                }
                if (ImGui.TabItemButton(Plugin.GuiMain?.Visible ?? false ? "隐藏 Neko 界面" : "显示 Neko 界面"))
                    Plugin.ToggleMainGui();
                Common.ToolTip("在聊天中输入 /neko 来查看主界面");
            }
            ImGui.EndTabBar();
        }
        finally
        {
            ImGui.End();
        }
    }

    private void DrawLook()
    {
        // Draw Header
        if (Plugin.Config.ShowHeaders)
        {
            headerImage.DrawFullWidth();
            var text = "所有 Neko Fans 用户查看的的图片总数。";
            if (!Plugin.Config.EnableTelemetry)
                text += "\n由于您禁用了“贡献公共图片计数”，因此您未计入总数";
            Common.ToolTip(text);
        }

        ImGui.PushItemWidth(-200 * ImGui.GetIO().FontGlobalScale);

        // Background opacity slider
        if (ImGui.SliderFloat("背景不透明度", ref Plugin.Config.GuiMainOpacity, 0, 100, "%3.0f%%"))
        {
            Plugin.Config.GuiMainOpacity = Math.Clamp(Plugin.Config.GuiMainOpacity, 0, 100);
            Plugin.Config.Save();
        }
        ImGui.SameLine(); Common.HelpMarker("CTRL+点击输入准确数值。");

        // Gif Animation Speed
        if (ImGui.SliderFloat("GIF动画速度", ref Plugin.Config.GIFSpeed, 0, 300, "%3.0f%%"))
        {
            Plugin.Config.GIFSpeed = Math.Clamp(Plugin.Config.GIFSpeed, 0, 300);
            Plugin.Config.Save();
        }
        ImGui.SameLine(); Common.HelpMarker("GIF动画的播放速度。 默认: 100%\nCTRL+点击输入准确数值。");

        // Allow resizing
        if (ImGui.Checkbox("允许调整大小", ref Plugin.Config.GuiMainAllowResize))
            Plugin.Config.Save();
        ImGui.SameLine(); Common.HelpMarker("显示窗口边缘附近的箭头来调整其大小。");

        if (Plugin.Config.GuiMainAllowResize)
        {
            // Show resize
            if (ImGui.Checkbox("显示调整大小按钮", ref Plugin.Config.GuiMainShowResize))
                Plugin.Config.Save();
            ImGui.SameLine(); Common.HelpMarker("显示或隐藏窗口右下角的灰色三角形。");
        }

        // Lock Window
        if (ImGui.Checkbox("锁定位置", ref Plugin.Config.GuiMainLocked))
            Plugin.Config.Save();
        ImGui.SameLine(); Common.HelpMarker("锁定窗口的位置，不允许其移动。\n您始终可以通过按住选定的热键并拖动鼠标来移动窗口。");

        // Show Title Bar
        if (ImGui.Checkbox("显示标题栏", ref Plugin.Config.GuiMainShowTitleBar))
            Plugin.Config.Save();
        ImGui.SameLine(); Common.HelpMarker("显示或隐藏图片顶部的标题栏");

        ImGui.Separator();

        // Show / Hide Header
        if (ImGui.Checkbox("显示顶部图片", ref Plugin.Config.ShowHeaders))
            Plugin.Config.Save();
        ImGui.SameLine(); Common.HelpMarker("显示或隐藏设置窗口顶部的图片。它显示了所有Neko Fans用户下载的图片总量。\n" +
                                            "“图像源”选项卡中的顶部图片显示了您下载的图像数量。");

        ImGui.Separator();

        // Slideshow Enable / Disable
        if (ImGui.Checkbox("轮播模式", ref Plugin.Config.SlideshowEnabled))
        {
            Plugin.Config.Save();
            Plugin.GuiMain?.Slideshow.UpdateFromConfig();
        }
        ImGui.SameLine(); Common.HelpMarker("在指定的时间间隔后自动显示新图片。");

        // Slideshow Interval
        if (Plugin.Config.SlideshowEnabled)
        {
            if (ImGui.InputDouble("时间间隔", ref Plugin.Config.SlideshowIntervalSeconds, 1, 60, Helper.SecondsToString(Plugin.Config.SlideshowIntervalSeconds)))
            {
                // Check for miminimum interval
                if (Plugin.Config.SlideshowIntervalSeconds < Sources.Slideshow.MININTERVAL)
                    Plugin.Config.SlideshowIntervalSeconds = Sources.Slideshow.MININTERVAL;
                Plugin.Config.Save();
                Plugin.GuiMain?.Slideshow.UpdateFromConfig();
            }
            Common.ToolTip("输入间隔长度（秒）。\n按住 CTRL 的同时按下 + 或 - 按钮，可将输入值更改1分钟。");
            ImGui.SameLine(); Common.HelpMarker("显示新图片之前要等待多长时间。");
        }

        ImGui.Separator();

        // Image Alignment Submenu
        if (ImGui.CollapsingHeader("图片对齐"))
            DrawAlign();

        // List Hotkeys
        if (ImGui.CollapsingHeader("热键"))
        {
            var keybinds = new List<(Hotkey, string)>() {
                (Plugin.Config.Hotkeys.NextImage, "显示下一张图片。您可以随时点击图片来显示下一张图片。"),
                (Plugin.Config.Hotkeys.ToggleWindow, "打开或关闭 Neko 窗口。"),
                (Plugin.Config.Hotkeys.MoveWindow,"移动 Neko 窗口。"),
                (Plugin.Config.Hotkeys.OpenInBrowser, "在默认浏览器中打开当前图片。"),
                (Plugin.Config.Hotkeys.CopyToClipboard, "将当前图片的链接复制到剪贴板") };

            DrawKeybinds(keybinds);
        }

        ImGui.PopItemWidth();
    }

    private void DrawAdvanced()
    {
        ImGui.PushItemWidth(-200);

        ImGui.PushItemWidth(150 * ImGui.GetIO().FontGlobalScale);
        // Image Queue System
        ImGui.Text("图片预加载系统");
        ImGui.SameLine(); Common.HelpMarker("让图片在后台预加载，以便更快地显示下一张图片。");

        // Int Downloaded
        if (ImGui.InputInt("预加载数量##Advanced", ref QueueDonwloadCount, 1))
        {
            if (QueueDonwloadCount < 1 || QueueDonwloadCount > 50 || QueuePreloadCount > QueueDonwloadCount)
                QueueDonwloadCount = Plugin.Config.QueueDownloadCount;
            Plugin.Config.QueueDownloadCount = QueueDonwloadCount;
            Plugin.Config.Save();
            Plugin.GuiMain?.Queue.UpdateQueueLength();
        }
        ImGui.SameLine(); Common.HelpMarker("从网络预加载的图片数量。\n" +
                                            "增加此值将导致更高的内存占用。推荐值：5");
        if (Plugin.GuiMain != null)
        {
            var usage = Plugin.GuiMain.Queue.RAMUsage();
            if (Plugin.GuiMain.ImageCurrent != null)
                usage += Plugin.GuiMain.ImageCurrent.RAMUsage;
            ImGui.SameLine(); ImGui.TextDisabled(Helper.SizeSuffix(usage));
        }

        // Int in VRAM
        if (ImGui.InputInt("加载到显存##Advanced", ref QueuePreloadCount, 1))
        {
            if (QueuePreloadCount < 1 || QueuePreloadCount > 25 || QueuePreloadCount > QueueDonwloadCount)
                QueuePreloadCount = Plugin.Config.QueuePreloadCount;
            Plugin.Config.QueuePreloadCount = QueuePreloadCount;
            Plugin.Config.Save();
            Plugin.GuiMain?.Queue.UpdateQueueLength();
        }
        ImGui.SameLine(); Common.HelpMarker("解码并加载到 GPU 中的图片数量。\n" +
                                            "增加此值将导致更高的显存占用。推荐值：2");
        if (Plugin.GuiMain != null)
        {
            var usage = Plugin.GuiMain.Queue.VRAMUsage();
            if (Plugin.GuiMain.ImageCurrent != null)
                usage += Plugin.GuiMain.ImageCurrent.VRAMUsage;
            ImGui.SameLine(); ImGui.TextDisabled(Helper.SizeSuffix(usage));
        }
        ImGui.PopItemWidth();

        ImGui.Separator();

        // Telemetry
        if (ImGui.Checkbox("贡献公共图片计数", ref Plugin.Config.EnableTelemetry))
            Plugin.Config.Save();
        ImGui.SameLine(); Common.HelpMarker("将您下载的图像数量发送到 Neko Fans 服务器，为公共图片计数做出贡献。\n" +
                                            "将发送图源名称和下载的图片数量。");

        ImGui.Separator();

        // Clear Image queue
        if (ImGui.Button("清除所有下载的图片##Advanced") && Plugin.GuiMain != null)
            Plugin.GuiMain.Queue.Refresh();

        ImGui.SameLine(); Common.HelpMarker("这将强制重新下载所有图片。");
        ImGui.PopItemWidth();

        // Clear Image queue
        if (ImGui.Button("重新启用所有有问题的图源##Advanced"))
            Plugin.ImageSource.ResetFaultySources();
        ImGui.SameLine(); Common.HelpMarker("如果某个 API 有问题，它会被禁用。如果 API 名称是红色的，则表示它已被禁用。\n" +
                                            "单击此按钮将重置所有已禁用的 API。\\n 您还可以在“图源”菜单中禁用和启用 API 来重置它们。");

        // Reload from Config
        if (ImGui.Button("根据设置重新加载图源##Advanced"))
            Plugin.ReloadSources();
        ImGui.SameLine(); Common.HelpMarker("这将根据配置文件中保存的状态重新加载所有图源。");

        // Force Garbage Collection
        if (Plugin.PluginInterface.IsDevMenuOpen)
        {
            if (ImGui.Button("Force Garbage collection##Advanced"))
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            ImGui.SameLine(); Common.HelpMarker("这会导致游戏卡顿。请仅在您清楚自己要做什么的情况下才使用此按钮！");
        }

        ImGui.PopItemWidth();
    }

    private static void DrawAlign()
    {
        // Center Child
        var windowWidth = ImGui.GetWindowWidth();
        var childSize = new Vector2(180, 175);
        ImGui.SetCursorPosX((windowWidth - childSize.X) / 2);

        ImGui.PushStyleColor(ImGuiCol.ChildBg, RedColor);
        ImGui.BeginChild("Align", childSize, border: true);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, 0);

        var buttonSize = new Vector2(50, 50);
        string[] names = {
                "\n   左上",
                "\n上",
                "\n右上  ",
                "    左",
                "中间",
                "右    ",
                "   左下\n ",
                "下\n ",
                "右下   \n " };
        Configuration.ImageAlignment[] alignmentents = {
                Configuration.ImageAlignment.TopLeft,
                Configuration.ImageAlignment.Top,
                Configuration.ImageAlignment.TopRight,
                Configuration.ImageAlignment.Left,
                Configuration.ImageAlignment.Center,
                Configuration.ImageAlignment.Right,
                Configuration.ImageAlignment.BottomLeft,
                Configuration.ImageAlignment.Bottom,
                Configuration.ImageAlignment.BottomRight };

        for (var y = 0; y < 3; y++)
        {
            for (var x = 0; x < 3; x++)
            {
                var alignment = new Vector2(x / 2f, y / 2f);
                var isSelected = Plugin.Config.Alignment == alignmentents[(y * 3) + x];

                if (x > 0)
                    ImGui.SameLine();
                ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, alignment);
                if (ImGui.Selectable(names[(y * 3) + x], isSelected, ImGuiSelectableFlags.None, buttonSize))
                {
                    Plugin.Config.Alignment = alignmentents[(y * 3) + x];
                    Plugin.Config.Save();
                }
                ImGui.PopStyleVar();
            }
        }
        ImGui.EndChild();
        ImGui.PopStyleColor();
    }

    private static void DrawDev()
    {
        if (ImGui.CollapsingHeader("Currently Displayed"))
        {
            if (Plugin.GuiMain?.ImageCurrent != null)
                ImGui.Text(Plugin.GuiMain?.ImageCurrent?.ToString());
            else
                ImGui.Text("当前没有显示图片");
            if (Plugin.GuiMain?.ImageNext != null)
            {
                ImGui.Spacing(); ImGui.Separator();
                ImGui.Text("下一张图片: \n" + Plugin.GuiMain?.ImageNext?.ToString());
            }
        }

        if (ImGui.CollapsingHeader("Image Queue"))
            ImGui.Text(Plugin.GuiMain?.Queue.ToString() ?? "GuiMain not loaded");
        if (ImGui.CollapsingHeader("Image Sources"))
            ImGui.Text(Plugin.ImageSource.ToString());
        if (ImGui.CollapsingHeader("Slideshow Status"))
            ImGui.Text(Plugin.GuiMain?.Slideshow.ToString() ?? "GuiMain not loaded");
        if (ImGui.CollapsingHeader("Plugin Config"))
            ImGui.Text(Plugin.Config.ToString());
    }

    private static Key[]? Keys;
    private static string[]? KeyNames;
    private static float? KeyLongestName;
    private static HotkeyCondition[]? Conditions;
    private static string[]? ConditionNames;
    private static float? ConditionLongestName;

    private static void DrawKeybinds(List<(Hotkey, string)> keybinds)
    {
        // Set static fields
        Keys ??= Hotkey.KeyNames.Keys.ToArray();
        KeyNames ??= Keys.Select(Hotkey.GetKeyName).ToArray();
        KeyLongestName ??= KeyNames.Max(x => ImGui.CalcTextSize(x).X);
        Conditions ??= Hotkey.ConditionNames.Keys.ToArray();
        ConditionNames ??= Conditions.Select(x => Hotkey.ConditionNames[x]).ToArray();
        ConditionLongestName ??= ConditionNames.Max(x => ImGui.CalcTextSize(x).X);

        ImGui.BeginTable("Keybinds##ConfigWindow", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);

        var ConditionColumnwidth = (ConditionLongestName.Value * ImGui.GetIO().FontGlobalScale * 1.5f) - 10;
        ImGui.TableSetupColumn("触发条件##ConfigWindow", ImGuiTableColumnFlags.WidthFixed, ConditionColumnwidth);
        var KeyColumnwidth = (KeyLongestName.Value * ImGui.GetIO().FontGlobalScale * 1.5f) - 35;
        ImGui.TableSetupColumn("按键##ConfigWindow", ImGuiTableColumnFlags.WidthFixed, KeyColumnwidth);
        ImGui.TableSetupColumn("功能##ConfigWindowu", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        for (var i = 0; i < keybinds.Count; i++)
        {
            var (hotkey, description) = keybinds[i];

            ImGui.TableNextRow();
            // Condition combo box
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(ConditionColumnwidth);
            if (ImGui.BeginCombo($"##Keybinds_Condition_{i}", hotkey.ConditionName))
            {
                for (var j = 0; j < ConditionNames.Length; j++)
                {
                    var name = ConditionNames[j];
                    if (ImGui.Selectable($"{name}##Keybinds_Condition_{i}_{j}", Conditions[j] == hotkey.Condition))
                    {
                        hotkey.Condition = Conditions[j];
                        Plugin.Config.Save();
                    }
                }
                ImGui.EndCombo();
            }

            // Key combo box
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(KeyColumnwidth);
            if (ImGui.BeginCombo($"##Keybinds_Combo_{i}", Hotkey.GetKeyName(hotkey.Key)))
            {
                for (var j = 0; j < KeyNames.Length; j++)
                {
                    var name = KeyNames[j];
                    if (ImGui.Selectable($"{name}##Keybinds_Combo_{i}_{j}", hotkey.Key == Keys[j]))
                    {
                        hotkey.Key = Keys[j];
                        Plugin.Config.Save();
                    }
                }
                ImGui.EndCombo();
            }

            // Description
            ImGui.TableNextColumn();
            Common.FontAwesomeIcon(FontAwesomeIcon.LongArrowAltRight); ImGui.SameLine();
            ImGui.Text(hotkey.Name); ImGui.SameLine();
            Common.HelpMarker(description);
        }
        ImGui.EndTable();

        // Check for duplicate keybinds
        var containsDuplicate = false;
        foreach (var (key, desc) in keybinds)
        {
            if (keybinds.Where(x => x.Item1.Key == key.Key).Skip(1).Any())
            {
                containsDuplicate = true;
                break;
            }
        }
        if (containsDuplicate)
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "警告：检测到重复的按键！");
            Common.ToolTip("您为同一个按键设置了多个操作。这会导致问题。");
        }
    }
}
