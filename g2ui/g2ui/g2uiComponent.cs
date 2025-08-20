using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.Extensions;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Primitives;
using Grasshopper2.UI.Skinning;
using GrasshopperIO;
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
using Pens = Eto.Drawing.Pens;
using PointF = Eto.Drawing.PointF;
using RectangleF = Eto.Drawing.RectangleF;
using Size = Eto.Drawing.Size;
using SystemFonts = Eto.Drawing.SystemFonts;
using ku;
namespace g2ui
{
    [IoId("61832b1e-a334-4bab-9cf3-8f94d98a860e")]
    public sealed class ButtonComponent : Component
    {
        public ButtonComponent() : base(new Nomen(
            "github查看器",
            "Description",
            "ui",
            "Section"))
        {

        }

        public ButtonComponent(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The IDataAccess object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
        {

        }
        protected override IAttributes CreateAttributes()
        {
            return new MyComponentAttributes(this);
        }

        // ---------------- 按键类 ----------------
        private class MyComponentAttributes : ComponentAttributes
        {
            private RectangleF buttonRect;   // 按钮矩形
            private bool pressed = false;    // 按钮状态

            public MyComponentAttributes(Component owner) : base(owner) { }

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
               RectangleF buttonRect = new RectangleF( Bounds.X, Bounds.Y+25, Bounds.Width, Bounds.Height-26);

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
                var text = "按键";
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
        

        //-----------------圆角方法 ----------------
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

    }


}