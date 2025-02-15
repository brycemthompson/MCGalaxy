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
using MCGalaxy.Network;

namespace MCGalaxy 
{
    public enum CpeMessageType : byte 
    {
        Normal = 0, Status1 = 1, Status2 = 2, Status3 = 3,
        BottomRight1 = 11, BottomRight2 = 12, BottomRight3 = 13,
        Announcement = 100, BigAnnouncement = 101, SmallAnnouncement = 102 
    }
    
    public enum EnvProp : byte 
    {
        SidesBlock = 0, EdgeBlock = 1, EdgeLevel = 2,
        CloudsLevel = 3, MaxFog = 4, CloudsSpeed = 5,
        WeatherSpeed = 6, WeatherFade = 7, ExpFog = 8,
        SidesOffset = 9, SkyboxHorSpeed = 10, SkyboxVerSpeed = 11,
        
        Max,
        Weather = 255, // this is internal, not an official env prop
    }
    
    public enum EntityProp : byte 
    {
        RotX = 0, RotY = 1, RotZ = 2, ScaleX = 3, ScaleY = 4, ScaleZ = 5,
    }
    
    
    public class CpeExt 
    {
    	public string Name;
    	public byte ServerVersion;
    	public byte ClientVersion;
    	
        public const string ClickDistance = "ClickDistance";
        public const string CustomBlocks = "CustomBlocks";
        public const string HeldBlock = "HeldBlock";
        public const string TextHotkey = "TextHotKey";
        public const string ExtPlayerList = "ExtPlayerList";
        public const string EnvColors = "EnvColors";
        public const string SelectionCuboid = "SelectionCuboid";
        public const string BlockPermissions = "BlockPermissions";
        public const string ChangeModel = "ChangeModel";
        public const string EnvMapAppearance = "EnvMapAppearance";
        public const string EnvWeatherType = "EnvWeatherType";
        public const string HackControl = "HackControl";
        public const string EmoteFix = "EmoteFix";
        public const string MessageTypes = "MessageTypes";
        public const string LongerMessages = "LongerMessages";
        public const string FullCP437 = "FullCP437";
        public const string BlockDefinitions = "BlockDefinitions";
        public const string BlockDefinitionsExt = "BlockDefinitionsExt";
        public const string TextColors = "TextColors";
        public const string BulkBlockUpdate = "BulkBlockUpdate";
        public const string EnvMapAspect = "EnvMapAspect";
        public const string PlayerClick = "PlayerClick";
        public const string EntityProperty = "EntityProperty";
        public const string ExtEntityPositions = "ExtEntityPositions";
        public const string TwoWayPing = "TwoWayPing";
        public const string InventoryOrder = "InventoryOrder";
        public const string InstantMOTD = "InstantMOTD";
        public const string FastMap = "FastMap";
        public const string ExtBlocks = "ExtendedBlocks";
        public const string ExtTextures = "ExtendedTextures";
        public const string SetHotbar = "SetHotbar";
        public const string SetSpawnpoint = "SetSpawnpoint";
        public const string VelocityControl = "VelocityControl";
        public const string CustomParticles = "CustomParticles";
        public const string CustomModels = "CustomModels";
    }
    
    public sealed class CpeExtension 
    {
        public string Name;
        public byte Version = 1;
        public string Desc;
        public bool Enabled;
        
        public CpeExtension(string name) { Name = name; }
        public CpeExtension(string name, byte version) {
            Name = name; Version = version;
        }
        
        
        /// <summary> Array of all supported CPE extensions </summary>
        public static CpeExtension[] All = new CpeExtension[] {
            new CpeExtension(CpeExt.ClickDistance),    
            new CpeExtension(CpeExt.CustomBlocks),
            new CpeExtension(CpeExt.HeldBlock),        
            new CpeExtension(CpeExt.TextHotkey),
            new CpeExtension(CpeExt.ExtPlayerList, 2), 
            new CpeExtension(CpeExt.EnvColors),
            new CpeExtension(CpeExt.SelectionCuboid),  
            new CpeExtension(CpeExt.BlockPermissions),
            new CpeExtension(CpeExt.ChangeModel),      
            new CpeExtension(CpeExt.EnvMapAppearance, 2),
            new CpeExtension(CpeExt.EnvWeatherType),   
            new CpeExtension(CpeExt.HackControl),
            new CpeExtension(CpeExt.EmoteFix),         
            new CpeExtension(CpeExt.MessageTypes),
            new CpeExtension(CpeExt.LongerMessages),   
            new CpeExtension(CpeExt.FullCP437),
            new CpeExtension(CpeExt.BlockDefinitions), 
            new CpeExtension(CpeExt.BlockDefinitionsExt, 2),
            new CpeExtension(CpeExt.TextColors),       
            new CpeExtension(CpeExt.BulkBlockUpdate),
            new CpeExtension(CpeExt.EnvMapAspect),     
            new CpeExtension(CpeExt.PlayerClick),
            new CpeExtension(CpeExt.EntityProperty),   
            new CpeExtension(CpeExt.ExtEntityPositions),
            new CpeExtension(CpeExt.TwoWayPing),       
            new CpeExtension(CpeExt.InventoryOrder),
            new CpeExtension(CpeExt.InstantMOTD),      
            new CpeExtension(CpeExt.FastMap),
            new CpeExtension(CpeExt.ExtTextures),      
            new CpeExtension(CpeExt.SetHotbar),
            new CpeExtension(CpeExt.SetSpawnpoint),    
            new CpeExtension(CpeExt.VelocityControl),
            new CpeExtension(CpeExt.CustomParticles),  
            new CpeExtension(CpeExt.CustomModels, 2),
            #if TEN_BIT_BLOCKS
            new CpeExtension(CpeExt.ExtBlocks),
            #endif
        };
        internal static CpeExt[] Empty = new CpeExt[0];
        
        /// <summary> Retrieves a list of all supported and enabled CPE extensions </summary>
        public static CpeExt[] GetAllEnabled() {
            if (!Server.Config.EnableCPE) return Empty;
            CpeExtension[] all = All;
            CpeExt[] exts = new CpeExt[all.Length];
            
            for (int i = 0; i < exts.Length; i++)
            {
                CpeExtension e = all[i];
                exts[i] = new CpeExt() { Name = e.Name, ServerVersion = e.Version };
            }
            return exts;
        }
    }
}