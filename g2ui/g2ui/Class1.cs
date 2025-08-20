using Eto.Drawing;

using Eto.Forms;

using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;

using Grasshopper2.Extensions;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.ContentBrowser;
using Grasshopper2.UI.Flex;

using Grasshopper2.UI.Primitives;

using Grasshopper2.UI.Skinning;

using GrasshopperIO;
using ku;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Brushes = Eto.Drawing.Brushes;
using Color = Eto.Drawing.Color;
using FontStyle = Eto.Drawing.FontStyle;
using Path = System.IO.Path;
using Pens = Eto.Drawing.Pens;
using PointF = Eto.Drawing.PointF;
using RectangleF = Eto.Drawing.RectangleF;
using Size = Eto.Drawing.Size;
using SystemFonts = Eto.Drawing.SystemFonts;
namespace ku
{
    public class GitHubBrowser
    {
        public void ShowGitHubFileListWindow2(string path = "")
        {
            var form = new Form
            {
                Title = "GitHub 仓库文件列表",
                ClientSize = new Size(600, 400)
            };

            var grid = new GridView();
            grid.ShowHeader = true;

            // 文件名列
            grid.Columns.Add(new GridColumn
            {
                HeaderText = "文件名",
                DataCell = new TextBoxCell { Binding = Binding.Property<FileItem, string>(r => r.Name) },
                Expand = true
            });

            // 类型列
            grid.Columns.Add(new GridColumn
            {
                HeaderText = "类型",
                DataCell = new TextBoxCell { Binding = Binding.Property<FileItem, string>(r => r.Type) }
            });

            // 顶部返回按钮
            var backButton = new Button { Text = "← 返回上一级" };
            backButton.Enabled = false;

            var layout = new DynamicLayout();
            layout.AddRow(backButton);
            layout.AddRow(grid);
            form.Content = layout;

            string currentPath = path;

            async System.Threading.Tasks.Task LoadFiles(string relativePath)
            {
                string repoApiUrl = $"https://api.github.com/repos/wuq1/gh2_jzbf/contents/{relativePath}";
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "request");
                        string json = await client.GetStringAsync(repoApiUrl);
                        var files = JArray.Parse(json);

                        var items = new List<FileItem>();
                        foreach (var file in files)
                        {
                            items.Add(new FileItem
                            {
                                Name = (string)file["name"],
                                Type = (string)file["type"],
                                DownloadUrl = (string)file["download_url"]
                            });
                        }

                        grid.DataStore = items;
                    }

                    backButton.Enabled = !string.IsNullOrEmpty(relativePath);
                }
                catch (Exception ex)
                {
                    grid.DataStore = new List<FileItem> { new FileItem { Name = $"请求失败: {ex.Message}", Type = "" } };
                    backButton.Enabled = false;
                }
            }

            // 首次加载
            _ = LoadFiles(currentPath);

            // 返回上一级按钮
            backButton.Click += async (sender, e) =>
            {
                if (!string.IsNullOrEmpty(currentPath))
                {
                    int lastSlash = currentPath.LastIndexOf('/');
                    currentPath = lastSlash > 0 ? currentPath.Substring(0, lastSlash) : "";
                    await LoadFiles(currentPath);
                }
            };

            // 双击进入子目录 或 下载文件
            grid.CellDoubleClick += async (sender, e) =>
            {
                if (e.Item is FileItem file)
                {
                    if (file.Type == "dir")
                    {
                        currentPath = string.IsNullOrEmpty(currentPath) ? file.Name : $"{currentPath}/{file.Name}";
                        await LoadFiles(currentPath);
                    }
                    else if (file.Type == "file")
                    {
                        await DownloadFile(file, form);
                      
                    }
                }
            };

            // 右键菜单下载
            var menu = new ContextMenu();
            var downloadItem = new ButtonMenuItem { Text = "下载到桌面并且在参考窗口打开" };
            downloadItem.Click += async (sender, e) =>
            {

                if (grid.SelectedItem is FileItem file && file.Type == "file") 
                { await DownloadFile(file, form); }
            };
            menu.Items.Add(downloadItem);
            grid.ContextMenu = menu;

            form.Show();
        }

        private async System.Threading.Tasks.Task DownloadFile(FileItem file, Form parent)
        {
           
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "request");

                    string savePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        file.Name
                    );
                    byte[] bytes = await client.GetByteArrayAsync(file.DownloadUrl);
                    File.WriteAllBytes(savePath, bytes);

                    MessageBox.Show(parent, $"已下载: {file.Name}\n保存到桌面");
                    if (file.Name.EndsWith(".ghz", StringComparison.OrdinalIgnoreCase))
                    {
                        // 下载完成后，直接打开 GH2 文档
                        var use = new Use();
                        use.ShowFileListWindow2(savePath);   // ✅ 注意这里传完整路径
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(parent, $"下载失败: {ex.Message}");
            }
        }

        // 数据结构
        class FileItem
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string DownloadUrl { get; set; }
        }
    }
    public class MyComponentAttributes2 : ComponentAttributes
    {
        private RectangleF buttonRect;   // 按钮矩形
        private bool pressed = false;    // 按钮状态

        public MyComponentAttributes2(Component owner) : base(owner) { }

        protected override void LayoutBounds(Shape shape)
        {
            base.LayoutBounds(shape);

            // 在电池底部增加 25 高的矩形区域作为按钮
            float extraHeight = 25f;
            buttonRect = RectangleF.FromSides(
                Bounds.Left,
                Bounds.Bottom,
                Bounds.Right,
                Bounds.Bottom + extraHeight
            );

            // 扩展电池整体 Bounds，包含按钮
            Bounds = RectangleF.Union(Bounds, buttonRect);
        }

        protected override void DrawForeground(Context context, Skin skin, Capsule capsule, Shade shade)
        {
            base.DrawForeground(context, skin, capsule, shade);

            // 按钮颜色
            var fill = pressed ? Colors.DarkGray : Colors.DarkGray;
            var border = Colors.Black;
            // 原始按钮区域（比如在电池下面）
            RectangleF buttonRect = new RectangleF(Bounds.X, Bounds.Y + 25, Bounds.Width, Bounds.Height - 26);

            // 在四周都收缩 2 像素
            buttonRect.Inflate(-2, -2);
            // 绘制按钮
            //context.Graphics.FillRectangle(fill, buttonRect);
            // context.Graphics.DrawRectangle(border, buttonRect);
            // 在你的电池 UI 绘制里用：

            float cornerRadius = 4f;
            using (var path = CreateRoundedRect(buttonRect, cornerRadius))
            {
                context.Graphics.FillPath(fill, path);
                context.Graphics.DrawPath(Pens.Gray, path);
            }

            // 绘制按钮文字
            var text = "快来逛逛";
            //var font = SystemFonts.Default(10);
            var font = new Eto.Drawing.Font(Eto.Drawing.SystemFont.Default, 10);

            var size = context.Graphics.MeasureString(font, text);
            var center = new PointF(
                buttonRect.Left + (buttonRect.Width - size.Width) / 2,
                buttonRect.Top + (buttonRect.Height - size.Height) / 2
            );
            context.Graphics.DrawText(font, Colors.Black, center, text);
        }

        protected override Response HandleMouseDown(MouseEventArgs e)
        {
            RectangleF clickRect = buttonRect;
            clickRect.Inflate(-2, -2);  // 同步缩小
            if (buttonRect.Contains(e.Location))
            {
                pressed = !pressed; // 切换按钮状态

                // 弹出窗口
                /*
                var form = new Eto.Forms.Form
                {
                    Title = "按钮窗口",
                    ClientSize = new Eto.Drawing.Size(200, 100)
                };
                form.Content = new Eto.Forms.Label
                {
                    Text = "你点击了按钮！",
                    VerticalAlignment = Eto.Forms.VerticalAlignment.Center,
                     TextAlignment = Eto.Forms.TextAlignment.Center  // 水平居中
                };
                form.Show();*/
                //
                // ShowFileListWindow();

                GitHubBrowser b = new GitHubBrowser();
                b.ShowGitHubFileListWindow2();
                Owner.Document?.Solution.DelayedExpire(Owner); // 刷新电池
                return Response.Handled;
            }

            return base.HandleMouseDown(e);
        }
        //绘制窗口
        private void ShowFileListWindow()
        {
            var form = new Form
            {
                Title = "GH 目录文件列表",
                ClientSize = new Size(400, 300)
            };

            string dirPath = @"C:\Users\32035\Desktop\咸鱼\0625\0611\gh";
            var array1 = Directory.Exists(dirPath) ? Directory.GetFiles(dirPath) : new string[0];
            var array2 = Directory.Exists(dirPath) ? Directory.GetDirectories(dirPath) : new string[0];
            var list = new List<string>(array1);
            list.AddRange(array2);

            string[] combined = list.ToArray();

            var listBox = new ListBox { DataStore = combined };
            form.Content = listBox;

            form.Show();
        }
        private async void ShowGitHubFileListWindow()
        {
            var form = new Eto.Forms.Form
            {
                Title = "GitHub 仓库文件列表",
                ClientSize = new Eto.Drawing.Size(500, 400)
            };

            var listBox = new Eto.Forms.ListBox();
            form.Content = listBox;

            string repoApiUrl = "https://api.github.com/repos/wuq1/gh2_jzbf/contents/";

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    // GitHub API 要求添加 User-Agent
                    client.DefaultRequestHeaders.Add("User-Agent", "request");

                    string json = await client.GetStringAsync(repoApiUrl);
                    var files = Newtonsoft.Json.Linq.JArray.Parse(json);

                    var fileNames = new List<string>();
                    foreach (var file in files)
                    {
                        string name = (string)file["name"];
                        string type = (string)file["type"]; // file / dir
                        fileNames.Add($"{name} [{type}]");
                    }

                    listBox.DataStore = fileNames;
                }
            }
            catch (Exception ex)
            {
                listBox.DataStore = new List<string> { $"请求失败: {ex.Message}" };
            }

            form.Show();
        }
        private async void ShowGitHubFileListWindow2(string path = "")
        {
            var form = new Eto.Forms.Form
            {
                Title = "GitHub 仓库文件列表",
                ClientSize = new Eto.Drawing.Size(500, 400)
            };

            var listBox = new Eto.Forms.ListBox();

            // 顶部返回按钮
            var backButton = new Eto.Forms.Button { Text = "← 返回上一级" };
            backButton.Enabled = false; // 初始根目录不可返回

            // 布局：按钮在上，列表在下
            var layout = new Eto.Forms.DynamicLayout();
            layout.AddRow(backButton);
            layout.AddRow(listBox);
            form.Content = layout;

            string currentPath = path;

            async Task LoadFiles(string relativePath)
            {
                string repoApiUrl = $"https://api.github.com/repos/wuq1/gh2_jzbf/contents/{relativePath}";
                try
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "request");
                        string json = await client.GetStringAsync(repoApiUrl);
                        var files = Newtonsoft.Json.Linq.JArray.Parse(json);

                        var items = new List<string>();
                        foreach (var file in files)
                        {
                            string name = (string)file["name"];
                            string type = (string)file["type"]; // file / dir
                            items.Add($"{name} [{type}]");
                        }

                        listBox.DataStore = items;
                    }

                    // 根目录禁用返回按钮
                    backButton.Enabled = !string.IsNullOrEmpty(relativePath);
                }
                catch (Exception ex)
                {
                    listBox.DataStore = new List<string> { $"请求失败: {ex.Message}" };
                    backButton.Enabled = false;
                }
            }

            await LoadFiles(currentPath);

            // 返回上一级按钮点击
            backButton.Click += async (sender, e) =>
            {
                if (!string.IsNullOrEmpty(currentPath))
                {
                    int lastSlash = currentPath.LastIndexOf('/');
                    currentPath = lastSlash > 0 ? currentPath.Substring(0, lastSlash) : "";
                    await LoadFiles(currentPath);
                }
            };

            // 双击事件进入子目录
            listBox.MouseDoubleClick += async (sender, e) =>
            {
                if (listBox.SelectedValue is string selected)
                {
                    int idx = selected.LastIndexOf('[');
                    if (idx > 0)
                    {
                        string name = selected.Substring(0, idx).Trim();
                        string type = selected.Substring(idx + 1, selected.Length - idx - 2);

                        if (type == "dir")
                        {
                            currentPath = string.IsNullOrEmpty(currentPath) ? name : $"{currentPath}/{name}";
                            await LoadFiles(currentPath);
                        }
                        else
                        {
                            Eto.Forms.MessageBox.Show(form, $"这是文件: {name}");
                        }
                    }
                }
            };

            form.Show();
        }



        GraphicsPath CreateRoundedRect(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();

            float x = rect.X;
            float y = rect.Y;
            float w = rect.Width;
            float h = rect.Height;
            float r = radius;

            // 左上角
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            // 右上角
            path.AddArc(x + w - 2 * r, y, r * 2, r * 2, 270, 90);
            // 右下角
            path.AddArc(x + w - 2 * r, y + h - 2 * r, r * 2, r * 2, 0, 90);
            // 左下角
            path.AddArc(x, y + h - 2 * r, r * 2, r * 2, 90, 90);

            path.CloseFigure();
            return path;
        }

        private Color Lerp(Color a, Color b, float t)
        {
            float r = a.R + (b.R - a.R) * t;
            float g = a.G + (b.G - a.G) * t;
            float bl = a.B + (b.B - a.B) * t;
            float al = a.A + (b.A - a.A) * t;
            return new Color(r, g, bl, al);
        }
    }
    public class Use
    {
        public void ShowFileListWindow2(string path)
        {
            var form = new Form
            {
                Title = "GH2 FileUtility 测试",
                ClientSize = new Size(800, 600)
            };

            // 创建 Canvas
            var canvas = new Canvas();

            // 打开 GH2 文档
            //path = @"C:\Users\32035\Desktop\001.ghz";
            bool ok = canvas.TryOpenDocument(path, OpenDocumentOptions.Activate);

            // 获取当前文档（Canvas 内部会创建一个 Document）
            Document doc = canvas.Document;
            if (doc != null)
            {
                doc.Notes = "这是一个测试文档";
                doc.Modify();
            }

            // 把 Canvas 加入窗口
            form.Content = canvas;

            // 在窗口关闭时清理
            form.Closed += (sender, e) =>
            {
                if (canvas.Document != null)
                {
                    // 关闭文档释放资源
                    canvas.Document.Close();
                    Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
                }

                // 清空 Canvas
                //canvas.Document = null;
            };

            // 显示窗口
            form.Show();
        }


    }
}