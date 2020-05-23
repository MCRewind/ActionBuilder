using System;
using System.Collections.Generic;
using ActionBuilder.Editor;
using ActionBuilder.Utils;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Num = System.Numerics;

namespace ActionBuilder
{
    public class ActionBuilder : Game
    {
        private GraphicsDeviceManager _graphics;
        private ImGuiRenderer _imGuiRenderer;
        private SpriteBatch _spriteBatch;
        
        private Texture2D _xnaTexture;
        private IntPtr _imGuiTexture;

        private SpriteFont _font;

        private Dictionary<string, Widget> _widgets;

        private readonly Color _clearColor = new Color(18, 18, 18);
        
        public ActionBuilder()
        {
            _widgets = new Dictionary<string, Widget>();
            
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1024,
                PreferredBackBufferHeight = 768,
                PreferMultiSampling = true,
                SynchronizeWithVerticalRetrace = false
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        
        protected override void Initialize()
        {
            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            //io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            
            CreateWidgets();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("DefaultFont");

            _xnaTexture = CreateTexture(GraphicsDevice, 300, 150, pixel =>
            {
                var red = pixel % 300 / 2;
                return new Color(red, 1, 1);
            });

            _imGuiTexture = _imGuiRenderer.BindTexture(_xnaTexture);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_clearColor);

            _imGuiRenderer.BeforeLayout(gameTime);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Num.Vector2(0.0f, 0.0f));
          
            foreach (var widget in _widgets.Values)
            {
                if (widget.Begin())
                {
                    widget.Tick();
                    widget.End();
                }
            }
            
            ImGui.PopStyleVar(1);
            ImGuiLayout();

            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }

        private void CreateWidgets()
        {
            var widgetTypes = ReflectiveEnumerator.GetEnumerableOfType<Widget>();
            foreach (var widgetType in widgetTypes)
            {
                var widget = (Widget) Activator.CreateInstance(widgetType, GraphicsDevice);
                if (widget != null)
                {
                    _widgets.Add(widget.Title, widget);
                }
            }

            foreach (var widget in _widgets.Values)
            {
                if (widget.RequiredWidgets == null) continue;

                var widgetList = new List<string>();
                
                foreach (var requiredWidget in widget.RequiredWidgets)
                {
                    if (_widgets[requiredWidget.Key] != null)
                    {
                        widgetList.Add(requiredWidget.Key);
                    }
                }

                foreach (var widgetName in widgetList)
                {
                    widget.RequiredWidgets[widgetName] = _widgets[widgetName];
                }
            }
        }
        
        private bool _showTestWindow;
        private bool _draggingWindow;

        protected virtual void ImGuiLayout()
        {
            /*if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) &&
                ImGui.IsWindowFocused())
            {
                if (!_draggingWindow)
                {
                    IsMouseVisible = false;
                    ImGui.GetIO().MouseDrawCursor = true;

                    _draggingWindow = true;
                }
            }
            else
            {
                IsMouseVisible = true;
                ImGui.GetIO().MouseDrawCursor = false;

                _draggingWindow = false;
            }*/

            if (_showTestWindow)
            {
                ImGui.SetNextWindowPos(new Num.Vector2(650, 20), ImGuiCond.FirstUseEver);
                ImGui.ShowDemoWindow(ref _showTestWindow);
            }
        }
        
        public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
        {
            var texture = new Texture2D(device, width, height);

            var data = new Color[width * height];
            for(var pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = paint(pixel);
            }

            texture.SetData(data);

            return texture;
        }
    }
}