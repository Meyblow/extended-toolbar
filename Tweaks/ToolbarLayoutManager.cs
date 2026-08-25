using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Toolbar;
using osu.Framework.Testing;
using osucc.Client;
using osucc.Plugin;
using osuTK;
using osuTK.Input;
using ExtendedToolbar.Localisation;
using ExtendedToolbar.Models;
using ExtendedToolbar.UI;
using ExtendedToolbar.Utils;

namespace ExtendedToolbar.Tweaks
{
    /// <summary>
    /// Главный менеджер компоновки и зон модульного тулбара (Drag & Drop, Presets, Edit Mode).
    /// </summary>
    public partial class ToolbarLayoutManager : CompositeDrawable
    {
        public static ToolbarLayoutManager? Instance { get; private set; }

        private readonly IOsuCcPluginHost host;
        private readonly ExtendedToolbarSettings settings;
        private string configFilePath;

        private Toolbar? toolbar;
        private Drawable? originalGridContainer;
        private FillFlowContainer? originalLeftFlow;
        private FillFlowContainer? originalRightFlow;
        private Drawable? originalRulesetSelector;

        private ToolbarZoneContainer leftZone = null!;
        private ToolbarZoneContainer centerZone = null!;
        private ToolbarZoneContainer rightZone = null!;

        private ToolbarContextMenu contextMenu = null!;
        private DragGhostContainer dragGhostContainer = null!;
        private DraggableEditHintBanner editHintBanner = null!;
        private DragGhostBadge? activeGhost;

        private readonly Dictionary<string, ToolbarBlockContainer> allBlocks = new();
        public IReadOnlyDictionary<string, ToolbarBlockContainer> AllBlocks => allBlocks;

        private readonly List<string> originalLeftItems = new();
        private readonly List<string> originalRightItems = new();

        private bool isEditMode;
        private ToolbarBlockContainer? draggingBlock;
        private ToolbarZoneContainer? currentTargetZone;
        private int currentTargetIndex;

        private ToolbarOverlayPositioner? overlayPositioner;

        public ToolbarLayoutManager(IOsuCcPluginHost host, ExtendedToolbarSettings settings)
        {
            Instance = this;
            this.host = host;
            this.settings = settings;

            configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu", "osu-cc", "plugins", "extended-toolbar", "layout.json");

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Depth = float.MinValue;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            dragGhostContainer = new DragGhostContainer
            {
                RelativeSizeAxes = Axes.Both,
                AlwaysPresent = true,
                Depth = float.MinValue
            };

            InternalChildren = new Drawable[]
            {
                leftZone = new ToolbarZoneContainer(ToolbarZone.Left),
                centerZone = new ToolbarZoneContainer(ToolbarZone.Center),
                rightZone = new ToolbarZoneContainer(ToolbarZone.Right),
                overlayPositioner = new ToolbarOverlayPositioner(this, host)
            };

            contextMenu = new ToolbarContextMenu();
            editHintBanner = new DraggableEditHintBanner(colours, SaveAndExitEditMode);

            leftZone.OnZoneRightClicked += onZoneRightClicked;
            centerZone.OnZoneRightClicked += onZoneRightClicked;
            rightZone.OnZoneRightClicked += onZoneRightClicked;
        }

        public void AttachToolbar(
            Toolbar newToolbar,
            ToolbarStyleManager styleManager,
            ToolbarProfileLayoutManager profileManager,
            ToolbarVisibilityManager visibilityManager)
        {
            toolbar = newToolbar;
            ExtendedToolbarLog.Info($"ToolbarLayoutManager.AttachToolbar called with toolbar HashCode={newToolbar.GetHashCode()}");

            host.Scheduler?.Add(() =>
            {
                try
                {
                    initManager(styleManager, profileManager, visibilityManager);
                }
                catch (Exception ex)
                {
                    ExtendedToolbarLog.Error("Exception in AttachToolbar scheduler", ex);
                }
            });
        }

        private void initManager(
            ToolbarStyleManager styleManager,
            ToolbarProfileLayoutManager profileManager,
            ToolbarVisibilityManager visibilityManager)
        {
            if (toolbar == null)
                return;

            originalGridContainer = findGridContainer(toolbar);
            originalLeftFlow = findLeftFlow(toolbar);
            originalRightFlow = findRightFlow(toolbar);

            if (originalLeftFlow == null || originalRightFlow == null)
            {
                ExtendedToolbarLog.Error($"initManager: originalLeftFlow={originalLeftFlow != null}, originalRightFlow={originalRightFlow != null}");
                return;
            }

            if (allBlocks.Count == 0)
            {
                originalLeftItems.Clear();

                // 1. Left buttons (Settings, Home)
                var leftChildren = originalLeftFlow.Children.ToList();
                foreach (var child in leftChildren)
                {
                    string baseId = identifyDrawable(child);
                    string id = baseId;
                    int counter = 1;
                    while (allBlocks.ContainsKey(id))
                    {
                        id = $"{baseId}_{counter++}";
                    }

                    originalLeftItems.Add(id);
                    ToolbarBlockContainer.DetachFromParent(child);

                    var block = new ToolbarBlockContainer(id, getFriendlyName(id, child), child);
                    bindBlockEvents(block);
                    allBlocks[id] = block;
                }

                // 2. RulesetSelector
                originalRulesetSelector = toolbar.GetType().GetField("rulesetSelector", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(toolbar) as Drawable
                                          ?? findVisualChild(toolbar, "ToolbarRulesetSelector")
                                          ?? findVisualChild(toolbar, "rulesetSelector");

                if (originalRulesetSelector != null)
                {
                    ToolbarBlockContainer.DetachFromParent(originalRulesetSelector);

                    string rulesetId = "rulesets";
                    originalLeftItems.Add(rulesetId);
                    var rulesetBlock = new ToolbarBlockContainer(rulesetId, getFriendlyName(rulesetId, originalRulesetSelector), originalRulesetSelector);
                    bindBlockEvents(rulesetBlock);
                    allBlocks[rulesetId] = rulesetBlock;
                    ExtendedToolbarLog.Info("initManager: Extracted rulesetSelector successfully!");
                }

                // 3. Right buttons
                originalRightItems.Clear();
                var rightChildren = originalRightFlow.Children.ToList();
                foreach (var child in rightChildren)
                {
                    string baseId = identifyDrawable(child);
                    string id = baseId;
                    int counter = 1;
                    while (allBlocks.ContainsKey(id))
                    {
                        id = $"{baseId}_{counter++}";
                    }

                    originalRightItems.Add(id);
                    ToolbarBlockContainer.DetachFromParent(child);

                    var block = new ToolbarBlockContainer(id, getFriendlyName(id, child), child);
                    bindBlockEvents(block);
                    allBlocks[id] = block;
                }

                ExtendedToolbarLog.Info($"initManager: Extracted total {allBlocks.Count} modular blocks.");
            }

            if (originalGridContainer != null)
            {
                originalGridContainer.Alpha = 0;
                originalGridContainer.AlwaysPresent = false;
            }

            if (Parent == null)
            {
                toolbar.Add(this);
            }

            if (host.Game is Container<Drawable> gameContainer)
            {
                if (editHintBanner.Parent == null) gameContainer.Add(editHintBanner);
                if (contextMenu.Parent == null) gameContainer.Add(contextMenu);
                if (dragGhostContainer.Parent == null) gameContainer.Add(dragGhostContainer);
            }
            else if (toolbar.Parent is Container<Drawable> gameRoot)
            {
                if (editHintBanner.Parent == null) gameRoot.Add(editHintBanner);
                if (contextMenu.Parent == null) gameRoot.Add(contextMenu);
                if (dragGhostContainer.Parent == null) gameRoot.Add(dragGhostContainer);
            }

            try
            {
                if (host.Data != null)
                {
                    string? path = host.Data.GetFullPath("layout.json");
                    if (path != null) configFilePath = path;
                    ToolbarPresetManager.Init(host.Data);
                }
            }
            catch { }

            // One-time legacy migration check
            performOneTimeLegacyImport();

            var config = ToolbarLayoutConfig.Load(configFilePath);
            applyConfig(config);

            // Connect delegates & managers
            if (profileManager.Parent == null)
            {
                AddInternal(profileManager);
            }

            if (allBlocks.TryGetValue("user_profile", out var userProfileBlock))
            {
                profileManager.Attach(userProfileBlock.ContentDrawable);
            }

            visibilityManager.Attach(allBlocks);
            visibilityManager.UpdateVisibilityForCurrentScreen();

            Box? bgBox = toolbar.ChildrenOfType<Box>().FirstOrDefault(b => b.RelativeSizeAxes == Axes.Both);
            styleManager.Attach(toolbar, this, bgBox);

            settings.ToolbarSpacing.BindValueChanged(e =>
            {
                leftZone.SetSpacing(e.NewValue);
                centerZone.SetSpacing(e.NewValue);
                rightZone.SetSpacing(e.NewValue);
            }, true);

            overlayPositioner?.BindOverlays();
        }

        private void performOneTimeLegacyImport()
        {
            try
            {
                if (!File.Exists(configFilePath))
                {
                    string legacyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu", "osu-cc", "plugins", "osu-tweaks", "layout.json");
                    if (File.Exists(legacyPath))
                    {
                        File.Copy(legacyPath, configFilePath, true);
                        ExtendedToolbarLog.Info("One-time legacy layout imported from osu-tweaks");
                    }
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("Legacy import check failed", ex);
            }
        }

        public void ApplyPreset(string presetName)
        {
            var config = ToolbarPresetManager.LoadPreset(presetName);
            applyConfig(config);
            config.Save(configFilePath);
            settings.ActivePresetName.Value = presetName;
            host.Notify($"Пресет тулбара '{presetName}' применён", NotificationKind.Success);
        }

        public void ApplyConfig(ToolbarLayoutConfig config)
        {
            applyConfig(config);
            config.Save(configFilePath);
        }

        private void bindBlockEvents(ToolbarBlockContainer block)
        {
            block.OnBlockRightClicked += onBlockRightClicked;
            block.OnBlockDragStarted += onBlockDragStarted;
            block.OnBlockDragged += onBlockDragged;
            block.OnBlockDragEnded += onBlockDragEnded;
        }

        private ToolbarBlockContainer? getOrCreateBlock(ToolbarItemConfig item, ToolbarZone zone)
        {
            if (allBlocks.TryGetValue(item.Id, out var existing))
            {
                existing.IsHidden.Value = item.IsHidden;
                existing.CurrentZone = zone;
                return existing;
            }

            if (item.Id.StartsWith("spacer", StringComparison.OrdinalIgnoreCase))
            {
                var spacer = new ToolbarSpacer();
                var block = new ToolbarBlockContainer(item.Id, ExtendedToolbarStrings.BlockSpacer.ToString(), spacer);
                block.IsHidden.Value = item.IsHidden;
                block.CurrentZone = zone;
                block.IsEditMode = isEditMode;
                bindBlockEvents(block);
                allBlocks[item.Id] = block;
                return block;
            }

            return null;
        }

        private void applyConfig(ToolbarLayoutConfig config)
        {
            leftZone.Flow.Clear(false);
            centerZone.Flow.Clear(false);
            rightZone.Flow.Clear(false);

            var incomingIds = new HashSet<string>(
                config.Left.Select(i => i.Id)
                .Concat(config.Center.Select(i => i.Id))
                .Concat(config.Right.Select(i => i.Id))
            );

            var spacersToRemove = allBlocks.Keys.Where(k => k.StartsWith("spacer", StringComparison.OrdinalIgnoreCase) && !incomingIds.Contains(k)).ToList();
            foreach (var k in spacersToRemove)
            {
                allBlocks.Remove(k);
            }

            var placed = new HashSet<string>();

            int leftPos = 0;
            foreach (var item in config.Left)
            {
                var block = getOrCreateBlock(item, ToolbarZone.Left);
                if (block != null)
                {
                    leftZone.Flow.Add(block);
                    leftZone.Flow.SetLayoutPosition(block, leftPos++);
                    placed.Add(item.Id);
                }
            }

            int centerPos = 0;
            foreach (var item in config.Center)
            {
                var block = getOrCreateBlock(item, ToolbarZone.Center);
                if (block != null)
                {
                    centerZone.Flow.Add(block);
                    centerZone.Flow.SetLayoutPosition(block, centerPos++);
                    placed.Add(item.Id);
                }
            }

            int rightPos = 0;
            foreach (var item in config.Right)
            {
                var block = getOrCreateBlock(item, ToolbarZone.Right);
                if (block != null)
                {
                    rightZone.Flow.Add(block);
                    rightZone.Flow.SetLayoutPosition(block, rightPos++);
                    placed.Add(item.Id);
                }
            }

            foreach (var kvp in allBlocks)
            {
                if (!placed.Contains(kvp.Key))
                {
                    kvp.Value.CurrentZone = ToolbarZone.Right;
                    rightZone.Flow.Add(kvp.Value);
                    rightZone.Flow.SetLayoutPosition(kvp.Value, rightPos++);
                }
            }

            leftZone.UpdatePlaceholder();
            centerZone.UpdatePlaceholder();
            rightZone.UpdatePlaceholder();
            ExtendedToolbarLog.Info($"applyConfig: Layout applied. Left={leftZone.Flow.Count}, Center={centerZone.Flow.Count}, Right={rightZone.Flow.Count}");
        }

        public void EnterEditMode()
        {
            if (isEditMode) return;
            isEditMode = true;

            leftZone.IsEditMode = true;
            centerZone.IsEditMode = true;
            rightZone.IsEditMode = true;

            if (editHintBanner.Parent == null)
            {
                if (toolbar?.Parent is Container<Drawable> parentContainer)
                {
                    parentContainer.Add(editHintBanner);
                }
                else if (host.Game is Container<Drawable> gameContainer)
                {
                    gameContainer.Add(editHintBanner);
                }
            }

            editHintBanner.FadeIn(200);
            ExtendedToolbarLog.Info("EnterEditMode: Entered edit mode.");
        }

        public void SaveAndExitEditMode()
        {
            if (!isEditMode) return;

            var config = captureCurrentConfig();
            config.Save(configFilePath);

            isEditMode = false;
            leftZone.IsEditMode = false;
            centerZone.IsEditMode = false;
            rightZone.IsEditMode = false;

            editHintBanner.FadeOut(150);
            host.Notify(ExtendedToolbarStrings.NotifyLayoutSaved, NotificationKind.Success);
            ExtendedToolbarLog.Info("SaveAndExitEditMode: Saved and exited.");
        }

        public void CancelEditMode()
        {
            if (!isEditMode) return;

            var config = ToolbarLayoutConfig.Load(configFilePath);
            applyConfig(config);

            isEditMode = false;
            leftZone.IsEditMode = false;
            centerZone.IsEditMode = false;
            rightZone.IsEditMode = false;

            editHintBanner.FadeOut(150);
            host.Notify("Изменения отменены", NotificationKind.Info);
        }

        public void ResetToDefault()
        {
            if (IsDisposed) return;

            ExtendedToolbarLog.Info("ResetToDefault: Applying default layout...");

            try
            {
                if (isEditMode)
                {
                    isEditMode = false;
                    leftZone.IsEditMode = false;
                    centerZone.IsEditMode = false;
                    rightZone.IsEditMode = false;
                    editHintBanner.FadeOut(150);
                }

                ApplyPreset("Default");
                host.Notify("Тулбар сброшен по умолчанию", NotificationKind.Info);
                ExtendedToolbarLog.Info("ResetToDefault: Vanilla toolbar restored.");
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("ResetToDefault error", ex);
            }
        }

        private ToolbarLayoutConfig captureCurrentConfig()
        {
            return new ToolbarLayoutConfig
            {
                Left = leftZone.GetVisualOrderedChildren().Select(c => new ToolbarItemConfig { Id = c.ItemId, IsHidden = c.IsHidden.Value }).ToList(),
                Center = centerZone.GetVisualOrderedChildren().Select(c => new ToolbarItemConfig { Id = c.ItemId, IsHidden = c.IsHidden.Value }).ToList(),
                Right = rightZone.GetVisualOrderedChildren().Select(c => new ToolbarItemConfig { Id = c.ItemId, IsHidden = c.IsHidden.Value }).ToList()
            };
        }

        public ToolbarLayoutConfig GetCurrentConfig() => captureCurrentConfig();

        public void ShowSavePresetDialog(Action<string> onSaved)
        {
            var config = captureCurrentConfig();
            var dialog = new SavePresetDialog(config, name =>
            {
                if (string.IsNullOrWhiteSpace(name) || name.Equals("Default", StringComparison.OrdinalIgnoreCase))
                {
                    host.Notify(ExtendedToolbarStrings.NotifyDefaultPresetProtected, NotificationKind.Error);
                    return;
                }

                if (!ToolbarPresetManager.SaveCustomPreset(name, config))
                {
                    host.Notify(ExtendedToolbarStrings.NotifyDefaultPresetProtected, NotificationKind.Error);
                    return;
                }

                onSaved(name);
                settings.ActivePresetName.Value = name;
                host.Notify($"Пресет \"{name}\" успешно сохранён!", NotificationKind.Success);
            });

            if (host.Game is Container<Drawable> gameContainer)
            {
                gameContainer.Add(dialog);
                dialog.ShowDialog();
            }
            else if (toolbar?.Parent is Container<Drawable> parentContainer)
            {
                parentContainer.Add(dialog);
                dialog.ShowDialog();
            }
        }

        private void onZoneRightClicked(ToolbarZoneContainer zone, MouseDownEvent e)
        {
            showGlobalMenu(e.ScreenSpaceMouseDownPosition, zone);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Right)
            {
                showGlobalMenu(e.ScreenSpaceMouseDownPosition, null);
                return true;
            }
            return base.OnMouseDown(e);
        }

        private void showGlobalMenu(Vector2 pos, ToolbarZoneContainer? clickedZone)
        {
            var menuItems = new List<ContextMenuItemData>();

            if (!isEditMode)
            {
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ButtonEnterEditMode,
                    Icon = FontAwesome.Solid.SlidersH,
                    Action = EnterEditMode
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ButtonSavePreset,
                    Icon = FontAwesome.Solid.Save,
                    Action = () => ShowSavePresetDialog(_ => { })
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ButtonOpenPresetsFolder,
                    Icon = FontAwesome.Solid.FolderOpen,
                    Action = ToolbarPresetManager.OpenPresetsFolder
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ButtonResetToDefault,
                    Icon = FontAwesome.Solid.Undo,
                    Action = ResetToDefault
                });
            }
            else
            {
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ContextMenuAddSpacer,
                    Icon = FontAwesome.Solid.Plus,
                    Action = () => addSpacer(clickedZone ?? rightZone)
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.EditBannerSaveButton,
                    Icon = FontAwesome.Solid.Check,
                    Action = SaveAndExitEditMode
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ButtonSavePreset,
                    Icon = FontAwesome.Solid.Save,
                    Action = () => ShowSavePresetDialog(_ => { })
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ButtonOpenPresetsFolder,
                    Icon = FontAwesome.Solid.FolderOpen,
                    Action = ToolbarPresetManager.OpenPresetsFolder
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.DialogCancelButton,
                    Icon = FontAwesome.Solid.Times,
                    Action = CancelEditMode
                });
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ButtonResetToDefault,
                    Icon = FontAwesome.Solid.Undo,
                    IsDangerous = true,
                    Action = ResetToDefault
                });
            }

            contextMenu.ShowAt(pos, menuItems);
        }

        private void onBlockRightClicked(ToolbarBlockContainer block, MouseDownEvent e)
        {
            if (!isEditMode)
            {
                showGlobalMenu(e.ScreenSpaceMouseDownPosition, null);
                return;
            }

            var menuItems = new List<ContextMenuItemData>
            {
                new ContextMenuItemData
                {
                    Title = block.IsHidden.Value ? ExtendedToolbarStrings.ContextMenuShow : ExtendedToolbarStrings.ContextMenuHide,
                    Icon = block.IsHidden.Value ? FontAwesome.Solid.Eye : FontAwesome.Solid.EyeSlash,
                    Action = () => block.IsHidden.Value = !block.IsHidden.Value
                },
                new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ContextMenuMoveLeft,
                    Icon = FontAwesome.Solid.ArrowLeft,
                    Action = () => moveBlockToZone(block, ToolbarZone.Left)
                },
                new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ContextMenuMoveCenter,
                    Icon = FontAwesome.Solid.AlignCenter,
                    Action = () => moveBlockToZone(block, ToolbarZone.Center)
                },
                new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ContextMenuMoveRight,
                    Icon = FontAwesome.Solid.ArrowRight,
                    Action = () => moveBlockToZone(block, ToolbarZone.Right)
                }
            };

            if (block.ContentDrawable is ToolbarSpacer)
            {
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ContextMenuRemoveSpacer,
                    Icon = FontAwesome.Solid.Trash,
                    IsDangerous = true,
                    Action = () => removeSpacer(block)
                });
            }
            else
            {
                menuItems.Add(new ContextMenuItemData
                {
                    Title = ExtendedToolbarStrings.ContextMenuResetBlock,
                    Icon = FontAwesome.Solid.Undo,
                    Action = () => resetBlockToDefault(block)
                });
            }

            contextMenu.ShowAt(e.ScreenSpaceMouseDownPosition, menuItems);
        }

        private void resetBlockToDefault(ToolbarBlockContainer block)
        {
            var def = ToolbarLayoutConfig.CreateDefault();
            int leftIdx = def.Left.FindIndex(i => i.Id == block.ItemId);
            if (leftIdx >= 0)
            {
                insertBlockIntoZone(block, leftZone, leftIdx);
                block.IsHidden.Value = false;
                host.Notify(ExtendedToolbarStrings.NotifyBlockReset(block.DisplayName), NotificationKind.Info);
                return;
            }

            int rightIdx = def.Right.FindIndex(i => i.Id == block.ItemId);
            if (rightIdx >= 0)
            {
                insertBlockIntoZone(block, rightZone, rightIdx);
                block.IsHidden.Value = false;
                host.Notify(ExtendedToolbarStrings.NotifyBlockReset(block.DisplayName), NotificationKind.Info);
                return;
            }

            insertBlockIntoZone(block, rightZone, rightZone.Flow.Count);
            block.IsHidden.Value = false;
        }

        private void insertBlockIntoZone(ToolbarBlockContainer block, ToolbarZoneContainer targetZone, int targetIndex)
        {
            var sourceZone = getZone(block.CurrentZone);
            sourceZone.Flow.Remove(block, false);

            var targetFlow = targetZone.Flow;
            var targetList = targetZone.GetVisualOrderedChildren();
            targetList.Remove(block);

            int insertIdx = Math.Clamp(targetIndex, 0, targetList.Count);
            targetList.Insert(insertIdx, block);

            targetFlow.Clear(false);
            for (int i = 0; i < targetList.Count; i++)
            {
                targetFlow.Add(targetList[i]);
                targetFlow.SetLayoutPosition(targetList[i], i);
            }

            block.CurrentZone = targetZone.Zone;

            leftZone.UpdatePlaceholder();
            centerZone.UpdatePlaceholder();
            rightZone.UpdatePlaceholder();

            ExtendedToolbarLog.Info($"insertBlockIntoZone: '{block.DisplayName}' placed into zone {targetZone.Zone} at index {insertIdx} (total: {targetList.Count})");
        }

        private void moveBlockToZone(ToolbarBlockContainer block, ToolbarZone targetZone)
        {
            var targetContainer = getZone(targetZone);
            insertBlockIntoZone(block, targetContainer, targetContainer.GetVisualOrderedChildren().Count);
        }

        private void addSpacer(ToolbarZoneContainer zone)
        {
            string id = "spacer_" + Guid.NewGuid().ToString("N")[..6];
            var spacer = new ToolbarSpacer();
            var block = new ToolbarBlockContainer(id, "Разделитель", spacer);
            block.CurrentZone = zone.Zone;
            block.IsEditMode = true;
            bindBlockEvents(block);

            allBlocks[id] = block;
            insertBlockIntoZone(block, zone, zone.GetVisualOrderedChildren().Count);
        }

        private void removeSpacer(ToolbarBlockContainer block)
        {
            var flow = getZone(block.CurrentZone).Flow;
            flow.Remove(block, true);
            allBlocks.Remove(block.ItemId);
            leftZone.UpdatePlaceholder();
            centerZone.UpdatePlaceholder();
            rightZone.UpdatePlaceholder();
        }

        private ToolbarZoneContainer getZone(ToolbarZone zone) => zone switch
        {
            ToolbarZone.Left => leftZone,
            ToolbarZone.Center => centerZone,
            ToolbarZone.Right => rightZone,
            _ => leftZone
        };

        private void onBlockDragStarted(ToolbarBlockContainer block, DragStartEvent e)
        {
            draggingBlock = block;
            block.FadeTo(0.3f, 100);

            if (activeGhost != null) dragGhostContainer.Remove(activeGhost, true);

            activeGhost = new DragGhostBadge(block.DisplayName);
            activeGhost.Position = dragGhostContainer.ToLocalSpace(e.ScreenSpaceMouseDownPosition);
            dragGhostContainer.Add(activeGhost);

            ExtendedToolbarLog.Info($"onBlockDragStarted: Dragging '{block.DisplayName}' ({block.ItemId})");
        }

        private void onBlockDragged(ToolbarBlockContainer block, DragEvent e)
        {
            if (draggingBlock == null) return;

            Vector2 mousePos = e.ScreenSpaceMousePosition;

            if (activeGhost != null)
            {
                activeGhost.Position = dragGhostContainer.ToLocalSpace(mousePos);
            }

            var hoverZone = getZoneUnderMouse(mousePos);

            if (hoverZone != null)
            {
                currentTargetZone = hoverZone;
                currentTargetIndex = hoverZone.GetInsertionIndexForPosition(mousePos);

                leftZone.HideInsertIndicator();
                centerZone.HideInsertIndicator();
                rightZone.HideInsertIndicator();

                hoverZone.ShowInsertIndicator(currentTargetIndex);
            }
        }

        private void onBlockDragEnded(ToolbarBlockContainer block, DragEndEvent e)
        {
            if (draggingBlock == null) return;

            block.FadeTo(block.IsHidden.Value ? 0.35f : 1f, 100);
            leftZone.HideInsertIndicator();
            centerZone.HideInsertIndicator();
            rightZone.HideInsertIndicator();

            if (activeGhost != null)
            {
                activeGhost.FadeOut(100).Expire();
                activeGhost = null;
            }

            var mousePos = e.ScreenSpaceMousePosition;
            var targetZone = currentTargetZone ?? getZoneUnderMouse(mousePos) ?? getZone(block.CurrentZone);
            int targetIndex = currentTargetZone != null ? currentTargetIndex : targetZone.GetInsertionIndexForPosition(mousePos);

            insertBlockIntoZone(block, targetZone, targetIndex);

            draggingBlock = null;
            currentTargetZone = null;
        }

        private ToolbarZoneContainer? getZoneUnderMouse(Vector2 screenSpacePos)
        {
            if (leftZone.ReceivePositionalInputAt(screenSpacePos)) return leftZone;
            if (centerZone.ReceivePositionalInputAt(screenSpacePos)) return centerZone;
            if (rightZone.ReceivePositionalInputAt(screenSpacePos)) return rightZone;

            Vector2 local = ToLocalSpace(screenSpacePos);
            float width = DrawWidth;
            if (local.X < width / 3f) return leftZone;
            if (local.X > (width * 2f) / 3f) return rightZone;
            return centerZone;
        }

        private static string identifyDrawable(Drawable d)
        {
            string typeName = d.GetType().Name;

            if (typeName.Contains("Settings")) return "settings";
            if (typeName.Contains("Home")) return "home";
            if (typeName.Contains("Ruleset")) return "rulesets";
            if (typeName.Contains("Clock")) return "clock";
            if (typeName.Contains("Notification")) return "notifications";
            if (typeName.Contains("Ranking") || typeName.Contains("Performance") || typeName.Contains("Historical")) return "rankings";
            if (typeName.Contains("News")) return "news";
            if (typeName.Contains("Changelog")) return "changelog";
            if (typeName.Contains("Wiki")) return "wiki";
            if (typeName.Contains("BeatmapListing") || typeName.Contains("Direct")) return "beatmap_listing";
            if (typeName.Contains("Chat")) return "chat";
            if (typeName.Contains("Social")) return "social";
            if (typeName.Contains("Music")) return "music";
            if (typeName == "ToolbarUserButton" || typeName.StartsWith("ToolbarUser")) return "user_profile";
            if (d is ToolbarSpacer) return "spacer_" + Guid.NewGuid().ToString("N")[..6];

            return typeName.ToLowerInvariant();
        }

        private static string getFriendlyName(string id, Drawable d) => id switch
        {
            "settings" => ExtendedToolbarStrings.BlockSettings.ToString(),
            "home" => ExtendedToolbarStrings.BlockHome.ToString(),
            "rulesets" => ExtendedToolbarStrings.BlockRulesets.ToString(),
            "clock" => ExtendedToolbarStrings.BlockClock.ToString(),
            "notifications" => ExtendedToolbarStrings.BlockNotifications.ToString(),
            "rankings" => ExtendedToolbarStrings.BlockRankings.ToString(),
            "news" => ExtendedToolbarStrings.BlockNews.ToString(),
            "changelog" => ExtendedToolbarStrings.BlockChangelog.ToString(),
            "wiki" => ExtendedToolbarStrings.BlockWiki.ToString(),
            "beatmap_listing" => ExtendedToolbarStrings.BlockBeatmaps.ToString(),
            "chat" => ExtendedToolbarStrings.BlockChat.ToString(),
            "social" => ExtendedToolbarStrings.BlockSocial.ToString(),
            "music" => ExtendedToolbarStrings.BlockMusic.ToString(),
            "user_profile" => ExtendedToolbarStrings.BlockUserProfile.ToString(),
            _ => d is ToolbarSpacer ? ExtendedToolbarStrings.BlockSpacer.ToString() : d.GetType().Name
        };

        private static Drawable? findGridContainer(Drawable root)
        {
            foreach (var child in getChildren(root))
            {
                if (child is GridContainer gc)
                    return gc;

                var nested = findGridContainer(child);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        private static Drawable? findVisualChild(Drawable root, string typeOrName)
        {
            foreach (var child in getChildren(root))
            {
                if (child.GetType().Name.Contains(typeOrName) || child.Name == typeOrName)
                    return child;

                var nested = findVisualChild(child, typeOrName);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        private static FillFlowContainer? findRightFlow(Drawable root)
        {
            foreach (var child in getChildren(root))
            {
                if (child.Name == "Right buttons" || child.Name == "Right flow")
                {
                    var flow = getChildren(child).OfType<FillFlowContainer>().FirstOrDefault() ?? (child as FillFlowContainer);
                    if (flow != null)
                        return flow;
                }

                if (child is FillFlowContainer f && (f.Anchor == Anchor.TopRight || f.Origin == Anchor.TopRight))
                    return f;

                var found = findRightFlow(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static FillFlowContainer? findLeftFlow(Drawable root)
        {
            foreach (var child in getChildren(root))
            {
                if (child.Name == "Left buttons" || child.Name == "Left flow")
                {
                    var flow = getChildren(child).OfType<FillFlowContainer>().FirstOrDefault() ?? (child as FillFlowContainer);
                    if (flow != null)
                        return flow;
                }

                if (child is FillFlowContainer f && (f.Anchor == Anchor.TopLeft || f.Origin == Anchor.TopLeft))
                    return f;

                var found = findLeftFlow(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static IEnumerable<Drawable> getChildren(Drawable drawable)
        {
            if (drawable == null) yield break;

            var childrenProp = drawable.GetType().GetProperty("Children", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (childrenProp?.GetValue(drawable) is IEnumerable<Drawable> children)
            {
                foreach (var child in children)
                    yield return child;
            }

            var contentProp = drawable.GetType().GetProperty("Content", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (contentProp?.GetValue(drawable) is IEnumerable content)
            {
                foreach (var row in content)
                {
                    if (row is Drawable cellDrawable)
                    {
                        yield return cellDrawable;
                    }
                    else if (row is IEnumerable rowContent)
                    {
                        foreach (var cell in rowContent)
                        {
                            if (cell is Drawable inner)
                                yield return inner;
                        }
                    }
                }
            }
        }

        private sealed partial class DragGhostContainer : Container
        {
            public override bool HandlePositionalInput => false;
            public override bool PropagatePositionalInputSubTree => false;
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => false;
        }

        private sealed partial class DragGhostBadge : CompositeDrawable
        {
            public override bool HandlePositionalInput => false;
            public override bool PropagatePositionalInputSubTree => false;
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => false;

            public DragGhostBadge(string title)
            {
                AutoSizeAxes = Axes.Both;
                Origin = Anchor.Centre;
                Depth = float.MinValue;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 6,
                        BorderThickness = 2,
                        BorderColour = Colour4.FromHex("#ff66aa"),
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.Black.Opacity(0.85f)
                            },
                            new OsuSpriteText
                            {
                                Padding = new MarginPadding { Horizontal = 10, Vertical = 6 },
                                Text = title,
                                Font = OsuFont.GetFont(size: 13, weight: FontWeight.Bold),
                                Colour = Colour4.White
                            }
                        }
                    }
                };
            }
        }
    }
}
