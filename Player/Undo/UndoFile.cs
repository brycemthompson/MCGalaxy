﻿/*
    Copyright 2015 MCGalaxy
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCGalaxy.Util {

    public abstract class UndoFile {
        
        protected const string undoDir = "extra/undo", prevUndoDir = "extra/undoPrevious";
        public static UndoFile OldFormat = new UndoFileText();
        public static UndoFile NewFormat = new UndoFileBin();
        
        protected abstract void SaveUndoData(List<Player.UndoPos> buffer, string path);
        
        protected abstract void ReadUndoData(List<Player.UndoPos> buffer, string path);
        
        protected abstract bool UndoEntry(Player p, string path, long seconds);
        
        protected abstract bool HighlightEntry(Player p, string path, long seconds);
        
        protected abstract string Extension { get; }
        
        public static void SaveUndo(Player p) {
            if( p == null || p.UndoBuffer == null || p.UndoBuffer.Count < 1) return;
            
            CreateDefaultDirectories();
            if (Directory.GetDirectories(undoDir).Length >= Server.totalUndo) {
                Directory.Delete(prevUndoDir, true);
                Directory.Move(undoDir, prevUndoDir);
                Directory.CreateDirectory(undoDir);
            }

            string playerDir = Path.Combine(undoDir, p.name.ToLower());
            if (!Directory.Exists(playerDir))
                Directory.CreateDirectory(playerDir);
            
            int numFiles = Directory.GetFiles(playerDir).Length;
            string path = Path.Combine(playerDir, numFiles + NewFormat.Extension);
            NewFormat.SaveUndoData(p.UndoBuffer, path);
        }
        
        public static void UndoPlayer(Player p, string targetName, long seconds, ref bool FoundUser) {
            if (p != null)
                p.RedoBuffer.Clear();
            FilterEntries(p, undoDir, targetName, seconds, false, ref FoundUser);
            FilterEntries(p, prevUndoDir, targetName, seconds, false, ref FoundUser);
        }
        
        public static void HighlightPlayer(Player p, string targetName, long seconds, ref bool FoundUser) {
            FilterEntries(p, undoDir, targetName, seconds, true, ref FoundUser);
            FilterEntries(p, prevUndoDir, targetName, seconds, true, ref FoundUser);
        }
        
        static void FilterEntries(Player p, string dir, string name, long seconds, bool highlight, ref bool FoundUser) {
            string path = Path.Combine(dir, name);
            if (!Directory.Exists(path))
                return;
            string[] files = Directory.GetFiles(path);
            Array.Sort<string>(files, CompareFiles);
            
            for (int i = files.Length - 1; i >= 0; i--) {
                path = files[i];
                string file = Path.GetFileName(path);
                if (file.Length == 0 || file[0] < '0' || file[0] > '9')
                    continue;
                
                UndoFile format = null;
                if (path.EndsWith(OldFormat.Extension)) format = OldFormat;
                if (path.EndsWith(NewFormat.Extension)) format = NewFormat;
                if (format == null) continue;
                
                if (highlight) {
                    if (!format.HighlightEntry(p, path, seconds)) break;
                } else {
                    if (!format.UndoEntry(p, path, seconds)) break;
                }
            }
            FoundUser = true;
        }
        
        static int CompareFiles(string a, string b) {
            int aNumEnd = a.IndexOf('.'), bNumEnd = b.IndexOf('.');
            if (aNumEnd < 0 || bNumEnd < 0) return a.CompareTo(b);
            
            int aNum, bNum;
            if (!int.TryParse(a.Substring(0, aNumEnd), out aNum) ||
                !int.TryParse(b.Substring(0, bNumEnd), out bNum))
                return a.CompareTo(b);
            return aNum.CompareTo(bNum);
        }
        
        public static void CreateDefaultDirectories() {
            if (!Directory.Exists(undoDir))
                Directory.CreateDirectory(undoDir);
            if (!Directory.Exists(prevUndoDir))
                Directory.CreateDirectory(prevUndoDir);
        }
        
        public static void UpgradePlayerUndoFiles(string name) {
            UpgradeFiles(undoDir, name);
            UpgradeFiles(prevUndoDir, name);
        }
        
        static void UpgradeFiles(string dir, string name) {
            string path = Path.Combine(dir, name);
            if (!Directory.Exists(path))
                return;
            string[] files = Directory.GetFiles(path);
            List<Player.UndoPos> buffer = new List<Player.UndoPos>();
            
            for (int i = 0; i < files.Length; i++) {
                path = files[i];
                if (!path.EndsWith(OldFormat.Extension)) 
                    continue;
                buffer.Clear();
                OldFormat.ReadUndoData(buffer, path);
                
                string newPath = Path.ChangeExtension(path, NewFormat.Extension);
                NewFormat.SaveUndoData(buffer, newPath);
                File.Delete(path);
            }
        }
    }
}