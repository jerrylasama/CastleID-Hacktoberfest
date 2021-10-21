using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Subtitles;
using System;
using System.Drawing;
using System.IO;

namespace StorybrewScripts
{
    public class TanukiLyrics : StoryboardObjectGenerator
    {
        [Configurable]
        public string SubtitlesPath = "lyrics.srt";

        [Configurable]
        public string FontName = "Verdana";

        [Configurable]
        public string SpritesPath = "sb/f";

        [Configurable]
        public int FontSize = 26;

        [Configurable]
        public float FontScale = 0.5f;

        [Configurable]
        public Color4 FontColor = Color4.White;

        [Configurable]
        public FontStyle FontStyle = FontStyle.Regular;

        [Configurable]
        public int GlowRadius = 0;

        [Configurable]
        public Color4 GlowColor = new Color4(255, 255, 255, 100);

        [Configurable]
        public bool AdditiveGlow = true;

        [Configurable]
        public int OutlineThickness = 3;

        [Configurable]
        public Color4 OutlineColor = new Color4(50, 50, 50, 200);

        [Configurable]
        public int ShadowThickness = 0;

        [Configurable]
        public Color4 ShadowColor = new Color4(0, 0, 0, 100);

        [Configurable]
        public Vector2 Padding = Vector2.Zero;

        [Configurable]
        public float SubtitleY = 400;

        [Configurable]
        public float SubtitleX = 40;

        [Configurable]
        public bool PerCharacter = true;

        [Configurable]
        public bool VerticalMode = false;

        [Configurable]
        public KaraEffect KaraokeEffect = KaraEffect.Default;

        [Configurable]
        public bool TrimTransparency = true;

        [Configurable]
        public bool EffectsOnly = false;

        [Configurable]
        public bool Debug = false;

        [Configurable]
        public OsbOrigin Origin = OsbOrigin.Centre;

        public enum KaraEffect
        {
            Default,
            Klasik,
            Jalan,
            JalanV2,
            Jajal,
        }

        public override void Generate()
        {
            var font = LoadFont(SpritesPath, new FontDescription()
            {
                FontPath = FontName,
                FontSize = FontSize,
                Color = FontColor,
                Padding = Padding,
                FontStyle = FontStyle,
                TrimTransparency = TrimTransparency,
                EffectsOnly = EffectsOnly,
                Debug = Debug,
            },
            new FontGlow()
            {
                Radius = AdditiveGlow ? 0 : GlowRadius,
                Color = GlowColor,
            },
            new FontOutline()
            {
                Thickness = OutlineThickness,
                Color = OutlineColor,
            },
            new FontShadow()
            {
                Thickness = ShadowThickness,
                Color = ShadowColor,
            });

            var subtitles = LoadSubtitles(SubtitlesPath);

            if (GlowRadius > 0 && AdditiveGlow)
            {
                var glowFont = LoadFont(Path.Combine(SpritesPath, "glow"), new FontDescription()
                {
                    FontPath = FontName,
                    FontSize = FontSize,
                    Color = FontColor,
                    Padding = Padding,
                    FontStyle = FontStyle,
                    TrimTransparency = TrimTransparency,
                    EffectsOnly = true,
                    Debug = Debug,
                },
                new FontGlow()
                {
                    Radius = GlowRadius,
                    Color = GlowColor,
                });
                generateLyrics(glowFont, subtitles, "glow", true);
            }
            generateLyrics(font, subtitles, "", false);
        }

        public void generateLyrics(FontGenerator font, SubtitleSet subtitles, string layerName, bool additive)
        {
            var layer = GetLayer(layerName);
            if (PerCharacter) generatePerCharacter(font, subtitles, layer, additive);
            else generatePerLine(font, subtitles, layer, additive);
        }

        public void generatePerLine(FontGenerator font, SubtitleSet subtitles, StoryboardLayer layer, bool additive)
        {
            foreach (var line in subtitles.Lines)
            {
                var texture = font.GetTexture(line.Text);
                var position = new Vector2(320 - texture.BaseWidth * FontScale * 0.5f, SubtitleY)
                    + texture.OffsetFor(Origin) * FontScale;

                var sprite = layer.CreateSprite(texture.Path, Origin, position);
                sprite.Scale(line.StartTime, FontScale);
                sprite.Fade(line.StartTime - 200, line.StartTime, 0, 1);
                sprite.Fade(line.EndTime - 200, line.EndTime, 1, 0);
                if (additive) sprite.Additive(line.StartTime - 200, line.EndTime);
            }
        }

        public void generatePerCharacter(FontGenerator font, SubtitleSet subtitles, StoryboardLayer layer, bool additive)
        {
            foreach (var subtitleLine in subtitles.Lines)
            {
                var letterY = SubtitleY;
                var letterX = SubtitleX;
                foreach (var line in subtitleLine.Text.Split('\n'))
                {
                    var lineWidth = 0f;
                    var lineHeight = 0f;
                    foreach (var letter in line)
                    {
                        var texture = font.GetTexture(letter.ToString());
                        if (VerticalMode){
                            lineHeight += texture.BaseHeight * FontScale;
                            lineWidth = Math.Max(lineWidth, texture.BaseWidth * FontScale);
                        } else {
                            lineWidth += texture.BaseWidth * FontScale;
                            lineHeight = Math.Max(lineHeight, texture.BaseHeight * FontScale);
                        }
                    }

                    if (VerticalMode) {
                        letterY = 240 - lineHeight * 0.5f;
                    } else {
                        letterX = 320 - lineWidth * 0.5f;
                    }

                    string katake = "";
                    foreach (var letter in line)
                    {
                        katake = katake + letter;
                        var texture = font.GetTexture(letter.ToString());
                        if (!texture.IsEmpty)
                        {
                            var position = new Vector2(letterX, letterY)
                                + texture.OffsetFor(Origin) * FontScale;

                            var sprite = layer.CreateSprite(texture.Path, Origin, position);
                            sprite.Scale(subtitleLine.StartTime, FontScale);
                            var jarakdelay = (subtitleLine.EndTime - subtitleLine.StartTime)/line.ToString().Length;

                            switch(KaraokeEffect){
                                case KaraEffect.Default:
                                    sprite.Fade(subtitleLine.StartTime, subtitleLine.StartTime + jarakdelay, 0, 1);
                                    sprite.Fade(subtitleLine.EndTime, subtitleLine.EndTime + jarakdelay, 1, 0);
                                break;
                                case KaraEffect.Klasik:
                                    sprite.Move(OsbEasing.InOutExpo,subtitleLine.StartTime + (jarakdelay*katake.Length), subtitleLine.EndTime - (jarakdelay*(line.Length-katake.Length+1)), position.X, position.Y, position.X, position.Y);
                                break;
                                case KaraEffect.Jalan:
                                    sprite.Fade(subtitleLine.StartTime , subtitleLine.StartTime , 0, 0);
                                    sprite.Fade(subtitleLine.StartTime + (jarakdelay*katake.Length), (subtitleLine.StartTime + (jarakdelay*(katake.Length+2))) - jarakdelay, 1, 0);
                                break;
                                case KaraEffect.JalanV2:
                                    sprite.Fade(subtitleLine.StartTime - (jarakdelay*katake.Length), subtitleLine.StartTime - (jarakdelay*katake.Length+1), 1, 0);
                                    sprite.Fade(subtitleLine.EndTime - (jarakdelay*(line.Length-katake.Length)), subtitleLine.EndTime , 1, 0);
                                break;
                                case KaraEffect.Jajal:
                                    sprite.Fade(subtitleLine.StartTime - (jarakdelay*katake.Length), subtitleLine.StartTime - (jarakdelay*katake.Length+1), 1, 0);
                                    sprite.Fade(subtitleLine.EndTime - (jarakdelay*(line.Length-katake.Length)), subtitleLine.EndTime , 1, 0);
                                    sprite.Rotate(OsbEasing.InExpo, subtitleLine.StartTime + (jarakdelay*katake.Length), subtitleLine.StartTime + (jarakdelay*(katake.Length+1)) , MathHelper.DegreesToRadians(Random(Math.PI * 2)), MathHelper.DegreesToRadians(360));
                                break;
                            }

                            if (additive) sprite.Additive(subtitleLine.StartTime - 200, subtitleLine.EndTime);
                        }
                        if (VerticalMode){
                            letterY += texture.BaseHeight * FontScale;
                        } else {
                            letterX += texture.BaseWidth * FontScale;
                        }
                    }
                    if (VerticalMode){
                        letterX += lineWidth;
                    } else {
                        letterY += lineHeight;
                    }
                }
            }
        }
    }
}
