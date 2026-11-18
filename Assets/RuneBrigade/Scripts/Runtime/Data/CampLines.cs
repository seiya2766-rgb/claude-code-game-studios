using System;
using System.Collections.Generic;
using RuneBrigade.Core;
using UnityEngine;

namespace RuneBrigade
{
    /// <summary>
    /// Everything the camp says: Garo's lines when the player first wakes, the line
    /// when each feature returns, and the first-run shatter hint. Text only, no voice
    /// (design/game-concept.md, "物語の見せ方").
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Brigade/Camp Lines", fileName = "CampLines")]
    public sealed class CampLines : ScriptableObject
    {
        /// <summary>One spoken line.</summary>
        [Serializable]
        public sealed class Line
        {
            public string speaker = "ガロ";
            [TextArea(2, 4)] public string text = "";

            public Line() { }

            public Line(string speaker, string text)
            {
                this.speaker = speaker;
                this.text = text;
            }
        }

        /// <summary>The line spoken when a feature returns.</summary>
        [Serializable]
        public sealed class Announcement
        {
            public CampFeature feature;
            public Line line = new Line();

            public Announcement() { }

            public Announcement(CampFeature feature, string speaker, string text)
            {
                this.feature = feature;
                line = new Line(speaker, text);
            }
        }

        [Tooltip("Spoken in order the very first time the player wakes in camp.")]
        public List<Line> wakeLines = DefaultWakeLines();

        [Tooltip("Spoken when the player comes back and a feature has returned.")]
        public List<Announcement> announcements = DefaultAnnouncements();

        [Tooltip("First run only: shown the first time an enemy is frozen while not playing the Warden.")]
        public Line shatterHint = new Line("ガロ", "今だ！ 守護者に替えて（1キー）、槌の三段目で砕け！");

        [Tooltip("Shown when the player returns and nothing new came back.")]
        public Line welcomeBack = new Line("ガロ", "よく戻った。火にあたっていけ。");

        /// <summary>The announcement for <paramref name="feature"/>, or null.</summary>
        public Line For(CampFeature feature)
        {
            foreach (Announcement announcement in announcements)
            {
                if (announcement.feature == feature) return announcement.line;
            }
            return null;
        }

        private void Reset()
        {
            wakeLines = DefaultWakeLines();
            announcements = DefaultAnnouncements();
        }

        private static List<Line> DefaultWakeLines()
        {
            return new List<Line>
            {
                new Line("ガロ", "……目が覚めたか。灯が、お前の名前を書き直してくれた。"),
                new Line("ガロ", "左腕を見てみろ。光っている刻印が二つある。守（まもり）と変（かわり）だ。"),
                new Line("ガロ", "お前は灯の担い手だ。四つの刻印を全部宿せるのは、お前だけだ。"),
                new Line("ガロ", "下の村が見えるか。あそこにはもう、名前が残っていない。……門から降りていけ。"),
            };
        }

        private static List<Announcement> DefaultAnnouncements()
        {
            return new List<Announcement>
            {
                new Announcement(CampFeature.Forge, "ミルダ", "ふん、生きて帰ってきたかい。欠片を持ってきな。刻んでやるよ。"),
                new Announcement(CampFeature.Archive, "トト", "あっ、それ魔物の記録ですよね！ 書かせてください、全部！"),
                new Announcement(CampFeature.HunterRole, "ガロ", "刻印がもう一つ光ったな。射（いぬき）……狩人の力だ。"),
                new Announcement(CampFeature.Watchtower, "ガロ", "見張り台を直しておいた。もっと深い侵食にも挑めるぞ。"),
                new Announcement(CampFeature.VillageMistCleared, "ガロ", "見ろ、村の霧が晴れてきた。屋根の色が戻ってる。"),
                new Announcement(CampFeature.OracleRole, "ガロ", "四つ目の刻印だ。結（むすび）……祈祷師の力だ。"),
                new Announcement(CampFeature.Wardrobe, "ガロ", "天幕に布が戻ってきた。好きな色を選んでいけ。"),
            };
        }
    }
}
